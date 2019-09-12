using System;
using System.Diagnostics;
using Foundation;
using IUBH.TOR.iOS.Utilities;
using IUBH.TOR.Modules.Courses.Services;
using IUBH.TOR.Utilities.BackgroundSync;
using IUBH.TOR.Utilities.Hud;
using KeyboardOverlap.Forms.Plugin.iOSUnified;
using TinyIoC;
using UIKit;
using UserNotifications;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace IUBH.TOR.iOS
{
    [Register(nameof(AppDelegate))]
    public class AppDelegate : FormsApplicationDelegate
    {
        public override bool FinishedLaunching(
            UIApplication uiApplication,
            NSDictionary launchOptions
        )
        {
            RegisterPlatformDependencies();

            Forms.Init();
            FormsMaterial.Init();
            KeyboardOverlapRenderer.Init();

            LoadApplication(new App());

            SetStyle();

            return base.FinishedLaunching(uiApplication, launchOptions);
        }

        private static void RegisterPlatformDependencies()
        {
            TinyIoCContainer.Current.Register<IBackgroundSyncUtility, IosBackgroundSyncUtility>();
            TinyIoCContainer.Current.Register<IHudUtility, IosHudUtility>();
        }

        private static void SetStyle()
        {
            UINavigationBar.Appearance.BarTintColor = UIColor.White;
        }

        public override async void PerformFetch(
            UIApplication application,
            Action<UIBackgroundFetchResult> completionHandler
        )
        {
            try
            {
                Debug.WriteLine("Fetch started");

                var updater = TinyIoCContainer.Current.Resolve<ICourseUpdater>();

                var updateResult = await updater.TryUpdateAsync().ConfigureAwait(false);

                if (!updateResult.IsSuccessful)
                {
                    Debug.WriteLine("Fetch failed: " + updateResult.ErrorMessage);

                    completionHandler(UIBackgroundFetchResult.Failed);
                    return;
                }

                if (!updateResult.Value.UpdatesFetched)
                {
                    Debug.WriteLine("Fetch finished successfully. But no updates.");

                    completionHandler(UIBackgroundFetchResult.NoData);
                    return;
                }

                Debug.WriteLine("Fetch finished successfully. And we've got news!.");

                UNUserNotificationCenter.Current.GetNotificationSettings(
                    settings =>
                    {
                        // If we do not have the permission to send notifications, we just
                        // don't do it.
                        if (settings.AuthorizationStatus != UNAuthorizationStatus.Authorized)
                        {
                            return;
                        }

                        var content = new UNMutableNotificationContent
                        {
                            Title = Constants.NotificationTitle,
                            Body = Constants.NotificationText,
                            Sound = UNNotificationSound.Default
                        };

                        var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(0.01, false);
                        var identifier = $"iubh-tor-update-{Guid.NewGuid()}";

                        var request = UNNotificationRequest.FromIdentifier(
                            identifier,
                            content,
                            trigger
                        );

                        // Fire the notification
                        UNUserNotificationCenter.Current.AddNotificationRequest(request, null);
                    }
                );

                completionHandler(UIBackgroundFetchResult.NewData);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Fetch failed (Exception thrown): " + e.Message);

                completionHandler(UIBackgroundFetchResult.Failed);
            }
        }
    }
}
