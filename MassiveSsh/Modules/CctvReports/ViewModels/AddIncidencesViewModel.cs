using Acabus.DataAccess;
using Acabus.Models;
using Acabus.Modules.Attendances.ViewModels;
using Acabus.Modules.CctvReports.Models;
using Acabus.Modules.CctvReports.Services;
using Acabus.Utils.Mvvm;
using Acabus.Window;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Acabus.Modules.CctvReports
{
    public sealed class AddIncidencesViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Incidences'.
        /// </summary>
        private ObservableCollection<Incidence> _incidences;

        /// <summary>
        /// Campo que provee a la propiedad 'IsBusIncidences'.
        /// </summary>
        private Boolean _isBusIncidences;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedDescription'.
        /// </summary>
        private DeviceFault _selectedDescription;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedDevice'.
        /// </summary>
        private Device _selectedDevice;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedLocation'.
        /// </summary>
        private AssignableSection _selectedLocation;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedPriority'.
        /// </summary>
        private Priority _selectedPriority;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedVehicle'.
        /// </summary>
        private Vehicle _selectedVehicle;

        /// <summary>
        /// Campo que provee a la propiedad 'StartTime'.
        /// </summary>
        private TimeSpan _startTime;

        /// <summary>
        /// Campo que provee a la propiedad 'WhoReport'.
        /// </summary>
        private String _whoReporting;

        /// <summary>
        ///
        /// </summary>
        public AddIncidencesViewModel()
        {
            AddCommand = new CommandBase(AddCommandExecute, AddCommandCanExec);
            CloseCommand = DialogHost.CloseDialogCommand;

            _startTime = DateTime.Now.TimeOfDay;
        }

        /// <summary>
        ///
        /// </summary>
        public ICommand AddCommand { get; }

        /// <summary>
        /// Obtiene una lista de empresas que realizan reportes habitualmente.
        /// </summary>
        public ObservableCollection<String> Business => AcabusData.Companies;

        /// <summary>
        ///
        /// </summary>
        public ICommand CloseCommand { get; }

        /// <summary>
        /// Obtiene una lista de todas las fallas disponibles.
        /// </summary>
        public IEnumerable<DeviceFault> DeviceFaults {
            get {
                if (SelectedDevice is null)
                    return null;

                return Core.DataAccess.AcabusData.AllFaults
                    .Where(fault => (fault as DeviceFault).Category.DeviceType == SelectedDevice.Type);
            }
        }

        /// <summary>
        /// Obtiene una lista de equipos disponibles en la estación.
        /// </summary>
        public IEnumerable<Device> Devices {
            get {
                if (IsRefundOfMoney)
                    return SelectedLocation is null
                        ? Core.DataAccess.AcabusData.AllDevices
                                    .Where(device => (device as Device).Type == DeviceType.KVR)
                        : Core.DataAccess.AcabusData.AllDevices
                                    .Where(device => (device as Device).Station?.ID == (SelectedLocation as Station).ID)
                                    .Where(device => (device as Device).Type == DeviceType.KVR);
                if (IsBusIncidences)
                    return SelectedVehicle is null
                        ? null
                        : Core.DataAccess.AcabusData.AllDevices
                                    .Where(device => (device as Device).Vehicle?.EconomicNumber == SelectedVehicle?.EconomicNumber);
                else
                    return SelectedLocation is null
                        ? Core.DataAccess.AcabusData.AllDevices
                                    .Where(device => (device as Device).Vehicle is null)
                        : Core.DataAccess.AcabusData.AllDevices
                                    .Where(device => (device as Device).Station?.ID == (SelectedLocation as Station).ID);
            }
        }

        /// <summary>
        /// Obtiene el nombre del cuadro de texto para vehículo o equipo.
        /// </summary>
        public String HeaderTextDeviceOrVehicle => IsBusIncidences ? "Equipo abordo" : "Equipo";

        /// <summary>
        /// Obtiene el nombre del cuadro de texto para ubicación.
        /// </summary>
        public String HeaderTextRouteOrStation => IsBusIncidences ? "Ruta" : "Estación";

        /// <summary>
        /// Obtiene o establece la lista de incidencias.
        /// </summary>
        public ObservableCollection<Incidence> Incidences {
            get => _incidences;
            set {
                _incidences = value;
                OnPropertyChanged("Incidences");
            }
        }

        /// <summary>
        /// Obtiene o establece si la incidencias es originada en un autobus.
        /// </summary>
        public Boolean IsBusIncidences {
            get => _isBusIncidences;
            set {
                _isBusIncidences = value;
                OnPropertyChanged("IsBusIncidences");
                OnPropertyChanged("Locations");
                OnPropertyChanged("Devices");
                OnPropertyChanged("Vehicles");
                OnPropertyChanged("HeaderTextDeviceOrVehicle");
                OnPropertyChanged("HeaderTextRouteOrStation");
                ClearErrors();
            }
        }

        /// <summary>
        /// Obtiene una lista de todas las ubicaciones disponibles.
        /// </summary>
        public IEnumerable<AssignableSection> Locations {
            get {
                if (IsBusIncidences)
                    return Core.DataAccess.AcabusData.AllRoutes.Cast<AssignableSection>();

                return Core.DataAccess.AcabusData.AllStations.Cast<AssignableSection>();
            }
        }

        /// <summary>
        /// Obtiene una lista de las prioridades de incidencias.
        /// </summary>
        public IEnumerable<Priority> Priorities => Enum.GetValues(typeof(Priority)).Cast<Priority>();

        /// <summary>
        /// Obtiene o establece la descripción de la incidencia.
        /// </summary>
        public DeviceFault SelectedDescription {
            get => _selectedDescription;
            set {
                _selectedDescription = value;
                OnPropertyChanged("SelectedDescription");
            }
        }

        /// <summary>
        /// Obtiene o establece el equipo que presenta la incidencia.
        /// </summary>
        public Device SelectedDevice {
            get => _selectedDevice;
            set {
                _selectedDevice = value;
                OnPropertyChanged("SelectedDevice");
                OnPropertyChanged("DeviceFaults");
                if (!IsBusIncidences && SelectedDevice != null && !SelectedDevice.Station.Equals(SelectedLocation))
                {
                    _selectedLocation = SelectedDevice.Station;
                    OnPropertyChanged(nameof(SelectedLocation));
                }
            }
        }

        /// <summary>
        /// Obtiene o establece la ubicación de la incidencia.
        /// </summary>
        public AssignableSection SelectedLocation {
            get => _selectedLocation;
            set {
                _selectedLocation = value;
                OnPropertyChanged("SelectedLocation");
                OnPropertyChanged("Devices");
                OnPropertyChanged("Vehicles");
            }
        }

        /// <summary>
        /// Obtiene o establece la prioridad de la incidencia.
        /// </summary>
        public Priority SelectedPriority {
            get => _selectedPriority;
            set {
                _selectedPriority = value;
                OnPropertyChanged("SelectedPriority");
            }
        }

        /// <summary>
        /// Obtiene o establece el autobus seleccionado.
        /// </summary>
        public Vehicle SelectedVehicle {
            get => _selectedVehicle;
            set {
                _selectedVehicle = value;
                OnPropertyChanged("SelectedVehicle");
                OnPropertyChanged("Devices");
                if (value != null)
                {
                    _selectedLocation = value.Route;
                    OnPropertyChanged("SelectedLocation");
                }
            }
        }

        /// <summary>
        /// Obtiene o establece el tiempo de inicio de la incidencia.
        /// </summary>
        public TimeSpan StartTime {
            get => _startTime;
            set {
                _startTime = value;
                OnPropertyChanged("StartTime");
            }
        }

        /// <summary>
        /// Obtiene o establece el listado de los autobuses.
        /// </summary>
        public IEnumerable<Vehicle> Vehicles {
            get {
                if (!IsBusIncidences)
                    return null;
                if (SelectedLocation is null)
                    return Core.DataAccess.AcabusData.AllVehicles;
                else
                    return Core.DataAccess.AcabusData.AllVehicles
                        .Where(vehicle => (vehicle as Vehicle).Route?.ID == (SelectedLocation as Route)?.ID);
            }
        }

        /// <summary>
        /// Obtiene o establece quien reporta la incidencia.
        /// </summary>
        public String WhoReporting {
            get => _whoReporting;
            set {
                _whoReporting = value;
                OnPropertyChanged("WhoReporting");
            }
        }

        #region RefundOfCash

        /// <summary>
        /// Campo que provee a la propiedad 'CashDestiny'.
        /// </summary>
        private CashDestiny _cashDestiny;

        /// <summary>
        /// Campo que provee a la propiedad 'IsMoney'.
        /// </summary>
        private Boolean _isMoney;

        /// <summary>
        /// Campo que provee a la propiedad 'IsNewIncidences'.
        /// </summary>
        private Boolean _isNewIncidences;

        /// <summary>
        /// Campo que provee a la propiedad 'Observations'.
        /// </summary>
        private String _observations;

        /// <summary>
        /// Campo que provee a la propiedad 'Quantity'.
        /// </summary>
        private String _quantity;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedTechnician'.
        /// </summary>
        private Technician _selectedTechnician;

        /// <summary>
        /// Campo que provee a la propiedad 'TitleSection'.
        /// </summary>
        private String _titleSection;

        /// <summary>
        /// Obtiene una lista con los destino de dinero.
        /// </summary>
        public IEnumerable<CashDestiny> CashDestinies
            => Core.DataAccess.AcabusData.AllCashDestinies
            .Where(cashDestiny =>
            {
                if (IsMoney && (cashDestiny as CashDestiny).CashType == CashType.MONEY)
                    return true;
                if (!IsMoney && (cashDestiny as CashDestiny).CashType == CashType.BILL)
                    return true;
                return false;
            });

        /// <summary>
        /// Obtiene o establece el destino del dinero.
        /// </summary>
        public CashDestiny CashDestiny {
            get => _cashDestiny;
            set {
                _cashDestiny = value;
                OnPropertyChanged("CashDestiny");
                SetDescriptionAndObservation();
            }
        }

        /// <summary>
        /// Obtiene el titulo del tipo de dinero.
        /// </summary>
        public String CashTypeName => IsMoney ? "MONEDAS" : "BILLETES";

        /// <summary>
        /// Obtiene o establece si la devolución de dinero son monedas.
        /// </summary>
        public Boolean IsMoney {
            get => _isMoney;
            set {
                _isMoney = value;
                OnPropertyChanged("IsMoney");
                OnPropertyChanged("CashDestinies");
                OnPropertyChanged("CashTypeName");
                SetDescriptionAndObservation();
            }
        }

        /// <summary>
        /// Obtiene o establece si se está generando una incidencia nueva.
        /// </summary>
        public Boolean IsNewIncidences {
            get => _isNewIncidences;
            set {
                _isNewIncidences = value;
                OnPropertyChanged("IsNewIncidences");
                _titleSection = _isNewIncidences ? "Nueva incidencia" : "Devolución de dinero";
                SetDescriptionAndObservation();
            }
        }

        /// <summary>
        /// Obtiene o establece si se está devolviendo dinero.
        /// </summary>
        public Boolean IsRefundOfMoney {
            get => !IsNewIncidences;
            set {
                IsNewIncidences = !value;
                OnPropertyChanged("IsRefundOfMoney");
                SetDescriptionAndObservation();
            }
        }

        /// <summary>
        /// Obtiene o establece las observaciones de la devolución.
        /// </summary>
        public String Observations {
            get => _observations;
            set {
                _observations = value;
                OnPropertyChanged("Observations");
            }
        }

        /// <summary>
        /// Obtiene o establece la cantidad de dinero a devolver.
        /// </summary>
        public String Quantity {
            get => _quantity;
            set {
                _quantity = value;
                OnPropertyChanged("Quantity");
                SetDescriptionAndObservation();
            }
        }

        /// <summary>
        /// Obtiene o establece el nombre de la persona que solucionó la incidencia.
        /// </summary>
        public Technician SelectedTechnician {
            get => _selectedTechnician;
            set {
                _selectedTechnician = value;
                OnPropertyChanged("SelectedTechnician");
            }
        }

        /// <summary>
        /// Obtiene una lista de los técnicos seleccionables.
        /// </summary>
        public IEnumerable<Technician> Technicians => Core.DataAccess.AcabusData.AllTechnicians
            .Where(technicia => technicia.Name != "SISTEMA" && technicia.Enabled);

        /// <summary>
        /// Obtiene el titulo del cuadro de dialogo
        /// </summary>
        public String TitleSection => _titleSection;

        /// <summary>
        ///
        /// </summary>
        private void SetDescriptionAndObservation()
        {
            Single.TryParse(Quantity, out float quantity);
            if (IsRefundOfMoney)
            {
                var faults = Core.DataAccess.AcabusData.AllFaults
                     .Where(fault => (fault as DeviceFault).Category?.DeviceType == DeviceType.KVR);

                SelectedDescription = IsMoney
                     ? faults.Where(fault => (fault as DeviceFault).Description
                     .Equals("MONEDAS ENCONTRADAS EN TOLVA, NOTIFICAR EL TOTAL E INTRODUCIR A LA ALCANCÍA"))
                     .FirstOrDefault() as DeviceFault
                     : faults.Where(fault => (fault as DeviceFault).Description
                     .Equals("BILLETE ATASCADO, NOTIFICAR EL TOTAL Y CANALIZAR EL DINERO A CAU"))
                     .FirstOrDefault() as DeviceFault;

                Observations = String.Format("DEVOLUCIÓN DE {0} (${1:F2}) A {2}", IsMoney ? "MONEDAS" : "BILLETE",
                    quantity, CashDestiny?.Description);
            }
        }

        #endregion RefundOfCash

        protected override void OnValidation(string propertyName)
        {
            switch (propertyName)
            {
                case "WhoReporting":
                    if (String.IsNullOrEmpty(WhoReporting))
                        AddError("WhoReporting", "Falta ingresar quién reporta");
                    break;

                case "SelectedDescription":
                    if (SelectedDescription is null)
                        if (SelectedDevice is null)
                            AddError("SelectedDescription", "Seleccione primero el equipo que presenta la incidencia.");
                        else
                            AddError("SelectedDescription", "Falta ingresar la descripción de la incidencia.");
                    break;

                case "SelectedLocation":
                    if (String.IsNullOrEmpty(SelectedLocation?.ToString()))
                        AddError("SelectedLocation", String.Format("Falta seleccionar {0}.", IsBusIncidences ? "la ruta" : "la estación"));
                    break;

                case "SelectedVehicle":
                    if (IsBusIncidences && String.IsNullOrEmpty(SelectedVehicle?.ToString()))
                        AddError("SelectedVehicle", "Falta seleccionar la unidad.");
                    break;

                case "SelectedDevice":
                    if (String.IsNullOrEmpty(SelectedDevice?.ToString()))
                        AddError("SelectedDevice", "Falta seleccionar el equipo.");
                    if (IsRefundOfMoney && !(SelectedDevice?.Type == DeviceType.KVR))
                        AddError("SelectedDevice", "El equipo debe ser un KVR");
                    break;

                case "SelectedPriority":
                    if (IsNewIncidences && SelectedPriority == Priority.NONE)
                        AddError("SelectedPriority", "Debe asignar una prioridad a la incidencia.");
                    break;

                case "CashDestiny":
                    if (IsRefundOfMoney && String.IsNullOrEmpty(CashDestiny?.ToString()))
                        AddError("CashDestiny", "Falta agregar el destino del dinero.");
                    break;

                case "Quantity":
                    if (IsRefundOfMoney && !Single.TryParse(Quantity, out float result))
                        AddError("Quantity", "Falta agregar la cantidad.");
                    break;

                case "SelectedTechnician":
                    if (IsRefundOfMoney && SelectedTechnician is null)
                        AddError("SelectedTechnician", "Falta agregar el técnico que hará la devolución.");
                    break;
            }
        }

        private bool AddCommandCanExec(object parameter)
        {
            if (!Validate()) return false;
            if (IsRefundOfMoney) return true;

            var incidences = ViewModelService.GetViewModel<CctvReportsViewModel>().Incidences;
            bool exists = false;
            foreach (var incidence in incidences)
            {
                if (incidence.Status == IncidenceStatus.CLOSE) continue;

                if (exists = (incidence.Description?.Category?.ID == SelectedDescription?.Category?.ID
                    && incidence.Device == SelectedDevice)) break;
            }
            if (exists)
                AddError("SelectedDescription", "Ya existe una incidencia abierta igual para el equipo");

            return !exists;
        }

        private void AddCommandExecute(Object parameter)
        {
            var incidences = ViewModelService.GetViewModel<CctvReportsViewModel>().Incidences;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var incidence = CctvService.CreateIncidence(incidences,
                    SelectedDescription,
                    SelectedDevice,
                    DateTime.Now.Date.AddTicks(StartTime.Ticks),
                    IsRefundOfMoney ? Priority.NONE : SelectedPriority,
                    WhoReporting
                );

                incidence.Observations = Observations;

                if (IsRefundOfMoney)
                {
                    incidence.Status = CashDestiny.Description == "CAU" ? IncidenceStatus.UNCOMMIT : IncidenceStatus.CLOSE;
                    incidence.Technician = SelectedTechnician;
                    incidence.FinishDate = CashDestiny.Description == "CAU" ? null : (DateTime?)incidence.StartDate;
                    var refundOfMoney = new RefundOfMoney(incidence)
                    {
                        Quantity = Single.Parse(Quantity),
                        CashDestiny = CashDestiny,
                        Status = CashDestiny?.Description == "CAU" ? RefundOfMoneyStatus.UNCOMMIT : RefundOfMoneyStatus.COMMIT,
                        RefundDate = CashDestiny?.Description == "CAU" ? null : (DateTime?)incidence.StartDate
                    };
                    if (refundOfMoney.Save() && incidence.Update())
                    {
                        ViewModelService.GetViewModel<CctvReportsViewModel>().UpdateData();
                        AcabusControlCenterViewModel.ShowDialog("Devolución registrada correctamente.");
                    }
                }
                else
                {
                    try
                    {
                        Clipboard.Clear();
                        Clipboard.SetDataObject(incidence.ToReportString().ToUpper());
                        AcabusControlCenterViewModel.ShowDialog("Incidencia agregada y copiada al portapapeles.");
                    }
                    catch { }
                }
            });
            ViewModelService.GetViewModel<AttendanceViewModel>()?.UpdateCounters();
            CloseCommand.Execute(parameter);
        }

        private Boolean Validate()
        {
            ValidateProperty("WhoReporting");
            ValidateProperty("SelectedDescription");
            ValidateProperty("SelectedLocation");
            ValidateProperty("SelectedDevice");
            ValidateProperty("SelectedPriority");

            if (IsBusIncidences)
                ValidateProperty("SelectedVehicle");

            if (IsRefundOfMoney)
            {
                ValidateProperty("CashDestiny");
                ValidateProperty("Quantity");
                ValidateProperty("SelectedTechnician");
            }

            if (HasErrors) return false;

            return true;
        }
    }
}