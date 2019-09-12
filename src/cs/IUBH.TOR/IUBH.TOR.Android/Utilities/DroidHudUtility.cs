using System;
using Android.App;
using Android.Content;
using Android.Views.InputMethods;
using IUBH.TOR.Utilities;
using IUBH.TOR.Utilities.Hud;

namespace IUBH.TOR.Droid.Utilities
{
    public class DroidHudUtility : IHudUtility
    {
        private ProgressDialog _dialog;

        private static readonly Lazy<DroidHudUtility> s_instance =
            new Lazy<DroidHudUtility>(() => new DroidHudUtility());

        public static DroidHudUtility Instance => s_instance.Value;

        private DroidHudUtility()
        {
        }

        public void Show(string message)
        {
            Dismiss();

            MainActivity.Instance.RunOnUiThread(
                () =>
                {
                    if (MainActivity.Instance?.CurrentFocus?.WindowToken != null)
                    {
                        var inputManger =
                            (InputMethodManager)MainActivity.Instance.GetSystemService(
                                Context.InputMethodService
                            );

                        inputManger.HideSoftInputFromWindow(
                            MainActivity.Instance.CurrentFocus.WindowToken,
                            0
                        );
                    }

                    _dialog = ProgressDialog.Show(
                        MainActivity.Instance,
                        string.Empty,
                        message,
                        indeterminate: true,
                        cancelable: false
                    );
                }
            );
        }

        public void Dismiss()
        {
            MainActivity.Instance.RunOnUiThread(
                () =>
                {
                    try
                    {
                        _dialog?.Dismiss();
                        _dialog?.Dispose();
                        _dialog = null;
                    }
                    catch
                    {
                        // Nothing to do here
                    }
                }
            );
        }
    }
}
