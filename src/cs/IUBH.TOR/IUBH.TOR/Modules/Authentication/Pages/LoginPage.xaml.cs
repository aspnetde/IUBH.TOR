using System;
using IUBH.TOR.Modules.Courses.Pages;
using Xamarin.Forms;

namespace IUBH.TOR.Modules.Authentication.Pages
{
    public partial class LoginPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void OnEntryCompleted(object sender, EventArgs e)
        {
            if (sender == UserNameEntry)
            {
                PasswordEntry.Focus();
            }
            else
            {
                if (ViewModel.SignInCommand.CanExecute(null))
                {
                    ViewModel.SignInCommand.Execute(null);
                }
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            ViewModel.SignedIn += OnSignedIn;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            ViewModel.SignedIn -= OnSignedIn;
        }

        private static void OnSignedIn(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(
                () =>
                {
                    Application.Current.MainPage = new NavigationPage(new CourseListPage());
                }
            );
        }
    }
}
