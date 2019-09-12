using System;
using System.Threading.Tasks;
using IUBH.TOR.Data;
using IUBH.TOR.Domain;
using IUBH.TOR.Modules.Courses.Data;
using IUBH.TOR.Modules.Courses.Domain;
using IUBH.TOR.Modules.Courses.Services;
using IUBH.TOR.Utilities.Date;
using IUBH.TOR.Utilities.Preferences;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace IUBH.TOR.Tests.Modules.Courses
{
    public class CourseUpdaterSpecs
    {
        private static CourseUpdater CreateCourseUpdater(
            ICoursePageHtmlDownloader downloader = null,
            ICoursePageHtmlParser parser = null,
            ICourseSetComparer comparer = null,
            ICourseRepository repository = null,
            IUnitOfWork unitOfWork = null,
            IDateTimeUtility dateTime = null,
            IPreferencesUtility preferences = null
        )
        {
            if (downloader == null)
            {
                downloader = Substitute.For<ICoursePageHtmlDownloader>();

                downloader.TryDownloadCoursePageHtmlAsync(Arg.Any<string>())
                    .Returns(Task.FromResult(Result.WithSuccess<string>()));
            }

            if (parser == null)
            {
                parser = Substitute.For<ICoursePageHtmlParser>();

                parser.TryParseCoursePage(Arg.Any<string>())
                    .Returns(Result.WithSuccess(new RawCourse[0]));
            }

            if (repository == null)
            {
                repository = Substitute.For<ICourseRepository>();
                repository.All().Returns(new Course[0]);
            }

            if (unitOfWork == null)
            {
                unitOfWork = Substitute.For<IUnitOfWork>();

                var session = Substitute.For<IUnitOfWorkSession>();
                unitOfWork.OpenSession().Returns(session);

                session.Commit().Returns(Result.Success);
            }

            if (dateTime == null)
            {
                dateTime = Substitute.For<IDateTimeUtility>();
                dateTime.UtcNow.Returns(DateTime.UtcNow);
            }

            comparer = comparer ?? new CourseSetComparer();
            preferences = preferences ?? Substitute.For<IPreferencesUtility>();

            return new CourseUpdater(
                downloader,
                parser,
                comparer,
                repository,
                unitOfWork,
                dateTime,
                preferences
            );
        }

        [TestFixture]
        public class When_Course_Updates_are_being_requested
        {
            [Test]
            public async Task The_Course_List_HTML_Document_is_being_downloaded()
            {
                var downloader = Substitute.For<ICoursePageHtmlDownloader>();

                downloader.TryDownloadCoursePageHtmlAsync(Arg.Any<string>())
                    .Returns(Task.FromResult(Result.WithSuccess<string>()));

                var sut = CreateCourseUpdater(downloader);

                var result = await sut.TryUpdateAsync().ConfigureAwait(false);
                result.ShouldBeSuccessful();

                await downloader.Received().TryDownloadCoursePageHtmlAsync(Arg.Any<string>())
                    .ConfigureAwait(false);
            }

            [Test]
            public async Task The_Course_List_HTML_Document_is_being_parsed()
            {
                var parser = Substitute.For<ICoursePageHtmlParser>();

                parser.TryParseCoursePage(Arg.Any<string>())
                    .Returns(Result.WithSuccess(new RawCourse[0]));

                var sut = CreateCourseUpdater(parser: parser);

                var result = await sut.TryUpdateAsync().ConfigureAwait(false);
                result.ShouldBeSuccessful();

                parser.Received().TryParseCoursePage(Arg.Any<string>());
            }

            [Test]
            public async Task Added_Courses_are_being_filtered_and_added_to_the_Database()
            {
                var addedRawCourse = new RawCourse
                {
                    Id = "Course01"
                };

                var parser = Substitute.For<ICoursePageHtmlParser>();

                parser.TryParseCoursePage(Arg.Any<string>()).Returns(
                    Result.WithSuccess(
                        new[]
                        {
                            addedRawCourse
                        }
                    )
                );

                var repository = Substitute.For<ICourseRepository>();
                repository.All().Returns(new Course[0]);

                var unitOfWork = Substitute.For<IUnitOfWork>();
                var session = Substitute.For<IUnitOfWorkSession>();
                unitOfWork.OpenSession().Returns(session);

                session.Commit().Returns(Result.Success);

                var sut = CreateCourseUpdater(
                    parser: parser,
                    repository: repository,
                    unitOfWork: unitOfWork
                );

                var result = await sut.TryUpdateAsync().ConfigureAwait(false);
                result.ShouldBeSuccessful();

                session.Received().Add(Arg.Is<Course>(c => c.Id == addedRawCourse.Id));

                session.DidNotReceive().Remove(Arg.Any<Course>());
                session.DidNotReceive().Update(Arg.Any<Course>());
            }

            [Test]
            public async Task Modified_Courses_are_being_filtered_and_updated_in_the_Database()
            {
                var modifiedCourse = new RawCourse
                {
                    Id = "Course01",
                    Title = "New Title"
                };

                var parser = Substitute.For<ICoursePageHtmlParser>();

                parser.TryParseCoursePage(Arg.Any<string>()).Returns(
                    Result.WithSuccess(
                        new[]
                        {
                            modifiedCourse
                        }
                    )
                );

                var repository = Substitute.For<ICourseRepository>();

                repository.All().Returns(
                    new[]
                    {
                        new Course
                        {
                            Id = "Course01",
                            Title = "Old Title"
                        }
                    }
                );

                var unitOfWork = Substitute.For<IUnitOfWork>();
                var session = Substitute.For<IUnitOfWorkSession>();
                unitOfWork.OpenSession().Returns(session);

                session.Commit().Returns(Result.Success);

                var utcNow = DateTime.UtcNow;
                var dateTime = Substitute.For<IDateTimeUtility>();
                dateTime.UtcNow.Returns(utcNow);

                var sut = CreateCourseUpdater(
                    parser: parser,
                    repository: repository,
                    unitOfWork: unitOfWork,
                    dateTime: dateTime
                );

                var result = await sut.TryUpdateAsync().ConfigureAwait(false);
                result.ShouldBeSuccessful();

                // Last Update must be "updated
                session.Received().Update(
                    Arg.Is<Course>(c => c.Id == modifiedCourse.Id && c.DateOfLastUpdate == utcNow)
                );

                session.DidNotReceive().Add(Arg.Any<Course>());
                session.DidNotReceive().Remove(Arg.Any<Course>());
            }

            [Test]
            public async Task Removed_Courses_are_being_filtered_and_removed_from_the_Database()
            {
                var modifiedCourse = new RawCourse
                {
                    Id = "Course02"
                };

                var parser = Substitute.For<ICoursePageHtmlParser>();

                parser.TryParseCoursePage(Arg.Any<string>()).Returns(
                    Result.WithSuccess(
                        new[]
                        {
                            modifiedCourse
                        }
                    )
                );

                var repository = Substitute.For<ICourseRepository>();

                repository.All().Returns(
                    new[]
                    {
                        new Course
                        {
                            Id = "Course01"
                        }
                    }
                );

                var unitOfWork = Substitute.For<IUnitOfWork>();
                var session = Substitute.For<IUnitOfWorkSession>();
                unitOfWork.OpenSession().Returns(session);

                session.Commit().Returns(Result.Success);

                var sut = CreateCourseUpdater(
                    parser: parser,
                    repository: repository,
                    unitOfWork: unitOfWork
                );

                var result = await sut.TryUpdateAsync().ConfigureAwait(false);
                result.ShouldBeSuccessful();

                session.Received().Remove(Arg.Is<Course>(c => c.Id == "Course01"));
                session.Received().Add(Arg.Is<Course>(c => c.Id == "Course02"));

                session.DidNotReceive().Update(Arg.Any<Course>());
            }

            [Test]
            public async Task The_date_of_the_last_update_is_being_stored()
            {
                var utcNow = DateTime.UtcNow;
                var dateTime = Substitute.For<IDateTimeUtility>();
                dateTime.UtcNow.Returns(utcNow);

                var preferences = Substitute.For<IPreferencesUtility>();

                var sut = CreateCourseUpdater(dateTime: dateTime, preferences: preferences);

                await sut.TryUpdateAsync().ConfigureAwait(false);

                preferences.Received().LastCourseUpdateCheck = utcNow;
            }
        }

        [TestFixture]
        public class CourseUpdateInfo_UpdatesFetched_is_true
        {
            [Test]
            public async Task When_there_are_Courses_added()
            {
                var comparer = Substitute.For<ICourseSetComparer>();

                comparer.WhenForAnyArgs(
                    x => x.Compare(
                        Arg.Any<Course[]>(),
                        Arg.Any<Course[]>(),
                        out Arg.Any<Course[]>(),
                        out Arg.Any<Course[]>(),
                        out Arg.Any<Course[]>()
                    )
                ).Do(
                    x =>
                    {
                        x[2] = new[]
                        {
                            new Course()
                        };

                        x[3] = new Course[0];
                        x[4] = new Course[0];
                    }
                );

                var sut = CreateCourseUpdater(comparer: comparer);
                var result = await sut.TryUpdateAsync().ConfigureAwait(false);

                result.ShouldBeSuccessful();
                result.Value.UpdatesFetched.ShouldBeTrue();
            }

            [Test]
            public async Task When_there_are_Courses_modified()
            {
                var comparer = Substitute.For<ICourseSetComparer>();

                comparer.WhenForAnyArgs(
                    x => x.Compare(
                        Arg.Any<Course[]>(),
                        Arg.Any<Course[]>(),
                        out Arg.Any<Course[]>(),
                        out Arg.Any<Course[]>(),
                        out Arg.Any<Course[]>()
                    )
                ).Do(
                    x =>
                    {
                        x[2] = new Course[0];

                        x[3] = new[]
                        {
                            new Course()
                        };

                        x[4] = new Course[0];
                    }
                );

                var sut = CreateCourseUpdater(comparer: comparer);
                var result = await sut.TryUpdateAsync().ConfigureAwait(false);

                result.ShouldBeSuccessful();
                result.Value.UpdatesFetched.ShouldBeTrue();
            }
        }

        [TestFixture]
        public class CourseUpdateInfo_UpdatesFetched_is_false
        {
            [Test]
            public async Task
                When_there_are_no_Courses_added_nor_modified_but_at_least_one_removed()
            {
                var comparer = Substitute.For<ICourseSetComparer>();

                comparer.WhenForAnyArgs(
                    x => x.Compare(
                        Arg.Any<Course[]>(),
                        Arg.Any<Course[]>(),
                        out Arg.Any<Course[]>(),
                        out Arg.Any<Course[]>(),
                        out Arg.Any<Course[]>()
                    )
                ).Do(
                    x =>
                    {
                        x[2] = new Course[0];
                        x[3] = new Course[0];

                        x[4] = new[]
                        {
                            new Course()
                        };
                    }
                );

                var sut = CreateCourseUpdater(comparer: comparer);
                var result = await sut.TryUpdateAsync().ConfigureAwait(false);

                result.ShouldBeSuccessful();
                result.Value.UpdatesFetched.ShouldBeFalse();
            }
        }

        [TestFixture]
        public class
            When_Course_Updates_are_being_requested_a_not_successful_result_is_being_returned
        {
            [Test]
            public async Task When_the_Course_List_cannot_be_downloaded()
            {
                string errorMessage = Guid.NewGuid().ToString();

                var downloader = Substitute.For<ICoursePageHtmlDownloader>();

                downloader.TryDownloadCoursePageHtmlAsync(Arg.Any<string>()).Returns(
                    Task.FromResult(Result.WithError<string>(errorMessage))
                );

                var sut = CreateCourseUpdater(downloader);

                var result = await sut.TryUpdateAsync().ConfigureAwait(false);

                result.ShouldNotBeSuccessful();
                result.ErrorMessage.ShouldBe(errorMessage);
            }

            [Test]
            public async Task When_the_Course_List_cannot_be_parsed()
            {
                string errorMessage = Guid.NewGuid().ToString();

                var parser = Substitute.For<ICoursePageHtmlParser>();

                parser.TryParseCoursePage(Arg.Any<string>())
                    .Returns(Result.WithError<RawCourse[]>(errorMessage));

                var sut = CreateCourseUpdater(parser: parser);

                var result = await sut.TryUpdateAsync().ConfigureAwait(false);

                result.ShouldNotBeSuccessful();
                result.ErrorMessage.ShouldBe(errorMessage);
            }

            [Test]
            public async Task When_Database_Updates_cannot_be_commited()
            {
                var addedRawCourse = new RawCourse
                {
                    Id = "Course01"
                };

                var parser = Substitute.For<ICoursePageHtmlParser>();

                parser.TryParseCoursePage(Arg.Any<string>()).Returns(
                    Result.WithSuccess(
                        new[]
                        {
                            addedRawCourse
                        }
                    )
                );

                var repository = Substitute.For<ICourseRepository>();
                repository.All().Returns(new Course[0]);

                var unitOfWork = Substitute.For<IUnitOfWork>();
                var session = Substitute.For<IUnitOfWorkSession>();
                unitOfWork.OpenSession().Returns(session);

                string errorMessage = Guid.NewGuid().ToString();

                session.Commit().Returns(Result.WithError(errorMessage));

                var sut = CreateCourseUpdater(
                    parser: parser,
                    repository: repository,
                    unitOfWork: unitOfWork
                );

                var result = await sut.TryUpdateAsync().ConfigureAwait(false);

                result.ShouldNotBeSuccessful();
                result.ErrorMessage.ShouldBe(errorMessage);
            }
        }

        [TestFixture]
        public class When_Courses_are_being_constructed
        {
            [Test]
            public async Task Their_existing_update_date_is_being_used_if_any()
            {
                DateTime lastUpdate = DateTime.UtcNow;

                var modifiedCourse = new RawCourse
                {
                    Id = "Course01"
                };

                var parser = Substitute.For<ICoursePageHtmlParser>();

                parser.TryParseCoursePage(Arg.Any<string>()).Returns(
                    Result.WithSuccess(
                        new[]
                        {
                            modifiedCourse
                        }
                    )
                );

                var repository = Substitute.For<ICourseRepository>();

                repository.All().Returns(
                    new[]
                    {
                        new Course
                        {
                            Id = "Course01",
                            DateOfLastUpdate = lastUpdate
                        }
                    }
                );

                var comparer = Substitute.For<ICourseSetComparer>();

                comparer.WhenForAnyArgs(
                    x => x.Compare(
                        Arg.Any<Course[]>(),
                        Arg.Any<Course[]>(),
                        out Arg.Any<Course[]>(),
                        out Arg.Any<Course[]>(),
                        out Arg.Any<Course[]>()
                    )
                ).Do(
                    x =>
                    {
                        x[2] = new Course[0];
                        x[3] = new Course[0];
                        x[4] = new Course[0];
                    }
                );

                var sut = CreateCourseUpdater(
                    parser: parser,
                    comparer: comparer,
                    repository: repository
                );

                var result = await sut.TryUpdateAsync().ConfigureAwait(false);
                result.ShouldBeSuccessful();

                comparer.Received().Compare(
                    Arg.Any<Course[]>(),
                    Arg.Is<Course[]>(x => x.Length == 1 && x[0].DateOfLastUpdate == lastUpdate),
                    out _,
                    out _,
                    out _
                );
            }

            [Test]
            public async Task The_currrent_UTC_DateTime_now_is_being_used_if_they_dont_exist_yet()
            {
                DateTime utcNow = DateTime.UtcNow;

                var addedCourse = new RawCourse
                {
                    Id = "Course01"
                };

                var parser = Substitute.For<ICoursePageHtmlParser>();

                parser.TryParseCoursePage(Arg.Any<string>()).Returns(
                    Result.WithSuccess(
                        new[]
                        {
                            addedCourse
                        }
                    )
                );

                var comparer = Substitute.For<ICourseSetComparer>();

                comparer.WhenForAnyArgs(
                    x => x.Compare(
                        Arg.Any<Course[]>(),
                        Arg.Any<Course[]>(),
                        out Arg.Any<Course[]>(),
                        out Arg.Any<Course[]>(),
                        out Arg.Any<Course[]>()
                    )
                ).Do(
                    x =>
                    {
                        x[2] = new Course[0];
                        x[3] = new Course[0];
                        x[4] = new Course[0];
                    }
                );

                var dateTime = Substitute.For<IDateTimeUtility>();
                dateTime.UtcNow.Returns(utcNow);

                var sut = CreateCourseUpdater(
                    parser: parser,
                    comparer: comparer,
                    dateTime: dateTime
                );

                var result = await sut.TryUpdateAsync().ConfigureAwait(false);
                result.ShouldBeSuccessful();

                comparer.Received().Compare(
                    Arg.Any<Course[]>(),
                    Arg.Is<Course[]>(x => x.Length == 1 && x[0].DateOfLastUpdate == utcNow),
                    out _,
                    out _,
                    out _
                );
            }
        }
    }
}
