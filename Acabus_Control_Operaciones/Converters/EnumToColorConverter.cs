using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace Acabus.Converters
{
    /// <summary>
    /// Convertidor de valor utilizado para representar enumeraciones en diversos colores del tipo <see cref="Brush"/>.
    /// </summary>
    /// <typeparam name="T">Enumeración a utilizar para la conversión.</typeparam>
    [ValueConversion(typeof(Enum), typeof(Brush))]
    public abstract class EnumToColorConverter<T> : IValueConverter
    {
        /// <summary>
        /// Diccionario que contiene todos los colores traducidos de la enumeración.
        /// </summary>
        private Dictionary<T, Brush> _keys;

        /// <summary>
        /// Crea una instancia nueva del convertidor de color.
        /// </summary>
        /// <param name="keys">Diccionario de representación de la enumeración.</param>
        public EnumToColorConverter(Dictionary<T, Brush> keys) { _keys = keys; }

        /// <summary>
        /// Convierte una instancia o una enumeración del tipo <see cref="T"/> a un color que la represente.
        /// </summary>
        /// <param name="value">Instancia de <see cref="T"/>.</param>
        /// <param name="targetType">Tipo de dato del objetivo.</param>
        /// <param name="parameter">Parametros del convertidor.</param>
        /// <param name="culture">Referencia cultural utilizada para la conversión.</param>
        /// <returns>Color que representa la instancia <see cref="T"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush))
                throw new InvalidOperationException("El tipo de dato del objetivo de la conversión debe ser una cadena de texto (System.Windows.Media.Brush)");

            if (value is T)
                return TranslateValue((T)value);
            else if (value is IEnumerable<T>)
                return ((IEnumerable<T>)value).Select((item) => TranslateValue(item));
            else
                throw new ArgumentException("El tipo de dato del valor a convertir no es correcto para este tradutor.");
        }

        /// <summary>
        /// Convierte un color con en una instancia de <see cref="T"/>.
        /// </summary>
        /// <param name="value">Cadena a convertir en una instancia de <see cref="T"/>.</param>
        /// <param name="targetType">Tipo de dato del objetivo.</param>
        /// <param name="parameter">Parametros del convertidor.</param>
        /// <param name="culture">Referencia cultural utilizada para la conversión.</param>
        /// <returns>Una instancia del tipo <see cref="T"/>.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(Brush))
                throw new ArgumentException("El tipo de dato del valor a convertir de vuelta no es del tipo System.Windows.Media.Brush");

            foreach (var item in _keys.Keys)
                if (_keys[item] == (value as Brush))
                    return item;

            throw new ArgumentException("El valor a convertir de vuelta no se encuentra dentro del diccionario de traducción.");
        }

        /// <summary>
        /// Obtiene una instancia del tipo <see cref="Brush"/> a partir de un código hexadecimal de un color.
        /// </summary>
        /// <param name="colorCode">Código hexadecimal de color.</param>
        /// <returns>Una instancia del tipo <see cref="Brush"/>.</returns>
        protected static Brush FromHex(String colorCode)
            => new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorCode));

        /// <summary>
        /// Traduce el valor de una instancia <see cref="T"/> al color que le corresponde.
        /// </summary>
        /// <param name="key">Instancia a traducir.</param>
        /// <returns>Un color que representa el valor de la instancia.</returns>
        private Brush TranslateValue(T key)
        {
            _keys.TryGetValue(key, out Brush value);
            return value;
        }
    }
}