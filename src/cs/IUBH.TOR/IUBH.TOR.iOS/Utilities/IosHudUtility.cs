using BigTed;
using IUBH.TOR.Utilities;
using IUBH.TOR.Utilities.Hud;
using UIKit;

namespace IUBH.TOR.iOS.Utilities
{
    public class IosHudUtility : IHudUtility
    {
        public void Show(string message)
        {
            Dismiss();

            UIApplication.SharedApplication.InvokeOnMainThread(
                () =>
                {
                    BTProgressHUD.Show(message, maskType: ProgressHUD.MaskType.Black);
                }
            );
        }

        public void Dismiss()
        {
            UIApplication.SharedApplication.InvokeOnMainThread(BTProgressHUD.Dismiss);
        }
    }
}
