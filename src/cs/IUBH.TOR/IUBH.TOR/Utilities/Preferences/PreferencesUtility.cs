using System;

namespace IUBH.TOR.Utilities.Preferences
{
    /// <summary>
    /// Allows storing of information that do not fit into the database
    /// nor the Secure Storage.
    /// </summary>
    internal class PreferencesUtility : IPreferencesUtility
    {
        private const string LastCourseUpdateCheckKey = nameof(LastCourseUpdateCheck);

        /// <summary>
        /// Provides date and time of the last successful update check.
        /// </summary>
        public DateTime LastCourseUpdateCheck
        {
            get => Xamarin.Essentials.Preferences.Get(LastCourseUpdateCheckKey, DateTime.MinValue);
            set => Xamarin.Essentials.Preferences.Set(LastCourseUpdateCheckKey, value);
        }

        /// <summary>
        /// Removes all the preferences from the Storage/sets them back
        /// to their default values.
        /// </summary>
        public void Nuke()
        {
            LastCourseUpdateCheck = DateTime.MinValue;
        }
    }
}
