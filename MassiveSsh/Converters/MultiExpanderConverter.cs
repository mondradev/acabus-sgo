using Acabus.Utils.Mvvm;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Acabus.Converters
{
    [ValueConversion(typeof(int), typeof(bool))]
    public sealed class MultiExpanderConverter : NotifyPropertyChanged, IValueConverter
    {
        /// <summary>
        /// Campo que provee a la propiedad 'SelectedExpander'.
        /// </summary>
        private int _selectedExpander;

        /// <summary>
        /// Obtiene o establece el <see cref="System.Windows.Controls.Expander"/> actualmente desplegado.
        /// </summary>
        public int SelectedExpander {
            get => _selectedExpander;
            set {
                _selectedExpander = value;
                OnPropertyChanged("SelectedExpander");
            }
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value.Equals(int.Parse(parameter.ToString())));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value) return parameter;
            return -1;
        }
    }
}