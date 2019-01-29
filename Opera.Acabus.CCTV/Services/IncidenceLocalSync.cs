using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using Opera.Acabus.Cctv.Helpers;
using Opera.Acabus.Cctv.Models;
using Opera.Acabus.Core.Services;
using System;

namespace Opera.Acabus.Cctv.Services
{
    /// <summary>
    /// Esta clase mantiene sincronizada la entidad <see cref="Incidence"/>.
    /// </summary>
    public sealed class IncidenceLocalSync : EntityLocalSyncBase<Incidence>
    {
        /// <summary>
        /// Obtiene el identificador del campo utilizado para el ID de la entidad.
        /// </summary>
        protected override int IDField => 14;

        /// <summary>
        /// Obtiene el nombre del servicio a consumir.
        /// </summary>
        protected override string ModuleName => "Cctv_Service";

        /// <summary>
        /// Obtiene el identificador del campo utilizado para almacenar esta entidad en bytes.
        /// </summary>
        protected override int SourceField => 61;

        /// <summary>
        /// Obtiene una instancia a partir de una secuencia de bytes.
        /// </summary>
        /// <param name="source">Secuencia de bytes origen.</param>
        /// <returns>Una instancia creada desde la secuencia.</returns>
        protected override Incidence FromBytes(byte[] source)
            => CctvDataHelper.GetIncidence(source);

        /// <summary>
        /// Asigna las propiedades de la incidencia en los campos del mensaje requeridos para la
        /// petición de creación.
        /// </summary>
        /// <param name="instance">Instancia a crear.</param>
        /// <param name="message">Mensaje de la petición de creación.</param>
        protected override void InstanceToMessage(Incidence instance, IMessage message)
        {
            message[40] = instance.WhoReporting;
            message[41] = instance.FaultObservations;
            message[14] = instance.Device?.ID ?? 0;
            message[37] = instance.AssignedStaff?.ID ?? 0;
            message[38] = instance.Activity?.ID ?? 0;
            message.SetDateTime(29, instance.StartDate);
            message.SetBoolean(23, instance.LockAssignation);
        }

        /// <summary>
        /// Obtiene una secuencia de bytes a partir de una instancia especificada.
        /// </summary>
        /// <param name="instance">Instancia a serializar.</param>
        /// <returns>Una secuencia de bytes que representan a la instancia.</returns>
        protected override byte[] ToBytes(Incidence instance)
            => instance.Serialize();
    }
}