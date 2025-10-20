using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Computer_Status_Viewer
{
    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && parameter is string colors)
            {
                var colorArray = colors.Split(',');
                if (colorArray.Length == 2)
                {
                    return isChecked
                        ? (object)new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorArray[0]))
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorArray[1]));
                }
            }
            return Brushes.Black; // Значение по умолчанию
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}