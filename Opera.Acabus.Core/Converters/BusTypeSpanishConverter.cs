using InnSyTech.Standard.Mvvm.Utils;
using Opera.Acabus.Core.Models;
using System.Collections.Generic;

namespace Opera.Acabus.Core.Converters
{
    /// <summary>
    /// Esta clase define un convertidor de valores para la enumaración <see cref="BusType"/>.
    /// </summary>
    public sealed class BusTypeSpanishConverter : TranslateEnumConverter<BusType>
    {
        /// <summary>
        /// Crea una nueva instancia de <see cref="BusTypeSpanishConverter"/>.
        /// </summary>
        public BusTypeSpanishConverter()
            : base(new Dictionary<BusType, string>() {
            { BusType.ARTICULATED, "ARTICULADO" },
            { BusType.STANDARD, "PADRÓN" },
            { BusType.CONVENTIONAL, "CONVENCIONAL" },
            { BusType.NONE, "NINGUNO" }
        })
        { }
    }
}