using System;

namespace IUBH.TOR.Utilities.Date
{
    public interface IDateTimeUtility
    {
        /// <summary>
        /// Passes along the current DateTime.UtcNow. Used for
        /// testing purposes.
        /// </summary>
        DateTime UtcNow { get; }
    }
}
