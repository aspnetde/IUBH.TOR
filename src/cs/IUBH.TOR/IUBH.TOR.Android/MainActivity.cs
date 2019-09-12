using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using IUBH.TOR.Droid.Utilities;
using IUBH.TOR.Droid.Utilities.BackgroundSync;
using IUBH.TOR.Utilities.BackgroundSync;
using IUBH.TOR.Utilities.Hud;
using TinyIoC;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace IUBH.TOR.Droid
{
    [Activity(
        Label = "IUBH TOR",
        Icon = "@mipmap/icon",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        WindowSoftInputMode = SoftInput.AdjustPan
    )]
    public class MainActivity : FormsAppCompatActivity
    {
        public static MainActivity Instance { get; private set; }
        public const string NotificationChannelId = "IUBH-TOR";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Instance = this;

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            RegisterPlatformDependencies();
            CreateNotificationChannel();
            
            Forms.Init(this, savedInstanceState);
            FormsMaterial.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(
            int requestCode,
            string[] permissions,
            Permission[] grantResults
        )
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(
                requestCode,
                permissions,
                grantResults
            );

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private static void RegisterPlatformDependencies()
        {
            TinyIoCContainer.Current.Register<IBackgroundSyncUtility, DroidBackgroundSyncUtility>();
            TinyIoCContainer.Current.Register(typeof(IHudUtility), DroidHudUtility.Instance);
        }

        /// <summary>
        /// A Notification Channel is necessary to be able to send local
        /// notification to the system.
        /// </summary>
        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var channel = new NotificationChannel(
                NotificationChannelId,
                "IUBH TOR",
                NotificationImportance.Default
            )
            {
                Description = "Delivers updates of your Transcript of Records."
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}
