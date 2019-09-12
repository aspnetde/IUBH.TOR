using System;
using System.Threading.Tasks;
using IUBH.TOR.Data;
using IUBH.TOR.Domain;
using IUBH.TOR.Modules.Courses.Data;
using IUBH.TOR.Modules.Courses.Domain;
using IUBH.TOR.Modules.Courses.Pages;
using IUBH.TOR.Modules.Courses.Services;
using IUBH.TOR.Utilities.BackgroundSync;
using IUBH.TOR.Utilities.Date;
using IUBH.TOR.Utilities.Messaging;
using IUBH.TOR.Utilities.Preferences;
using IUBH.TOR.Utilities.SecureStorage;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace IUBH.TOR.Tests.Modules.Courses
{
    public class CourseListViewModelSpecs
    {
        private static CourseListViewModel CreateViewModel(
            ICourseUpdater courseUpdater = null,
            ICourseRepository repository = null,
            IPreferencesUtility preferences = null,
            IDateTimeUtility dateTime = null,
            IDbConnection dbConnection = null,
            ISecureStorageUtility secureStorage = null,
            IBackgroundSyncUtility backgroundSync = null,
            IMessenger messenger = null
        )
        {
            if (courseUpdater == null)
            {
                courseUpdater = Substitute.For<ICourseUpdater>();

                courseUpdater.TryUpdateAsync().Returns(
                    Task.FromResult(Result.WithSuccess(new CourseUpdateInfo(true)))
                );
            }

            if (repository == null)
            {
                repository = Substitute.For<ICourseRepository>();
                repository.All().Returns(new Course[0]);
            }

            var utcNow = DateTime.UtcNow;

            if (dateTime == null)
            {
                dateTime = Substitute.For<IDateTimeUtility>();
                dateTime.UtcNow.Returns(utcNow);
            }

            if (preferences == null)
            {
                preferences = Substitute.For<IPreferencesUtility>();
                preferences.LastCourseUpdateCheck.CompareTo(utcNow.AddMinutes(-30));
            }

            dbConnection = dbConnection ?? Substitute.For<IDbConnection>();
            secureStorage = secureStorage ?? Substitute.For<ISecureStorageUtility>();
            backgroundSync = backgroundSync ?? Substitute.For<IBackgroundSyncUtility>();
            messenger = messenger ?? Substitute.For<IMessenger>();

            return new CourseListViewModel(
                courseUpdater,
                repository,
                preferences,
                dateTime,
                dbConnection,
                secureStorage,
                backgroundSync,
                messenger
            );
        }

        [TestFixture]
        public class After_the_ViewModel_has_been_constructed
        {
            [Test]
            public void The_state_is_set_to_loading()
            {
                var sut = CreateViewModel();
                sut.State.ShouldBe(CourseListState.Loading);
            }
        }

        [TestFixture]
        public class When_the_ViewModel_is_being_initialized
        {
            public class And_the_last_Update_had_been_performed_in_the_last_fifteen_minutes
            {
                [Test]
                public async Task Data_is_being_fetched_from_the_database_only()
                {
                    var utcNow = DateTime.UtcNow;

                    var dateTime = Substitute.For<IDateTimeUtility>();
                    dateTime.UtcNow.Returns(utcNow);

                    var preferences = Substitute.For<IPreferencesUtility>();
                    preferences.LastCourseUpdateCheck.Returns(utcNow.AddMinutes(-5));

                    var courseUpdater = Substitute.For<ICourseUpdater>();

                    courseUpdater.TryUpdateAsync().Returns(
                        Task.FromResult(Result.WithSuccess(new CourseUpdateInfo(true)))
                    );

                    var repository = Substitute.For<ICourseRepository>();
                    repository.All().Returns(new Course[0]);

                    var sut = CreateViewModel(
                        courseUpdater: courseUpdater,
                        dateTime: dateTime,
                        repository: repository,
                        preferences: preferences
                    );

                    await sut.InitializeAsync().ConfigureAwait(false);

                    await courseUpdater.DidNotReceive().TryUpdateAsync().ConfigureAwait(false);

                    repository.Received().All();
                }
            }

            public class And_data_is_older_than_fifteen_minutes
            {
                [Test]
                public async Task Course_Updates_are_being_fetched()
                {
                    var utcNow = DateTime.UtcNow;

                    var dateTime = Substitute.For<IDateTimeUtility>();
                    dateTime.UtcNow.Returns(utcNow);

                    var preferences = Substitute.For<IPreferencesUtility>();
                    preferences.LastCourseUpdateCheck.Returns(utcNow.AddMinutes(-30));

                    var courseUpdater = Substitute.For<ICourseUpdater>();

                    courseUpdater.TryUpdateAsync().Returns(
                        Task.FromResult(Result.WithSuccess(new CourseUpdateInfo(true)))
                    );

                    var sut = CreateViewModel(
                        courseUpdater: courseUpdater,
                        dateTime: dateTime,
                        preferences: preferences
                    );

                    await sut.InitializeAsync().ConfigureAwait(false);

                    await courseUpdater.Received().TryUpdateAsync().ConfigureAwait(false);
                }

                [Test]
                public async Task The_state_is_being_set_to_Loading()
                {
                    var utcNow = DateTime.UtcNow;

                    var dateTime = Substitute.For<IDateTimeUtility>();
                    dateTime.UtcNow.Returns(utcNow);

                    var preferences = Substitute.For<IPreferencesUtility>();
                    preferences.LastCourseUpdateCheck.Returns(utcNow.AddMinutes(-30));

                    var courseUpdater = Substitute.For<ICourseUpdater>();

                    courseUpdater.TryUpdateAsync().Returns(
                        Task.FromResult(Result.WithSuccess(new CourseUpdateInfo(true)))
                    );

                    var sut = CreateViewModel(
                        courseUpdater: courseUpdater,
                        dateTime: dateTime,
                        preferences: preferences
                    );

                    await sut.InitializeAsync().ConfigureAwait(false);
                    sut.State.ShouldBe(CourseListState.Empty);

                    bool propertySet = false;

                    sut.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(CourseListViewModel.State)
                            && ((CourseListViewModel)sender).State == CourseListState.Loading)
                        {
                            propertySet = true;
                        }
                    };

                    await sut.InitializeAsync().ConfigureAwait(false);

                    propertySet.ShouldBeTrue();
                }
            }
        }

        [TestFixture]
        public class After_the_ViewModel_has_been_initialized
        {
            [Test]
            public async Task It_is_subscribed_to_the_WokeUpMessage()
            {
                var messenger = Substitute.For<IMessenger>();
                var sut = CreateViewModel(messenger: messenger);

                await sut.InitializeAsync().ConfigureAwait(false);

                messenger.Received().Subscribe(
                    Arg.Is(sut),
                    Arg.Any<Action<object, WokeUpMessage>>()
                );
            }
        }

        [TestFixture]
        public class When_a_WokeUpMessage_is_being_received
        {
            [Test]
            public async Task Courses_are_being_freshly_fetched_from_the_database()
            {
                var messenger = new FormsMessenger();

                var repository = Substitute.For<ICourseRepository>();
                repository.All().Returns(new Course[0]);

                var sut = CreateViewModel(messenger: messenger, repository: repository);

                await sut.InitializeAsync().ConfigureAwait(false);

                repository.Received().All();
            }
        }

        [TestFixture]
        public class When_a_ReInitialization_is_being_started
        {
            [Test]
            public async Task The_whole_Loading_process_is_being_triggered()
            {
                var courseUpdater = Substitute.For<ICourseUpdater>();

                courseUpdater.TryUpdateAsync().Returns(
                    Task.FromResult(Result.WithSuccess(new CourseUpdateInfo(true)))
                );

                var sut = CreateViewModel(courseUpdater: courseUpdater);
                await sut.ReInitializeAsync().ConfigureAwait(false);

                await courseUpdater.Received().TryUpdateAsync().ConfigureAwait(false);
            }
        }

        [TestFixture]
        public class When_a_Refresh_is_being_executed
        {
            [Test]
            public async Task IsRefreshing_is_being_set_to_true()
            {
                var sut = CreateViewModel();

                bool IsRefreshingSet = false;

                sut.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(CourseListViewModel.IsRefreshing)
                        && ((CourseListViewModel)sender).IsRefreshing)
                    {
                        IsRefreshingSet = true;
                    }
                };

                await sut.RefreshAsync().ConfigureAwait(false);

                IsRefreshingSet.ShouldBeTrue();
            }

            [Test]
            public async Task IsRefreshing_is_being_set_to_false_again()
            {
                var sut = CreateViewModel();

                bool IsRefreshingSet = false;

                sut.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(CourseListViewModel.IsRefreshing)
                        && ((CourseListViewModel)sender).IsRefreshing == false)
                    {
                        IsRefreshingSet = true;
                    }
                };

                await sut.RefreshAsync().ConfigureAwait(false);

                IsRefreshingSet.ShouldBeTrue();
            }

            [Test]
            public async Task The_whole_Loading_process_is_being_triggered()
            {
                var courseUpdater = Substitute.For<ICourseUpdater>();

                courseUpdater.TryUpdateAsync().Returns(
                    Task.FromResult(Result.WithSuccess(new CourseUpdateInfo(true)))
                );

                var sut = CreateViewModel(courseUpdater: courseUpdater);
                await sut.RefreshAsync().ConfigureAwait(false);

                await courseUpdater.Received().TryUpdateAsync().ConfigureAwait(false);
            }
        }

        [TestFixture]
        public class After_Courses_have_been_updated_successfully
        {
            [Test]
            public async Task The_List_of_Courses_is_being_fetched_from_the_database()
            {
                var repository = Substitute.For<ICourseRepository>();
                repository.All().Returns(new Course[0]);

                var sut = CreateViewModel(repository: repository);

                await sut.InitializeAsync().ConfigureAwait(false);

                repository.Received().All();
            }

            [Test]
            public async Task
                The_List_of_Courses_is_being_set_ordered_by_examination_date_descending()
            {
                var repository = Substitute.For<ICourseRepository>();

                repository.All().Returns(
                    new[]
                    {
                        new Course
                        {
                            Id = "X",
                            Title = "3",
                            DateOfExamination = new DateTime(2019, 1, 2)
                        },
                        new Course
                        {
                            Id = "X",
                            Title = "1",
                            DateOfExamination = new DateTime(2019, 1, 1)
                        },
                        new Course
                        {
                            Id = "X",
                            Title = "2",
                            DateOfExamination = new DateTime(2019, 1, 3)
                        }
                    }
                );

                var sut = CreateViewModel(repository: repository);

                await sut.InitializeAsync().ConfigureAwait(false);

                sut.Courses.Length.ShouldBe(3);
                sut.Courses[0].Title.ShouldBe("2");
                sut.Courses[1].Title.ShouldBe("3");
                sut.Courses[2].Title.ShouldBe("1");
            }

            [Test]
            public async Task The_State_is_set_to_Default_if_there_are_Courses()
            {
                var repository = Substitute.For<ICourseRepository>();

                repository.All().Returns(
                    new[]
                    {
                        new Course
                        {
                            Id = "3",
                            DateOfLastUpdate = new DateTime(2019, 1, 2)
                        }
                    }
                );

                var sut = CreateViewModel(repository: repository);

                await sut.InitializeAsync().ConfigureAwait(false);

                sut.State.ShouldBe(CourseListState.Default);
            }

            [Test]
            public async Task The_State_is_set_to_Empty_if_there_are_no_Courses()
            {
                var repository = Substitute.For<ICourseRepository>();
                repository.All().Returns(new Course[0]);

                var sut = CreateViewModel(repository: repository);

                await sut.InitializeAsync().ConfigureAwait(false);

                sut.State.ShouldBe(CourseListState.Empty);
            }
        }

        [TestFixture]
        public class When_Course_Updates_could_not_be_fetched
        {
            [Test]
            public async Task The_State_is_set_to_Error()
            {
                var courseUpdater = Substitute.For<ICourseUpdater>();

                courseUpdater.TryUpdateAsync().Returns(
                    Task.FromResult(Result.WithError<CourseUpdateInfo>("Foo"))
                );

                var sut = CreateViewModel(courseUpdater: courseUpdater);

                await sut.InitializeAsync().ConfigureAwait(false);

                sut.State.ShouldBe(CourseListState.Error);
            }

            [Test]
            public async Task The_Results_ErrorMessage_is_being_set_to_the_VM()
            {
                string uniqueErrorMessage = Guid.NewGuid().ToString();

                var courseUpdater = Substitute.For<ICourseUpdater>();

                courseUpdater.TryUpdateAsync().Returns(
                    Task.FromResult(Result.WithError<CourseUpdateInfo>(uniqueErrorMessage))
                );

                var sut = CreateViewModel(courseUpdater: courseUpdater);

                await sut.InitializeAsync().ConfigureAwait(false);

                sut.ErrorMessage.ShouldBe(uniqueErrorMessage);
            }

            [Test]
            public async Task The_Initialization_is_being_aborted()
            {
                var courseUpdater = Substitute.For<ICourseUpdater>();

                courseUpdater.TryUpdateAsync().Returns(
                    Task.FromResult(Result.WithError<CourseUpdateInfo>(""))
                );

                var repository = Substitute.For<ICourseRepository>();

                var sut = CreateViewModel(courseUpdater: courseUpdater, repository: repository);

                await sut.InitializeAsync().ConfigureAwait(false);

                repository.DidNotReceive().All();
            }
        }

        [TestFixture]
        public class The_LastUpdateCheck_Information
        {
            [Test]
            public void Always_uses_the_local_version_of_the_Date_stored_in_the_Preferences()
            {
                var utcDateTime = DateTime.UtcNow;

                var preferences = Substitute.For<IPreferencesUtility>();
                preferences.LastCourseUpdateCheck.Returns(utcDateTime);

                var sut = CreateViewModel(preferences: preferences);

                string expectedResult = $"◷ Last check: {utcDateTime.ToLocalTime():g}";

                sut.LastUpdateCheck.ShouldBe(expectedResult);
            }

            [Test]
            public void Shows_Never_when_the_Date_stored_in_the_Preferences_is_the_default()
            {
                var preferences = Substitute.For<IPreferencesUtility>();
                preferences.LastCourseUpdateCheck.Returns(DateTime.MinValue);

                var sut = CreateViewModel(preferences: preferences);

                string expectedResult = $"◷ Last check: Never";

                sut.LastUpdateCheck.ShouldBe(expectedResult);
            }
        }

        [TestFixture]
        public class When_the_User_signs_out
        {
            [Test]
            public void The_Database_is_being_nuked()
            {
                var dbConnection = Substitute.For<IDbConnection>();

                var sut = CreateViewModel(dbConnection: dbConnection);
                sut.SignoutCommand.Execute(null);

                dbConnection.Received().Nuke();
            }

            [Test]
            public void The_Preferences_are_being_nuked()
            {
                var preferences = Substitute.For<IPreferencesUtility>();

                var sut = CreateViewModel(preferences: preferences);
                sut.SignoutCommand.Execute(null);

                preferences.Received().Nuke();
            }

            [Test]
            public void The_Secure_Storage_is_being_nuked()
            {
                var secureStorage = Substitute.For<ISecureStorageUtility>();

                var sut = CreateViewModel(secureStorage: secureStorage);
                sut.SignoutCommand.Execute(null);

                secureStorage.Received().Nuke();
            }

            [Test]
            public void The_BackgroundSync_is_being_disabled()
            {
                var backgroundSync = Substitute.For<IBackgroundSyncUtility>();

                var sut = CreateViewModel(backgroundSync: backgroundSync);
                sut.SignoutCommand.Execute(null);

                backgroundSync.Received().Disable();
            }

            [Test]
            public void The_ViewModel_unsubscribes_itself_from_the_WokeUpMessage()
            {
                var messenger = Substitute.For<IMessenger>();

                var sut = CreateViewModel(messenger: messenger);
                sut.SignoutCommand.Execute(null);

                messenger.Received().Unsubscribe<WokeUpMessage>(Arg.Is(sut));
            }

            [Test]
            public void The_SignedOut_EventHandler_is_being_invoked()
            {
                var sut = CreateViewModel();

                bool eventHandlerCalled = false;

                sut.SignedOut += (sender, args) => eventHandlerCalled = true;

                eventHandlerCalled.ShouldBeFalse();

                sut.SignoutCommand.Execute(null);

                eventHandlerCalled.ShouldBeTrue();
            }
        }

        // This is a (hopefully) temporary workaround for iOS:
        // https://github.com/xamarin/Xamarin.Forms/issues/5920
        [TestFixture]
        public class When_a_Course_gets_selected
        {
            [Test]
            public void The_EventHandler_is_being_invoked()
            {
                var sut = CreateViewModel();
                var dummyCourse = new Course();
                var dummyListItem = new CourseListItem(dummyCourse);

                Course selectedCourse = null;

                sut.CourseSelected += (sender, course) => selectedCourse = course;

                selectedCourse.ShouldBeNull();

                sut.SelectCourseCommand.Execute(dummyListItem);

                selectedCourse.ShouldBe(dummyCourse);
            }
        }
    }
}
