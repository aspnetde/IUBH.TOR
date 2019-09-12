using System.Diagnostics;
using IUBH.TOR.Utilities.BackgroundSync;
using UIKit;
using UserNotifications;

namespace IUBH.TOR.iOS.Utilities
{
    public class IosBackgroundSyncUtility : IBackgroundSyncUtility
    {
        public void Enable()
        {
            Debug.WriteLine("Fetch enabled.");

            UIApplication.SharedApplication.InvokeOnMainThread(
                () =>
                {
                    // Set the update interval
                    UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(
                        Constants.FetchIntervalInMinutes * 60
                    );

                    // Request Permission for sending Notifications
                    UNUserNotificationCenter.Current.RequestAuthorization(
                        UNAuthorizationOptions.Alert,
                        (approved, err) =>
                        {
                            // Nothing to do here
                        }
                    );
                }
            );
        }

        public void Disable()
        {
            Debug.WriteLine("Fetch disabled.");

            UIApplication.SharedApplication.InvokeOnMainThread(
                () => UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(
                    UIApplication.BackgroundFetchIntervalNever
                )
            );
        }
    }
}
