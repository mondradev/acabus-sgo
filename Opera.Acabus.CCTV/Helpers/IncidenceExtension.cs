using InnSyTech.Standard.Utils;
using Opera.Acabus.Cctv.Models;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Opera.Acabus.Cctv.Helpers
{
    /// <summary>
    /// Provee de funciones extra a <see cref="Incidence"/> permitiendo determinar si el tiempo te
    /// atención se ha excedido. Requiere de un archivo de configuración con los valores para
    /// &lt;MaxAttentionTime bus="04:00:00" station="02:00:00" /&gt;.
    /// </summary>
    public static class IncidenceExtension
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

            incidence.AssignedStaff = GetAssination(incidence, staffList);
            staff = incidence.AssignedStaff;

            return true;
        }

        /// <summary>
        /// Obtiene el número de incidencias abiertas asignadas.
        /// </summary>
        /// <param name="staff">Elemento del personal a obtener sus incidencias.</param>
        /// <returns>El número de incidencias asignadas abiertas o -1 en caso de no tener acceso al módulo.</returns>
        public static int GetCountAssignedIncidences(this Staff staff)
        {
            if (AcabusDataContext.GetService("Cctv_Manager", out dynamic cctvManager))
                try
                {
                    IEnumerable<Incidence> openIncidences = cctvManager.OpenIncidences;
                    openIncidences = openIncidences.Where(i => i.AssignedStaff.Staff == staff);

                    return openIncidences.Count();
                }
                catch (MethodAccessException) { }
            return -1;
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
            => incidence.AssignedStaff == GetAssination(incidence, GetAssignableStaff(), false);

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
                        openedIncidence.AppendLine(incidence.ToReportString().Split('\n')?[0]
                            + (String.IsNullOrEmpty(incidence.Observations)
                                ? String.Empty
                                : String.Format("\n*Observaciones:* {0}", incidence.Observations)));
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
            {
                if (AcabusDataContext.GetService("Cctv_Manager", out dynamic staticAssignationService))
                    try
                    {
                        staffList = staticAssignationService.GetStaff();
                    }
                    catch (Exception) { }
            }

            return staffList;
        }

        /// <summary>
        /// Determina la asignación del personal para la incidencia especificada.
        /// </summary>
        /// <param name="incidence">Incidencia a determinar la asignación.</param>
        /// <param name="staffList">Personal disponible para la asignación</param>
        /// <param name="assignByLoad">Indica si se asigna por carga de trabajo.</param>
        /// <returns>El Personal asignado a la asistencia.</returns>
        private static AssignableStaff GetAssination(Incidence incidence, List<AssignableStaff> staffList, bool assignByLoad = true)
        {
            if (incidence.LockAssignation)
                return incidence.AssignedStaff;

            if (incidence.Status != IncidenceStatus.OPEN)
                throw new ArgumentException("La incidencia con estado distinto a abierta no pueden ser asignadas.");

            if (incidence == null) return null;
            if (staffList == null || staffList.Count == 0) return null;

            // Asignación por area de trabajo.
            var assignableArea = incidence.Fault?.Assignable;
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