using System.Threading.Tasks;
using IUBH.TOR.Domain;

namespace IUBH.TOR.Modules.Courses.Services
{
    public interface ICoursePageHtmlDownloader
    {
        /// <summary>
        /// Tries to download the HTML page off the CARE system.
        /// </summary>
        Task<Result<string>> TryDownloadCoursePageHtmlAsync(string url);
    }
}
