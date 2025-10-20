using System;
using System.Globalization;
using System.Windows.Data;

namespace Computer_Status_Viewer
{
    public class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "00"; // Возвращаем "00" для null значения, чтобы график не ломался
            }

            if (value is double seconds)
            {
                return TimeSpan.FromSeconds(seconds).ToString(@"ss");
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}