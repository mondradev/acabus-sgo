using Acabus.DataAccess;
using Acabus.Models;
using Acabus.Modules.Attendances.ViewModels;
using Acabus.Modules.CctvReports.Models;
using Acabus.Modules.CctvReports.Services;
using Acabus.Modules.CctvReports.Views;
using Acabus.Utils;
using Acabus.Utils.Mvvm;
using Acabus.Window;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Acabus.Modules.CctvReports
{
    public sealed class CctvReportsViewModel : ViewModelBase
    {
        private const string TEMP_NIGHT_TASKS = "~$tmp_counterfailings.dat";
        private const string TEMP_NIGHT_TASKS_KVR = "~$tmp_task_kvr.dat";

        /// <summary>
        /// Campo que provee a la propiedad 'Alarms'.
        /// </summary>
        private ObservableCollection<Alarm> _alarms;

        /// <summary>
        /// Campo que provee a la propiedad 'Alarms'.
        /// </summary>
        private ObservableCollection<Alarm> _alarmsShown = new ObservableCollection<Alarm>();

        /// <summary>
        ///
        /// </summary>
        private Timer _alarmsMonitor;

        /// <summary>
        ///
        /// </summary>
        private Timer _busAlarmsMonitor;

        /// <summary>
        /// Campo que provee a la propiedad 'BusDisconnectedAlarms'.
        /// </summary>
        private ObservableCollection<BusDisconnectedAlarm> _busDisconnectedAlarms;

        /// <summary>
        /// Indica si la actualización de los autobuses sin conexión esta ocurriendo.
        /// </summary>
        private bool _busUpdating;

        /// <summary>
        /// Campo que provee a la propiedad 'FolioToSearch'.
        /// </summary>
        private String _folioToSearch;

        /// <summary>
        /// Campo que provee a la propiedad 'Incidences'.
        /// </summary>
        private ObservableCollection<Incidence> _incidences;

        private bool _inLoad;

        /// <summary>
        /// Campo que provee a la propiedad 'NewWhoReporting'.
        /// </summary>
        private String _newWhoReporting;

        private Timer _nitghtWorking;

        private Timer _nitghtActivity;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedIncidence'.
        /// </summary>
        private Incidence _selectedIncidence;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedIncidences" />.
        /// </summary>
        private ICollection<Incidence> _selectedIncidences;

        /// <summary>
        /// Campo que provee a la propiedad 'ToSearchClosed'.
        /// </summary>
        private String _toSearchClosed;

        /// <summary>
        ///
        /// </summary>
        private Timer _updatePriority;

        private bool nightSearching = false;

        private bool searching = false;

        public int IncidencesOpenedCount => IncidencesOpened.Where(i => i.Status != IncidenceStatus.PENDING).Count();

        /// <summary>
        ///
        /// </summary>
        public CctvReportsViewModel()
        {
            ViewModelService.Register(this);

            SearchToHistoryCommand = new CommandBase(parameter =>
            {
                AcabusControlCenterViewModel.ShowDialog(new IncidencesHistoryView());
            });

            ReassignTechnician = new CommandBase(parameter =>
            {
                if (IncidencesOpened.Count == 0) return;
                ViewModelService.GetViewModel<AttendanceViewModel>()?.ReassignTechnician();
                AcabusControlCenterViewModel.ShowDialog("Reasignación completada.");
            });

            CopyingRowClipboardHandlerCommand = new CommandBase(parameter => CctvService.ToClipboard(parameter as Incidence));

            UpdateDataCommand = new CommandBase(parameter =>
            {
                var incidence = (parameter as Incidence);
                if (incidence.Status != IncidenceStatus.CLOSE && incidence.Status != IncidenceStatus.PENDING) return;
                UpdateData();
            });

            UpdateSelectionCommand = new CommandBase(parameter =>
            {
                if (parameter is null || !(parameter is IList))
                    return;

                SelectedIncidences.Clear();

                var selection = parameter as IList;

                foreach (Incidence item in selection)
                    SelectedIncidences.Add(item);

                CctvService.ToClipboard(SelectedIncidences);
            });

            CloseIncidenceDialogCommand = new CommandBase((parameter) =>
            {
                if (SelectedIncidence is null) return;
                if (SelectedIncidences.Count > 1)
                    AcabusControlCenterViewModel.ShowDialog(new MultiCloseIncidencesView());
                else
                    AcabusControlCenterViewModel.ShowDialog(new CloseIncidenceView());
            });

            AddIncidenceCommand = new CommandBase((parameter) =>
            {
                AcabusControlCenterViewModel.ShowDialog(new AddIncidencesView() { DataContext = new AddIncidencesViewModel() { IsNewIncidences = true } });
            });

            OpenStationIncidencesDialogCommand = new CommandBase((parameter) =>
            {
                AcabusControlCenterViewModel.ShowDialog(new MultiIncidenceView());
            });

            RefundCashDialogCommand = new CommandBase((parameter) =>
            {
                AcabusControlCenterViewModel.ShowDialog(new AddIncidencesView() { DataContext = new AddIncidencesViewModel() { IsNewIncidences = false } });
            });

            ModifyIncidenceDialogCommand = new CommandBase((parameter) =>
            {
                ClearErrors();

                if (parameter is null) return;
                if (SelectedIncidence is null) return;

                NewWhoReporting = SelectedIncidence.WhoReporting;
                AcabusControlCenterViewModel.ShowDialog(new ModifyIncidenceView() { DataContext = parameter },
                    CopyingRowClipboardHandlerCommand.Execute);
            });

            SaveIncidenceCommand = new CommandBase((parameter) =>
            {
                ClearErrors();

                if (SelectedIncidence is null) return;

                if (HasErrors) return;

                SelectedIncidence.WhoReporting = NewWhoReporting;
                SelectedIncidence.Update();
                NewWhoReporting = null;

                DialogHost.CloseDialogCommand.Execute(parameter, null);
            });

            OpenDialogExportCommand = new CommandBase(async (parameter) => await DialogHost.Show(parameter));

            OpenOffDutyVehiclesDialog = new CommandBase((parameter) =>
            {
                var dialogContent = new OffDutyVehicles.OffDutyVehiclesView();
                AcabusControlCenterViewModel.ShowDialog(dialogContent, (response) =>
                {
                    if (response.ToString().Equals("LISTO"))
                        InitBusAlarmsMonitor();
                });
            });

            WaitHandle[] waitHandles = new WaitHandle[] {
                new EventWaitHandle(false, EventResetMode.AutoReset),
                new EventWaitHandle(false, EventResetMode.AutoReset),
                new EventWaitHandle(false, EventResetMode.AutoReset),
                new EventWaitHandle(false, EventResetMode.AutoReset)
            };

            RefreshIncidences = new CommandBase(parameter =>
            {
                Task.Run(() =>
                {
                    Trace.WriteLine("Actualizando lista de incidencias", "DEBUG");

                    AcabusControlCenterViewModel.AddNotify("Actualizando lista de incidencias...");

                    if (_inLoad) return;

                    _inLoad = true;

                    if (_updatePriority != null) _updatePriority.Dispose(waitHandles[0]);
                    else (waitHandles[0] as EventWaitHandle).Set();
                    if (_alarmsMonitor != null) _alarmsMonitor.Dispose(waitHandles[1]);
                    else (waitHandles[1] as EventWaitHandle).Set();
                    if (_busAlarmsMonitor != null) _busAlarmsMonitor.Dispose(waitHandles[2]);
                    else (waitHandles[2] as EventWaitHandle).Set();
                    if (_nitghtWorking != null) _nitghtWorking.Dispose(waitHandles[3]);
                    else (waitHandles[3] as EventWaitHandle).Set();

                    WaitHandle.WaitAll(waitHandles, 600);

                    Incidences.Clear();
                    UpdateData();
                    Incidences.LoadFromDataBase();

                    _inLoad = false;

                    UpdateData();
                    InitAlarmsMonitor();
                    InitUpdatePriority();

                    AcabusControlCenterViewModel.AddNotify("Lista de incidencias actualizada");
                });
            });

            Alarms.CollectionChanged += AlarmsCollectionChanged;
            BusDisconnectedAlarms.CollectionChanged += BusAlarmsCollectionChanged;
            Incidences.CollectionChanged += IncidenceCollectionChanged;

            _inLoad = true;

            Incidences.LoadFromDataBase();

            _inLoad = false;

            UpdateData();
            InitUpdatePriority();
        }

        ~CctvReportsViewModel()
        {
            ViewModelService.UnRegister(this);
        }

        /// <summary>
        ///
        /// </summary>
        public ICommand AddIncidenceCommand { get; }

        /// <summary>
        /// Obtiene una lista de las alarmas lanzadas por los equipos en vía.
        /// </summary>
        public ObservableCollection<Alarm> Alarms {
            get {
                if (_alarms == null)
                    _alarms = new ObservableCollection<Alarm>();
                return _alarms;
            }
        }

        /// <summary>
        /// Obtiene una lista de las unidades sin conexión.
        /// </summary>
        public ObservableCollection<BusDisconnectedAlarm> BusDisconnectedAlarms {
            get {
                if (_busDisconnectedAlarms == null)
                    _busDisconnectedAlarms = new ObservableCollection<BusDisconnectedAlarm>();
                return _busDisconnectedAlarms;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public ICommand CloseIncidenceDialogCommand { get; }

        /// <summary>
        /// Obtiene el comando que se ejecuta cuando el evento <c>SelectionChanged</c>
        /// es deseandenado en el DataGrid.
        /// </summary>
        public ICommand CopyingRowClipboardHandlerCommand { get; }

        /// <summary>
        /// Obtiene o establece el folio a buscar en las incidencias abiertas.
        /// </summary>
        public String FolioToSearch {
            get => _folioToSearch;
            set {
                _folioToSearch = value;
                OnPropertyChanged("FolioToSearch");
                OnPropertyChanged("IncidencesOpened");
            }
        }

        /// <summary>
        /// Obtiene una lista de las incidencias actualmente abiertas.
        /// </summary>
        public ObservableCollection<Incidence> Incidences {
            get {
                if (_incidences == null)
                    _incidences = new ObservableCollection<Incidence>();
                return _incidences;
            }
        }

        /// <summary>
        /// Obtiene una lista de la incidencias generadas por alarmas.
        /// </summary>
        public ObservableCollection<Incidence> IncidencesClosed {
            get {
                try
                {
                    return new ObservableCollection<Incidence>(Incidences.Where((incidence)
                             =>
                         {
                             Boolean isClosed = incidence.Status == IncidenceStatus.CLOSE;
                             Boolean isMatch = String.IsNullOrEmpty(ToSearchClosed)
                             || (incidence.Technician != null && incidence.Technician.Name.ToUpper().Contains(ToSearchClosed.ToUpper()))
                             || incidence.Description.ToString().ToUpper().Contains(ToSearchClosed.ToUpper());

                             return isClosed && isMatch;
                         }).OrderByDescending(i => i.FinishDate));
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Obtiene una lista de las incidencias abiertas.
        /// </summary>
        public ObservableCollection<Incidence> IncidencesOpened {
            get {
                try
                {
                    return new ObservableCollection<Incidence>(Incidences.Where((incidence)
                            =>
                    {
                        Boolean isOpen = incidence.Status != IncidenceStatus.CLOSE;
                        Boolean isMatch = String.IsNullOrEmpty(FolioToSearch) || incidence.Folio.ToUpper().Contains(FolioToSearch.ToUpper());

                        return isOpen && isMatch;
                    }).OrderBy(i => i.Status).ThenByDescending(i => i.StartDate));
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public ICommand ModifyIncidenceDialogCommand { get; }

        /// <summary>
        /// Obtiene o establece el nuevo valor de quién reporta.
        /// </summary>
        public String NewWhoReporting {
            get => _newWhoReporting;
            set {
                _newWhoReporting = value;
                OnPropertyChanged("NewWhoReporting");
            }
        }

        /// <summary>
        ///
        /// </summary>
        public ICommand OpenDialogExportCommand { get; }

        /// <summary>
        ///
        /// </summary>
        public ICommand OpenOffDutyVehiclesDialog { get; }

        public ICommand OpenStationIncidencesDialogCommand { get; }
        public ICommand ReassignTechnician { get; }
        public ICommand RefreshIncidences { get; }

        /// <summary>
        ///
        /// </summary>
        public ICommand RefundCashDialogCommand { get; }

        /// <summary>
        ///
        /// </summary>
        public ICommand SaveIncidenceCommand { get; }

        public ICommand SearchToHistoryCommand { get; }

        /// <summary>
        /// Obtiene o establece la incidencia actualmente seleccionada.
        /// </summary>
        public Incidence SelectedIncidence {
            get => _selectedIncidence;
            set {
                _selectedIncidence = value;
                OnPropertyChanged("SelectedIncidence");
            }
        }

        /// <summary>
        /// Obtiene una lista de las incidencias seleccionadas en la vista.
        /// </summary>
        public ICollection<Incidence> SelectedIncidences
            => _selectedIncidences ?? (_selectedIncidences = new ObservableCollection<Incidence>());

        /// <summary>
        /// Obtiene o establece el criterio de busqueda la incidencia cerrada.
        /// </summary>
        public String ToSearchClosed {
            get => _toSearchClosed;
            set {
                _toSearchClosed = value;
                OnPropertyChanged("ToSearchClosed");
                OnPropertyChanged("IncidencesClosed");
            }
        }

        /// <summary>
        ///
        /// </summary>
        public ICommand UpdateDataCommand { get; }

        public ICommand UpdateSelectionCommand { get; }

        public void ReloadData()
        {
            Incidences.Clear();
            Incidences.LoadFromDataBase();
        }

        public void UpdateData()
        {
            OnPropertyChanged("IncidencesOpened");
            OnPropertyChanged("IncidencesOpenedCount");
            OnPropertyChanged("IncidencesClosed");
        }

        protected override void OnLoad(object arg) => InitAlarmsMonitor();

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlarmsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Alarm alarm in e.NewItems)
                {
                    try
                    {
                        if (alarm is null) continue;
                        if (alarm.Device is null) continue;

                        Boolean exists = false;
                        var incidences = Incidences.Where(incidence => incidence.Device == alarm.Device);
                        if (incidences.Count() > 0)
                            foreach (var incidence in incidences)
                                if (exists = CctvService.Equals(alarm, incidence)) break;
                        if (alarm.Device.Type == DeviceType.CONT)
                        {
                            incidences = Incidences.Where(incidenceTemp
                                  => incidenceTemp.Status == IncidenceStatus.CLOSE
                                  && incidenceTemp.Device.Type == DeviceType.CONT
                                  && alarm.Device.Vehicle == incidenceTemp.Device.Vehicle
                                  && (DateTime.Now - incidenceTemp.FinishDate) < TimeSpan.FromDays(2));
                            if (incidences.Count() > 0)
                                exists = true;
                        }
                        if (!exists)
                        {
                            DeviceFault deviceFault = CctvService.CreateDeviceFault(alarm);

                            if (deviceFault is null)
                            {
                                if (!_alarmsShown.Any(a => a.Device == alarm.Device && a.Description == alarm.Description && a.DateTime == alarm.DateTime)
                                    && (DateTime.Now - alarm.DateTime) <= TimeSpan.FromMinutes(10))
                                {
                                    AcabusControlCenterViewModel.ShowDialog(String.Format("{0}\n{1}\n{2}", alarm.Device, alarm.Description, alarm.DateTime));
                                    _alarmsShown.Add(alarm);
                                }
                                continue;
                            }

                            if (deviceFault.Description.ToUpper().Contains("AUTENTICA"))
                            {
                                alarm.IsHistorial = true;
                                AcabusControlCenterViewModel.AddNotify(String.Format("{0}\n{1}\n{2}", alarm.Device, alarm.Description, alarm.DateTime));
                            }

                            if (alarm.IsHistorial)
                                Incidences.CreateIncidence(
                                deviceFault,
                                alarm.Device,
                                alarm.DateTime,
                                alarm.Priority,
                                "SISTEMA",
                                alarm.Comments,
                                IncidenceStatus.CLOSE,
                                null,
                                Core.DataAccess.AcabusData.AllTechnicians
                                                    .FirstOrDefault(technician => technician.Name == "Sistema"),
                                alarm.DateTime
                                );
                            else
                                Incidences.CreateIncidence(
                                    deviceFault,
                                    alarm.Device,
                                    alarm.DateTime,
                                    alarm.Priority,
                                    "SISTEMA",
                                    alarm.Comments
                                    );
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message, "ERROR");
                        continue;
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BusAlarmsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (BusDisconnectedAlarm alarm in e.NewItems)
                {
                    Boolean exists = false;
                    foreach (var incidence in Incidences)
                        if (exists = CctvService.Equals(alarm, incidence)) break;
                    if (!exists)
                        Incidences.CreateIncidence(
                            Core.DataAccess.AcabusData.AllFaults
                                .Where(fault => (fault as DeviceFault).Category?.DeviceType == DeviceType.PCA)
                                .FirstOrDefault(fault => (fault as DeviceFault).Description.ToUpper().Contains("SIN CONEXIÓN GPS")),
                            Core.DataAccess.AcabusData.AllDevices
                                .FirstOrDefault(deviceBus => deviceBus.Vehicle != null
                                    && deviceBus.Vehicle.EconomicNumber == alarm.EconomicNumber),
                            DateTime.Now,
                            alarm.Priority,
                            "SISTEMA",
                            alarm.LastSentLocation.Year <= 2012 ? $"POSIBLE FECHA/HORA DESCONFIGURADA; ULTIMA CONEXIÓN {alarm.LastSentLocation}" : ""
                        );
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IncidenceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Incidence item in e.NewItems)
                {
                    if (item.Status == IncidenceStatus.OPEN && !_inLoad)
                        AcabusControlCenterViewModel.AddNotify(String.Format("{0:dd/MM/yyyy HH:mm:ss} {1} - {2}",
                            item.StartDate,
                            item.Device?.Station is null
                                ? String.Format("{0} {1}", item.Device?.Vehicle, item.Device)
                                : item.Device?.ToString(),
                            item.Description));
                    Task.Run(() => item.Save());
                }
            }
            if (!_inLoad)
            {
                OnPropertyChanged("IncidencesOpened");
                OnPropertyChanged("IncidencesClosed");
            }
        }

        /// <summary>
        /// Inicializa el monitor de alarmas.
        /// </summary>
        private void InitAlarmsMonitor()
        {
            InitNightTasks();

            _alarmsMonitor = new Timer(delegate
            {
                if (DateTime.Now.TimeOfDay.Between(TimeSpan.Parse(AcabusData.GetProperty("MinAlarms", "CCTV_Setting") ?? "06:00:00"),
                       TimeSpan.Parse(AcabusData.GetProperty("MaxAlarms", "CCTV_Setting") ?? "22:00:00")))
                {
                    Trace.WriteLine("Actualizando alarmas", "DEBUG");
                    Alarms.GetAlarms();
                }
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(0.2));

            if (_busAlarmsMonitor == null && Core.DataAccess.AcabusData.OffDutyVehicles.Count == 0) return;

            InitBusAlarmsMonitor();
        }

        /// <summary>
        ///
        /// </summary>
        private void InitBusAlarmsMonitor()
        {
            if (_busAlarmsMonitor != null) return;

            _busAlarmsMonitor = new Timer(delegate
            {
                if (_busUpdating) return;
                _busUpdating = true;

                if (DateTime.Now.TimeOfDay.Between(TimeSpan.Parse(AcabusData.GetProperty("MinBusDisconnect", "CCTV_Setting") ?? "03:00:00"),
                    TimeSpan.Parse(AcabusData.GetProperty("MaxBusDisconnect", "CCTV_Setting") ?? "21:00:00")))
                {
                    Trace.WriteLine("Actualizando autobuses sin conexión", "DEBUG");
                    BusDisconnectedAlarms.GetBusDisconnectedAlarms();

                    Incidence[] incidences;

                    lock (Incidences)
                    {
                        incidences = new Incidence[Incidences.Count];
                        Incidences.CopyTo(incidences, 0);
                    }

                    DeviceFault disconnectionBus = Core.DataAccess.AcabusData.AllFaults
                              .Where(fault => (fault as DeviceFault).Category?.DeviceType == DeviceType.PCA)
                               .FirstOrDefault(fault => (fault as DeviceFault).Description.ToUpper().Contains("SIN CONEXIÓN GPS"));

                    foreach (var incidence in incidences)
                    {
                        bool exists = false;

                        /// Verificar desconexiones de autobuses reconectados

                        if (incidence.Status == IncidenceStatus.UNCOMMIT && incidence.Device.Vehicle != null)
                        {
                            exists = BusDisconnectedAlarms.Any(b => CctvService.Equals(b, incidence));

                            if (exists)
                            {
                                incidence.Status = IncidenceStatus.OPEN;
                                incidence.Technician = null;
                                incidence.FinishDate = null;
                                incidence.Priority = Priority.HIGH;
                                incidence.Observations = "DESCONEXIONES RECURRENTES";
                                incidence.Update();
                                UpdateData();
                                continue;
                            }
                        }

                        /// Verificación de las incidencias de SIN CONEXIÓN DE DATOS por confirmar

                        if (incidence.Status == IncidenceStatus.UNCOMMIT && incidence.Description.Equals(disconnectionBus) && incidence.Device.Vehicle != null)
                            /// A pasado el tiempo para cerrar automáticamente ? 10 MIN
                            if ((DateTime.Now - incidence.FinishDate) > TimeSpan.Parse(AcabusData.GetProperty("CloseByReconnection", "CCTV_Setting") ?? "10:00"))
                            {
                                incidence.Status = IncidenceStatus.CLOSE;
                                if (Core.DataAccess.AcabusData.OffDutyVehicles.Where(vehicle
                                    => vehicle.EconomicNumber == incidence.Device.Vehicle.EconomicNumber).Count() > 0)
                                    incidence.Observations = "UNIDAD EN TALLER O SIN ENERGÍA";
                                else incidence.Observations = "SE REESTABLECE CONEXIÓN AUTOMÁTICAMENTE";
                                incidence.Update();
                                UpdateData();
                                continue;
                            }

                        /// Verificación de las incidencias ABIERTAS

                        if (incidence.Status != IncidenceStatus.OPEN
                                || (incidence.Description != null && !incidence.Description.Equals(disconnectionBus))
                                    || incidence.Device?.Vehicle is null) continue;

                        exists = false;

                        foreach (var bus in BusDisconnectedAlarms)
                            if (CctvService.Equals(bus, incidence))
                            {
                                exists = true;
                                break;
                            }

                        if (!exists)
                        {
                            incidence.Status = IncidenceStatus.UNCOMMIT;
                            if (Core.DataAccess.AcabusData.OffDutyVehicles.Where(vehicle
                                     => vehicle.EconomicNumber == incidence.Device.Vehicle.EconomicNumber).Count() > 0)
                                incidence.Observations = "UNIDAD EN TALLER O SIN ENERGÍA";
                            else
                                incidence.Observations = "RECONEXIÓN DE LA UNIDAD";
                            incidence.Priority = Priority.NONE;
                            incidence.FinishDate = DateTime.Now;
                            incidence.Technician = Core.DataAccess.AcabusData.AllTechnicians
                                                    .FirstOrDefault(technician => technician.Name == "SISTEMA");
                            incidence.Update();
                        }
                    }
                }

                _busUpdating = false;
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        private void InitNightTasks()
        {
            if (_nitghtWorking == null)
                _nitghtWorking = new Timer(delegate
                {
                    try
                    {
                        if (!nightSearching && DateTime.Now.TimeOfDay.Between(TimeSpan.Parse(AcabusData.GetProperty("MinNightTask", "CCTV_Setting") ?? "22:00:00"),
                            TimeSpan.Parse(AcabusData.GetProperty("MaxNightTask", "CCTV_Setting") ?? "23:59:59")))
                        {
                            try
                            {
                                if (File.Exists(TEMP_NIGHT_TASKS))
                                {
                                    String date = File.ReadAllText(TEMP_NIGHT_TASKS);
                                    DateTime lastUpdate = DateTime.Parse(date);

                                    if (lastUpdate.Date.Equals(DateTime.Now.Date))
                                        return;
                                }
                                nightSearching = true;
                                Trace.WriteLine("BUSCANDO VERIFICANDO ESTADO DE CONTADORES", "NOTIFY");
                                Alarms.SearchCountersFailing();
                                Trace.WriteLine("BUSCANDO BACKUPS FALTANTES", "NOTIFY");
                                Alarms.SearchMissingBackups();
                                File.WriteAllText(TEMP_NIGHT_TASKS, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                                nightSearching = false;
                            }
                            catch (IOException)
                            {
                                File.Delete(TEMP_NIGHT_TASKS);
                            }
                        }
                    }
                    catch (Exception ex) { Trace.WriteLine(ex.StackTrace, "ERROR"); }
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

            if (_nitghtActivity == null)
                _nitghtActivity = new Timer(delegate
            {
                try
                {
                    if (!searching && DateTime.Now.TimeOfDay.Between(TimeSpan.Parse(AcabusData.GetProperty("MinSearchingTask", "CCTV_Setting") ?? "02:00:00"),
                        TimeSpan.Parse(AcabusData.GetProperty("MaxSearchingTask", "CCTV_Setting") ?? "03:00:00")))
                    {
                        try
                        {
                            if (File.Exists(TEMP_NIGHT_TASKS_KVR))
                            {
                                String date = File.ReadAllText(TEMP_NIGHT_TASKS_KVR);
                                DateTime lastUpdate = DateTime.Parse(date);

                                if (lastUpdate.Date.Equals(DateTime.Now.Date))
                                    return;
                            }
                            searching = true;
                            Trace.WriteLine("BUSCANDO RECAUDOS", "NOTIFY");
                            Alarms.SearchPickUpMoney();
                            Trace.WriteLine("BUSCANDO SUMINISTROS", "NOTIFY");
                            Alarms.SearchFillStock();
                            File.WriteAllText(TEMP_NIGHT_TASKS_KVR, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                            searching = false;
                        }
                        catch (IOException)
                        {
                            File.Delete(TEMP_NIGHT_TASKS);
                        }
                    }
                }
                catch (Exception ex) { Trace.WriteLine(ex.StackTrace, "ERROR"); }
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }

        private void InitUpdatePriority()
        {
            _updatePriority = new Timer((target) =>
            {
                if (_inLoad) return;

                foreach (var item in IncidencesOpened)
                {
                    if (_inLoad) break;

                    if (item.Status == IncidenceStatus.PENDING)
                        continue;

                    if (item.Status == IncidenceStatus.UNCOMMIT && item.Priority != Priority.NONE)
                    {
                        item.Priority = Priority.NONE;
                        item.Update();
                    }

                    if (item.Status == IncidenceStatus.UNCOMMIT
                       || item.Priority == Priority.NONE
                       || item.Priority == Priority.HIGH) continue;

                    var time = DateTime.Now - item.StartDate;
                    var isBus = item.Device.Vehicle != null;
                    var maxLowPriority = isBus
                               ? AcabusData.TimeMaxLowPriorityIncidenceBus : AcabusData.TimeMaxLowPriorityIncidence;
                    var maxMediumPriority = isBus
                               ? AcabusData.TimeMaxMediumPriorityIncidenceBus : AcabusData.TimeMaxMediumPriorityIncidence;

                    var oldPriority = item.Priority;

                    if (time > maxMediumPriority)
                        item.Priority = Priority.HIGH;
                    else if (time > maxLowPriority && item.Priority < Priority.HIGH)
                        item.Priority = Priority.MEDIUM;

                    if (item.Status == IncidenceStatus.OPEN && !oldPriority.Equals(item.Priority))
                        item.Update();
                }
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }
    }
}