using System;
using System.Globalization;
using System.Windows.Data;

namespace Acabus.Converters
{
    public class AndBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool response = true;
            foreach (var item in values)
            {
                if (item is bool)
                    response &= (bool)item;
                else
                {
                    response = false;
                    break;
                }
            }
            return response;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
