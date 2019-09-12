using IUBH.TOR.Data;
using IUBH.TOR.Modules.Courses.Data;
using IUBH.TOR.Modules.Courses.Services;

namespace IUBH.TOR.Modules.Courses
{
    internal static class CourseDependencies
    {
        public static void Register()
        {
            var container = TinyIoC.TinyIoCContainer.Current;

            container.Register<ICoursePageHtmlDownloader, CoursePageHtmlDownloader>();
            container.Register<ICoursePageHtmlParser, CoursePageHtmlParser>();

            container.Register<ICourseRepository>(
                (x, y) => new SqliteCourseRepository(DbConnection.Instance)
            );

            container.Register<ICourseSetComparer, CourseSetComparer>();
            container.Register<ICourseUpdater, CourseUpdater>();
        }
    }
}
