using System;
using System.Threading.Tasks;
using IUBH.TOR.Data;
using IUBH.TOR.Domain;
using IUBH.TOR.Modules.Authentication;
using IUBH.TOR.Modules.Authentication.Pages;
using IUBH.TOR.Modules.Courses;
using IUBH.TOR.Modules.Courses.Pages;
using IUBH.TOR.Modules.Shared;
using IUBH.TOR.Modules.Shared.Services;
using IUBH.TOR.Utilities;
using IUBH.TOR.Utilities.BackgroundSync;
using IUBH.TOR.Utilities.Messaging;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IUBH.TOR
{
    /// <summary>
    /// This is the starting point of the whole Xamarin.Forms application.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App
    {
        public App()
        {
            InitializeComponent();
            RegisterCoreDependencies();

#if DEBUG
          HotReloader.Current.Run(this);
#endif
        }

        /// <summary>
        /// Whenever the app starts we need to decide on what to show: The Login or
        /// the Course List Page
        /// </summary>
        protected override async void OnStart()
        {
            base.OnStart();

            var credentialStorage = TinyIoC.TinyIoCContainer.Current.Resolve<ICredentialStorage>();
            var backgroundSync = TinyIoC.TinyIoCContainer.Current.Resolve<IBackgroundSyncUtility>();

            Type initialPageType = await InitializeAsync(credentialStorage, backgroundSync)
                .ConfigureAwait(true);

            MainPage = new NavigationPage((Page)Activator.CreateInstance(initialPageType));
        }

        protected override void OnResume()
        {
            base.OnResume();

            // When we just woke up, we sent a message about that event. So anyone
            // who's interested in it can do its stuff (like refreshing the list
            // of courses).
            var messenger = TinyIoC.TinyIoCContainer.Current.Resolve<IMessenger>();
            messenger.Send(new WokeUpMessage());
        }

        /// <summary>
        /// Returns either the type of the Course List Page (when the user is already
        /// authenticated) or of the Login Page (when the user isn't authenticated).
        /// </summary>
        internal static async Task<Type> InitializeAsync(
            ICredentialStorage credentialStorage,
            IBackgroundSyncUtility backgroundSync
        )
        {
            var credentialsResult =
                await credentialStorage.GetCredentialsAsync().ConfigureAwait(false);

            if (credentialsResult.IsSuccessful)
            {
                backgroundSync.Enable();
            }

            return credentialsResult.IsSuccessful ? typeof(CourseListPage) : typeof(LoginPage);
        }

        /// <summary>
        /// Registers all the dependencies from the
        /// different parts of the application.
        /// </summary>
        private static void RegisterCoreDependencies()
        {
            AuthenticationDependencies.Register();
            CourseDependencies.Register();
            DataDependencies.Register();
            SharedDependencies.Register();
            UtilityDependencies.Register();
        }
    }
}
