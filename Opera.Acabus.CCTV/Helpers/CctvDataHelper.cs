using InnSyTech.Standard.Utils;
using Opera.Acabus.Cctv.DataAccess;
using Opera.Acabus.Cctv.Models;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;
using Opera.Acabus.Core.Models.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

/// <summary>
///
/// </summary>
namespace Opera.Acabus.Cctv.Helpers
{
    /// <summary>
    /// Provee de funciones extra a <see cref="Incidence"/> permitiendo determinar si el tiempo te
    /// atención se ha excedido. Requiere de un archivo de configuración con los valores para
    /// &lt;MaxAttentionTime bus="04:00:00" station="02:00:00" /&gt;.
    /// </summary>
    public static class CctvDataHelper
    {
        /// <summary>
        /// Obtiene el tiempo máximo de atención de la incidencias de autobus. De manera
        /// predeterminada son 4hrs de atención.
        /// </summary>
        public static TimeSpan MaxAttentionBus
            => TimeSpan.Parse(AcabusDataContext.ConfigContext["MaxAttentionTime"]?.ToString("bus")
                ?? "04:00:00");

        /// <summary>
        /// Obtiene el tiempo máximo de atención de la incidencias de estaciones. De manera
        /// predeterminada son 2hrs de atención.
        /// </summary>
        public static TimeSpan MaxAttentionStation
            => TimeSpan.Parse(AcabusDataContext.ConfigContext["MaxAttentionTime"]?.ToString("station")
                ?? "02:00:00");

        /// <summary>
        /// Asigna el personal para la atención de la incidencia.
        /// </summary>
        /// <param name="incidence">Incidencia a realizar la asignación.</param>
        /// <returns>Un valor true si la asignación es realizada.</returns>
        public static bool AssignStaff(this Incidence incidence)
            => AssignStaff(incidence, out AssignableStaff staff);

        /// <summary>
        /// Asigna el personal para la atención de la incidencia.
        /// </summary>
        /// <param name="incidence">Incidencia a realizar la asignación.</param>
        /// <param name="staff">El personal asignado a la incidencia.</param>
        /// <returns>Un valor true si la asignación es realizada.</returns>
        public static bool AssignStaff(this Incidence incidence, out AssignableStaff staff)
        {
            List<AssignableStaff> staffList = GetAssignableStaff();
            staff = null;

            if (staffList == null)
                return false;

            incidence.AssignedStaff = GetAssignation(incidence, staffList);
            staff = incidence.AssignedStaff;

            return true;
        }

        /// <summary>
        /// Convierte una secuencia de bytes en una instancia de <see cref="Activity"/>.
        /// </summary>
        /// <param name="source">Secuencia de datos origen.</param>
        public static Activity GetActivity(byte[] source)
        {
            DataHelper.Deserialize(ref source,
                out string createUser,
                out DateTime createTime,
                out string modifyUser,
                out DateTime modifyTime,
                out bool active
                );

            var id = BitConverter.ToUInt64(source, 0);
            var descriptionLenght = BitConverter.ToInt32(source, 8);
            var idCategory = BitConverter.ToUInt64(source, 12 + descriptionLenght);
            var priority = (Priority)BitConverter.ToUInt16(source, 20 + descriptionLenght);
            var assignableArea = (AssignableArea)BitConverter.ToUInt64(source, 22 + descriptionLenght);

            var description = descriptionLenght > 0 ? Encoding.UTF8.GetString(source, 12, descriptionLenght) : String.Empty;
            var category = DataHelper.GetByIDFromDb<ActivityCategory>(idCategory);

            Activity activity = new Activity(id, description)
            {
                Assignable = assignableArea,
                Category = category,
                Priority = priority
            };

            AcabusEntityBase.AssignData(activity, createUser, createTime, modifyUser, modifyTime, active);

            return activity;
        }

        /// <summary>
        /// Convierte una secuencia de bytes en una instancia de <see cref="ActivityCategory"/>.
        /// </summary>
        /// <param name="source">Secuencia de datos origen.</param>
        public static ActivityCategory GetActivityCategory(byte[] source)
        {
            DataHelper.Deserialize(ref source,
                out string createUser,
                out DateTime createTime,
                out string modifyUser,
                out DateTime modifyTime,
                out bool active
                );

            var id = BitConverter.ToUInt64(source, 0);
            var nameLenght = BitConverter.ToInt32(source, 8);
            var type = (DeviceType)BitConverter.ToUInt16(source, 12 + nameLenght);

            var name = nameLenght > 0 ? Encoding.UTF8.GetString(source, 12, nameLenght) : String.Empty;

            ActivityCategory activityCategory = new ActivityCategory(id, type) { Name = name };

            AcabusEntityBase.AssignData(activityCategory, createUser, createTime, modifyUser, modifyTime, active);

            return activityCategory;
        }

        /// <summary>
        /// Obtiene el número de incidencias abiertas asignadas.
        /// </summary>
        /// <param name="staff">Elemento del personal a obtener sus incidencias.</param>
        /// <returns>El número de incidencias asignadas abiertas.</returns>
        public static int GetCountAssignedIncidences(this Staff staff)
            => CctvContext.Incidences.Where(i => i.AssignedStaff.Staff.ID == staff.ID)
                                    .Where(i => i.Status == IncidenceStatus.OPEN || i.Status == IncidenceStatus.RE_OPEN)
                                    .Where(i => i.Active)
                                    .ToList().Count();

        /// <summary>
        /// Convierte una secuencia de bytes con formato válido en una instancia <see cref="Incidence"/>.
        /// </summary>
        /// <param name="source">Secuencia de datos.</param>
        /// <returns>Una instancia de <see cref="Incidence"/>.</returns>
        public static Incidence GetIncidence(byte[] source)
        {
            DataHelper.Deserialize(ref source,
                out string createUser,
                out DateTime createTime,
                out string modifyUser,
                out DateTime modifyTime,
                out bool active
                );

            var id = BitConverter.ToUInt64(source, 0);
            var idActivity = BitConverter.ToUInt64(source, 8);
            var idStaff = BitConverter.ToUInt64(source, 16);
            var commentsLength = BitConverter.ToInt32(source, 24);
            var idDevice = BitConverter.ToUInt64(source, 28 + commentsLength);
            var faultObservationsLength = BitConverter.ToInt32(source, 36 + commentsLength);
            var finishTime = DateTime.FromBinary(BitConverter.ToInt64(source, 40 + commentsLength + faultObservationsLength));
            var lockAssignation = BitConverter.ToBoolean(source, 48 + commentsLength + faultObservationsLength);
            var priority = (Priority)BitConverter.ToUInt16(source, 49 + commentsLength + faultObservationsLength);
            var startDate = DateTime.FromBinary(BitConverter.ToInt64(source, 51 + commentsLength + faultObservationsLength));
            var status = (IncidenceStatus)BitConverter.ToUInt16(source, 59 + commentsLength + faultObservationsLength);
            var idStaffThatResolve = BitConverter.ToUInt64(source, 61 + commentsLength + faultObservationsLength);
            var whoReportingLength = BitConverter.ToInt32(source, 69 + commentsLength + faultObservationsLength);

            var faultObservations = faultObservationsLength > 0
                ? Encoding.UTF8.GetString(source, 40 + commentsLength, faultObservationsLength)
                : String.Empty;

            var comments = commentsLength > 0
                ? Encoding.UTF8.GetString(source, 28, commentsLength)
                : String.Empty;

            var whoReporting = whoReportingLength > 0
                ? Encoding.UTF8.GetString(source, 73 + commentsLength + faultObservationsLength, whoReportingLength)
                : String.Empty;

            Activity activity = DataHelper.GetByIDFromDb<Activity>(idActivity);
            AssignableStaff assignableStaff = DataHelper.GetByIDFromDb<AssignableStaff>(idStaff);
            Device device = DataHelper.GetByIDFromDb<Device>(idDevice);
            Staff staff = DataHelper.GetByIDFromDb<Staff>(idStaffThatResolve);

            Incidence incidence = new Incidence(id, status)
            {
                Activity = activity,
                AssignedStaff = assignableStaff,
                Comments = comments,
                Device = device,
                FaultObservations = faultObservations,
                FinishDate = finishTime,
                LockAssignation = lockAssignation,
                Priority = priority,
                StaffThatResolve = staff,
                StartDate = startDate,
                Status = status,
                WhoReporting = whoReporting
            };

            AcabusEntityBase.AssignData(incidence, createUser, createTime, modifyUser, modifyTime, active);

            return incidence;
        }

        /// <summary>
        /// Obtiene el tiempo restante para atención oportuna de la incidencia.
        /// </summary>
        /// <param name="incidence">Incidencia a calcular el tiempo restante.</param>
        /// <returns>Tiempo restante de atención.</returns>
        public static TimeSpan GetTimeLeft(this Incidence incidence)
        {
            var maxAttentionTime = TimeSpan.Zero;

            if (incidence?.Device?.Bus != null)
                maxAttentionTime = MaxAttentionBus;

            if (incidence?.Device?.Station != null)
                maxAttentionTime = MaxAttentionStation;

            return maxAttentionTime - (DateTime.Now - incidence.StartDate);
        }

        /// <summary>
        /// Obtiene una instancia de seguimiento de una secuencia de bytes.
        /// </summary>
        /// <param name="source">Secuencia de bytes origen.</param>
        /// <returns>Una instancia de TrackIncidence.</returns>
        public static TrackIncidence GetTrack(byte[] source)
        {
            DataHelper.Deserialize(ref source,
                out string createUser,
                out DateTime createTime,
                out string modifyUser,
                out DateTime modifyTime,
                out bool active
                );

            var id = BitConverter.ToUInt64(source, 0);
            var commentsLength = BitConverter.ToInt32(source, 8);
            var faultObservationsLength = BitConverter.ToInt32(source, 12 + commentsLength);
            var finishTime = DateTime.FromBinary(BitConverter.ToInt64(source, 16 + commentsLength + faultObservationsLength));
            var startDate = DateTime.FromBinary(BitConverter.ToInt64(source, 24 + commentsLength + faultObservationsLength));
            var idStaffThatResolve = BitConverter.ToUInt64(source, 32 + commentsLength + faultObservationsLength);
            var whoReportingLength = BitConverter.ToInt32(source, 40 + commentsLength + faultObservationsLength);
            var idIncidence = BitConverter.ToUInt64(source, 44);

            var faultObservations = faultObservationsLength > 0
                ? Encoding.UTF8.GetString(source, 16 + commentsLength, faultObservationsLength)
                : String.Empty;

            var comments = commentsLength > 0
                ? Encoding.UTF8.GetString(source, 12, commentsLength)
                : String.Empty;

            var whoReporting = whoReportingLength > 0
                ? Encoding.UTF8.GetString(source, 40 + commentsLength + faultObservationsLength, whoReportingLength)
                : String.Empty;

            Staff staff = DataHelper.GetByIDFromDb<Staff>(idStaffThatResolve);
            Incidence incidence = DataHelper.GetByIDFromDb<Incidence>(idIncidence);

            TrackIncidence track = new TrackIncidence(id, incidence)
            {
                Comments = comments,
                FaultObservations = faultObservations,
                FinishDate = finishTime,
                StaffThatResolve = staff,
                StartDate = startDate,
                WhoReporting = whoReporting
            };

            AcabusEntityBase.AssignData(track, createUser, createTime, modifyUser, modifyTime, active);

            return track;
        }

        /// <summary>
        /// Obtiene un valor que indica si el tiempo de atención de la incidencia ha expirado.
        /// </summary>
        /// <param name="incidence">Incidencia a determinar si ha expirado.</param>
        /// <returns>Un valor true si la incidencia ha expirado.</returns>
        public static bool IsExpired(this Incidence incidence)
            => incidence.GetTimeLeft() <= TimeSpan.Zero;

        /// <summary>
        /// Obtiene un valor que indica si la incidencia requiere reasignación.
        /// </summary>
        /// <param name="incidence">Incidencia a determinar si requiere reasignación.</param>
        /// <returns>Un valor true si se requiere reasignación.</returns>
        public static bool RequireReassign(this Incidence incidence)
            => incidence.AssignedStaff == GetAssignation(incidence, GetAssignableStaff(), false);

        /// <summary>
        /// Obtiene una secuencia de datos que representa a la instancia de <see cref="RefundOfMoney"/>.
        /// </summary>
        /// <param name="refund">Instancia a serializar.</param>
        /// <returns>Una secuencia de bytes resultado de la serialización.</returns>
        public static Byte[] Serialize(this RefundOfMoney refund)
        {
            var bID = BitConverter.GetBytes(refund.ID);
            var bAmount = BitConverter.GetBytes(refund.Amount);
            var bRefundDate = BitConverter.GetBytes(refund.RefundDate?.ToBinary() ?? 0);
            var bRefundStatus = BitConverter.GetBytes((UInt16)refund.Status);
            var bIDIndicidence = BitConverter.GetBytes(refund.Incidence?.ID ?? 0);
            var bIDCashDestiny = BitConverter.GetBytes(refund.CashDestiny?.ID ?? 0);

            var bEntity = DataHelper.Serialize(refund);

            return new[]
            {
                bEntity,
                bID,
                bAmount,
                bRefundDate,
                bRefundStatus,
                bIDIndicidence,
                bIDCashDestiny
            }.Merge().ToArray();
        }

        /// <summary>
        /// Convierte una instancia de TrackIncidence en una secuencia de bytes.
        /// </summary>
        /// <param name="track">Incidencia a utilizar.</param>
        /// <returns>Una secuencia de bytes.</returns>
        public static Byte[] Serialize(this TrackIncidence track)
        {
            var bID = BitConverter.GetBytes(track.ID);
            var bComments = Encoding.UTF8.GetBytes(track.Comments);
            var bCommentsLength = BitConverter.GetBytes(bComments.Length);
            var bFaultObservations = Encoding.UTF8.GetBytes(track.FaultObservations);
            var bFaultObservationsLength = BitConverter.GetBytes(bFaultObservations.Length);
            var bFinishTime = BitConverter.GetBytes(track.FinishDate?.ToBinary() ?? 0);
            var bStartDate = BitConverter.GetBytes(track.StartDate.ToBinary());
            var bIDStaffThatResolve = BitConverter.GetBytes(track.StaffThatResolve?.ID ?? 0);
            var bWhoReporting = Encoding.UTF8.GetBytes(track.WhoReporting);
            var bWhoReportingLength = BitConverter.GetBytes(bWhoReporting.Length);
            var bIDIncidence = BitConverter.GetBytes(track.Incidence?.ID ?? 0);

            var bEntity = DataHelper.Serialize(track);

            return new[]
            {
                bID, bCommentsLength, bComments, bFaultObservationsLength,
                bFaultObservations, bFinishTime, bStartDate, bIDStaffThatResolve,
                bWhoReportingLength, bWhoReporting, bIDIncidence
            }.Merge().ToArray();
        }

        /// <summary>
        /// Obtiene una secuencia de datos que representa a la instancia de <see cref="AssignableStaff"/>.
        /// </summary>
        /// <param name="staff">Instancia a serializar.</param>
        /// <returns>Una secuencia de bytes resultado de la serialización.</returns>
        public static Byte[] Serialize(this AssignableStaff staff)
        {
            var bID = BitConverter.GetBytes(staff.ID);
            var bHasKvrKey = BitConverter.GetBytes(staff.HasKvrKey);
            var bHasNemaKey = BitConverter.GetBytes(staff.HasNemaKey);
            var bAssignedSection = Encoding.UTF8.GetBytes(staff.AssignedSection);
            var bAssignedSectionLenght = BitConverter.GetBytes(bAssignedSection.Length);
            var bIDStaff = BitConverter.GetBytes(staff.Staff?.ID ?? 0);

            var bEntity = DataHelper.Serialize(staff);

            return new[]
            {
                bEntity,
                bID,
                bHasKvrKey,
                bHasNemaKey,
                bAssignedSectionLenght,
                bAssignedSection,
                bIDStaff
            }.Merge().ToArray();
        }

        /// <summary>
        /// Obtiene una secuencia de datos que representa a la instancia de <see cref="CashDestiny"/>.
        /// </summary>
        /// <param name="destiny">Instancia a serializar.</param>
        /// <returns>Una secuencia de bytes resultado de la serialización.</returns>
        public static Byte[] Serialize(this CashDestiny destiny)
        {
            var bID = BitConverter.GetBytes(destiny.ID);
            var bDescription = Encoding.UTF8.GetBytes(destiny.Description);
            var bType = BitConverter.GetBytes((UInt16)destiny.CashType);
            var bReqMove = BitConverter.GetBytes(destiny.RequiresMoveToCAU);

            var bDescriptionLenght = BitConverter.GetBytes(bDescription.Length);

            var bEntity = DataHelper.Serialize(destiny);

            return new[]
            {
                bEntity,
                bID,
                bDescriptionLenght,
                bDescription,
                bType,
                bReqMove
            }.Merge().ToArray();
        }

        /// <summary>
        /// Obtiene una secuencia de datos que representa a la instancia de <see cref="ActivityCategory"/>.
        /// </summary>
        /// <param name="category">Instancia a serializar.</param>
        /// <returns>Una secuencia de bytes resultado de la serialización.</returns>
        public static Byte[] Serialize(this ActivityCategory category)
        {
            var bID = BitConverter.GetBytes(category.ID);
            var bName = Encoding.UTF8.GetBytes(category.Name);
            var bNameLenght = BitConverter.GetBytes(bName.Length);
            var bType = BitConverter.GetBytes((UInt16)category.DeviceType);

            var bEntity = DataHelper.Serialize(category);

            return new[]
            {
                bEntity,
                bID,
                bNameLenght,
                bName,
                bType
            }.Merge().ToArray();
        }

        /// <summary>
        /// Obtiene una secuencia de datos que representa a la instancia de <see cref="Activity"/>.
        /// </summary>
        /// <param name="activity">Instancia a serializar.</param>
        /// <returns>Una secuencia de bytes que representa a la instancia.</returns>
        public static Byte[] Serialize(this Activity activity)
        {
            var bID = BitConverter.GetBytes(activity.ID);
            var bDescription = Encoding.UTF8.GetBytes(activity.Description);
            var bDescriptionLenght = BitConverter.GetBytes(bDescription.Length);
            var bCategory = BitConverter.GetBytes(activity.Category?.ID ?? 0);
            var bPriority = BitConverter.GetBytes((UInt16)activity.Priority);
            var bAssignable = BitConverter.GetBytes((UInt16)activity.Assignable);

            var bEntity = DataHelper.Serialize(activity);

            return new[] {
                bEntity,
                bID,
                bDescriptionLenght,
                bDescription,
                bCategory,
                bPriority,
                bAssignable
            }.Merge().ToArray();
        }

        /// <summary>
        /// Obtiene una secuencia de datos que representa a la instancia de <see cref="Incidence"/>.
        /// </summary>
        /// <param name="incidence">Instancia a serializar.</param>
        /// <returns>Una secuencia de bytes que representa a la instancia.</returns>
        public static Byte[] Serialize(this Incidence incidence)
        {
            var bID = BitConverter.GetBytes(incidence.ID);
            var bIDActivity = BitConverter.GetBytes(incidence.Activity?.ID ?? 0);
            var bIDStaff = BitConverter.GetBytes(incidence.AssignedStaff?.ID ?? 0);
            var bComments = Encoding.UTF8.GetBytes(incidence.Comments);
            var bCommentsLength = BitConverter.GetBytes(bComments.Length);
            var bIDDevice = BitConverter.GetBytes(incidence.Device?.ID ?? 0);
            var bFaultObservations = Encoding.UTF8.GetBytes(incidence.FaultObservations);
            var bFaultObservationsLength = BitConverter.GetBytes(bFaultObservations.Length);
            var bFinishTime = BitConverter.GetBytes(incidence.FinishDate?.ToBinary() ?? 0);
            var bLockAssignation = BitConverter.GetBytes(incidence.LockAssignation);
            var bPriority = BitConverter.GetBytes((UInt16)incidence.Priority);
            var bStartDate = BitConverter.GetBytes(incidence.StartDate.ToBinary());
            var bStatus = BitConverter.GetBytes((UInt16)incidence.Status);
            var bIDStaffThatResolve = BitConverter.GetBytes(incidence.StaffThatResolve?.ID ?? 0);
            var bWhoReporting = Encoding.UTF8.GetBytes(incidence.WhoReporting);
            var bWhoReportingLength = BitConverter.GetBytes(bWhoReporting.Length);

            var bEntity = DataHelper.Serialize(incidence);

            return new[] { bEntity, bID, bIDActivity, bIDStaff, bCommentsLength, bComments, bIDDevice, bFaultObservationsLength,
                bFaultObservations, bFinishTime, bLockAssignation, bPriority, bStartDate, bStatus, bIDStaffThatResolve,
                bWhoReportingLength, bWhoReporting }.Merge().ToArray();
        }

        /// <summary>
        /// Copia a portapapeles del sistema operativo todas las incidencias de la secuencia especificada.
        /// </summary>
        /// <param name="incidences">Secuencia que contiene las incidencias a copiar al portapapeles.</param>
        public static void ToClipboard(this IEnumerable<Incidence> incidences)
        {
            try
            {
                if (incidences.Count() == 0) return;

                StringBuilder openedIncidence = new StringBuilder();
                foreach (var assignedStaffIncidences in incidences.GroupBy(i => i?.AssignedStaff))
                {
                    foreach (Incidence incidence in assignedStaffIncidences)
                        openedIncidence.AppendLine(incidence.ToStringForChat().Split('\n')?[0]
                            + (String.IsNullOrEmpty(incidence.FaultObservations)
                                ? String.Empty
                                : String.Format("\n*Observaciones:* {0}", incidence.FaultObservations)));
                    if (assignedStaffIncidences.Key?.Staff != null)
                        openedIncidence.AppendFormat("*Asignado:* {0}", assignedStaffIncidences.Key.Staff);
                    openedIncidence.AppendLine();
                    openedIncidence.AppendLine();
                }

                Clipboard.Clear();
                Clipboard.SetDataObject(openedIncidence.ToString().Trim());
            }
            catch (ExternalException ex)
            {
                Trace.WriteLine(ex.PrintMessage().JoinLines(), "ERROR");
            }
        }

        /// <summary>
        /// Copia la incidencia a portapapeles del sistema operativo, mostrando un mensaje similar
        /// </summary>
        /// <param name="incidence">Incidencia a copiar a portapapeles.</param>
        public static void ToClipboard(this Incidence incidence)
        {
            if (incidence != null)
                ToClipboard(new[] { incidence });
        }

        /// <summary>
        /// Obtiene la lista del personal asignable a las incidencias.
        /// </summary>
        /// <returns>Una lista del personal asignable.</returns>
        private static List<AssignableStaff> GetAssignableStaff()
        {
            List<AssignableStaff> staffList = null;

            if (AcabusDataContext.GetService("Assistance_Manager", out dynamic assistanceService))
                try
                {
                    staffList = assistanceService.GetStaffInWork();
                }
                catch (Exception) { }

            if (staffList == null)
                staffList = CctvContext.GetStaticAssignableStaff();

            return staffList;
        }

        /// <summary>
        /// Determina la asignación del personal para la incidencia especificada.
        /// </summary>
        /// <param name="incidence">Incidencia a determinar la asignación.</param>
        /// <param name="staffList">Personal disponible para la asignación</param>
        /// <param name="assignByLoad">Indica si se asigna por carga de trabajo.</param>
        /// <returns>El Personal asignado a la asistencia.</returns>
        private static AssignableStaff GetAssignation(Incidence incidence, List<AssignableStaff> staffList,
            bool assignByLoad = true)
        {
            if (incidence == null) return null;
            if (staffList == null || staffList.Count == 0) return null;

            if (incidence.LockAssignation && incidence.AssignedStaff != null)
                return incidence.AssignedStaff;

            if (incidence.Status != IncidenceStatus.OPEN)
                throw new ArgumentException("La incidencia con estado distinto a abierta no pueden ser asignadas.");

            // Asignación por area de trabajo.
            var assignableArea = incidence.Activity?.Assignable;
            var filtered = staffList.Where(s => s.Staff.Area == (s.Staff.Area & assignableArea));

            if (filtered.Count() == 0)
                return null;

            if (filtered.Count() == 1)
                return filtered.First();

            // Asignación por sección
            var lastFiltered = filtered;
            filtered = filtered.Where(s => s.AssignedSection == incidence.Device?.Station?.AssignedSection
                                          || s.AssignedSection == incidence.Device?.Bus?.Route?.AssignedSection);

            if (filtered.Count() == 1)
                return filtered.First();

            if (filtered.Count() == 0)
                filtered = lastFiltered;

            // Asignación por llaves
            lastFiltered = filtered;
            filtered = filtered.Where(s => (s.HasKvrKey && incidence.Device?.Station != null)
                                        || (s.HasNemaKey && incidence.Device?.Bus != null));

            if (filtered.Count() == 1)
                return filtered.First();

            if (filtered.Count() == 0)
                filtered = lastFiltered;

            if (!assignByLoad)
                return null;

            // Asignación por carga
            lastFiltered = filtered;
            return filtered.Aggregate((s1, s2)
                => s1.Staff.GetCountAssignedIncidences() < s2.Staff.GetCountAssignedIncidences()
                ? s1
                : s2);
        }
    }
}