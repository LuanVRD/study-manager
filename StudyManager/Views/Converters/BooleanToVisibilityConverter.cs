using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StudyManager.Views.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is bool b && b;
            bool inverse = parameter != null && parameter.ToString()?.ToLower() == "inverse";

            if (inverse)
            {
                return flag ? Visibility.Collapsed : Visibility.Visible;
            }
            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility vis)
            {
                bool flag = vis == Visibility.Visible;
                bool inverse = parameter != null && parameter.ToString()?.ToLower() == "inverse";
                return inverse ? !flag : flag;
            }
            return false;
        }
    }
}
