using System;
using System.Globalization;
using Xamarin.Forms;

namespace IUBH.TOR.Modules.Courses.Pages.Converters
{
    public class HideEmptyDataConverter : IValueConverter
    {
        /// <summary>
        /// Returns true only when the value isn't null and doesn't
        /// have a length of 0.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && value.ToString().Length != 0;
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
