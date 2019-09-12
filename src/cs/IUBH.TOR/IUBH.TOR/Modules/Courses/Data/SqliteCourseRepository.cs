using IUBH.TOR.Data;
using IUBH.TOR.Modules.Courses.Domain;

namespace IUBH.TOR.Modules.Courses.Data
{
    internal class SqliteCourseRepository : ICourseRepository
    {
        private readonly IDbConnection _connection;

        internal SqliteCourseRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Returns all courses currently saved to the database.
        /// </summary>
        public Course[] All()
        {
            return _connection.Connection.Table<Course>().ToArray();
        }
    }
}
