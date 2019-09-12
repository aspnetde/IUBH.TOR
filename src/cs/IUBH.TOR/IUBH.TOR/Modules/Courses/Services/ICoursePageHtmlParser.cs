using IUBH.TOR.Domain;
using IUBH.TOR.Modules.Courses.Domain;

namespace IUBH.TOR.Modules.Courses.Services
{
    public interface ICoursePageHtmlParser
    {
        /// <summary>
        /// Converts an HTML page to a set of (raw) course information.
        /// </summary>
        Result<RawCourse[]> TryParseCoursePage(string coursePageHtml);
    }
}
