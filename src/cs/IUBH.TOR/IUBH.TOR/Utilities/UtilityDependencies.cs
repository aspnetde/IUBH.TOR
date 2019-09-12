using IUBH.TOR.Utilities.Alerts;
using IUBH.TOR.Utilities.Date;
using IUBH.TOR.Utilities.Messaging;
using IUBH.TOR.Utilities.Preferences;
using IUBH.TOR.Utilities.SecureStorage;

namespace IUBH.TOR.Utilities
{
    internal static class UtilityDependencies
    {
        public static void Register()
        {
            var container = TinyIoC.TinyIoCContainer.Current;

            container.Register<IAlertUtility, AlertUtility>();
            container.Register<IDateTimeUtility, DateTimeUtility>();
            container.Register<IMessenger, FormsMessenger>();
            container.Register<IPreferencesUtility, PreferencesUtility>();
            container.Register<ISecureStorageUtility, SecureStorageUtility>();
        }
    }
}
