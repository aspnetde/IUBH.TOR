using System;

namespace IUBH.TOR.Utilities.Preferences
{
    /// <summary>
    /// Allows storing of information that do not fit into the database
    /// nor the Secure Storage.
    /// </summary>
    public interface IPreferencesUtility
    {
        /// <summary>
        /// Provides date and time of the last successful update check.
        /// </summary>
        DateTime LastCourseUpdateCheck { get; set; }

        /// <summary>
        /// Removes all the preferences from the Storage.
        /// </summary>
        void Nuke();
    }
}
