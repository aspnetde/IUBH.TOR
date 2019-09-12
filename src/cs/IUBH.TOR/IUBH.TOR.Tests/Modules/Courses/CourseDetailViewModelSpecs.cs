using System;
using IUBH.TOR.Modules.Courses.Domain;
using IUBH.TOR.Modules.Courses.Pages;
using NUnit.Framework;
using Shouldly;

namespace IUBH.TOR.Tests.Modules.Courses
{
    public class CourseDetailViewModelSpecs
    {
        [TestFixture]
        public class When_a_Course_is_set
        {
            [Test]
            public void The_Title_is_set()
            {
                var course = new Course
                {
                    Title = "Foo"
                };

                var sut = new CourseDetailViewModel();
                
                sut.Title.ShouldBeNull();
                sut.SetCourse(course);
                sut.Title.ShouldBe("Foo");
            }

            [Test]
            public void The_Module_is_set()
            {
                var course = new Course
                {
                    Module = "Foo"
                };

                var sut = new CourseDetailViewModel();
                
                sut.Module.ShouldBeNull();
                sut.SetCourse(course);
                sut.Module.ShouldBe("Foo");
            }

            [TestCase(CourseStatus.Passed, "Passed")]
            [TestCase(CourseStatus.Transferred, "Transferred")]
            [TestCase(CourseStatus.Failed, "Failed")]
            [TestCase(CourseStatus.ExamEnrolment, "Enrolled for exam")]
            [TestCase(CourseStatus.CourseEnrolment, "Enrolled for course")]
            [TestCase(CourseStatus.CombinationExam, "Combination exam")]
            [TestCase(CourseStatus.MissingResult, "Result missing")]
            [TestCase(CourseStatus.ModuleExamination, "Module examination")]
            [TestCase(CourseStatus.Unknown, "Unknown")]
            public void The_Status_is_set(CourseStatus status, string expectedResult)
            {
                var course = new Course
                {
                    Status = status
                };

                var sut = new CourseDetailViewModel();
                
                sut.Status.ShouldBeNull();
                sut.SetCourse(course);
                sut.Status.ShouldBe(expectedResult);
            }

            [TestCase(1, "1.0")]
            [TestCase("1.0", "1.0")]
            [TestCase("1.7", "1.7")]
            [TestCase(0, null)]
            [TestCase(-1, null)]
            public void The_Grade_is_set(decimal grade, string expectedResult)
            {
                var course = new Course
                {
                    Grade = grade
                };

                var sut = new CourseDetailViewModel();
                
                sut.Grade.ShouldBeNull();
                sut.SetCourse(course);
                sut.Grade.ShouldBe(expectedResult);
            }

            [TestCase("67.77", "67.77 / 100")]
            [TestCase(100, "100 / 100")]
            [TestCase(0, null)]
            public void The_Rating_is_set(decimal rating, string expectedResult)
            {
                var course = new Course
                {
                    Rating = rating
                };

                var sut = new CourseDetailViewModel();
                
                sut.Rating.ShouldBeNull();
                sut.SetCourse(course);
                sut.Rating.ShouldBe(expectedResult);
            }

            [TestCase(5, "5 / 5")]
            [TestCase(0, null)]
            [TestCase(-1, null)]
            public void The_Credits_are_set(int credits, string expectedResult)
            {
                var course = new Course
                {
                    Credits = credits
                };

                var sut = new CourseDetailViewModel();
                
                sut.Credits.ShouldBeNull();
                sut.SetCourse(course);
                sut.Credits.ShouldBe(expectedResult);
            }

            [TestCase(1, "1")]
            [TestCase(0, null)]
            [TestCase(-1, null)]
            public void The_Attempts_are_set(int attempts, string expectedResult)
            {
                var course = new Course
                {
                    Attempts = attempts
                };

                var sut = new CourseDetailViewModel();
                
                sut.Attempts.ShouldBeNull();
                sut.SetCourse(course);
                sut.Attempts.ShouldBe(expectedResult);
            }

            [Test]
            public void The_Exam_Date_is_set()
            {
                var now = DateTime.UtcNow;
                
                var course = new Course
                {
                    DateOfExamination = now
                };

                var sut = new CourseDetailViewModel();
                
                sut.ExamDate.ShouldBeNull();
                sut.SetCourse(course);
                sut.ExamDate.ShouldBe($"{now.ToLocalTime():d}");
            }

            [Test]
            public void The_Last_Update_Date_is_set()
            {
                var now = DateTime.UtcNow;
                
                var course = new Course
                {
                    DateOfLastUpdate = now
                };

                var sut = new CourseDetailViewModel();
                
                sut.LastUpdate.ShouldBeNull();
                sut.SetCourse(course);
                sut.LastUpdate.ShouldBe($"{now.ToLocalTime():g}");
            }
        }
    }
}
