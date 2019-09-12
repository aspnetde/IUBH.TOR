using System;

namespace IUBH.TOR.Utilities.Date
{
    internal class DateTimeUtility : IDateTimeUtility
    {
        /// <summary>
        /// Passes along the current DateTime.UtcNow. Used for
        /// testing purposes.
        /// </summary>
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
