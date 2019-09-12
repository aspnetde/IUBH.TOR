using System.Linq;
using IUBH.TOR.Modules.Courses.Domain;

namespace IUBH.TOR.Modules.Courses.Services
{
    internal class CourseSetComparer : ICourseSetComparer
    {
        /// <summary>
        /// Compares two sets of courses and identifies all
        /// those that have been added, modified, or removed.
        /// </summary>
        public void Compare(
            Course[] existingCourses,
            Course[] currentCourses,
            out Course[] addedCourses,
            out Course[] modifiedCourses,
            out Course[] removedCourses
        )
        {
            // A course has been added when no course with its ID already exists.
            addedCourses = currentCourses.Where(c => existingCourses.All(ec => ec.Id != c.Id))
                .ToArray();

            // A course has been removed when no course with its ID exists anymore.
            removedCourses = existingCourses.Where(c => currentCourses.All(ec => ec.Id != c.Id))
                .ToArray();

            // Courses have been modified when they are not structurally equal anymore.
            modifiedCourses = currentCourses.Where(
                c =>
                {
                    var existingCourse = existingCourses.SingleOrDefault(ec => ec.Id == c.Id);
                    return existingCourse != null && c != existingCourse;
                }
            ).ToArray();
        }
    }
}
