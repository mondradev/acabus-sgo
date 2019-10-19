using Acabus.DataAccess;
using Acabus.Models;
using Acabus.Utils.Mvvm;
using Acabus.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Input;

namespace Acabus.Modules.Core.Config.ViewModels
{
    public sealed class AddDeviceViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad 'CanReplicate'.
        /// </summary>
        private bool _canReplicate;

        /// <summary>
        /// Campo que provee a la propiedad 'Enabled'.
        /// </summary>
        private bool _enabled;

        /// <summary>
        /// Campo que provee a la propiedad 'HasDatabase'.
        /// </summary>
        private bool _hasDatabase;

        /// <summary>
        /// Campo que provee a la propiedad 'IP'.
        /// </summary>
        private String _ipAddress;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedDeviceType'.
        /// </summary>
        private DeviceType? _selectedDeviceType;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedStation'.
        /// </summary>
        private Station _selectedStation;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedVehicle'.
        /// </summary>
        private Vehicle _selectedVehicle;

        /// <summary>
        /// Campo que provee a la propiedad 'SshEnabled'.
        /// </summary>
        private bool _sshEnabled;

        /// <summary>
        /// Campo que provee a la propiedad 'SerialNumber'.
        /// </summary>
        private String serialNumber;

        public AddDeviceViewModel()
        {
            IP = "0.0.0.0";
            AddDeviceCommand = new CommandBase(AddDeviceExecute, AddDeviceCanExecute);
        }

        public ICommand AddDeviceCommand { get; }

        /// <summary>
        /// Obtiene o establece si el equipo nuevo replica información a CCO.
        /// </summary>
        public bool CanReplicate {
            get => _canReplicate;
            set {
                _canReplicate = value;
                OnPropertyChanged("CanReplicate");
            }
        }

        /// <summary>
        /// Obtiene una lista de los vehículos disponibles.
        /// </summary>
        public IEnumerable<DeviceType> DeviceTypes => Enum.GetValues(typeof(DeviceType)).Cast<DeviceType>();

        /// <summary>
        /// Obtiene o establece si el equipo nuevo está activo.
        /// </summary>
        public bool Enabled {
            get => _enabled;
            set {
                _enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        /// <summary>
        /// Obtiene o establece si el equipo nuevo tiene base de datos.
        /// </summary>
        public bool HasDatabase {
            get => _hasDatabase;
            set {
                _hasDatabase = value;
                OnPropertyChanged("HasDatabase");
                if (!value)
                    CanReplicate = false;
            }
        }

        /// <summary>
        /// Obtiene o establece la dirección IP del equipo.
        /// </summary>
        public String IP {
            get => _ipAddress;
            set {
                _ipAddress = value;
                OnPropertyChanged("IP");
            }
        }

        /// <summary>
        /// Obtiene o establece el tipo del nuevo equipo.
        /// </summary>
        public DeviceType? SelectedDeviceType {
            get => _selectedDeviceType;
            set {
                _selectedDeviceType = value;
                OnPropertyChanged("SelectedDeviceType");
            }
        }

        /// <summary>
        /// Obtiene o establece la estación del nuevo equipo.
        /// </summary>
        public Station SelectedStation {
            get => _selectedStation;
            set {
                _selectedStation = value;
                OnPropertyChanged("SelectedStation");
            }
        }

        /// <summary>
        /// Obtiene o establece el vehículo del nuevo equipo.
        /// </summary>
        public Vehicle SelectedVehicle {
            get => _selectedVehicle;
            set {
                _selectedVehicle = value;
                OnPropertyChanged("SelectedVehicle");
            }
        }

        /// <summary>
        /// Obtiene o establece el número de serie del nuevo equipo.
        /// </summary>
        public String SerialNumber {
            get => serialNumber;
            set {
                serialNumber = value;
                OnPropertyChanged("SerialNumber");
            }
        }

        /// <summary>
        /// Obtiene o establece si el equipo nuevo tiene activa la consola segura (SSH).
        /// </summary>
        public bool SshEnabled {
            get => _sshEnabled;
            set {
                _sshEnabled = value;
                OnPropertyChanged("SshEnabled");
            }
        }

        /// <summary>
        /// Obtiene una lista de las estaciones disponibles.
        /// </summary>
        public IEnumerable<Station> Stations => DataAccess.AcabusData.AllStations;

        /// <summary>
        /// Obtiene una lista de los vehículos disponibles.
        /// </summary>
        public IEnumerable<Vehicle> Vehicles => DataAccess.AcabusData.AllVehicles.OrderBy(vehicle => vehicle.EconomicNumber);

        /// <summary>
        /// Determina si los campos del formulario está correctamente llenos.
        /// </summary>
        public bool Validate()
        {
            ValidateProperty("SelectedDeviceType");
            ValidateProperty("SelectedStation");
            ValidateProperty("SelectedVehicle");
            ValidateProperty("IP");

            return !HasErrors;
        }

        protected override void OnValidation(string propertyName)
        {
            switch (propertyName)
            {
                case "SelectedDeviceType":
                    if (SelectedDeviceType is null)
                        AddError("SelectedDeviceType", "Seleccione un tipo de dispositivo valido.");
                    break;

                case "SelectedStation":
                    if (SelectedStation != null && SelectedVehicle != null)
                        AddError("SelectedStation", "No puede asignar cuando ya fue asigando a un vehiculo.");
                    if (SelectedVehicle is null && SelectedStation is null)
                        AddError("SelectedStation", "Seleccione una estación valida.");
                    break;

                case "SelectedVehicle":
                    if (SelectedStation != null && SelectedVehicle != null)
                        AddError("SelectedVehicle", "No puede asignar cuando ya fue asigando a una estación.");
                    if (SelectedVehicle is null && SelectedStation is null)
                        AddError("SelectedVehicle", "Seleccione un vehículo valido.");
                    break;

                case "IP":
                    if (!IPAddress.TryParse(IP, out IPAddress address))
                        AddError("IP", "La dirección IP no es valida.");
                    break;
            }
        }

        private bool AddDeviceCanExecute(object arg)
            => Validate();

        private void AddDeviceExecute(object obj)
        {
            Device device = new Device(0, SelectedDeviceType.Value, SelectedStation, SerialNumber)
            {
                CanReplicate = CanReplicate,
                Enabled = Enabled,
                SshEnabled = SshEnabled,
                HasDatabase = HasDatabase,
                Vehicle = SelectedVehicle,
                IP = IP
            };
            if (AcabusData.Session.Create(device))
                AcabusControlCenterViewModel.ShowDialog($"Equipo: {device} agregado correctamente.");
            else
                AcabusControlCenterViewModel.ShowDialog("No se pudo guardar el equipo nuevo.");
        }
    }
}