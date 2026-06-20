using System;
using System.Globalization;
using System.Windows.Data;
using StudyManager.Models;
using StudyManager.ViewModels;

namespace StudyManager.Views.Converters
{
    public class ThemeParamConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is StudyTopic topic && values[1] is StudyTheme theme)
            {
                return new ThemeCommandParameter(topic, theme);
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
