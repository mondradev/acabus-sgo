using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using Opera.Acabus.Cctv.DataAccess;
using Opera.Acabus.Cctv.Helpers;
using Opera.Acabus.Cctv.Models;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Models;
using Opera.Acabus.Core.Models.Base;
using Opera.Acabus.Core.Services;
using Opera.Acabus.Server.Core;
using Opera.Acabus.Server.Core.Utils;
using System;
using System.Linq;

namespace Opera.Acabus.Cctv.Service.Server
{
    /// <summary>
    /// Esta clase ofrece funciones que permite crear, leer, actualizar o eliminar incidencias dentro
    /// del servidor y notificar a los clientes conectados.
    /// </summary>
    internal static class IncidenceService
    {
        /// <summary>
        /// Crea una nueva incidencia especificando sus atributos iniciales.
        /// </summary>
        /// <param name="whoReporting">El nombre o descripción de la entidad que reporta la incidencia o actividad.</param>
        /// <param name="startTime">Fecha/hora de la apertura de la incidencia.</param>
        /// <param name="faultObservations">Observaciones de la apertura de la incidencia.</param>
        /// <param name="idDevice">Identificador del equipo donde ocurre la incidencia.</param>
        /// <param name="idAssignedStaff">Identificador del personal asignado para la resolución de la incidencia.</param>
        /// <param name="idActivity">Identificador de la actividad/falla reportada en la incidencia.</param>
        /// <param name="lockAssignation">Indica si la asignación de la incidencia no puede ser cambiada.</param>
        /// <param name="message">Mensaje de la petición de creación.</param>
        public static void CreateIncidence(
                String whoReporting,
                DateTime startTime,
                String faultObservations,
                UInt64 idDevice,
                UInt64 idAssignedStaff,
                UInt64 idActivity,
                Boolean lockAssignation,
                IMessage message
            )
        {
            Device device = idDevice == 0 ? null : AcabusDataContext.AllDevices.FirstOrDefault(x => x.ID == idDevice);
            AssignableStaff assignableStaff = idAssignedStaff == 0 ? null : CctvContext.Staff.FirstOrDefault(x => x.ID == idAssignedStaff);
            Activity activity = idActivity == 0 ? null : CctvContext.Activities.FirstOrDefault(x => x.ID == idActivity);

            Incidence incidence = new Incidence(0, IncidenceStatus.OPEN)
            {
                WhoReporting = whoReporting,
                StartDate = startTime,
                FaultObservations = faultObservations,
                Priority = Priority.LOW,
                Device = device,
                AssignedStaff = assignableStaff,
                Activity = activity,
                LockAssignation = lockAssignation
            };

            bool res = AcabusDataContext.DbContext.Create(incidence);

            if (res)
                ServerNotify.Notify(new PushAcabus(nameof(Incidence), incidence.ID, LocalSyncOperation.CREATE));

            message.SetBoolean(22, res);
            message[61] = incidence.Serialize();

            if (res)
                Dispatcher.SendNotify("CCTV: Nueva incidencia " + incidence);
        }

        /// <summary>
        /// Elimina la incidencia especificada por el ID.
        /// </summary>
        /// <param name="id">ID de la incidencia a eliminar.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void DeleteIncidence(UInt64 id, IMessage message)
        {
            try
            {
                Incidence incidence = CctvContext.Incidences.FirstOrDefault(x => x.ID == id);
                incidence.Delete();

                bool deleted = AcabusDataContext.DbContext.Update(incidence);

                if (!deleted)
                    throw new InvalidOperationException(
                        "Existen entidades vinculadas a la incidencia que intenta eliminar.");

                message.SetBoolean(22, deleted);

                if (deleted)
                    ServerNotify.Notify(new PushAcabus(nameof(Incidence), id, LocalSyncOperation.DELETE));
            }
            catch (Exception ex)
            {
                message[AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()] = 400;
                message[AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32()] = ex.Message;
                message.SetBoolean(22, false);
            }
        }

        /// <summary>
        /// Obtiene las incidencias.
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void DownloadIncidence(DateTime? lastDownLoad, IMessage message)
        {
            if (lastDownLoad == null)
                lastDownLoad = DateTime.MinValue;

            IQueryable<Incidence> incidenceQuery = CctvContext.Incidences
                .Where(x => x.ModifyTime > lastDownLoad)
                .OrderBy(x => x.ModifyTime);

            ServerHelper.Enumerating(message, incidenceQuery.ToList().Count,
                  i => message[61] = incidenceQuery.ToList()[i].Serialize());
        }

        /// <summary>
        /// Obtiene la incidencia con el ID especificado.
        /// </summary>
        /// <param name="id">Identificador de la incidencia.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetIncidenceByID(UInt64 id, IMessage message)
        {
            Route route = AcabusDataContext.AllRoutes.FirstOrDefault(x => x.ID == id);
            message[61] = route.Serialize();
        }

        /// <summary>
        /// Actualiza la incidencia especificada por el ID.
        /// </summary>
        /// <param name="id">Identificador de la incidencia.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void UpdateIncidence(IMessage message)
        {
            try
            {
                Incidence incidence = CctvDataHelper.GetIncidence(message.GetBytes(61));
                incidence.Commit();

                bool updated = AcabusDataContext.DbContext.Update(incidence);

                if (!updated)
                    throw new InvalidOperationException("No se logró actualizar el equipo, verifique las relaciones.");

                message.SetBoolean(22, updated);

                if (updated)
                    ServerNotify.Notify(new PushAcabus(nameof(Incidence), incidence.ID, LocalSyncOperation.UPDATE));
            }
            catch (Exception ex)
            {
                message[AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()] = 400;
                message[AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32()] = ex.Message;
                message.SetBoolean(22, false);
            }
        }
    }
}