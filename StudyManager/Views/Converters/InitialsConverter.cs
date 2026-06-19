using System;
using System.Globalization;
using System.Windows.Data;

namespace StudyManager.Views.Converters
{
    public class InitialsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && !string.IsNullOrWhiteSpace(s))
            {
                var parts = s.Trim().Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    string initial1 = parts[0].Substring(0, 1).ToUpper();
                    string initial2 = parts[1].Substring(0, 1).ToUpper();
                    return initial1 + initial2;
                }
                else if (parts.Length == 1 && parts[0].Length > 0)
                {
                    return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
                }
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
