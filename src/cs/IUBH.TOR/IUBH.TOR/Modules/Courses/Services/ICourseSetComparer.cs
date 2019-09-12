using IUBH.TOR.Modules.Courses.Domain;

namespace IUBH.TOR.Modules.Courses.Services
{
    public interface ICourseSetComparer
    {
        /// <summary>
        /// Compares two sets of courses and identifies all
        /// those that have been added, modified, or removed.
        /// </summary>
        void Compare(
            Course[] existingCourses,
            Course[] currentCourses,
            out Course[] addedCourses,
            out Course[] modifiedCourses,
            out Course[] removedCourses
        );
    }
}
