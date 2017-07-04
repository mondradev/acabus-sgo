using System;
using System.Collections.Generic;

namespace Opera.Acabus.Core.Models
{
    /// <summary>
    /// Estructura de una ubicación que contiene dispositivos.
    /// </summary>
    public interface ILocation
    {
        /// <summary>
        /// Obtiene la lista de todos los dispositivos asignados a esta ubicación.
        /// </summary>
        ICollection<Object> Devices { get; }

        /// <summary>
        /// Obtiene o establece el nombre de esta ubicación.
        /// </summary>
        String Name { get; set; }
    }
}