using Acabus.DataAccess;
using Acabus.Models;
using Acabus.Modules.CctvReports.Models;
using Acabus.Modules.CctvReports.Services;
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
        /// Campo que provee a la propiedad 'WhoReport'.
        /// </summary>
        private String _whoReporting;

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

        /// <summary>
        /// Campo que provee a la propiedad 'Description'.
        /// </summary>
        private String _description;

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
        /// Campo que provee a la propiedad 'Location'.
        /// </summary>
        private Location _location;

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
        /// Campo que provee a la propiedad 'Device'.
        /// </summary>
        private Device _device;

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
        /// Campo que provee a la propiedad 'Priority'.
        /// </summary>
        private Priority _priority;

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
        /// Obtiene una lista de empresas que realizan reportes habitualmente.
        /// </summary>
        public ObservableCollection<String> Business => AcabusData.Companies;

        /// <summary>
        /// Obtiene una lista de las prioridades de incidencias.
        /// </summary>
        public IEnumerable<Priority> Priorities => Enum.GetValues(typeof(Priority)).Cast<Priority>();

        /// <summary>
        /// Obtiene una lista de equipos disponibles en la estación.
        /// </summary>
        public IEnumerable<Device> Devices {
            get {
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
        /// Campo que provee a la propiedad 'SelectedIndexLocation'.
        /// </summary>
        private Int32 _selectedIndexLocation;

        /// <summary>
        /// Obtiene o establece el indice de la lista de la ubicación actualmente seleccionada.
        /// </summary>
        public Int32 SelectedIndexLocation {
            get => _selectedIndexLocation;
            set {
                _selectedIndexLocation = value;
                OnPropertyChanged("SelectedIndexLocation");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'IsBusIncidences'.
        /// </summary>
        private Boolean _isBusIncidences;

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
        /// Campo que provee a la propiedad 'Incidences'.
        /// </summary>
        private ObservableCollection<Incidence> _incidences;

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
        /// Obtiene el nombre del cuadro de texto para vehículo o equipo.
        /// </summary>
        public String HeaderTextDeviceOrVehicle => IsBusIncidences ? "Vehículo" : "Equipo";

        /// <summary>
        /// Obtiene el nombre del cuadro de texto para ubicación.
        /// </summary>
        public String HeaderTextRouteOrStation => IsBusIncidences ? "Ruta" : "Estación";

        /// <summary>
        /// 
        /// </summary>
        public ICommand AddCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public ICommand CloseCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public AddIncidencesViewModel()
        {

            AddCommand = new CommandBase(AddCommandExecute, AddCommandCanExec);
            CloseCommand = new CommandBase(parameter => DialogHost.CloseDialogCommand.Execute(parameter, null));
        }

        private bool AddCommandCanExec(object parameter)
        {
            if (parameter is null) return false;
            if (!Validate()) return false;

            var incidences = (IList<Incidence>)parameter;
            bool exists = false;
            foreach (var incidence in incidences)
            {
                if (incidence.Status == IncidenceStatus.CLOSE) continue;

                if (exists = (incidence.Description == Description
                    && incidence.Device == Device
                    && incidence.Location == Location))
                    break;
            }

            return !exists;
        }

        private void AddCommandExecute(Object parameter)
        {
            var incidences = (IList<Incidence>)parameter;

            Application.Current.Dispatcher.Invoke(() =>
            {
                incidences.CreateIncidence(
                    Description,
                    Device,
                    DateTime.Now,
                    Priority,
                    Location,
                    WhoReporting
                );

            });

            CloseCommand.Execute(parameter);
        }

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
                    break;
            }
        }

        private Boolean Validate()
        {
            ValidateProperty("WhoReporting");
            ValidateProperty("Description");
            ValidateProperty("Location");
            ValidateProperty("Device");

            if (HasErrors) return false;

            return true;
        }
    }
}
