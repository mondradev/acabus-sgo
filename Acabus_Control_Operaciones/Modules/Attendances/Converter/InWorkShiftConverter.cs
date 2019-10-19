using Acabus.Modules.Attendances.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Acabus.Modules.Attendances.Converter
{
    public sealed class InWorkShiftConverter : IValueConverter
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
            if (value is null)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
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
            throw new NotImplementedException();
        }
    }
}
