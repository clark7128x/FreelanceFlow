using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using FreelanceFlow.Models;

namespace FreelanceFlow.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProjectStatus status)
            {
                return status switch
                {
                    ProjectStatus.Active => new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                    ProjectStatus.OnHold => new SolidColorBrush(Color.FromRgb(255, 193, 7)),
                    ProjectStatus.Completed => new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                    ProjectStatus.Cancelled => new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                    _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))
                };
            }

            return new SolidColorBrush(Color.FromRgb(158, 158, 158));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}