using System;
using IUBH.TOR.Modules.Courses.Domain;
using NUnit.Framework;
using Shouldly;

namespace IUBH.TOR.Tests.Modules.Courses
{
    [TestFixture]
    public class CourseListItemSpecs
    {
        [Test]
        public void Title()
        {
            var course = new Course
            {
                Title = "Title"
            };

            var listItem = new CourseListItem(course);
            
            listItem.Title.ShouldBe("Title");
        }
        
        [Test]
        public void ExamDate()
        {
            var date = new DateTime(2019, 4, 15);
            
            var course = new Course
            {
                DateOfExamination = date
            };

            var listItem = new CourseListItem(course);

            string expectedResult = $"Exam date: {date:d}";
            
            listItem.ExamDate.ShouldBe(expectedResult);
        }
        
        [TestCase(CourseStatus.Passed, "Passed")]
        [TestCase(CourseStatus.Transferred, "Transferred")]
        [TestCase(CourseStatus.Failed, "Failed")]
        [TestCase(CourseStatus.ExamEnrolment, "Enrolled for exam")]
        [TestCase(CourseStatus.CourseEnrolment, "Enrolled for course")]
        [TestCase(CourseStatus.CombinationExam, "Combination exam")]
        [TestCase(CourseStatus.MissingResult, "Result missing")]
        [TestCase(CourseStatus.ModuleExamination, "Module examination")]
        [TestCase(CourseStatus.Unknown, "Status unknown")]
        public void Status(CourseStatus status, string expectedResult)
        {
            var date = new DateTime(2019, 4, 15);
            
            var course = new Course
            {
                DateOfExamination = date,
                Status = status
            };

            var listItem = new CourseListItem(course);

            listItem.Status.ShouldBe(expectedResult);
        }

        [Test]
        public void StatusPassed_with_grade()
        {
            var date = new DateTime(2019, 4, 15);
            
            var course = new Course
            {
                DateOfExamination = date,
                Status = CourseStatus.Passed,
                Grade = 1.3m,
                Rating = 93.3m
            };

            var listItem = new CourseListItem(course);

            const string expectedResult = "Passed: 1.3 (93.3 / 100)";
            
            listItem.Status.ShouldBe(expectedResult);
        }
        
        [Test]
        public void StatusTransferred_with_grade()
        {
            var date = new DateTime(2019, 4, 15);
            
            var course = new Course
            {
                DateOfExamination = date,
                Status = CourseStatus.Transferred,
                Grade = 1.3m,
                Rating = 93.3m
            };

            var listItem = new CourseListItem(course);

            const string expectedResult = "Transferred: 1.3 (93.3 / 100)";
            
            listItem.Status.ShouldBe(expectedResult);
        }
        
        [Test]
        public void StatusPassed_with_grade_1()
        {
            var date = new DateTime(2019, 4, 15);
            
            var course = new Course
            {
                DateOfExamination = date,
                Status = CourseStatus.Passed,
                Grade = 1,
                Rating = 100
            };

            var listItem = new CourseListItem(course);

            const string expectedResult = "Passed: 1.0 (100 / 100)";
            
            listItem.Status.ShouldBe(expectedResult);
        }
        
        [TestCase(CourseStatus.Passed, "#4DDB4D")]
        [TestCase(CourseStatus.Transferred, "#4DDB4D")]
        [TestCase(CourseStatus.Failed, "#FF4D4D")]
        [TestCase(CourseStatus.ExamEnrolment, "#8080B1")]
        [TestCase(CourseStatus.CombinationExam, "#FFFFFF")]
        [TestCase(CourseStatus.ModuleExamination, "#FFFFFF")]
        [TestCase(CourseStatus.CourseEnrolment, "#EDC66B")]
        [TestCase(CourseStatus.MissingResult, "#CACAC8")]
        [TestCase(CourseStatus.Unknown, "#FFFFFF")]
        public void Color(CourseStatus status, string expectedResult)
        {
            var course = new Course
            {
                Status = status
            };

            var listItem = new CourseListItem(course);

            listItem.Color.ShouldBe(expectedResult);
        }
    }
}
