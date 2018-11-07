using System;

namespace Opera.Acabus.Core.Models
{
    /// <summary>
    /// Provee de funciones extra a la enumeración <see cref="DeviceType"/>.
    /// </summary>
    public static class DeviceTypeExtension
    {
        ///<summary>
        /// Traductor al español de la enumeración <see cref="DeviceType"/>.
        ///</summary>
        private static DeviceTypeSpanishTranslator _translate;

        /// <summary>
        /// Constructo estático de la clase <see cref="DeviceTypeExtension"/>.
        /// </summary>
        static DeviceTypeExtension()
        {
            _translate = new DeviceTypeSpanishTranslator();
        }

        /// <summary>
        /// Traduce al español la enumeración <see cref="DeviceType"/>.
        /// </summary>
        /// <param name="type">Valor de la enumeración <see cref="DeviceType"/> a traducir.</param>
        /// <returns>Una cadena en español que representa la enumeración <see cref="DeviceType"/>.</returns>
        public static String TranslateToSpanish(this DeviceType type)
        => _translate.Translate(type);
    }
}