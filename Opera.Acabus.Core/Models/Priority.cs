using System;

namespace Opera.Acabus.Core.Models
{
    /// <summary>
    /// Define las prioridades que establece una tarea.
    /// </summary>
    [Flags]
    public enum Priority
    {
        /// <summary>
        /// Sin prioridad.
        /// </summary>
        NONE = 0,

        /// <summary>
        /// Prioridad baja.
        /// </summary>
        LOW = 1,

        /// <summary>
        /// Prioridad media.
        /// </summary>
        MEDIUM = 2,

        /// <summary>
        /// Prioridad alta.
        /// </summary>
        HIGH = 4,
    }
}