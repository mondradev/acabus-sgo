using System;

namespace Opera.Acabus.Core.Models
{
    /// <summary>
    /// Enumera todos los tipos de autobuses.
    /// </summary>
    [Flags]
    public enum BusType
    {
        /// <summary>
        /// Sin tipo de autobus (Valor predeterminado).
        /// </summary>
        NONE,

        /// <summary>
        /// Autobus tipo convencional.
        /// </summary>
        CONVENTIONAL = 1,

        /// <summary>
        /// Autobus tipo padrón.
        /// </summary>
        STANDARD = 2,

        /// <summary>
        /// Autobus tipo articulado.
        /// </summary>
        ARTICULATED = 4
    }
}