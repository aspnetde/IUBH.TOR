using System.Threading.Tasks;
using IUBH.TOR.Domain;
using IUBH.TOR.Modules.Courses.Domain;

namespace IUBH.TOR.Modules.Courses.Services
{
    public interface ICourseUpdater
    {
        /// <summary>
        /// Executes a full update operation on courses: Downloads, parses,
        /// compares, adds/removes/updates courses to the database.
        /// </summary>
        Task<Result<CourseUpdateInfo>> TryUpdateAsync();
    }
}
