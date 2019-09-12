namespace IUBH.TOR.Modules.Courses.Domain
{
    /// <summary>
    /// Provides information about the outcome of an update operation.
    /// </summary>
    public class CourseUpdateInfo
    {
        /// <summary>
        /// True if updates have been fetched.
        /// </summary>
        public bool UpdatesFetched { get; }

        internal CourseUpdateInfo(bool updatesFetched) => UpdatesFetched = updatesFetched;
    }
}
