using Acabus.Models;
using System.Collections.Generic;

namespace Acabus.Converters
{
    /// <summary>
    /// Convertidor para la traducción de la enumeración <see cref="VehicleStatus"/>.
    /// </summary>
    public sealed class VehicleStatusConverter : TranslateEnumConverter<VehicleStatus>
    {
        /// <summary>
        /// Crea una instancia del traductor de <see cref="VehicleStatus"/>.
        /// </summary>
        public VehicleStatusConverter() : base(new Dictionary<VehicleStatus, string>() {
            { VehicleStatus.IN_REPAIR, "EN TALLER" },
            { VehicleStatus.WITHOUT_ENERGY, "SIN ENERGÍA" },
            { VehicleStatus.UNKNOWN, "DESCONOCIDO" }
        })
        { }
    }
}
