using Acabus.Utils;
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
        /// 
        /// </summary>
        private EnumTranslator<T> _translator;

        /// <summary>
        /// Crea una instancia nueva del traductor.
        /// </summary>
        /// <param name="keys">Valores posibles para la traducción.</param>
        public TranslateEnumConverter(Dictionary<T, String> keys) { _translator = new EnumTranslator<T>(keys); }

        /// <summary>
        /// Crea una instancia a partir de un traductor <see cref="EnumTranslator{T}"/>
        /// </summary>
        public TranslateEnumConverter(EnumTranslator<T> translator)
        {
            _translator = translator;
        }

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
                return _translator.Translate((T)value);
            if (value is IEnumerable<T>)
                return ((IEnumerable<T>)value).Select((item) => _translator.Translate(item));

            return null;
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

            return _translator.TranslateBack(value.ToString());
        }
    }
}
