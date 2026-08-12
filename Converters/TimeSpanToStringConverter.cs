using System;
using System.Globalization;
using System.Windows.Data;

namespace FreelanceFlow.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan ts)
            {
                string format = parameter as string ?? "full";

                return format switch
                {
                    "short" => $"{ts.TotalHours:F1}h",
                    "hours" => $"{ts.TotalHours:F2}",
                    "full" => $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}",
                    _ => $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}"
                };
            }

            return "00:00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (TimeSpan.TryParse(value?.ToString(), out var result))
                return result;

            return TimeSpan.Zero;
        }
    }
}