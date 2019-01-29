using InnSyTech.Standard.Translates;
using System.Collections.Generic;

namespace Opera.Acabus.Cctv.Models
{
    /// <summary>
    /// Representa un traductor al idioma español de la enumeración <see cref="IncidenceStatus"/>.
    /// </summary>
    public sealed class IncidenceStatusTranslator : EnumTranslator<IncidenceStatus>
    {
        /// <summary>
        /// Crea una nueva instancia del traductor.
        /// </summary>
        public IncidenceStatusTranslator() : base(new Dictionary<IncidenceStatus, string>()
        {
            { IncidenceStatus.OPEN, "ABIERTA" },
            { IncidenceStatus.CLOSE, "CERRADA" },
            { IncidenceStatus.UNCOMMIT, "POR CONFIRMAR" },
            { IncidenceStatus.PENDING, "PENDIENTE" },
            { IncidenceStatus.REFUND_IN_TRANSIT, "DINERO EN TRÁNSITO" },
            { IncidenceStatus.RE_OPEN, "RE-ABIERTA" }
        })
        { }
    }
}