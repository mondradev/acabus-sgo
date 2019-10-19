using System;
using System.Globalization;
using System.Windows.Data;

namespace Acabus.Converters
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class DateTimeConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan)
                return new DateTime(((TimeSpan)value).Ticks);
            if (value is DateTime)
                return ((DateTime)value).TimeOfDay;
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan)
                return new DateTime(((TimeSpan)value).Ticks);
            if (value is DateTime)
                return ((DateTime)value).TimeOfDay;
            return null;
        }
    }
}
