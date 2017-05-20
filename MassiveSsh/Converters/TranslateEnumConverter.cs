using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Acabus.Converters
{
    /// <summary>
    /// Convertidor para la traducción de la enumeración <see cref="T"/>.
    /// <typeparamref name="T">El tipo de dato que se desea traducir.</typeparamref>
    /// </summary>
    public abstract class TranslateEnumConverter<T> : IValueConverter
    {
        /// <summary>
        /// Diccionario que contiene todas las palabras traducidas de la enumeración.
        /// </summary>
        private Dictionary<T, String> _keys;

        /// <summary>
        /// Crea una instancia nueva del traductor.
        /// </summary>
        /// <param name="keys">Valores posibles para la traducción.</param>
        public TranslateEnumConverter(Dictionary<T, String> keys) { _keys = keys; }

        /// <summary>
        /// Convierte una instancia o una enumeración del tipo <see cref="T"/>
        /// a una cadena que la represente en español.
        /// </summary>
        /// <param name="value">Instancia de <see cref="T"/>.</param>
        /// <param name="targetType">Tipo de datos del objetivo.</param>
        /// <param name="parameter">Parametros del convertidor.</param>
        /// <param name="culture">Referencia cultural utilizada para la conversión.</param>
        /// <returns>Una cadena que representa la instancia <see cref="T"/>.</returns>
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is T)
                return TranslateValue((T)value);
            if (value is IEnumerable<T>)
                return ((IEnumerable<T>)value).Select((item) => TranslateValue(item));

            return null;
        }

        /// <summary>
        /// Traduce el valor de una instancia <see cref="T"/> a una cadena que la representa
        /// en español.
        /// </summary>
        /// <param name="key">Instancia a traducir.</param>
        /// <returns>Una cadena que representa el valor de la instancia.</returns>
        private String TranslateValue(T key)
        {
            _keys.TryGetValue(key, out String value);
            return value;
        }

        /// <summary>
        /// Convierte una cadena con el formato adecuado en una instancia de <see cref="T"/>.
        /// </summary>
        /// <param name="value">Cadena a convertir en una instancia de <see cref="T"/>.</param>
        /// <param name="targetType">Tipo de datos del objetivo.</param>
        /// <param name="parameter">Parametros del convertidor.</param>
        /// <param name="culture">Referencia cultural utilizada para la conversión.</param>
        /// <returns>Una instancia del tipo <see cref="T"/>.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is String)) return null;

            foreach (var item in _keys.Keys)
                if (_keys[item] == value.ToString())
                    return item;

            return null;
        }
    }
}
