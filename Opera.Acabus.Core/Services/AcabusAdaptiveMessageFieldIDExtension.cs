using System;

namespace Opera.Acabus.Core.Services
{
    /// <summary>
    /// Extensión de la clase <see cref="AcabusAdaptiveMessageFieldID"/>.
    /// </summary>
    public static class AcabusAdaptiveMessageFieldIDExtension
    {
        /// <summary>
        /// Convierte el valor de la enumeración en <see cref="Int32"/>.
        /// </summary>
        /// <param name="fieldID">Campo de la enumeración.</param>
        /// <returns>Valor númerico que representa el campo de la enumeración.</returns>
        public static Int32 ToInt32(this AcabusAdaptiveMessageFieldID fieldID)
            => (Int32)fieldID;
    }
}