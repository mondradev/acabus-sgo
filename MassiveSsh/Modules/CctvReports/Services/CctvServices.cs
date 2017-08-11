using Acabus.DataAccess;
using Acabus.Models;
using Acabus.Modules.Attendances.Models;
using Acabus.Modules.Attendances.ViewModels;
using Acabus.Modules.CctvReports.Models;
using Acabus.Utils;
using Acabus.Utils.Mvvm;
using Acabus.Utils.SecureShell;
using InnSyTech.Standard.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Acabus.Modules.CctvReports.Services
{
    public static class CctvService
    {
        private const string NEED_BACKUP = "SE REQUIERE BACKUP";
        private const string SEARCH_FILL_STOCK = "select fch_oper, uid_tarj, tarj_sumi from sitm_disp.sbop_sum_tarj where date(fch_oper)=date(now())-1 and upper(tipo_oper)='S'";
        private const string SEARCH_PICKUP_MONEY = "SELECT min(FCH_RECA) fch_oper, UID_TARJ FROM SITM_DISP.SBOP_RECA WHERE DATE(FCH_RECA)=DATE(NOW())-1 GROUP BY UID_TARJ";

        public static Boolean CommitRefund(this Incidence incidence, DateTime refundDateTime)
        {
            var refund = AcabusData.Session.GetObjects<RefundOfMoney>(new DbFilter()
                .AddWhere(new DbFilterExpression("Fk_Folio", incidence.Folio, WhereOperator.EQUALS))).FirstOrDefault();

            refund.RefundDate = refundDateTime;
            refund.Status = RefundOfMoneyStatus.COMMIT;
            if (refund.Incidence.FinishDate is null)
                refund.Incidence.FinishDate = refundDateTime;

            return AcabusData.Session.Update(ref refund);
        }

        public static DeviceFault CreateDeviceFault(Alarm alarm)
        {
            if (alarm is null) return null;

            var deviceType = alarm?.Device?.Type;
            var description = alarm?.Description;

            var faults = Core.DataAccess.AcabusData.AllFaults.Where(fault => (fault as DeviceFault).Category?.DeviceType == deviceType);

            switch (description)
            {
                case "FALLA EN TARJETA IO":
                case "FALLA TARJETA IO":
                case "COMUNICACIÓN CON VALIDADOR TARJETA ADR":
                case "NO SE PUDO ESTABLECER COMUNICACIÓN CON LA TARJETA ES (ADR)":
                    return (DeviceFault)faults.FirstOrDefault(fault
                        => (fault as DeviceFault).Description.Contains("FUERA DE SERVICIO"));

                case "FALLA SAM AV RECARGA":
                case "FALLA LECTOR RECARGA":
                case "FALLA EN LECTOR RECARGA":
                    return (DeviceFault)faults.FirstOrDefault(fault
                        => (fault as DeviceFault).Description == "SIN LECTURA DE TARJETAS");

                case "ALCANCIA LLENA":
                case "ALCANCÍA LLENA":
                    return (DeviceFault)faults.FirstOrDefault(fault
                        => (fault as DeviceFault).Description == "MONEDAS ATASCADAS, NOTIFICAR EL TOTAL E INTRODUCIRLO A LA ALCANCÍA");

                case "FALLA EN IMPRESORA":
                    return (DeviceFault)faults.FirstOrDefault(fault
                        => (fault as DeviceFault).Description == "NO IMPRIMIÓ TICKET E IMPRIMIR TICKET DE PRUEBA");

                case "FALLA EN VALIDADOR DE BILLETES":
                case "FALLA BILLETERO":
                    return (DeviceFault)faults.FirstOrDefault(fault
                        => (fault as DeviceFault).Description == "NO ACEPTA BILLETES");

                case "FUERA DE SERVICIO":
                    return faults.FirstOrDefault(fault
                        => (fault as DeviceFault).Description.Contains("FUERA DE SERVICIO"));

                case "FALLA SAM AV2 VENTA":
                case "FALLA LECTOR VENTA":
                case "FALLA EN EXPENDEDOR":
                case "FALLA EN EL DISPENSADOR":
                    return (DeviceFault)faults.FirstOrDefault(fault
                        => (fault as DeviceFault).Description.Contains("TARJETA ATASCADA"));

                case "BILLETERA LLENA":
                    return (DeviceFault)faults.FirstOrDefault(fault
                        => (fault as DeviceFault).Description == "BILLETE ATASCADO, NOTIFICAR EL TOTAL Y CANALIZAR EL DINERO A CAU");

                case "FALLA ENERGIA":
                    return (DeviceFault)faults.FirstOrDefault(fault
                        => (fault as DeviceFault).Description.Contains("APAGADO"));

                case "FALLA EN VALIDADOR DE MONEDAS":
                case "FALLA MONEDERO":
                    return (DeviceFault)faults.FirstOrDefault(fault
                        => (fault as DeviceFault).Description == "NO ACEPTA MONEDAS");

                case "FALLA CONTADOR EN CERO":
                    return faults.FirstOrDefault(fault => fault.Description == "NO SE TIENE CONTEO DE PASAJEROS");

                case "FALLA CONTADOR":
                    return faults.FirstOrDefault(fault => fault.Description == "EL CONTEO DE PASAJEROS ES MENOR A LAS VALIDACIONES");

                case NEED_BACKUP:
                    return faults.FirstOrDefault(fault => fault.Description.Contains("INFORMACIÓN DE LAS OPERACIONES"));

                default: return faults.FirstOrDefault(fault => fault.Description.Contains(description));
            }
        }

        public static Incidence CreateIncidence(this IList<Incidence> incidences, DeviceFault description, Device device, DateTime startTime,
                            Priority priority, String whoReporting, String observations = null)
        => CreateIncidence(incidences, description, device, startTime, priority, whoReporting, observations, IncidenceStatus.OPEN, ViewModelService.GetViewModel<AttendanceViewModel>()?
                                .GetTechnicianAssigned(device, startTime, description), null, null);

        public static Incidence CreateIncidence(this IList<Incidence> incidences, DeviceFault description, Device device, DateTime startTime,
                            Priority priority, string whoReporting, String observations, IncidenceStatus status, Attendance attendance, Technician technicianThatFix,
                            DateTime? finishDate)
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
                    Status = status,
                    AssignedAttendance = attendance,
                    Observations = observations,
                    Technician = technicianThatFix,
                    FinishDate = finishDate
                };

                incidences.Add(incidence);
                return incidence;
            }
        }

        public static Boolean Equals(Alarm alarm, Incidence incidence)
        {
            try
            {
                if (alarm is null) return false;
                if (alarm.Device != null && !alarm.Device.Equals(incidence?.Device)) return false;
                DeviceFault deviceFault = CreateDeviceFault(alarm);
                if (deviceFault is null)
                    throw new Exception($"No existe la falla especificada --> {alarm.Description}");
                if (deviceFault != null && deviceFault.ID.Equals(incidence.Description.ID)
                    || (alarm.Device.Type == DeviceType.CONT && deviceFault.Category == incidence.Description.Category))
                {
                    var alarmDatetime = alarm.DateTime;
                    var incidenceDatetime = incidence.StartDate;
                    alarmDatetime = alarmDatetime.AddMilliseconds(alarmDatetime.Millisecond * -1);
                    incidenceDatetime = incidenceDatetime.AddMilliseconds(incidenceDatetime.Millisecond * -1);

                    if (alarmDatetime.Equals(incidenceDatetime) || incidence.Status != IncidenceStatus.CLOSE
                        || (incidence.Status == IncidenceStatus.CLOSE && alarm.DateTime < incidence.StartDate))
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message, "ERROR");
                return false;
            }
        }

        public static Boolean Equals(BusDisconnectedAlarm alarm, Incidence incidence)
        {
            if (incidence.Device?.Vehicle != null)
                if (alarm.EconomicNumber == incidence.Device?.Vehicle?.EconomicNumber
                    && incidence.Status != IncidenceStatus.CLOSE)
                    return true;
            return false;
        }

        public static void GetAlarms(this ObservableCollection<Alarm> alarms)
        {
            if (DateTime.Now.TimeOfDay > TimeSpan.FromHours(23)
               || DateTime.Now.TimeOfDay < TimeSpan.FromHours(6)) return;

            alarms.Clear();
            var response = AcabusData.ExecuteQueryInServerDB(String.Format(AcabusData.TrunkAlertQuery, DateTime.Now.AddMinutes(-30)));
            if (response == null || response.Length < 2) return;
            for (UInt16 i = 1; i < response.Length; i++)
            {
                var row = response[i];
                if (row.Length < 2) break;
                var numeseri = row[1];
                var exists = false;
                foreach (var alert in alarms)
                {
                    if (alert.ID == 0) continue;
                    if (alert.ID == UInt32.Parse(row[0]) && alert.Device.NumeSeri == numeseri)
                        exists = true;
                }
                if (!exists)
                    Application.Current.Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                alarms.Add(Alarm.CreateAlarm(UInt32.Parse(row[0]), numeseri, row[2], DateTime.Parse(row[3]), (Priority)(UInt16.Parse(row[4]) - 1)));
                            }
                            catch (InvalidOperationException)
                            {
                                Trace.WriteLine($"ALARMA [{row[2]}] DE EQUIPO DESCONOCIDO ({numeseri}) FUE IGNORADA", "NOTIFY");
                            }
                        });
            }
        }

        public static void GetBusDisconnectedAlarms(this ObservableCollection<BusDisconnectedAlarm> busAlarms)
        {
            if (DateTime.Now.TimeOfDay > TimeSpan.FromHours(21)
                || DateTime.Now.TimeOfDay < TimeSpan.FromHours(3)) return;

            busAlarms.Clear();
            Vehicle[] vehicles = new Vehicle[Core.DataAccess.AcabusData.OffDutyVehicles.Count];
            Core.DataAccess.AcabusData.OffDutyVehicles.CopyTo(vehicles, 0);

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
            var filter = new DbFilter();
            filter.AddWhere(new DbFilterExpression(nameof(Incidence.Status), (Int16)IncidenceStatus.CLOSE, WhereOperator.EQUALS), WhereType.AND);
            filter.AddWhere(new DbFilterExpression(nameof(Incidence.StartDate), DateTime.Now.AddDays(-10), WhereOperator.GREAT_THAT), WhereType.AND);

            ICollection<Incidence> incidencesFromDb = AcabusData.Session.GetObjects<Incidence>(new DbFilter(new List<DbFilterValue>() {
                new DbFilterValue(new DbFilterExpression(nameof(Incidence.Status), (Int16)IncidenceStatus.CLOSE, WhereOperator.NO_EQUALS), WhereType.AND)
            }));

            foreach (var incidenceData in incidencesFromDb)
                incidences.Add(incidenceData as Incidence);

            ICollection<Incidence> history = AcabusData.Session.GetObjects<Incidence>(filter);
            foreach (var incidenceData in history)
                incidences.Add(incidenceData as Incidence);
        }

        public static Boolean Save(this Incidence incidence)
            => AcabusData.Session.Save(ref incidence);

        public static Boolean Save(this RefundOfMoney refundOfMoney)
            => AcabusData.Session.Save(ref refundOfMoney);

        public static void SearchCountersFailing(this ObservableCollection<Alarm> alarms)
        {
            var response = AcabusData.ExecuteQueryInServerDB(AcabusData.CountersFailingQuery);
            foreach (var row in response)
            {
                if (row[0] == "no_econ")
                    continue;

                if (row.Length < 2) break;
                var economicNumber = row[0];
                var exists = false;
                foreach (var alert in alarms.Where(alarm => alarm.Device != null
                                                && alarm.Device.Vehicle != null
                                                && alarm.Device.Vehicle.EconomicNumber == economicNumber))
                    if (alert.Device.Type == DeviceType.CONT)
                        exists = true;
                if (!exists)
                {
                    Device alarmDevice = Core.DataAccess.AcabusData.AllDevices.FirstOrDefault(device
                                                      => device.Type == DeviceType.CONT
                                                          && device.Vehicle != null
                                                          && device.Vehicle.EconomicNumber == economicNumber);
                    if (alarmDevice == null)
                    {
                        Trace.WriteLine($"ALARMA [{row[3]}] DE CONTADOR DE PASAJEROS DEL VEHÍCULO DESCONOCIDO DESCONOCIDO ({economicNumber}) FUE IGNORADA", "NOTIFY");
                        continue;
                    }

                    Application.Current.Dispatcher.Invoke(()
                        => alarms.Add(new Alarm(0)
                        {
                            DateTime = DateTime.Now,
                            Description = row[3],
                            Device = alarmDevice,
                            Priority = row[3].Contains("CERO") ? Priority.HIGH : Priority.MEDIUM
                        }));
                }
            }
        }

        /// <summary>
        /// Extrae los suministros realizados durante el día.
        /// </summary>
        public static void SearchFillStock(this ObservableCollection<Alarm> alarms)
        {
            var devices = Core.DataAccess.AcabusData.AllDevices
                .Where(device => device.HasDatabase && device.Type == DeviceType.KVR);
            List<Task> tasks = new List<Task>();

            foreach (var device in devices)
            {
                var task = Task.Run(() =>
                {
                    if (!ConnectionTCP.IsAvaibleIP(device.IP)) return;

                    SshPostgreSQL psql = SshPostgreSQL.CreateConnection("/opt/PostgreSQL/9.3/bin", device.IP, 5432, "postgres", "4c4t3k", "SITM", "teknei", "4c4t3k");
                    var response = psql.ExecuteQuery(SEARCH_FILL_STOCK);
                    foreach (var row in response)
                    {
                        if (row[0] == "fch_oper")
                            continue;
                        if (row.Length < 2)
                            continue;

                        var operationDate = DateTime.Parse(row[0]);
                        var uid = row[1];
                        var totalStock = Int32.Parse(row[2]);

                        bool exist = false;
                        foreach (var alert in alarms.Where(alarm => alarm.Device == device))
                            if (alert.Description == "SUMINISTRO DE TARJETAS" && alert.DateTime.Date.Equals(operationDate))
                            {
                                exist = true;
                                break;
                            }

                        if (!exist)
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                alarms.Add(Alarm.CreateAlarm(0,
                                    device.NumeSeri,
                                    "SUMINISTRO DE TARJETAS",
                                    operationDate,
                                    Priority.NONE,
                                    String.Format("TOTAL SUMINISTRADAS: {0}, UID MANTTO: {1}", totalStock, uid),
                                    true));
                            });
                    }
                });
                tasks.Add(task);
            }
            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch
            {
                Trace.WriteLine($"Ocurrió un error al momento de revisar los suministros obtenidos por KVR", "NOTIFY");
            }
        }

        public static void SearchMissingBackups(this ObservableCollection<Alarm> alarms)
        {
            var response = AcabusData.ExecuteQueryInServerDB(AcabusData.MissingBackupsQuery);
            foreach (var row in response)
            {
                if (row[0] == "no_econ")
                    continue;

                if (row.Length < 1) break;
                var serie = row[0];
                var exists = false;
                foreach (var alert in alarms.Where(alarm => alarm.Device != null
                                                && (alarm.Device.NumeSeri == serie || (alarm.Device.Vehicle != null
                                                && alarm.Device.Vehicle.EconomicNumber == serie))))
                    if (alert.Description == NEED_BACKUP)
                        exists = true;
                if (!exists)
                {
                    Device alarmDevice = Core.DataAccess.AcabusData.AllDevices.FirstOrDefault(device
                                                      => (device.Type == DeviceType.PCA
                                                              && device.Vehicle != null
                                                              && device.Vehicle.EconomicNumber == serie)
                                                          || (device.Type == DeviceType.KVR
                                                              && device.NumeSeri == serie));
                    if (alarmDevice == null)
                    {
                        Trace.WriteLine($"ALARMA [{NEED_BACKUP}] DE EQUIPO DESCONOCIDO ({serie}) FUE IGNORADA", "NOTIFY");
                        continue;
                    }

                    Application.Current.Dispatcher.Invoke(()
                        => alarms.Add(new Alarm(0)
                        {
                            DateTime = DateTime.Now,
                            Description = NEED_BACKUP,
                            Device = alarmDevice,
                            Priority = Priority.HIGH
                        }));
                }
            }
        }

        /// <summary>
        /// Extrae los recaudos realizados durante el día.
        /// </summary>
        public static void SearchPickUpMoney(this ObservableCollection<Alarm> alarms)
        {
            var devices = Core.DataAccess.AcabusData.AllDevices
                   .Where(device => device.HasDatabase && device.Type == DeviceType.KVR);
            List<Task> tasks = new List<Task>();
            foreach (var device in devices)
            {
                var task = Task.Run(() =>
                  {
                      if (!ConnectionTCP.IsAvaibleIP(device.IP)) return;

                      SshPostgreSQL psql = SshPostgreSQL.CreateConnection("/opt/PostgreSQL/9.3", device.IP, 5432, "postgres", "4c4t3k", "SITM", "teknei", "4c4t3k");
                      var response = psql.ExecuteQuery(SEARCH_PICKUP_MONEY);
                      foreach (var row in response)
                      {
                          if (row[0] == "fch_oper")
                              continue;
                          if (row.Length < 2)
                              continue;

                          var operationDate = DateTime.Parse(row[0]);
                          var uid = row[1];

                          bool exist = false;
                          foreach (var alert in alarms.Where(alarm => alarm.Device == device))
                              if (alert.Description == "RECAUDO DE VALORES" && alert.DateTime.Date.Equals(operationDate))
                              {
                                  exist = true;
                                  break;
                              }

                          if (!exist)
                              Application.Current.Dispatcher.Invoke(() =>
                              {
                                  alarms.Add(Alarm.CreateAlarm(0,
                                      device.NumeSeri,
                                      "RECAUDO DE VALORES",
                                      operationDate,
                                      Priority.NONE,
                                      String.Format("UID RECAUDADOR: {0}", uid),
                                      true));
                              });
                      }
                  });
                tasks.Add(task);
            }
            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch
            {
                Trace.WriteLine($"Ocurrió un error al momento de revisar los recaudos obtenidos por KVR", "NOTIFY");
            }
        }

        public static Boolean Update(this Incidence incidence)
                                            => AcabusData.Session.Update(ref incidence);
    }
}