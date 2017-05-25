using Acabus.Models;
using System.Collections.Generic;

namespace Acabus.Converters
{
    /// <summary>
    /// Convertidor para la traducción de la enumeración <see cref="Priority"/>.
    /// </summary>
    public sealed class PriorityConverter : TranslateEnumConverter<Priority>
    {
        /// <summary>
        /// Crea una instancia nueva del traductor de <see cref="Priority"/>.
        /// </summary>
        public PriorityConverter() : base(new Dictionary<Priority, string>()
        {
            { Priority.HIGH, "ALTA" },
            { Priority.MEDIUM, "MEDIA" },
            { Priority.LOW, "BAJA" },
            { Priority.NONE, "NINGUNA" }
        })
        { }
    }
}
