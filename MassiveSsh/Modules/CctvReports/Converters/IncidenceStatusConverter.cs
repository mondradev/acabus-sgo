using Acabus.Converters;
using Acabus.Modules.CctvReports.Models;
using System.Collections.Generic;

namespace Acabus.Modules.CctvReports.Converters
{
    /// <summary>
    /// Convertidor para la traducción de la enumeración <see cref="IncidenceStatus"/>.
    /// </summary>
    public sealed class IncidenceStatusConverter : TranslateEnumConverter<IncidenceStatus>
    {
        /// <summary>
        /// Crea una instancia del traductor de la enumaración <see cref="IncidenceStatus"/>.
        /// </summary>
        public IncidenceStatusConverter() : base(new Dictionary<IncidenceStatus, string>()
        {
            { IncidenceStatus.OPEN, "ABIERTA" },
            { IncidenceStatus.CLOSE, "CERRADA" },
            { IncidenceStatus.UNCOMMIT, "POR CONFIRMAR" }
        })
        { }
    }
}