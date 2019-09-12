using System.Globalization;
using IUBH.TOR.Modules.Courses.Domain;
using IUBH.TOR.Modules.Shared.Pages;

namespace IUBH.TOR.Modules.Courses.Pages
{
    public class CourseDetailViewModel : ViewModelBase
    {
        public string Title { get; private set; }
        public string Module { get; private set; }
        public string Status { get; private set; }
        public string Grade { get; private set; }
        public string Rating { get; private set; }
        public string Credits { get; private set; }
        public string Attempts { get; private set; }
        public string ExamDate { get; private set; }
        public string LastUpdate { get; private set; }

        public void SetCourse(Course course)
        {
            Title = course.Title;
            Module = course.Module;
            ExamDate = course.DateOfExamination.ToLocalTime().ToString("d");
            LastUpdate = course.DateOfLastUpdate.ToLocalTime().ToString("g");
            Status = GetCourseStatus(course);

            if (course.Attempts > 0)
            {
                Attempts = course.Attempts.ToString();
            }

            var ci = new CultureInfo("en-US");

            if (course.Grade > 0)
            {
                Grade = course.Grade.ToString("F1", ci);
            }

            if (course.Rating > 0)
            {
                Rating = course.Rating.ToString(ci) + " / 100";
            }

            if (course.Credits > 0)
            {
                Credits = $"{course.Credits} / {course.Credits}";
            }
        }

        private static string GetCourseStatus(Course course)
        {
            switch (course.Status)
            {
                case CourseStatus.Passed:
                    return "Passed";
                case CourseStatus.Transferred:
                    return "Transferred";
                case CourseStatus.Failed:
                    return "Failed";
                case CourseStatus.ExamEnrolment:
                    return "Enrolled for exam";
                case CourseStatus.CourseEnrolment:
                    return "Enrolled for course";
                case CourseStatus.CombinationExam:
                    return "Combination exam";
                case CourseStatus.MissingResult:
                    return "Result missing";
                case CourseStatus.ModuleExamination:
                    return "Module examination";
                default:
                    return "Unknown";
            }
        }
    }
}
