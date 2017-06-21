using Acabus.DataAccess;
using Acabus.Modules.Attendances.Models;
using Acabus.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using static Acabus.Modules.Attendances.Models.Attendance;

namespace Acabus.Modules.Attendances.Services
{
    public static class AttendanceService
    {
        public static void CountIncidences(Attendance technician)
            => technician.CountAssignedIncidences();

        public static WorkShift GetTurn(DateTime startTime)
        {
            if (startTime.TimeOfDay.Between(TimeSpan.FromHours(6), new TimeSpan(13, 59, 59)))
                return WorkShift.MONING_SHIFT;

            if (startTime.TimeOfDay.Between(TimeSpan.FromHours(14), new TimeSpan(21, 59, 59)))
                return WorkShift.AFTERNOON_SHIFT;

            return WorkShift.NIGHT_SHIFT;
        }

        public static void LoadFromDataBase(this ICollection<Attendance> attendances)
        {
            foreach (var attendancesData in AcabusData.Session.GetObjects<Attendance>()
                .Where(attendance => (attendance as Attendance).DateTimeDeparture is null))
                attendances.Add(attendancesData as Attendance);
        }

        /// <summary>
        /// Persiste los datos de una nueva instancia <see cref="Attendance"/> en una base de datos.
        /// </summary>
        public static bool Save(this Attendance attendance)
            => AcabusData.Session.Save(attendance);

        /// <summary>
        /// Persiste los datos de una instancia <see cref="Attendance"/> que han cambiado sus propiedades.
        /// </summary>
        public static Boolean Update(this Attendance attendance)
            => AcabusData.Session.Update(attendance);
    }
}