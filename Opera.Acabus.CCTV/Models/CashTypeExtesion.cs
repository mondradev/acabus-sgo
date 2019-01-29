using System;

namespace Opera.Acabus.Cctv.Models
{
    /// <summary>
    /// Provee de funciones extra a la enumeración <see cref="CashType"/>.
    /// </summary>
    public static class CashTypeExtesion
    {
        /// <summary>
        /// Traduce el valor de la enumeración al idioma español.
        /// </summary>
        /// <param name="cashType">Enumeración a traducir.</param>
        /// <returns>Una cadena que representa al objeto en español.</returns>
        public static String Translate(this CashType cashType)
            => new CashTypeTraslator().Translate(cashType);
    }
}