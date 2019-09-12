using Xamarin.Forms;

namespace IUBH.TOR.Utilities.Alerts
{
    internal class AlertUtility : IAlertUtility
    {
        /// <summary>
        /// Shows an Alert with the error message provided.
        /// </summary>
        public void ShowError(string errorMessage)
        {
            Device.BeginInvokeOnMainThread(
                () =>
                {
                    Application.Current.MainPage.DisplayAlert("Error", errorMessage, "OK");
                }
            );
        }
    }
}
