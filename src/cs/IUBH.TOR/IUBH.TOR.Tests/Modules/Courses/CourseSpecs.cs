using System;
using IUBH.TOR.Modules.Courses.Domain;
using NUnit.Framework;
using Shouldly;

namespace IUBH.TOR.Tests.Modules.Courses
{
    public class CourseSpecs
    {
        [TestFixture]
        public class When_a_Course_is_being_created_from_a_RawCourse
        {
            [Test]
            public void Its_Id_is_set()
            {
                var rawCourse = new RawCourse
                {
                    Id = Guid.NewGuid().ToString()
                };

                var result = Course.FromRawCourse(rawCourse, DateTime.UtcNow);

                result.Id.ShouldBe(rawCourse.Id);
            }
            
            [Test]
            public void Its_LastUpdate_Date_is_set()
            {
                var lastUpdate = DateTime.UtcNow;
                
                var rawCourse = new RawCourse
                {
                    Id = Guid.NewGuid().ToString()
                };

                var result = Course.FromRawCourse(rawCourse, lastUpdate);

                result.DateOfLastUpdate.ShouldBe(lastUpdate);
            }

            [Test]
            public void Its_Title_is_set()
            {
                var rawCourse = new RawCourse
                {
                    Title = Guid.NewGuid().ToString()
                };

                var result = Course.FromRawCourse(rawCourse, DateTime.UtcNow);

                result.Title.ShouldBe(rawCourse.Title);
            }

            [Test]
            public void Its_Module_is_set()
            {
                var rawCourse = new RawCourse
                {
                    Module = Guid.NewGuid().ToString()
                };

                var result = Course.FromRawCourse(rawCourse, DateTime.UtcNow);

                result.Module.ShouldBe(rawCourse.Module);
            }

            [TestCase("P", CourseStatus.Passed)]
            [TestCase("P, T", CourseStatus.Transferred)]
            [TestCase("F", CourseStatus.Failed)]
            [TestCase("EE", CourseStatus.ExamEnrolment)]
            [TestCase("E", CourseStatus.CourseEnrolment)]
            [TestCase("CE", CourseStatus.CombinationExam)]
            [TestCase("M", CourseStatus.MissingResult)]
            [TestCase("ME", CourseStatus.ModuleExamination)]
            [TestCase("Foo", CourseStatus.Unknown)]
            [TestCase("", CourseStatus.Unknown)]
            [TestCase(null, CourseStatus.Unknown)]
            public void Its_Status_is_being_mapped_and_set(
                string statusCode,
                CourseStatus expectedStatus
            )
            {
                var rawCourse = new RawCourse
                {
                    Status = statusCode
                };

                var result = Course.FromRawCourse(rawCourse, DateTime.UtcNow);

                result.Status.ShouldBe(expectedStatus);
            }

            [TestCase("1,0", "1.0")]
            [TestCase("3,7", "3.7")]
            [TestCase("1,35", "1.35")]
            [TestCase("", 0)]
            [TestCase(null, 0)]
            [TestCase("Passed", 0)]
            [TestCase("Foo", 0)]
            public void Its_Grade_is_being_mapped_and_set(string inputGrade, decimal expectedGrade)
            {
                var rawCourse = new RawCourse
                {
                    Grade = inputGrade
                };

                var result = Course.FromRawCourse(rawCourse, DateTime.UtcNow);

                result.Grade.ShouldBe(expectedGrade);
            }

            [TestCase("Passed")]
            [TestCase("passed")]
            [TestCase("Passed ")]
            public void IsPassed_is_true_when_a_grade_is_provided(string inputGrade)
            {
                var rawCourse = new RawCourse
                {
                    Grade = inputGrade
                };

                var result = Course.FromRawCourse(rawCourse, DateTime.UtcNow);

                result.IsPassed.ShouldBeTrue();
            }

            [TestCase("")]
            [TestCase(null)]
            [TestCase("1,7")]
            [TestCase("Foo")]
            public void IsPassed_is_false_when_the_Course_has_not_been_passed(string inputGrade)
            {
                var rawCourse = new RawCourse
                {
                    Grade = inputGrade
                };

                var result = Course.FromRawCourse(rawCourse, DateTime.UtcNow);

                result.IsPassed.ShouldBeFalse();
            }

            [TestCase("93.3 / 100", "93.3")]
            [TestCase("100 / 100", "100")]
            [TestCase("", 0)]
            [TestCase("F00", 0)]
            [TestCase(null, 0)]
            public void Its_Rating_is_being_mapped_and_set(
                string ratingInput,
                decimal expectedResult
            )
            {
                var rawCourse = new RawCourse
                {
                    Rating = ratingInput
                };

                var result = Course.FromRawCourse(rawCourse, DateTime.UtcNow);

                result.Rating.ShouldBe(expectedResult);
            }

            [TestCase("5 / 5", 5)]
            [TestCase("10 / 10", 10)]
            [TestCase("", 0)]
            [TestCase("Foo", 0)]
            [TestCase(null, 0)]
            public void Its_Credits_are_being_mapped_and_set(
                string inputCredits,
                int expectedResult
            )
            {
                var rawCourse = new RawCourse
                {
                    Credits = inputCredits
                };

                var result = Course.FromRawCourse(rawCourse, DateTime.UtcNow);

                result.Credits.ShouldBe(expectedResult);
            }

            [TestCase("05.05.2030", 2030, 5, 5)]
            [TestCase("01.09.2019", 2019, 9, 1)]
            [TestCase("", 1, 1, 1)]
            [TestCase("Foo", 1, 1, 1)]
            [TestCase(null, 1, 1, 1)]
            public void Its_Date_of_Examination_is_being_mapped_and_set(
                string inputDate,
                int expectedYear,
                int expectedMonth,
                int expectedDay
            )
            {
                var rawCourse = new RawCourse
                {
                    DateOfExamination = inputDate
                };

                var result = Course.FromRawCourse(rawCourse, DateTime.UtcNow);

                result.DateOfExamination.Year.ShouldBe(expectedYear);
                result.DateOfExamination.Month.ShouldBe(expectedMonth);
                result.DateOfExamination.Day.ShouldBe(expectedDay);
            }

            [TestCase("1", 1)]
            [TestCase("3", 3)]
            [TestCase("", 0)]
            [TestCase(null, 0)]
            [TestCase("FOO", 0)]
            public void Its_Attempts_are_being_mapped_and_set(
                string inputAttempts,
                int expectedResult
            )
            {
                var rawCourse = new RawCourse
                {
                    Attempts = inputAttempts
                };

                var result = Course.FromRawCourse(rawCourse, DateTime.UtcNow);

                result.Attempts.ShouldBe(expectedResult);
            }
        }

        private static Course DemoCourse()
            => new Course
            {
                Id = "Id",
                Title = "Title",
                Module = "Module",
                Status = CourseStatus.MissingResult,
                Grade = 1.3m,
                IsPassed = false,
                Rating = 67.66m,
                Credits = 5,
                DateOfExamination = new DateTime(2019, 4, 12),
                Attempts = 1,
                DateOfLastUpdate = DateTime.MinValue
            };

        [TestFixture]
        public class Two_Courses_A_and_B_are_seen_as_equal
        {
            [Test]
            public void When_all_of_their_properties_have_the_same_value()
            {
                Course a = DemoCourse();
                Course b = DemoCourse();

                a.Equals(b).ShouldBeTrue();
                (a == b).ShouldBeTrue();
            }
        }

        [TestFixture]
        public class Two_Courses_A_and_B_are_seen_as_different_when_they_do_not_have
        {
            [Test]
            public void The_same_Id()
            {
                Course a = DemoCourse();
                Course b = DemoCourse();
                
                a.Equals(b).ShouldBeTrue();

                b.Id = Guid.NewGuid().ToString();

                a.Equals(b).ShouldBeFalse();
            }

            [Test]
            public void The_same_Title()
            {
                Course a = DemoCourse();
                Course b = DemoCourse();
                
                a.Equals(b).ShouldBeTrue();

                b.Title = Guid.NewGuid().ToString();

                a.Equals(b).ShouldBeFalse();
            }

            [Test]
            public void The_same_Module()
            {
                Course a = DemoCourse();
                Course b = DemoCourse();
                
                a.Equals(b).ShouldBeTrue();

                b.Module = Guid.NewGuid().ToString();

                a.Equals(b).ShouldBeFalse();
            }

            [Test]
            public void The_same_Status()
            {
                Course a = DemoCourse();
                Course b = DemoCourse();
                
                a.Equals(b).ShouldBeTrue();

                b.Status = CourseStatus.Passed;

                a.Equals(b).ShouldBeFalse();
            }

            [Test]
            public void The_same_Grade()
            {
                Course a = DemoCourse();
                Course b = DemoCourse();
                
                a.Equals(b).ShouldBeTrue();

                b.Grade = b.Grade + 0.1m;

                a.Equals(b).ShouldBeFalse();
            }

            [Test]
            public void The_same_IsPassed_Value()
            {
                Course a = DemoCourse();
                Course b = DemoCourse();
                
                a.Equals(b).ShouldBeTrue();

                b.IsPassed = !b.IsPassed;

                a.Equals(b).ShouldBeFalse();
            }

            [Test]
            public void The_same_Rating()
            {
                Course a = DemoCourse();
                Course b = DemoCourse();
                
                a.Equals(b).ShouldBeTrue();

                b.Rating = b.Rating + 0.1m;

                a.Equals(b).ShouldBeFalse();
            }

            [Test]
            public void The_same_Credits()
            {
                Course a = DemoCourse();
                Course b = DemoCourse();
                
                a.Equals(b).ShouldBeTrue();

                b.Credits = b.Credits + 1;

                a.Equals(b).ShouldBeFalse();
            }

            [Test]
            public void The_same_Date_of_Examination()
            {
                Course a = DemoCourse();
                Course b = DemoCourse();
                
                a.Equals(b).ShouldBeTrue();

                b.DateOfExamination = b.DateOfExamination.AddDays(1);

                a.Equals(b).ShouldBeFalse();
            }

            [Test]
            public void The_same_Attempts()
            {
                Course a = DemoCourse();
                Course b = DemoCourse();
                
                a.Equals(b).ShouldBeTrue();

                b.Attempts = b.Attempts + 1;

                a.Equals(b).ShouldBeFalse();
            }
            
            [Test]
            public void The_same_LastUpdate_date()
            {
                Course a = DemoCourse();
                Course b = DemoCourse();
                b.DateOfLastUpdate = a.DateOfLastUpdate;
                
                a.Equals(b).ShouldBeTrue();

                b.DateOfLastUpdate = b.DateOfLastUpdate.AddSeconds(1);

                a.Equals(b).ShouldBeFalse();
            }
        }
    }
}
