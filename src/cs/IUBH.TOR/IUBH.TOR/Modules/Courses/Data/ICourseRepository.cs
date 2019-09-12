using IUBH.TOR.Modules.Courses.Domain;

namespace IUBH.TOR.Modules.Courses.Data
{
    public interface ICourseRepository
    {
        /// <summary>
        /// Returns all courses currently saved to the database.
        /// </summary>
        Course[] All();
    }
}
