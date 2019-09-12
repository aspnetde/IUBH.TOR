using System;
using System.Globalization;
using IUBH.TOR.Modules.Courses.Domain;
using Xamarin.Forms;

namespace IUBH.TOR.Modules.Courses.Pages.Converters
{
    public class CourseListStateConverter : IValueConverter
    {
        /// <summary>
        /// Returns true when the two course list states provided (value,
        /// parameter) are equal. Otherwise returns false.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (CourseListState)value == (CourseListState)parameter;
        }

        /// <summary>
        /// Not implemented!
        /// </summary>
        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            throw new NotImplementedException();
        }
    }
}
