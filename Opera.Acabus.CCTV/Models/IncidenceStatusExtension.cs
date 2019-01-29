using System;

namespace Opera.Acabus.Cctv.Models
{
    /// <summary>
    /// Provee de funciones a la enumeración <see cref="IncidenceStatus"/>.
    /// </summary>
    public static class IncidenceStatusExtension
    {
        /// <summary>
        /// Traduce al idioma español el valor de la enumeración especificada.
        /// </summary>
        /// <param name="incidenceStatus">Valor de la enumeración a traducir.</param>
        /// <returns>Una cadena que representa en idioma español el valor de la enumeración.</returns>
        public static String Translate(this IncidenceStatus incidenceStatus)
            => new IncidenceStatusTranslator().Translate(incidenceStatus);
    }
}