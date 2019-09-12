using System;
using IUBH.TOR.Modules.Authentication.Pages;
using IUBH.TOR.Modules.Courses.Domain;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IUBH.TOR.Modules.Courses.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CourseListPage : IReloadable
    {
        public CourseListPage()
        {
            InitializeComponent();
        }

        protected override void OnViewModelInitialized()
        {
            _courseList.ItemsSource = ViewModel.Courses;
        }

        /// <summary>
        /// This is a pure development feature allowing to refresh
        /// the list's data when the HotReload tool refreshes the
        /// XAML layout after editing. 
        /// </summary>
        public void OnLoaded()
        {
#if DEBUG
            _courseList.ItemsSource = ViewModel.Courses;
#endif
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ViewModel.SignedOut += OnSignedOut;
            ViewModel.CourseSelected += OnCourseSelected;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            ViewModel.SignedOut -= OnSignedOut;
            ViewModel.CourseSelected -= OnCourseSelected;
        }

        private static void OnSignedOut(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(
                () =>
                {
                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                }
            );
        }

        private void OnCourseSelected(object sender, Course course)
        {
            Device.BeginInvokeOnMainThread(
                () =>
                {
                    Navigation.PushAsync(new CourseDetailPage(course));
                }
            );
        }

        private void OnCourseCellTapped(object sender, ItemTappedEventArgs e)
            => ViewModel.SelectCourseCommand.Execute(e.Item);
    }
}
