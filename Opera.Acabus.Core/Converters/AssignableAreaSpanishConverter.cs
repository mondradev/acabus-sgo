using InnSyTech.Standard.Mvvm.Utils;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opera.Acabus.Core.Converters
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
                { AssignableArea.MANTTO_SUPERVISOR, "MANTENIMIENTO Y SUPERVISOR" },
                { AssignableArea.SUPERVISOR, "SUPERVISOR" },
                { AssignableArea.SUPERVISOR_SUPPORT, "SUPERVISOR Y SOPORTE" },
                { AssignableArea.SUPPORT, "SOPORTE TÉCNICO" },
                { AssignableArea.SUPPORT_DATABASE, "SOPORTE Y BASE DE DATOS" },
                { AssignableArea.DATABASE, "ANALISTA ADMINISTRADOR DE BASE DE DATOS" },
                { AssignableArea.DATABASE_IT_MANAGER, "BASE DE DATOS Y GERENCIA TI" },
                { AssignableArea.IT_MANAGER, "GERENCIA TI" }
        })
        { }
    }
}
