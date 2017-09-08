using InnSyTech.Standard.Mvvm.Utils;
using Opera.Acabus.Core.Models;
using System.Collections.Generic;

namespace Opera.Acabus.Core.Gui.Converters
{
    /// <summary>
    /// Esta clase define un convertidor de valores para la enumeración <see cref="RouteType"/>.
    /// </summary>
    public sealed class RouteTypeSpanishConverter : TranslateEnumConverter<RouteType>
    {
        /// <summary>
        /// Crea una instancia nueva de <see cref="RouteTypeSpanishConverter"/>.
        /// </summary>
        public RouteTypeSpanishConverter()
            : base(new Dictionary<RouteType, string>() {
                { RouteType.NONE, "NINGUNA" },
                { RouteType.ALIM, "ALIMENTADORA" },
                { RouteType.TRUNK, "TRONCAL" }
        })
        { }
    }
}