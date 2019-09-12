using System.Globalization;

namespace IUBH.TOR.Modules.Courses.Domain
{
    /// <summary>
    /// Provides all information of specific course that is necessary to
    /// display it in the list of all courses.
    /// </summary>
    public class CourseListItem
    {
        private static readonly CultureInfo _ci = new CultureInfo("en-US");
        
        public Course Course { get; }

        public string Title => Course.Title;
        public string ExamDate => $"Exam date: {Course.DateOfExamination:d}";

        public string Status
        {
            get
            {
                switch (Course.Status)
                {
                    case CourseStatus.Passed when Course.Grade > 0:

                        return
                            $"Passed: {Course.Grade.ToString("F1", _ci)} ({Course.Rating.ToString(_ci)} / 100)";
                    case CourseStatus.Passed:
                        return "Passed";
                    case CourseStatus.Transferred when Course.Grade > 0:

                        return
                            $"Transferred: {Course.Grade.ToString("F1", _ci)} ({Course.Rating.ToString(_ci)} / 100)";
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
                        return "Status unknown";
                }
            }
        }

        public string Color
        {
            get
            {
                switch (Course.Status)
                {
                    case CourseStatus.Passed:
                    case CourseStatus.Transferred:
                        return "#4DDB4D";
                    case CourseStatus.Failed:
                        return "#FF4D4D";
                    case CourseStatus.ExamEnrolment:
                        return "#8080B1";
                    case CourseStatus.CourseEnrolment:
                        return "#EDC66B";
                    case CourseStatus.MissingResult:
                        return "#CACAC8";
                    default:
                        return "#FFFFFF";
                }
            }
        }

        public CourseListItem(Course course)
        {
            Course = course;
        }
    }
}
