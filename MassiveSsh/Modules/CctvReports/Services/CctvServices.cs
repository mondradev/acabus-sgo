using Acabus.DataAccess;
using Acabus.Models;
using Acabus.Modules.Attendances.ViewModels;
using Acabus.Modules.CctvReports.Models;
using Acabus.Utils;
using Acabus.Utils.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Acabus.Modules.CctvReports.Services
{
    public static class CctvService
    {
        public static Boolean CommitRefund(this Incidence incidence, DateTime refundDateTime)
        {
            var refund = AcabusData.Session.GetObjects(typeof(RefundOfMoney))
                .FirstOrDefault(refundOfMoney => (refundOfMoney as RefundOfMoney).Incidence.Folio == incidence.Folio) as RefundOfMoney;

            refund.RefundDate = refundDateTime;
            refund.Status = RefundOfMoneyStatus.COMMIT;
            if (refund.Incidence.FinishDate is null)
                refund.Incidence.FinishDate = refundDateTime;

            return AcabusData.Session.Update(refund);
        }

        public static Incidence CreateIncidence(this IList<Incidence> incidences, DeviceFault description, Device device, DateTime startTime,
                    Priority priority, Location location, string whoReporting)
        {
            lock (incidences)
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
                    WhoReporting = whoReporting,
                    Status = IncidenceStatus.OPEN,
                    AssignedAttendance = ViewModelService.GetViewModel<AttendanceViewModel>()?
                                .GetTechnicianAssigned(location, device, startTime)
                };

                incidences.Add(incidence);
                return incidence;
            }
        }

        public static Boolean Equals(Alarm alarm, Incidence incidence)
        {
            if (alarm.Device.GetType() != incidence.Device.GetType()) return false;
            if (alarm.Device == incidence.Device
                && alarm.Description == incidence.Description.ToString())
                if (incidence.Status != IncidenceStatus.CLOSE
                    || alarm.DateTime == incidence.StartDate)
                    return true;

            return false;
        }

        public static Boolean Equals(BusDisconnectedAlarm alarm, Incidence incidence)
        {
            if (incidence.Device.Station is null)
                if (alarm.EconomicNumber == incidence.Device.Vehicle.EconomicNumber
                    && incidence.Status != IncidenceStatus.CLOSE)
                    return true;
            return false;
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
                var numeseri = row[1];
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

        public static void LoadFromDataBase(this IList<Incidence> incidences)
        {
            ICollection<object> incidencesFromDb = AcabusData.Session.GetObjects(typeof(Incidence));
            foreach (var incidenceData in incidencesFromDb.Where(incidence => (incidence as Incidence).Status != IncidenceStatus.CLOSE))
                incidences.Add(incidenceData as Incidence);
            foreach (var incidenceData in incidencesFromDb.Where(incidence => (incidence as Incidence).StartDate > DateTime.Now.AddDays(-45)))
                incidences.Add(incidenceData as Incidence);
        }

        public static Boolean Save(this Incidence incidence) => AcabusData.Session.Save(incidence);

        public static Boolean Save(this RefundOfMoney refundOfMoney) => AcabusData.Session.Save(refundOfMoney);

        public static Boolean Update(this Incidence incidence) => AcabusData.Session.Update(incidence);
    }
}