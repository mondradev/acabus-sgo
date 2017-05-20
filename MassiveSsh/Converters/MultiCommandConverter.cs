using Acabus.Utils.Mvvm;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace Acabus.Converters
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class MultiCommandConverter : IMultiValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var commands = values.Clone();
            return new CommandBase((param) =>
            {
                foreach (var item in ((object[])commands))
                {
                    if (item is ICommand)
                        ((ICommand)item).Execute(param);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetTypes"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
