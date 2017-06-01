using Acabus.DataAccess;
using Acabus.Models;
using Acabus.Modules.Attendances.ViewModels;
using Acabus.Modules.CctvReports.Models;
using Acabus.Modules.CctvReports.Services;
using Acabus.Utils;
using Acabus.Utils.Mvvm;
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
        /// Campo que provee a la propiedad 'Description'.
        /// </summary>
        private String _description;

        /// <summary>
        /// Campo que provee a la propiedad 'Device'.
        /// </summary>
        private Device _device;

        /// <summary>
        /// Campo que provee a la propiedad 'Incidences'.
        /// </summary>
        private ObservableCollection<Incidence> _incidences;

        /// <summary>
        /// Campo que provee a la propiedad 'IsBusIncidences'.
        /// </summary>
        private Boolean _isBusIncidences;

        /// <summary>
        /// Campo que provee a la propiedad 'Location'.
        /// </summary>
        private Location _location;

        /// <summary>
        /// Campo que provee a la propiedad 'Priority'.
        /// </summary>
        private Priority _priority;

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
            CloseCommand = new CommandBase(parameter => DialogHost.CloseDialogCommand.Execute(parameter, null));

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
        /// Obtiene o establece la descripción de la incidencia.
        /// </summary>
        public String Description {
            get => _description;
            set {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Obtiene o establece el equipo que presenta la incidencia.
        /// </summary>
        public Device Device {
            get => _device;
            set {
                _device = value;
                OnPropertyChanged("Device");
                if (Device != null)
                {
                    if (Device is Vehicle && (Device as Vehicle).Route != Location)
                        Location = (Device as Vehicle).Route;
                    if (!(Device is Vehicle) && Device.Station != Location)
                        Location = Device.Station;
                }
            }
        }

        /// <summary>
        /// Obtiene una lista de equipos disponibles en la estación.
        /// </summary>
        public IEnumerable<Device> Devices {
            get {
                if (IsRefundOfMoney)
                    return Location is null
                        ? AcabusData.FindDevices(device => device is Kvr)
                        : (Location as Station).Devices.Where(device => device is Kvr);
                if (!IsBusIncidences)
                    return Location is null || (!(Location is Route) && !(Location is Station))
                        ? AcabusData.FindDevices((device) => true) : (Location as Station).Devices;
                else
                    return Location is null ?
                        (IEnumerable<Vehicle>)(AcabusData.FindVehicles((vehicle) => true))
                            .OrderBy((vehicle) => vehicle.EconomicNumber)
                        : (Location as Route).Vehicles;
            }
        }

        /// <summary>
        /// Obtiene el nombre del cuadro de texto para vehículo o equipo.
        /// </summary>
        public String HeaderTextDeviceOrVehicle => IsBusIncidences ? "Vehículo" : "Equipo";

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
                OnPropertyChanged("HeaderTextDeviceOrVehicle");
                OnPropertyChanged("HeaderTextRouteOrStation");
                ClearErrors();
            }
        }

        /// <summary>
        /// Obtiene o establece la ubicación de la incidencia.
        /// </summary>
        public Location Location {
            get => _location;
            set {
                _location = value;
                OnPropertyChanged("Location");
                OnPropertyChanged("Devices");
            }
        }

        /// <summary>
        /// Obtiene una lista de todas las ubicaciones disponibles.
        /// </summary>
        public IEnumerable<Location> Locations {
            get {
                if (!IsBusIncidences)
                    return AcabusData.FindStations((station) => true);
                return AcabusData.Routes;
            }
        }

        /// <summary>
        /// Obtiene una lista de las prioridades de incidencias.
        /// </summary>
        public IEnumerable<Priority> Priorities => Enum.GetValues(typeof(Priority)).Cast<Priority>();

        /// <summary>
        /// Obtiene o establece la prioridad de la incidencia.
        /// </summary>
        public Priority Priority {
            get => _priority;
            set {
                _priority = value;
                OnPropertyChanged("Priority");
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
        private CashDestiny? _cashDestiny;

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
        private String _selectedTechnician;

        /// <summary>
        /// Campo que provee a la propiedad 'TitleSection'.
        /// </summary>
        private String _titleSection;

        /// <summary>
        /// Obtiene una lista con los destino de dinero.
        /// </summary>
        public IEnumerable<CashDestiny> CashDestinies => AcabusData.CashDestiny.Where(cashDestiny =>
        {
            if (IsMoney && cashDestiny.Type == CashType.MONEY)
                return true;
            if (!IsMoney && cashDestiny.Type == CashType.BILL)
                return true;
            return false;
        });

        /// <summary>
        /// Obtiene o establece el destino del dinero.
        /// </summary>
        public CashDestiny? CashDestiny {
            get => _cashDestiny;
            set {
                _cashDestiny = value;
                OnPropertyChanged("CashDestiny");
                SetDescriptionAndObservation();
            }
        }

        /// <summary>
        /// Obtiene o establece si la devolución de dinero son monedas.
        /// </summary>
        public Boolean IsMoney {
            get => _isMoney;
            set {
                _isMoney = value;
                OnPropertyChanged("IsMoney");
                OnPropertyChanged("CashDestinies");
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
        public String SelectedTechnician {
            get => _selectedTechnician;
            set {
                _selectedTechnician = value;
                OnPropertyChanged("SelectedTechnician");
            }
        }

        /// <summary>
        /// Obtiene una lista de los técnicos seleccionables.
        /// </summary>
        public ObservableCollection<String> Technicians => AcabusData.Technicians;

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
                Description = String.Format("{0}", _isMoney ? "MONEDAS DEVUELTAS" : "BILLETE DEVUELTO");
                Observations = String.Format("DEVOLUCIÓN DE {0} (${1:F2}) A {2}", _isMoney ? "MONEDAS" : "BILLETE", quantity, CashDestiny?.Description);
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

                case "Description":
                    if (String.IsNullOrEmpty(Description))
                        AddError("Description", "Falta ingresar la descripción de la incidencia");
                    break;

                case "Location":
                    if (String.IsNullOrEmpty(Location?.ToString()))
                        AddError("Location", String.Format("Falta seleccionar la {0}", IsBusIncidences ? "ruta" : "estación"));
                    break;

                case "Device":
                    if (String.IsNullOrEmpty(Device?.ToString()))
                        AddError("Device", String.Format("Falta seleccionar el {0}", IsBusIncidences ? "vehículo" : "equipo"));
                    if (IsRefundOfMoney && !(Device is Kvr))
                        AddError("Device", "El equipo debe ser un KVR");
                    break;

                case "Priority":
                    if (IsNewIncidences && Priority == Priority.NONE)
                        AddError("Priority", "Debe asignar una prioridad a la incidencia.");
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
                    if (IsRefundOfMoney && String.IsNullOrEmpty(SelectedTechnician))
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

                if (exists = (incidence.Description == Description
                  && (incidence.Device is Vehicle && Device is Vehicle
                        ? (incidence.Device as Vehicle).EconomicNumber == (Device as Vehicle).EconomicNumber
                        : incidence.Device.NumeSeri == Device.NumeSeri)
                    && incidence.Location == Location))
                    break;
            }
            if (exists)
                AddError("Description", "Ya existe una incidencia abierta igual para el equipo");

            return !exists;
        }

        private void AddCommandExecute(Object parameter)
        {
            var incidences = ViewModelService.GetViewModel<CctvReportsViewModel>().Incidences;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var incidence = CctvService.CreateIncidence(incidences,
                    Description,
                    Device,
                    DateTime.Now.Date.AddTicks(StartTime.Ticks),
                    IsRefundOfMoney ? Priority.NONE : Priority,
                    Location,
                    WhoReporting
                );

                if (IsRefundOfMoney)
                {
                    incidence.Status = CashDestiny.Value.Description == "CAU" ? IncidenceStatus.UNCOMMIT : IncidenceStatus.CLOSE;
                    incidence.Technician = SelectedTechnician;
                    incidence.Observations = Observations;
                    incidence.FinishDate = CashDestiny.Value.Description == "CAU" ? null : (DateTime?)incidence.StartDate;
                    var refundOfMoney = new RefundOfMoney(incidence)
                    {
                        Quantity = Single.Parse(Quantity),
                        CashDestiny = CashDestiny.Value,
                        Status = CashDestiny?.Description == "CAU" ? RefundOfMoneyStatus.UNCOMMIT : RefundOfMoneyStatus.COMMIT,
                        RefundDate = CashDestiny?.Description == "CAU" ? null : (DateTime?)incidence.StartDate
                    };
                    if (refundOfMoney.Save())
                        ViewModelService.GetViewModel<CctvReportsViewModel>().UpdateData();
                }
            });
            ViewModelService.GetViewModel<AttendanceViewModel>()?.UpdateCounters();
            CloseCommand.Execute(parameter);
        }

        private Boolean Validate()
        {
            ValidateProperty("WhoReporting");
            ValidateProperty("Description");
            ValidateProperty("Location");
            ValidateProperty("Device");
            ValidateProperty("Priority");
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