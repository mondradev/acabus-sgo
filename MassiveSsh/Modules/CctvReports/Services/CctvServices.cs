using Acabus.DataAccess;
using Acabus.Models;
using Acabus.Modules.CctvReports.Models;
using Acabus.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Acabus.Modules.CctvReports.Services
{
    public static class CctvService
    {
        public static void CreateIncidence(this IList<Incidence> incidences, string description, Device device, DateTime startTime,
            Priority priority, Location location, string whoReporting)
        {
            var lastFolio = AcabusData.FirstFolio;
            foreach (var item in incidences)
            {
                UInt64 nFolio = UInt64.Parse(item.Folio.Replace("F-", ""));
                lastFolio = lastFolio < nFolio ? nFolio : lastFolio;
            }

            var incidence = new Incidence(String.Format("F-{0:D5}", lastFolio + 1))
            {
                Description = description,
                Device = device,
                StartDate = startTime,
                Priority = priority,
                Location = location,
                WhoReporting = whoReporting,
                Status = IncidenceStatus.OPEN
            };

            incidences.Add(incidence);
        }

        public static void LoadFromDataBase(this IList<Incidence> incidences)
        {
            var response = SQLiteAccess.ExecuteQuery("SELECT * FROM Incidences WHERE status=0 UNION SELECT * FROM (SELECT * FROM Incidences WHERE status=1 ORDER BY folio DESC LIMIT 100)");
            foreach (var incidenceData in response)
            {
                Device deviceData = AcabusData.FindDevice((device) => device.NumeSeri == incidenceData[3].ToString());
                deviceData = deviceData is null ? AcabusData.FindVehicle((vehicle) => vehicle.EconomicNumber == incidenceData[3].ToString()) : deviceData;

                Location locationData = deviceData is Vehicle
                    ? (Location)AcabusData.Routes.FindRoute((route) => route.ToString() == incidenceData[4].ToString())
                    : AcabusData.FindStation((station) => station.Name == incidenceData[4].ToString());

                incidences.Add(new Incidence(incidenceData[0].ToString())
                {
                    Description = incidenceData[1].ToString(),
                    WhoReporting = incidenceData[2].ToString(),
                    Device = deviceData,
                    Location = locationData,
                    StartDate = (DateTime)incidenceData[5],
                    FinishDate = String.IsNullOrEmpty(incidenceData[6].ToString()) ? null : (DateTime?)incidenceData[6],
                    Status = (IncidenceStatus)UInt16.Parse(incidenceData[7].ToString()),
                    Technician = incidenceData[8].ToString(),
                    Observations = incidenceData[9].ToString(),
                    Priority = (Priority)incidenceData[10]
                });
            }
        }

        public static void GetAlarms(this ObservableCollection<Alarm> alarms)
        {
            if (DateTime.Now.TimeOfDay > TimeSpan.FromHours(23)
               || DateTime.Now.TimeOfDay < TimeSpan.FromHours(6)) return;

            alarms.Clear();
            var response = AcabusData.ExecuteQueryInServerDB(String.Format(AcabusData.TrunkAlertQuery, DateTime.Now.AddMinutes(-10)));
            if (response == null || response.Length < 2) return;
            for (UInt16 i = 1; i < response.Length; i++)
            {
                var row = response[i];
                if (row.Length < 2) break;
                var numeseri = row[1].Replace("TSI", "TOR").Replace("TS", "TOR").Replace("TD", "TOR");
                var exists = false;
                foreach (var alert in alarms)
                    if (alert.ID == UInt32.Parse(row[0]) && alert.Device.NumeSeri == numeseri)
                        exists = true;
                if (!exists)
                    Application.Current.Dispatcher.Invoke(()
                        => alarms.Add(Alarm.CreateAlarm(UInt32.Parse(row[0]), numeseri, row[2], DateTime.Parse(row[3]), (Priority)(UInt16.Parse(row[4]) - 1))));
            }
        }

        public static void GetBusDisconnectedAlarms(this ObservableCollection<BusDisconnectedAlarm> busAlarms)
        {
            if (DateTime.Now.TimeOfDay > TimeSpan.FromHours(21)
                || DateTime.Now.TimeOfDay < TimeSpan.FromHours(6)) return;

            busAlarms.Clear();
            Vehicle[] vehicles = new Vehicle[AcabusData.OffDutyVehicles.Count];
            AcabusData.OffDutyVehicles.CopyTo(vehicles, 0);

            var response = AcabusData.ExecuteQueryInServerDB(String.Format(AcabusData.BusDisconnectedQuery, vehicles.Length < 1 ? "''" : Util.ToString(vehicles, "'{0}'", (vehi) => vehi.EconomicNumber)));

            if (response == null || response.Length < 2) return;

            for (UInt16 i = 1; i < response.Length; i++)
            {
                var row = response[i];
                if (row.Length < 2) break;
                var exists = false;
                foreach (var alert in busAlarms)
                    if (exists = alert.EconomicNumber == row[0]) break;
                if (!exists)
                    Application.Current?.Dispatcher.Invoke(()
                        => busAlarms.Add(BusDisconnectedAlarm.CreateBusAlert(row[0], DateTime.Parse(row[1]))));
            }
        }

        public static Boolean Equals(Alarm alarm, Incidence incidence)
        {
            if (alarm.Device.GetType() != incidence.Device.GetType()) return false;
            if (alarm.Device == incidence.Device
                && alarm.Description == incidence.Description
                    && alarm.DateTime == incidence.StartDate)
                return true;
            return false;
        }

        public static Boolean Equals(BusDisconnectedAlarm alarm, Incidence incidence)
        {
            if (incidence.Device is Vehicle)
                if (alarm.EconomicNumber == ((Vehicle)incidence.Device).EconomicNumber && incidence.Status == IncidenceStatus.OPEN)
                    return true;
            return false;
        }

        public static Boolean UpdatePriority(this Incidence incidence)
        {
            var query = String.Format("UPDATE Incidences SET Priority={0} WHERE Folio='{1}'",
                (UInt16)incidence.Priority,
                incidence.Folio
                );
            return SQLiteAccess.Execute(query) > 0;
        }

        public static Boolean Update(this Incidence incidence)
        {
            var query = String.Format("UPDATE Incidences SET WhoReporting='{0}', FinishDate='{1}', Status={2}, Technician='{3}', Observations='{4}', Priority={5} WHERE Folio='{6}' AND Status=0",
                incidence.WhoReporting,
                incidence.FinishDate?.ToSqliteFormat(),
                (UInt16)incidence.Status,
                incidence.Technician,
                incidence.Observations,
                (UInt16)incidence.Priority,
                incidence.Folio
                );

            return SQLiteAccess.Execute(query) > 0;
        }

        public static Boolean CreateIncidence(this Incidence incidence)
        {
            var query = String.Format("SELECT COUNT(*) FROM Incidences WHERE Folio='{0}'", incidence.Folio);

            var response = SQLiteAccess.ExecuteQuery(query);

            if (response[0][0].ToString() != "0") return false;

            query = String.Format("INSERT INTO Incidences VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}',{7},'{8}','{9}',{10})",
               incidence.Folio,
               incidence.Description,
               incidence.WhoReporting,
               incidence.Device is Vehicle ? (incidence.Device as Vehicle).EconomicNumber : incidence.Device?.NumeSeri,
               incidence.Location is Route ? (incidence.Location as Route).ToString() : incidence.Location.Name,
               incidence.StartDate.ToSqliteFormat(),
               incidence.FinishDate?.ToSqliteFormat(),
               (UInt16)incidence.Status,
               incidence.Technician,
               incidence.Observations,
               (UInt16)incidence.Priority
               );
            return SQLiteAccess.Execute(query) > 0;
        }
    }
}
