using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace StudyManager.Views.Converters
{
    public class LinkCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = 0;
            if (value is int i)
            {
                count = i;
            }
            else if (value is ICollection collection)
            {
                count = collection.Count;
            }

            if (count == 1)
            {
                return "1 link";
            }
            else
            {
                return $"{count} links";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
