using Acabus.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Acabus.Modules.TrunkMonitor
{
    /// <summary>
    /// Permite la conversión de una valor del tipo AlertState a un color rojo si esta presenta una valor UNREAD.
    /// </summary>
    public sealed class ConverterAlertStateToColor : IValueConverter
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
            if ((AlertState)value == AlertState.UNREAD)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"));
            return null;
        }

        /// <summary>
        /// Función no implementada. No se requiere su uso.
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

