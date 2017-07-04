using System;

namespace Opera.Acabus.Core.Models
{
    /// <summary>
    /// Define la estructura de un objeto que puede ser asignado a una sección de atención.
    /// </summary>
    public interface IAssignableSection
    {
        /// <summary>
        /// /// Obtiene o establece la sección de atención a esta ubicación.
        /// </summary>
        String AssignedSection { get; set; }
    }
}