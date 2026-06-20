using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StudyManager.Views.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNullOrEmpty = value == null || 
                                 (value is string s && string.IsNullOrWhiteSpace(s)) ||
                                 (value is int i && i == 0) ||
                                 (value is System.Collections.ICollection col && col.Count == 0);

            bool inverse = parameter != null && parameter.ToString()?.ToLower() == "inverse";

            if (isNullOrEmpty)
            {
                return inverse ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return inverse ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
