using System;
using System.Threading.Tasks;
using IUBH.TOR.Domain;
using IUBH.TOR.Modules.Authentication.Services;
using IUBH.TOR.Modules.Shared.Domain;
using IUBH.TOR.Modules.Shared.Pages;
using IUBH.TOR.Modules.Shared.Services;
using IUBH.TOR.Utilities.Alerts;
using IUBH.TOR.Utilities.BackgroundSync;
using IUBH.TOR.Utilities.Hud;
using Xamarin.Forms;

namespace IUBH.TOR.Modules.Authentication.Pages
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly ICredentialValidator _credentialValidator;
        private readonly ICredentialStorage _credentialStorage;
        private readonly IHudUtility _hud;
        private readonly IAlertUtility _alerts;
        private readonly IBackgroundSyncUtility _backgroundSync;

        public string UserName { get; set; }
        public string Password { get; set; }

        public bool IsBusy { get; internal set; }

        internal const string LoginMessage = "You are being logged in...";

        public Command SignInCommand => new Command(async () => await SignInAsync(), CanSignIn);

        public event EventHandler SignedIn;

        public LoginViewModel(
            ICredentialValidator credentialValidator,
            ICredentialStorage credentialStorage,
            IHudUtility hud,
            IAlertUtility alerts,
            IBackgroundSyncUtility backgroundSync
        )
        {
            _credentialValidator = credentialValidator;
            _credentialStorage = credentialStorage;
            _hud = hud;
            _alerts = alerts;
            _backgroundSync = backgroundSync;
        }

        internal async Task SignInAsync()
        {
            try
            {
                IsBusy = true;

                _hud.Show(LoginMessage);

                var credentials = new Credentials(UserName, Password);

                Result validationResult = await _credentialValidator.ValidateAsync(credentials)
                    .ConfigureAwait(false);

                if (!validationResult.IsSuccessful)
                {
                    _alerts.ShowError(validationResult.ErrorMessage);
                    return;
                }

                var saveResult = await _credentialStorage.SaveCredentialsAsync(credentials)
                    .ConfigureAwait(false);

                if (!saveResult.IsSuccessful)
                {
                    _alerts.ShowError(saveResult.ErrorMessage);
                    return;
                }
                
                _backgroundSync.Enable();

                SignedIn?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                _hud.Dismiss();

                IsBusy = false;
            }
        }

        private bool CanSignIn()
            => !string.IsNullOrWhiteSpace(UserName)
               && !string.IsNullOrWhiteSpace(Password)
               && !IsBusy;
    }
}
