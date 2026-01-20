using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace BloodPressureTracker.Converters
{
    public class DayOfWeekConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                return date.DayOfWeek switch
                {
                    DayOfWeek.Monday => "Понедельник",
                    DayOfWeek.Tuesday => "Вторник",
                    DayOfWeek.Wednesday => "Среда",
                    DayOfWeek.Thursday => "Четверг",
                    DayOfWeek.Friday => "Пятница",
                    DayOfWeek.Saturday => "Суббота",
                    DayOfWeek.Sunday => "Воскресенье",
                    _ => date.ToString("dddd")
                };
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}