using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FreelanceFlow.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool b && b;
            bool invert = parameter?.ToString()?.ToLower() == "invert";

            if (invert)
                boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = parameter?.ToString()?.ToLower() == "invert";
            bool result = value is Visibility visibility && visibility == Visibility.Visible;

            return invert ? !result : result;
        }
    }
}