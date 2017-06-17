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
        /// Campo que provee a la propiedad 'ID'.
        /// </summary>
        private Int64 _id;

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
        /// <param name="id">Identificador de vehículo.</param>
        /// <param name="economicNumber">Número Económico de la unidad.</param>
        /// <param name="type">Tipo de la unidad.</param>
        public Vehicle(UInt16 id, String economicNumber, VehicleType type = VehicleType.NONE)
        {
            this._id = id;
            this._economicNumber = economicNumber;
            this._status = VehicleStatus.UNKNOWN;
            this._type = type;
        }

        /// <summary>
        /// Obtiene o establece el tipo de autobus.
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<VehicleType>))]
        public VehicleType BusType {
            get => _type;
            private set {
                _type = value;
                OnPropertyChanged("BusType");
            }
        }

        /// <summary>
        /// Obtiene la descripción del vehículo ([Ruta] [Número Económico]).
        /// </summary>
        [Column(IsIgnored = true)]
        public String Description => String.Format("{0} {1}", Route?.GetCodeRoute(), EconomicNumber);

        /// <summary>
        /// Obtiene una lista de los dispositivos dentro de la unidad.
        /// </summary>
        [Column(ForeignKeyName = "Fk_Vehicle_ID")]
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
        /// Obtiene o establece el identificador del autobus.
        /// </summary>
        [Column(IsPrimaryKey = true)]
        public Int64 ID {
            get => _id;
            private set {
                _id = value;
                OnPropertyChanged("ID");
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
        /// Compara dos instancias de <see cref="Vehicle"/> y determina si son diferentes
        /// </summary>
        public static bool operator !=(Vehicle vehicleA, Vehicle vehicleB)
            => !(vehicleA == vehicleB);

        /// <summary>
        /// Compara dos instancias de <see cref="Vehicle"/> y determina si son iguales.
        /// </summary>
        public static bool operator ==(Vehicle vehicleA, Vehicle vehicleB)
            => (vehicleA is null && vehicleB is null)
                || (!(vehicleA is null)
                && !(vehicleB is null)
                && vehicleA.ID == vehicleB.ID
                && vehicleA.EconomicNumber == vehicleB.EconomicNumber
                && vehicleA.BusType == vehicleB.BusType);

        /// <summary>
        /// Compara la instancia actual con la especificada y determina si son iguales.
        /// </summary>
        /// <param name="obj">Instancia a comparar.</param>
        /// <returns>Un valor <see cref="true"/> si las instancias son iguales.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != GetType()) return false;

            return this == (Vehicle)obj;
        }

        /// <summary>
        /// Obtiene el código hash de la instancia actual.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Obtiene la representación de una unidad a través de una cadena.
        /// </summary>
        /// <returns>El número económico de la unidad.</returns>
        public override string ToString() => EconomicNumber;
    }
}