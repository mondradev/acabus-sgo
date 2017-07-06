using InnSyTech.Standard.Mvvm.Utils;
using Opera.Acabus.Core.Models;
using System.Collections.Generic;

namespace Opera.Acabus.Core.Converters
{
    /// <summary>
    /// Esta clase define un convertidor de valores para la enumeración <see cref="BusStatus"/>.
    /// </summary>
    public sealed class BusStatusSpanishConverter : TranslateEnumConverter<BusStatus>
    {
        /// <summary>
        /// Crea una nueva instancia de <see cref="BusTypeSpanishConverter"/>.
        /// </summary>
        public BusStatusSpanishConverter()
            : base(new Dictionary<BusStatus, string>() {
            { BusStatus.IN_REPAIR, "EN TALLER" },
            { BusStatus.WITHOUT_ENERGY, "SIN ENERGÍA" },
            { BusStatus.OTHERS_REASONS, "OTRAS RAZONES" },
            { BusStatus.OPERATIONAL, "OPERANDO" }
        })
        { }
    }
}