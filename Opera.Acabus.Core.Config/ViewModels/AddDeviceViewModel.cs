using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Input;

namespace Opera.Acabus.Core.Config.ViewModels
{
    /// <summary>
    /// Define la estructura del modelo de la vista <see cref="Views.AddDeviceView"/>.
    /// </summary>
    public sealed class AddDeviceViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="IPString" />.
        /// </summary>
        private String _ipString;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedBus" />.
        /// </summary>
        private Bus _selectedBus;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedStation" />.
        /// </summary>
        private Station _selectedStation;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedType" />.
        /// </summary>
        private DeviceType? _selectedType;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SerialNumber"/>.
        /// </summary>
        private String serialNumber;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="IDEqui" />
        /// </summary>
        private UInt32 _idequi;

        /// <summary>
        /// Crea una instancia nueva de <see cref="AddDeviceViewModel"/>.
        /// </summary>
        public AddDeviceViewModel()
        {
            AddDeviceCommand = new Command(AddDeviceExecute, AddDeviceCanExecute);
        }

        /// <summary>
        /// Obtiene el comando que agrega el equipo al sistema.
        /// </summary>
        public ICommand AddDeviceCommand { get; }

        /// <summary>
        /// Obtiene una lista de los autobuses disponibles.
        /// </summary>
        public IEnumerable<Bus> Buses => AcabusDataContext.AllBuses.OrderBy(vehicle => vehicle.EconomicNumber);

        /// <summary>
        /// Obtiene una lista de los tipos de equipos disponibles.
        /// </summary>
        public IEnumerable<DeviceType> DeviceTypes => Enum.GetValues(typeof(DeviceType))
            .Cast<DeviceType>().Where(x => x != DeviceType.NONE);

        /// <summary>
        /// Obtiene o establece el ID de equipo.
        /// </summary>
        public UInt32 IDEqui {
            get => _idequi;
            set {
                _idequi = value;
                OnPropertyChanged(nameof(IDEqui));
            }
        }

        /// <summary>
        /// Obtiene o establece la dirección IP del equipo.
        /// </summary>
        public String IPString {
            get => _ipString ?? (_ipString = "0.0.0.0");
            set {
                _ipString = value;
                OnPropertyChanged(nameof(IPString));
            }
        }

        /// <summary>
        /// Obtiene o establece el autobus seleccionado.
        /// </summary>
        public Bus SelectedBus {
            get => _selectedBus;
            set {
                _selectedBus = value;
                OnPropertyChanged(nameof(SelectedBus));
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
            }
        }

        /// <summary>
        /// Obtiene o establece el tipo de equipo.
        /// </summary>
        public DeviceType? SelectedType {
            get => _selectedType;
            set {
                _selectedType = value;
                OnPropertyChanged(nameof(SelectedType));
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
        /// Obtiene una lista de las estaciones disponibles.
        /// </summary>
        public IEnumerable<Station> Stations => AcabusDataContext.AllStations.OrderBy(station => station.StationNumber);

        /// <summary>
        /// Determina si los campos del formulario está correctamente llenos.
        /// </summary>
        public bool Validate()
        {
            ValidateProperty(nameof(SelectedType));
            ValidateProperty(nameof(SelectedStation));
            ValidateProperty(nameof(SelectedBus));
            ValidateProperty(nameof(IPString));

            return !HasErrors;
        }

        /// <summary>
        /// Función que es llamada durante la validación del valor de las propiedades.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad a validar.</param>
        protected override void OnValidation(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(SelectedType):
                    if (SelectedType is null)
                        AddError(nameof(SelectedType), "Seleccione un tipo de dispositivo valido.");
                    break;

                case nameof(SelectedStation):
                    if (SelectedStation != null && SelectedBus != null)
                        AddError(nameof(SelectedStation), "No puede asignar cuando ya fue asigando a un vehiculo.");
                    if (SelectedBus is null && SelectedStation is null)
                        AddError(nameof(SelectedStation), "Seleccione una estación valida.");
                    break;

                case nameof(SelectedBus):
                    if (SelectedStation != null && SelectedBus != null)
                        AddError(nameof(SelectedBus), "No puede asignar cuando ya fue asigando a una estación.");
                    if (SelectedBus is null && SelectedStation is null)
                        AddError(nameof(SelectedBus), "Seleccione un vehículo valido.");
                    break;

                case nameof(IPString):
                    if (!String.IsNullOrEmpty(IPString) && !IPAddress.TryParse(IPString, out IPAddress address))
                        AddError(nameof(IPString), "La dirección IP no es valida.");
                    break;
            }
        }

        /// <summary>
        /// Determina si es posible ejecutar el comando <see cref="AddDeviceCommand" />.
        /// </summary>
        /// <param name="arg">Parametro del comando.</param>
        /// <returns>Un valor <see cref="true"/> si es posible ejecutar el comando.</returns>
        private bool AddDeviceCanExecute(object arg)
        {
            return Validate();
        }

        /// <summary>
        /// Crea una instancia <see cref="Device"/> a partir de la información de la instancia actual
        /// <see cref="AddDeviceViewModel"/> y la guarda en la base de datos. Acción que realiza el
        /// comando <see cref="AddDeviceCommand"/>.
        /// </summary>
        /// <param name="obj">Parametro del comando.</param>
        private void AddDeviceExecute(object obj)
        {
            object device = new Device(IDEqui, SerialNumber, SelectedType.Value)
            {
                Station = SelectedStation,
                Bus = SelectedBus,
                IPAddress = String.IsNullOrEmpty(IPString) ? null : IPAddress.Parse(IPString)
            };

            if (ServerContext.GetLocalSync("Device").Create(ref device, out Exception reason))
                Dispatcher.SendMessageToGUI($"Equipo: {device} agregado correctamente.");
            else
                Dispatcher.SendMessageToGUI("No se pudo guardar el equipo nuevo, razón: " + reason.Message);

            Dispatcher.CloseDialog();
        }
    }
}