using Acabus.DataAccess;
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
using System.Windows;
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
            => (ObservableCollection<Incidence>)Util.SelectFromList(Incidences, (incidence)
                => incidence.Status == IncidenceStatus.OPEN);

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
            => (ObservableCollection<Incidence>)Util.SelectFromList(Incidences, (incidence)
                => incidence.Status == IncidenceStatus.CLOSE);

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
            CopyingRowClipboardHandlerCommand = new CommandBase((parameter) =>
            {
                String incidenceData = SelectedIncidence?.ToReportString();

                if (!String.IsNullOrWhiteSpace(incidenceData))
                {
                    Clipboard.Clear();
                    Clipboard.SetDataObject(incidenceData.ToUpper());
                }
            });

            UpdateDataCommand = new CommandBase((parameter) =>
            {
                if ((parameter as Incidence).Status != IncidenceStatus.CLOSE) return;
                SelectedIncidence = null;
                OnPropertyChanged("IncidencesOpened");
                OnPropertyChanged("IncidencesClosed");
            });

            CloseIncidenceDialogCommand = new CommandBase((parameter) =>
            {
                if (SelectedIncidence is null) return;

                AcabusControlCenterViewModel.ShowDialog(new CloseIncidenceView());
            });

            AddIncidenceCommand = new CommandBase((parameter) =>
            {
                if (parameter is null) return;
                AcabusControlCenterViewModel.ShowDialog(new AddIncidencesView() { DataContext = parameter });
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
                if (String.IsNullOrEmpty(NewWhoReporting)
                    || SelectedIncidence.WhoReporting.Equals(NewWhoReporting))
                    AddError("NewWhoReporting", "Seleccione una entidad diferente a la que reporta previamente");

                if (HasErrors) return;

                SelectedIncidence.WhoReporting = NewWhoReporting;
                SelectedIncidence.SaveInDataBase();
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
                     var time = DateTime.Now - item.StartDate;
                     var type = item.Device?.Type;
                     var maxLowPriority = type == Acabus.Models.DeviceType.VEHICLE
                                ? AcabusData.TimeMaxLowPriorityIncidenceBus : AcabusData.TimeMaxLowPriorityIncidence;
                     var maxMediumPriority = type == Acabus.Models.DeviceType.VEHICLE
                                ? AcabusData.TimeMaxMediumPriorityIncidenceBus : AcabusData.TimeMaxMediumPriorityIncidence;

                     if (time > maxMediumPriority)
                         item.Priority = Acabus.Models.Priority.HIGH;
                     else if (time > maxLowPriority && item.Priority < Acabus.Models.Priority.HIGH)
                         item.Priority = Acabus.Models.Priority.MEDIUM;

                     item.SaveInDataBase();
                 }
             }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

        }

        /// <summary>
        /// 
        /// </summary>
        private void InitBusAlarmsMonitor()
        {
            if (_busAlarmsMonitor != null) return;

            _busAlarmsMonitor = new Timer(delegate
            {
                Trace.WriteLine("Actualizando autobuses sin conexión", "DEBUG");
                if (DateTime.Now.TimeOfDay > new TimeSpan(5, 30, 0))
                    BusDisconnectedAlarms.GetBusDisconnectedAlarms();
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(0.5));
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
                            AcabusData.FindVehicle((vehicle) => vehicle.EconomicNumber == alarm.EconomicNumber),
                            DateTime.Now,
                            alarm.Priority,
                            AcabusData.FindVehicle((vehicle) => vehicle.EconomicNumber == alarm.EconomicNumber)?.Route,
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
                            item.Device is null
                                ? item.Location.ToString()
                                : item.Device.ToString(),
                            item.Description));
                    item.SaveInDataBase();
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
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(0.5));

            if (_busAlarmsMonitor == null && AcabusData.OffDutyVehicles.Count == 0) return;

            InitBusAlarmsMonitor();
        }

    }
}
