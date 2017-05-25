using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Acabus.Modules.CctvReports.Converters
{
    public sealed class CollapseBooleanConverter : IValueConverter, IMultiValueConverter

    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = parameter.ToString() != "OR" ? true : false;
            foreach (var value in values)
                if (value is bool)
                    if (parameter.ToString() == "OR")
                        isVisible |= (bool)value;
                    else
                        isVisible &= (bool)value;
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
