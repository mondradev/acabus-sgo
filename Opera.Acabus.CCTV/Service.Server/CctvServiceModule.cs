using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using Opera.Acabus.Cctv.Models;
using Opera.Acabus.Server.Core.Gui;
using Opera.Acabus.Server.Core.Utils;
using System;
using System.Reflection;

namespace Opera.Acabus.Cctv.Service.Server
{
    /// <summary>
    /// Provee de todos los servicios requeridos por el módulo de Cctv y dependientes. Permite la
    /// gestión de incidencias en los equipos así como la alta de devoluciones de dinero.
    /// </summary>
    public sealed class CctvServiceModule : ServiceModuleBase
    {
        /// <summary>
        /// Obtiene el nombre del servicio.
        /// </summary>
        public override string ServiceName => "Cctv_Service";
        
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
           [ParameterField(40)] String whoReporting,
           [ParameterField(29)] DateTime startTime,
           [ParameterField(41, true)] String faultObservations,
           [ParameterField(14)] UInt64 idDevice,
           [ParameterField(37)] UInt64 idAssignedStaff,
           [ParameterField(38)] UInt64 idActivity,
           [ParameterField(23)] Boolean lockAssignation,
            IMessage message
            )
            => IncidenceService.CreateIncidence(whoReporting, startTime, faultObservations, idDevice,
                idAssignedStaff, idActivity, lockAssignation, message);

        /// <summary>
        /// Elimina la incidencia especificada por el ID.
        /// </summary>
        /// <param name="id">ID de la incidencia a eliminar.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void DeleteIncidence([ParameterField(14)]  UInt64 id, IMessage message)
            => IncidenceService.DeleteIncidence(id, message);

        /// <summary>
        /// Obtiene las incidencias.
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void DownloadIncidence([ParameterField(25, true)] DateTime? lastDownLoad, IMessage message)
            => IncidenceService.DownloadIncidence(lastDownLoad, message);

        /// <summary>
        /// Obtiene la incidencia con el ID especificado.
        /// </summary>
        /// <param name="id">Identificador de la incidencia.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetIncidenceByID([ParameterField(14)] UInt64 id, IMessage message)
            => IncidenceService.GetIncidenceByID(id, message);

        /// <summary>
        /// Actualiza la incidencia especificada por el ID.
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void UpdateIncidence(IMessage message)
            => IncidenceService.UpdateIncidence(message);

        /// <summary>
        /// Crea un seguimiento de incidencia especificando sus atributos iniciales.
        /// </summary>
        /// 
        /// <param name="message">Mensaje de la petición de creación.</param>
        public static void CreateTrackIncidence([ParameterField(61)] Incidence incidenceToReOpen, IMessage message)
            => TrackIncidenceService.CreateIncidence(incidenceToReOpen, message);

        /// <summary>
        /// Elimina la incidencia especificada por el ID.
        /// </summary>
        /// <param name="id">ID de la incidencia a eliminar.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void DeleteTrackIncidence([ParameterField(14)]  UInt64 id, IMessage message)
            => TrackIncidenceService.DeleteIncidence(id, message);

        /// <summary>
        /// Obtiene las incidencias.
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void DownloadTrackIncidence([ParameterField(25, true)] DateTime? lastDownLoad, IMessage message)
            => TrackIncidenceService.DownloadIncidence(lastDownLoad, message);

        /// <summary>
        /// Obtiene la incidencia con el ID especificado.
        /// </summary>
        /// <param name="id">Identificador de la incidencia.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetTrackIncidenceByID([ParameterField(14)] UInt64 id, IMessage message)
            => TrackIncidenceService.GetIncidenceByID(id, message);

        /// <summary>
        /// Actualiza la incidencia especificada por el ID.
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void UpdateTrackIncidence(IMessage message)
            => TrackIncidenceService.UpdateIncidence(message);
    }
}