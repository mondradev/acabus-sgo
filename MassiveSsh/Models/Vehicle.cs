using Acabus.Utils;
using Acabus.Utils.Mvvm;
using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using System;
using System.Collections.ObjectModel;

namespace Acabus.Models
{
    /// <summary>
    /// Provee de una estructura para el manejo de los vehiculos.
    /// </summary>
    [Entity(TableName = "Vehicles")]
    public sealed class Vehicle : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Devices'.
        /// </summary>
        private ObservableCollection<Device> _devices;

        /// <summary>
        /// Campo que provee a la propiedad 'EconomicNumber'.
        /// </summary>
        private String _economicNumber;

        /// <summary>
        /// Campo que provee a la propiedad 'Enabled'.
        /// </summary>
        private Boolean _enabled;

        /// <summary>
        /// Campo que provee a la propiedad 'IP'.
        /// </summary>
        private String _ip;

        /// <summary>
        /// Campo que provee a la propiedad 'Route'.
        /// </summary>
        private Route _route;

        /// <summary>
        /// Campo que provee a la propiedad 'Status'.
        /// </summary>
        private VehicleStatus _status;

        /// <summary>
        /// Campo que provee a la propiedad 'BusType'.
        /// </summary>
        private VehicleType _type;

        /// <summary>
        /// Crea una instancia básica de <see cref="Vehicle"/>.
        /// </summary>
        public Vehicle()
        {

            _status = VehicleStatus.UNKNOWN;
            BusType = VehicleType.NONE;
        }

        /// <summary>
        /// Crea una instancia nueva de 'Vehicle' indicando el número económico.
        /// </summary>
        /// <param name="economicNumber">Número Económico de la unidad.</param>
        /// <param name="status">Estado del funcionamiento de la unidad.</param>
        public Vehicle(String economicNumber, VehicleStatus status = VehicleStatus.UNKNOWN)
        {
            this._economicNumber = economicNumber;
            this._status = status;
        }

        /// <summary>
        /// Obtiene o establece el tipo de autobus.
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<VehicleType>))]
        public VehicleType BusType {
            get => _type;
            set {
                _type = value;
                OnPropertyChanged("BusType");
            }
        }

        /// <summary>
        /// Obtiene una lista de los dispositivos dentro de la unidad.
        /// </summary>
        [Column(IsIgnored = true)]
        public ObservableCollection<Device> Devices {
            get {
                if (_devices == null)
                    _devices = new ObservableCollection<Device>();
                return _devices;
            }
        }

        /// <summary>
        /// Obtiene el número económico de la unidad.
        /// </summary>
        [Column(IsPrimaryKey = true)]
        public String EconomicNumber {
            get => _economicNumber;
            private set {
                _economicNumber = value;
                OnPropertyChanged("EconomicNumber");
            }
        }

        /// <summary>
        /// Obtiene o establece si el vehículo está activo.
        /// </summary>
        public Boolean Enabled {
            get => _enabled;
            set {
                _enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        /// <summary>
        /// Obtiene o establece la dirección IP del vehículo.
        /// </summary>
        public String IP {
            get => _ip;
            set {
                _ip = value;
                OnPropertyChanged("IP");
            }
        }

        /// <summary>
        /// Obtiene o establece la ruta asignada de la unidad.
        /// </summary>
        [XmlAnnotation(Ignore = true)]
        [Column(Name = "Fk_Route_ID", IsForeignKey = true)]
        public Route Route {
            get => _route;
            set {
                _route = value;
                OnPropertyChanged("Route");
            }
        }

        /// <summary>
        /// Obtiene o establece el estado de la unidad.
        /// </summary>
        [XmlAnnotation(Ignore = true)]
        [Column(IsIgnored = true)]
        public VehicleStatus Status {
            get => _status;
            set {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        /// <summary>
        /// Crea una instancai de <see cref="Vehicle"/> especificando sus atributos más importantes.
        /// </summary>
        /// <param name="route">Ruta asignada a la unidad.</param>
        /// <param name="economicNumber">Número Económico de la unidad.</param>
        /// <param name="busType">Tipo de autobus <seealso cref="VehicleType"/>.</param>
        /// <returns></returns>
        public static Vehicle CreateVehicle(Route route, String economicNumber, VehicleType busType) => new Vehicle(economicNumber)
        {
            BusType = busType,
            Route = route
        };

        /// <summary>
        /// Obtiene la representación de una unidad a través de una cadena.
        /// </summary>
        /// <returns>El número económico de la unidad.</returns>
        public override string ToString()
        {
            return String.Format("{0} {1}", Route?.GetCodeRoute(), EconomicNumber);
        }
    }
}