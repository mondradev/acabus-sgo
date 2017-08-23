using Acabus.Models;
using Acabus.Modules.Attendances.ViewModels;
using Acabus.Modules.CctvReports.Models;
using Acabus.Modules.CctvReports.Services;
using Acabus.Utils.Mvvm;
using Acabus.Window;
using InnSyTech.Standard.Utils;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Acabus.Modules.CctvReports.ViewModels
{
    public sealed class MultiIncidenceViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="GlobalObservations" />.
        /// </summary>
        private String _globalObservations;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedDescription" />.
        /// </summary>
        private String _selectedDescription;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedDevices" />.
        /// </summary>
        private ObservableCollection<MultiIncidenceItem> _selectedDevices;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedDeviceType" />.
        /// </summary>
        private DeviceType? _selectedDeviceType;

        private ObservableCollection<DeviceType> _selectedDeviceTypes;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedPriority" />.
        /// </summary>
        private Priority _selectedPriority;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedStation" />.
        /// </summary>
        private Station _selectedStation;

        private ObservableCollection<Station> _selectedStations;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedWhoReporting" />.
        /// </summary>
        private String _selectedWhoReporting;

        public MultiIncidenceViewModel()
        {
            CloseCommand = DialogHost.CloseDialogCommand;

            AddCommand = new CommandBase(AddCommandExecute, AddCommandCanExec);

            DiscardCommand = new CommandBase(parameter =>
            {
                if (!(parameter is MultiIncidenceItem)) return;

                _selectedDevices.Remove(parameter as MultiIncidenceItem);

                OnPropertyChanged(nameof(SelectedDevices));
                OnPropertyChanged(nameof(DeviceFaults));
            });

            DiscardStationCommand = new CommandBase(parameter =>
            {
                if (!(parameter is Station)) return;

                _selectedStations.Remove(parameter as Station);

                UpdateDeviceList();

                OnPropertyChanged(nameof(AllStations));
            });

            DiscardTypeCommand = new CommandBase(parameter =>
            {
                if (!(parameter is DeviceType)) return;

                _selectedDeviceTypes.Remove((DeviceType)parameter);

                UpdateDeviceList();

                OnPropertyChanged(nameof(AllDeviceTypes));
            });
        }

        public CommandBase AddCommand { get; private set; }

        public IEnumerable<DeviceType> AllDeviceTypes
            => Enum.GetValues(typeof(DeviceType)).Cast<DeviceType>()
            .Where(dt => !SelectedDeviceTypes.Contains(dt)
                && !new[] {
                    DeviceType.TS,
                    DeviceType.TSI,
                    DeviceType.TD,
                    DeviceType.APP,
                    DeviceType.CONT,
                    DeviceType.DB,
                    DeviceType.DSPB,
                    DeviceType.MON,
                    DeviceType.MRV,
                    DeviceType.PCA,
                    DeviceType.PDE,
                    DeviceType.TA,
                    DeviceType.NONE
                }.Contains(dt));

        public IEnumerable<Station> AllStations => Core.DataAccess.AcabusData.AllStations
            .Where(s => !SelectedStations.Contains(s));

        public IEnumerable<String> Business => DataAccess.AcabusData.Companies;

        public ICommand CloseCommand { get; }

        public IEnumerable<String> DeviceFaults
                            => Core.DataAccess.AcabusData.AllFaults.Where(f => GetDevicesTypes().Contains(f.Category.DeviceType))
                            .GroupBy(f => f.Description)
                            .Where(g => g.Count() == GetDevicesTypes().Count())
                            .Select(g => g.FirstOrDefault().Description);

        public ICommand DiscardCommand { get; }
        public ICommand DiscardStationCommand { get; }
        public ICommand DiscardTypeCommand { get; }

        /// <summary>
        /// Obtiene o establece las observaciones globales de la incidencia.
        /// </summary>
        public String GlobalObservations {
            get => _globalObservations;
            set {
                _globalObservations = value;
                OnPropertyChanged(nameof(GlobalObservations));
                if (!String.IsNullOrEmpty(_globalObservations))
                    foreach (var device in SelectedDevices)
                        device.Observations = _globalObservations;
            }
        }

        public IEnumerable<Priority> Priorities
            => Enum.GetValues(typeof(Priority)).Cast<Priority>();

        /// <summary>
        /// Obtiene o establece la descripción de la incidencia.
        /// </summary>
        public String SelectedDescription {
            get => _selectedDescription;
            set {
                _selectedDescription = value;
                OnPropertyChanged(nameof(SelectedDescription));
            }
        }

        /// <summary>
        /// Obtiene una lista de los equipos a aplicar la falla descrita.
        /// </summary>
        public ObservableCollection<MultiIncidenceItem> SelectedDevices
            => _selectedDevices ?? (_selectedDevices = new ObservableCollection<MultiIncidenceItem>());

        /// <summary>
        /// Obtiene o establece el tipo de equipo seleccionado.
        /// </summary>
        public DeviceType? SelectedDeviceType {
            get => _selectedDeviceType;
            set {
                _selectedDeviceType = value;
                OnPropertyChanged(nameof(SelectedDeviceType));

                if (value != null && value != DeviceType.NONE && !SelectedDeviceTypes.Contains(value.Value))
                {
                    SelectedDeviceTypes.Add(value.Value);

                    UpdateDeviceList();

                    OnPropertyChanged(nameof(AllDeviceTypes));
                }
            }
        }

        public ObservableCollection<DeviceType> SelectedDeviceTypes
                    => _selectedDeviceTypes ?? (_selectedDeviceTypes = new ObservableCollection<DeviceType>());

        /// <summary>
        /// Obtiene o establece la prioridad de las incidencias.
        /// </summary>
        public Priority SelectedPriority {
            get => _selectedPriority;
            set {
                _selectedPriority = value;
                OnPropertyChanged(nameof(SelectedPriority));
            }
        }

        /// <summary>
        /// Obtiene o establece la estación seleccionada.
        /// </summary>
        public Station SelectedStation {
            get => _selectedStation;
            set {
                _selectedStation = value;
                OnPropertyChanged(nameof(SelectedStation));

                if (value != null)
                {
                    SelectedStations.Add(value);

                    UpdateDeviceList();

                    OnPropertyChanged(nameof(AllStations));
                }
            }
        }

        public ObservableCollection<Station> SelectedStations
            => _selectedStations ?? (_selectedStations = new ObservableCollection<Station>());

        /// <summary>
        /// Obtiene o establece quien reporta las incidencias.
        /// </summary>
        public String SelectedWhoReporting {
            get => _selectedWhoReporting;
            set {
                _selectedWhoReporting = value;
                OnPropertyChanged(nameof(SelectedWhoReporting));
            }
        }

        public IEnumerable<Station> Stations => Core.DataAccess.AcabusData.AllStations;

        protected override void OnValidation(string propertyName)
        {
            switch (propertyName)
            {
                case "SelectedWhoReporting":
                    if (String.IsNullOrEmpty(SelectedWhoReporting))
                        AddError("SelectedWhoReporting", "Falta ingresar quién reporta.");
                    break;

                case "SelectedDescription":
                    if (String.IsNullOrEmpty(SelectedDescription))
                        if (SelectedDevices.Count == 0)
                            AddError("SelectedDescription", "Falta seleccionar los equipos a reportar.");
                        else
                            AddError("SelectedDescription", "Falta ingresar la descripción de la incidencia.");
                    break;

                case "SelectedStations":
                    if (SelectedStations.Count == 0)
                        AddError("SelectedStations", "Falta seleccionar alguna estación.");
                    break;

                case "SelectedDeviceTypes":
                    if (SelectedDeviceTypes.Count == 0)
                        AddError("SelectedDeviceTypes", "Falta seleccionar al menos un tipo de equipo.");
                    break;

                case "SelectedDevices":
                    if (SelectedDevices.Count == 0)
                        AddError("SelectedDevices", "No hay equipos agregados.");
                    break;

                case "SelectedPriority":
                    if (SelectedPriority == Priority.NONE)
                        AddError("SelectedPriority", "Debe asignar una prioridad a la incidencia.");
                    break;
            }
        }

        private bool AddCommandCanExec(object parameter)
        {
            if (!Validate()) return false;

            var incidences = ViewModelService.GetViewModel<CctvReportsViewModel>().Incidences.Where(i => i.Status != IncidenceStatus.CLOSE);
            incidences = incidences.Where(i => SelectedDevices.Where(d => d.SelectedDevice == i.Device).Count() > 0 && i.Description.Description == SelectedDescription);

            if (incidences.Count() > 0)
                AddError("SelectedDescription", $"Ya existe una incidencia abierta igual para: {incidences.First().Folio}.");

            return incidences.Count() == 0;
        }

        private void AddCommandExecute(Object parameter)
        {
            var incidences = ViewModelService.GetViewModel<CctvReportsViewModel>().Incidences;
            Application.Current.Dispatcher.Invoke(() =>
            {
                List<Incidence> registeredIncidence = new List<Incidence>();
                foreach (var device in SelectedDevices)
                {
                    var incidence = CctvService.CreateIncidence(incidences,
                       GetDeviceFault(SelectedDescription, device.SelectedDevice.Type),
                        device.SelectedDevice,
                        DateTime.Now,
                         SelectedPriority,
                        SelectedWhoReporting
                    );

                    incidence.Observations = device.Observations;
                    registeredIncidence.Add(incidence);
                }
                try
                {
                    StringBuilder openedIncidence = new StringBuilder();
                    foreach (var group in registeredIncidence.GroupBy(i => i.AssignedAttendance))
                    {
                        foreach (Incidence incidence in group)
                            openedIncidence.AppendLine(incidence.ToReportString().Split('\n')?[0]
                                + (String.IsNullOrEmpty(incidence.Observations) ? String.Empty : String.Format("\n*OBSERVACIONES:* {0}", incidence.Observations)));
                        if (group.Key?.Technician != null)
                            openedIncidence.AppendFormat("*ASIGNADO:* {0}", group.Key.Technician);
                        openedIncidence.AppendLine();
                        openedIncidence.AppendLine();
                    }

                    try
                    {
                        System.Windows.Forms.Clipboard.Clear();
                        System.Windows.Forms.Clipboard.SetDataObject(openedIncidence.ToString());
                    }
                    catch { }
                    AcabusControlCenterViewModel.ShowDialog("Incidencias agregadas y copiada al portapapeles.");
                }
                catch { }
            });

            ViewModelService.GetViewModel<AttendanceViewModel>()?.UpdateCounters();
            CloseCommand.Execute(parameter);
        }

        private DeviceFault GetDeviceFault(string selectedDescription, DeviceType type)
            => Core.DataAccess.AcabusData.AllFaults.FirstOrDefault(f => f.Description == selectedDescription && f.Category?.DeviceType == type);

        private IEnumerable<DeviceType> GetDevicesTypes()
                                            => SelectedDevices.Select(m => m.SelectedDevice.Type).Distinct();

        private void UpdateDeviceList()
        {
            SelectedDevices.Clear();

            if (SelectedDeviceTypes.Count == 0) return;
            if (SelectedStations.Count == 0) return;

            foreach (var item in _selectedStations.Select(s => s.Devices).Merge()
                .Where(d => SelectedDeviceTypes.Contains(d.Type) || (SelectedDeviceTypes.Contains(DeviceType.TOR) && new[] { DeviceType.TD, DeviceType.TS, DeviceType.TSI }.Contains(d.Type))))
                SelectedDevices.Add(new MultiIncidenceItem() { SelectedDevice = item, Observations = "" });
            OnPropertyChanged(nameof(SelectedDevices));
            OnPropertyChanged(nameof(DeviceFaults));
        }

        private Boolean Validate()
        {
            ValidateProperty("SelectedWhoReporting");
            ValidateProperty("SelectedDescription");
            ValidateProperty("SelectedStation");
            ValidateProperty("SelectedStations");
            ValidateProperty("SelectedDeviceTypes");
            ValidateProperty("SelectedDevices");
            ValidateProperty("SelectedPriority");

            if (HasErrors) return false;

            return true;
        }

        public sealed class MultiIncidenceItem : NotifyPropertyChanged
        {
            /// <summary>
            /// Campo que provee a la propiedad <see cref="Observations" />.
            /// </summary>
            private String _observations;

            /// <summary>
            /// Campo que provee a la propiedad <see cref="SelectedDevice" />.
            /// </summary>
            private Device _selectedDevice;

            /// <summary>
            /// Obtiene o establece las observaciones de la incidencia.
            /// </summary>
            public String Observations {
                get => _observations;
                set {
                    _observations = value;
                    OnPropertyChanged(nameof(Observations));
                }
            }

            /// <summary>
            /// Obtiene o establece el valor de esta propiedad.
            /// </summary>
            public Device SelectedDevice {
                get => _selectedDevice;
                set {
                    _selectedDevice = value;
                    OnPropertyChanged(nameof(SelectedDevice));
                }
            }
        }
    }
}