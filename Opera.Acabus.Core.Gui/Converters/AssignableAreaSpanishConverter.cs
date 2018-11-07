using InnSyTech.Standard.Mvvm.Converters;
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
                { AssignableArea.EVERYBODY, "CUALQUIER" },
                { AssignableArea.MANTTO, "TÉCNICO DE MANTENIMIENTO" },
                { AssignableArea.SUPERVISOR, "SUPERVISOR DE MANTENIMIENTO" },
                { AssignableArea.SUPPORT, "SOPORTE TÉCNICO" },
                { AssignableArea.DATABASE, "ANALISTA ABD" },
                { AssignableArea.IT_MANAGER, "GERENTE TI" },
                { AssignableArea.CCTV, "MONITOREO/CCTV" },
                { AssignableArea.CAU, "CENTRO DE ATENCIÓN A USUARIOS" },
                { AssignableArea.CALL_CENTER, "ATENCIÓN A CLIENTES" }
        })
        { }
    }
}