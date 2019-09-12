using IUBH.TOR.Modules.Courses.Domain;
using Xamarin.Forms.Xaml;

namespace IUBH.TOR.Modules.Courses.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CourseDetailPage
    {
        public CourseDetailPage(Course course)
        {
            InitializeComponent();
            
            ViewModel.SetCourse(course);
        }
    }
}

