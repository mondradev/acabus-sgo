using Acabus.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Acabus.Utils.MVVM
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ConverterPriorityToColor : IValueConverter
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
            if (value is Priority)
                switch ((Priority)value)
                {
                    case Priority.LOW:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEB3B"));
                    case Priority.MEDIUM:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
                    case Priority.HIGH:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"));
                }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9E9E9E"));
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
