using System;
using IUBH.TOR.Modules.Courses.Domain;
using IUBH.TOR.Modules.Courses.Services;
using NUnit.Framework;
using Shouldly;

namespace IUBH.TOR.Tests.Modules.Courses
{
    public class CourseSetComparerSpecs
    {
        private static Course DemoCourse(string id)
            => new Course
            {
                Id = id,
                Title = "Title",
                Module = "Module",
                Status = CourseStatus.MissingResult,
                Grade = 1.3m,
                IsPassed = false,
                Rating = 67.66m,
                Credits = 5,
                DateOfExamination = new DateTime(2019, 4, 12),
                Attempts = 1
            };

        [TestFixture]
        public class When_two_course_sets_A_and_B_are_being_compared
        {
            [Test]
            public void
                Courses_with_an_ID_existent_in_B_but_not_A_are_being_returned_as_added_courses()
            {
                Course existingCourse1 = DemoCourse("Id1");
                Course existingCourse2 = DemoCourse("Id2");

                Course addedCourse3 = DemoCourse("Id3");

                Course[] a =
                {
                    existingCourse1,
                    existingCourse2
                };

                Course[] b =
                {
                    existingCourse1,
                    existingCourse2,
                    addedCourse3
                };

                var sut = new CourseSetComparer();

                sut.Compare(
                    a,
                    b,
                    out Course[] addedCourses,
                    out Course[] modifiedCourses,
                    out Course[] removedCourses
                );

                modifiedCourses.ShouldBeEmpty();
                removedCourses.ShouldBeEmpty();

                addedCourses.Length.ShouldBe(1);
                addedCourses.ShouldContain(addedCourse3);
            }

            [Test]
            public void
                Courses_with_an_ID_existent_in_A_but_not_B_are_being_returned_as_removed_courses()
            {
                Course existingCourse1 = DemoCourse("Id1");
                Course existingCourse2 = DemoCourse("Id2");

                Course removedCourse3 = DemoCourse("Id3");

                Course[] a =
                {
                    existingCourse1,
                    existingCourse2,
                    removedCourse3
                };

                Course[] b =
                {
                    existingCourse1,
                    existingCourse2,
                };

                var sut = new CourseSetComparer();

                sut.Compare(
                    a,
                    b,
                    out Course[] addedCourses,
                    out Course[] modifiedCourses,
                    out Course[] removedCourses
                );

                modifiedCourses.ShouldBeEmpty();
                addedCourses.ShouldBeEmpty();

                removedCourses.Length.ShouldBe(1);
                removedCourses.ShouldContain(removedCourse3);
            }

            [Test]
            public void
                Courses_existent_in_both_A_and_B_which_have_been_updated_are_being_returned_as_modified_courses()
            {
                Course existingCourse1 = DemoCourse("Id1");
                Course existingCourse2 = DemoCourse("Id2");

                Course modifiedCourse2 = DemoCourse("Id2");
                modifiedCourse2.Grade = modifiedCourse2.Grade + 0.1m;

                Course[] a =
                {
                    existingCourse1,
                    existingCourse2
                };

                Course[] b =
                {
                    existingCourse1,
                    modifiedCourse2,
                };

                var sut = new CourseSetComparer();

                sut.Compare(
                    a,
                    b,
                    out Course[] addedCourses,
                    out Course[] modifiedCourses,
                    out Course[] removedCourses
                );

                addedCourses.ShouldBeEmpty();
                removedCourses.ShouldBeEmpty();

                modifiedCourses.Length.ShouldBe(1);
                modifiedCourses.ShouldContain(modifiedCourse2);
            }
        }
    }
}
