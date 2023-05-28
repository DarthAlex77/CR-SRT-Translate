using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CR_SRT_Translate.Models
{
    internal class TimeFormatConverter:IValueConverter
    {
        public object? Convert(object?     value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is TimeSpan span)
            {
                return span.ToString(@"hh\:mm\:ss\,fff");
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                return TimeSpan.Parse(s);
            }
            return TimeSpan.Zero;
        }
    }
}