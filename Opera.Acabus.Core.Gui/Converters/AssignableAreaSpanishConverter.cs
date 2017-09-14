using InnSyTech.Standard.Mvvm.Utils;
using Opera.Acabus.Core.Models;
using System.Collections.Generic;

namespace Opera.Acabus.Core.Gui.Converters
{
    /// <summary>
    /// Esta clase define un convertidor de valores para la enumeración <see cref="AssignableArea"/>.
    /// </summary>
    public sealed class AssignableAreaSpanishConverter : TranslateEnumConverter<AssignableArea>
    {
        /// <summary>
        /// Crea una instancia nueva de <see cref="AssignableAreaSpanishConverter"/>.
        /// </summary>
        public AssignableAreaSpanishConverter()
            : base(new Dictionary<AssignableArea, string>() {
                { AssignableArea.MANTTO, "MANTENIMIENTO" },
                { AssignableArea.SUPERVISOR, "SUPERVISOR" },
                { AssignableArea.SUPPORT, "SOPORTE TÉCNICO" },
                { AssignableArea.DATABASE, "ANALISTA ADMINISTRADOR DE BASE DE DATOS" },
                { AssignableArea.IT_MANAGER, "GERENCIA TI" }
        })
        { }
    }
}