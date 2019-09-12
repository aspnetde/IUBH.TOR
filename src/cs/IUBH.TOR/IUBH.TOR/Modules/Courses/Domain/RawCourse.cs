namespace IUBH.TOR.Modules.Courses.Domain
{
    /// <summary>
    /// Provides the raw data of a Course as provided by CARE.
    /// </summary>
    public class RawCourse
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Module { get; set; }
        public string Status { get; set; }
        public string Grade { get; set; }
        public string Rating { get; set; }
        public string Credits { get; set; }
        public string DateOfExamination { get; set; }
        public string Attempts { get; set; }
    }
}
