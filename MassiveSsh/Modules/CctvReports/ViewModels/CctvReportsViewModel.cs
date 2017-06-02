using Acabus.DataAccess;
using Acabus.Models;
using Acabus.Modules.Attendances.ViewModels;
using Acabus.Modules.CctvReports.Models;
using Acabus.Modules.CctvReports.Services;
using Acabus.Utils;
using Acabus.Utils.Mvvm;
using Acabus.Window;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;

namespace Acabus.Modules.CctvReports
{
    public sealed class CctvReportsViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Alarms'.
        /// </summary>
        private ObservableCollection<Alarm> _alarms;

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

        public ICommand ReassignTechnician { get; }

        /// <summary>
        /// Campo que provee a la propiedad 'BusDisconnectedAlarms'.
        /// </summary>
        private ObservableCollection<BusDisconnectedAlarm> _busDisconnectedAlarms;

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
        /// Campo que provee a la propiedad 'Incidences'.
        /// </summary>
        private ObservableCollection<Incidence> _incidences;

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
        /// Obtiene una lista de las incidencias abiertas.
        /// </summary>
        public ObservableCollection<Incidence> IncidencesOpened
            => (ObservableCollection<Incidence>)Util.Where(Incidences, (incidence)
                =>
            {
                Boolean isOpen = incidence.Status != IncidenceStatus.CLOSE;
                Boolean isMatch = String.IsNullOrEmpty(FolioToSearch) || incidence.Folio.ToUpper().Contains(FolioToSearch.ToUpper());

                return isOpen && isMatch;
            });

        /// <summary>
        /// Obtiene el comando que se ejecuta cuando el evento <c>SelectionChanged</c>
        /// es deseandenado en el DataGrid.
        /// </summary>
        public ICommand CopyingRowClipboardHandlerCommand { get; }

        /// <summary>
        /// Obtiene un comando que se ejecuta cuando el evento <c>Loaded</c>
        /// del UserControl se desencadena.
        /// </summary>
        public ICommand LoadedHandlerCommand { get; }

        /// <summary>
        /// Obtiene una lista de la incidencias generadas por alarmas.
        /// </summary>
        public ObservableCollection<Incidence> IncidencesClosed
            => (ObservableCollection<Incidence>)Util.Where(Incidences, (incidence)
                =>
            {
                Boolean isClosed = incidence.Status == IncidenceStatus.CLOSE;
                Boolean isMatch = String.IsNullOrEmpty(ToSearchClosed)
                            || incidence.Technician.ToUpper().Contains(ToSearchClosed.ToUpper())
                            || incidence.Description.ToUpper().Contains(ToSearchClosed.ToUpper());

                return isClosed && isMatch;
            });

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedIncidence'.
        /// </summary>
        private Incidence _selectedIncidence;

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
        /// Campo que provee a la propiedad 'FolioToSearch'.
        /// </summary>
        private String _folioToSearch;

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
        /// Campo que provee a la propiedad 'ToSearchClosed'.
        /// </summary>
        private String _toSearchClosed;

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
        private Timer _alarmsMonitor;


        /// <summary>
        /// 
        /// </summary>
        private Timer _busAlarmsMonitor;

        /// <summary>
        /// 
        /// </summary>
        private Timer _updatePriority;

        /// <summary>
        /// 
        /// </summary>
        public ICommand UpdateDataCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public ICommand CloseIncidenceDialogCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public ICommand ModifyIncidenceDialogCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public ICommand SaveIncidenceCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public ICommand AddIncidenceCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public ICommand RefundCashDialogCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public ICommand OpenDialogExportCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public ICommand OpenOffDutyVehiclesDialog { get; }

        /// <summary>
        /// Campo que provee a la propiedad 'NewWhoReporting'.
        /// </summary>
        private String _newWhoReporting;

        /// <summary>
        /// Indica si la actualización de los autobuses sin conexión esta ocurriendo.
        /// </summary>
        private bool _busUpdating;

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
        public CctvReportsViewModel()
        {
            ViewModelService.Register(this);

            ReassignTechnician = new CommandBase(parameter =>
            {
                if (IncidencesOpened.Count == 0) return;
                ViewModelService.GetViewModel<AttendanceViewModel>()?.ReassignTechnician();
                AcabusControlCenterViewModel.ShowDialog("Reasignación completada.");
            });

            CopyingRowClipboardHandlerCommand = new CommandBase((parameter) =>
            {
                String incidenceData = SelectedIncidence?.ToReportString();

                if (!String.IsNullOrWhiteSpace(incidenceData))
                {
                    try
                    {
                        System.Windows.Forms.Clipboard.Clear();
                        System.Windows.Forms.Clipboard.SetDataObject(incidenceData.ToUpper());
                    }
                    catch { }
                }
            });

            UpdateDataCommand = new CommandBase(parameter =>
            {
                if ((parameter as Incidence).Status != IncidenceStatus.CLOSE) return;
                UpdateData();
            });

            CloseIncidenceDialogCommand = new CommandBase((parameter) =>
            {
                if (SelectedIncidence is null) return;

                AcabusControlCenterViewModel.ShowDialog(new CloseIncidenceView());
            });

            AddIncidenceCommand = new CommandBase((parameter) =>
            {
                AcabusControlCenterViewModel.ShowDialog(new AddIncidencesView() { DataContext = new AddIncidencesViewModel() { IsNewIncidences = true } });
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
               AcabusControlCenterViewModel.ShowDialog(new ModifyIncidenceView() { DataContext = parameter });
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

            LoadedHandlerCommand = new CommandBase((parameter) => InitAlarmsMonitor());
            OpenOffDutyVehiclesDialog = new CommandBase((parameter) =>
           {
               var dialogContent = new OffDutyVehicles.OffDutyVehiclesView();
               AcabusControlCenterViewModel.ShowDialog(dialogContent, (response) =>
               {
                   if (response.ToString().Equals("LISTO"))
                       InitBusAlarmsMonitor();
               });
           });

            Alarms.CollectionChanged += AlarmsCollectionChanged;
            BusDisconnectedAlarms.CollectionChanged += BusAlarmsCollectionChanged;
            Incidences.CollectionChanged += IncidenceCollectionChanged;

            Incidences.LoadFromDataBase();

            _updatePriority = new Timer((target) =>
             {
                 foreach (var item in IncidencesOpened)
                 {
                     if (item.Status == IncidenceStatus.UNCOMMIT && item.Priority != Priority.NONE)
                     {
                         item.Priority = Priority.NONE;
                         item.UpdatePriority();
                     }
                     if (item.Status == IncidenceStatus.UNCOMMIT || item.Priority == Priority.NONE) continue;
                     var time = DateTime.Now - item.StartDate;
                     var type = item.Device?.Type;
                     var maxLowPriority = type == DeviceType.DEVICE_BUS
                                ? AcabusData.TimeMaxLowPriorityIncidenceBus : AcabusData.TimeMaxLowPriorityIncidence;
                     var maxMediumPriority = type == DeviceType.DEVICE_BUS
                                ? AcabusData.TimeMaxMediumPriorityIncidenceBus : AcabusData.TimeMaxMediumPriorityIncidence;

                     if (time > maxMediumPriority)
                         item.Priority = Priority.HIGH;
                     else if (time > maxLowPriority && item.Priority < Priority.HIGH)
                         item.Priority = Priority.MEDIUM;

                     item.UpdatePriority();
                 }
             }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

        }

        public void UpdateData()
        {
            OnPropertyChanged("IncidencesOpened");
            OnPropertyChanged("IncidencesClosed");
        }

        public void ReloadData()
        {
            Incidences.Clear();
            Incidences.LoadFromDataBase();
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
                Trace.WriteLine("Actualizando autobuses sin conexión", "DEBUG");
                if (DateTime.Now.TimeOfDay > new TimeSpan(6, 0, 0))
                {
                    BusDisconnectedAlarms.GetBusDisconnectedAlarms();

                    Incidence[] incidences;

                    lock (Incidences)
                    {
                        incidences = new Incidence[Incidences.Count];
                        Incidences.CopyTo(incidences, 0);
                    }

                    foreach (var incidence in incidences)
                    {
                        /// Verificación de las incidencias de SIN CONEXIÓN DE DATOS por confirmar
                        if (incidence.Status == IncidenceStatus.UNCOMMIT && incidence.Description == "SIN CONEXIÓN DE DATOS"
                        && incidence.Location is Vehicle)
                            /// A pasado el tiempo para cerrar automáticamente ? 30 MIN
                            if ((DateTime.Now - incidence.FinishDate) > TimeSpan.FromMinutes(30))
                            {
                                incidence.Status = IncidenceStatus.CLOSE;
                                if (AcabusData.OffDutyVehicles.Where(vehicle
                                    => vehicle.EconomicNumber == (incidence.Location as Vehicle).EconomicNumber).Count > 0)
                                    incidence.Observations = "UNIDAD EN TALLER O SIN ENERGÍA";
                                else incidence.Observations = "SE REESTABLECE CONEXIÓN AUTOMATICAMENTE";
                                incidence.Update();
                                UpdateData();
                                continue;
                            }

                        /// Verificación de las incidencias ABIERTAS

                        if (incidence.Status != IncidenceStatus.OPEN || incidence.Description != "SIN CONEXIÓN DE DATOS"
                        || !(incidence.Location is Vehicle)) continue;

                        bool exists = false;

                        foreach (var bus in BusDisconnectedAlarms)
                            if (CctvService.Equals(bus, incidence))
                            {
                                exists = true;
                                break;
                            }

                        if (!exists)
                        {
                            incidence.Status = IncidenceStatus.UNCOMMIT;
                            incidence.Priority = Priority.NONE;
                            incidence.FinishDate = DateTime.Now;
                            incidence.Technician = "SISTEMA";
                            incidence.Update();
                        }
                    }

                }
                _busUpdating = false;
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
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
                            "SIN CONEXIÓN DE DATOS",
                            AcabusData.FindDeviceInVehicle((deviceBus) => deviceBus.Description.Contains("PC ABORDO")),
                            DateTime.Now,
                            alarm.Priority,
                            AcabusData.FindVehicle((vehicle) => vehicle.EconomicNumber == alarm.EconomicNumber),
                            "SISTEMA"
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
                    if (item.Status == IncidenceStatus.OPEN)
                        AcabusControlCenterViewModel.AddNotify(String.Format("{0:dd/MM/yyyy HH:mm:ss} {1} - {2}",
                            item.StartDate,
                            item.Location is Vehicle
                                ? String.Format("{0} {1}", item.Location, item.Device)
                                : item.Device.ToString(),
                            item.Description));
                    item.Save();
                }
            }
            OnPropertyChanged("IncidencesOpened");
            OnPropertyChanged("IncidencesClosed");
        }

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
                    Boolean exists = false;
                    foreach (var incidence in Incidences)
                        if (exists = CctvService.Equals(alarm, incidence)) break;
                    if (!exists)
                        Incidences.CreateIncidence(
                            alarm.Description,
                            alarm.Device,
                            alarm.DateTime,
                            alarm.Priority,
                            alarm.Device.Station,
                            "SISTEMA"
                        );
                }
            }
        }

        /// <summary>
        /// Inicializa el monitor de alarmas.
        /// </summary>
        private void InitAlarmsMonitor()
        {
            _alarmsMonitor = new Timer(delegate
            {
                Trace.WriteLine("Actualizando alarmas", "DEBUG");
                Alarms.GetAlarms();
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(0.2));

            if (_busAlarmsMonitor == null && AcabusData.OffDutyVehicles.Count == 0) return;

            InitBusAlarmsMonitor();
        }

        ~CctvReportsViewModel()
        {
            ViewModelService.UnRegister(this);
        }

    }
}
