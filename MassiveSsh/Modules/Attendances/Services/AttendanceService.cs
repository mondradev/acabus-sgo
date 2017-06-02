using Acabus.DataAccess;
using Acabus.Modules.Attendances.Models;
using Acabus.Modules.Attendances.ViewModels;
using Acabus.Utils;
using Acabus.Utils.Mvvm;
using System;
using System.Collections.Generic;
using static Acabus.Modules.Attendances.Models.Attendance;

namespace Acabus.Modules.Attendances.Services
{
    public static class AttendanceService
    {
        public static WorkShift GetTurn(DateTime startTime)
        {
            if (startTime.TimeOfDay.Between(TimeSpan.FromHours(6), new TimeSpan(13, 59, 59)))
                return WorkShift.MONING_SHIFT;

            if (startTime.TimeOfDay.Between(TimeSpan.FromHours(14), new TimeSpan(21, 59, 59)))
                return WorkShift.AFTERNOON_SHIFT;

            return WorkShift.NIGHT_SHIFT;
        }

        /// <summary>
        /// Persiste los datos de una nueva instancia <see cref="Attendance"/> en una base de datos.
        /// </summary>
        public static bool Save(this Attendance attendance)
        {
            var query = String.Format(@"INSERT INTO Attendances (
                            Technician,
                            Turn,
                            Section,
                            DateTimeEntry,
                            DateTimeDeparture,
                            HasKvrKey,
                            HasNemaKey,
                            Observations
                        )
                        VALUES (
                            '{0}',
                            {1},
                            '{2}',
                            '{3}',
                            '{4}',
                            {5},
                            {6},
                            '{7}'
                        )",
                  attendance.Technician,
                  (UInt16)attendance.Turn,
                  attendance.Section,
                  attendance.DateTimeEntry?.ToSqliteFormat(),
                  attendance.DateTimeDeparture?.ToSqliteFormat(),
                  attendance.HasKvrKey ? 1 : 0,
                  attendance.HasNemaKey ? 1 : 0,
                  attendance.Observations
                  );
            return SQLiteAccess.Execute(query) > 0;
        }

        /// <summary>
        /// Persiste los datos de una instancia <see cref="Attendance"/> que han cambiado sus propiedades.
        /// </summary>
        public static Boolean Update(this Attendance attendance)
        {
            var query = String.Format(@"UPDATE Attendances
                                           SET 
                                               Section = '{0}',
                                               DateTimeDeparture = '{1}',
                                               HasKvrKey = {2},
                                               HasNemaKey = {3},
                                               OpenedIncidences={4},
                                               Observations='{5}'
                                         WHERE Technician = '{6}' AND 
                                               DateTimeEntry = '{7}'",
                  attendance.Section,
                  attendance.DateTimeDeparture?.ToSqliteFormat(),
                  attendance.HasKvrKey ? 1 : 0,
                  attendance.HasNemaKey ? 1 : 0,
                  attendance.CountOpenedIncidences,
                  attendance.Observations,
                  attendance.Technician,
                  attendance.DateTimeEntry?.ToSqliteFormat()
                  );
            return SQLiteAccess.Execute(query) > 0;
        }

        public static void CountIncidences(string technician)
        {
            var attendances = ViewModelService.GetViewModel<AttendanceViewModel>()?.Attendances;
            if (attendances is null)
                return;
            foreach (var attendance in attendances)
                if (attendance.Technician == technician)
                    attendance.CountAssignedIncidences();
        }

        public static void LoadFromDataBase(this ICollection<Attendance> attendances)
        {
            var response = SQLiteAccess.ExecuteQuery("SELECT * FROM Attendances WHERE DateTimeDeparture = '' OR DateTimeDeparture IS NULL");
            foreach (var incidenceData in response)
            {
                attendances.Add(new Attendance()
                {
                    Technician = incidenceData[1].ToString(),
                    Turn = (WorkShift)UInt16.Parse(incidenceData[2].ToString()),
                    Section = incidenceData[3].ToString(),
                    DateTimeEntry = (DateTime)incidenceData[4],
                    HasKvrKey = (Boolean)incidenceData[6],
                    HasNemaKey = (Boolean)incidenceData[7],
                    Observations=incidenceData[9].ToString()
                });
            }
        }
    }
}
