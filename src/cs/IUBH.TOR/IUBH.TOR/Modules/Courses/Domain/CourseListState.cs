namespace IUBH.TOR.Modules.Courses.Domain
{
    public enum CourseListState
    {
        /// <summary>
        /// Data is currently being loaded from CARE.
        /// </summary>
        Loading = 0,
        /// <summary>
        /// Courses have been loaded and can be displayed.
        /// </summary>
        Default,
        /// <summary>
        /// Courses have been loaded, but there's none at the moment.
        /// </summary>
        Empty,
        /// <summary>
        /// While loading courses an error has occured.
        /// </summary>
        Error
    }
}
