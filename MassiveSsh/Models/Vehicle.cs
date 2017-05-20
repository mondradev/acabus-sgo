using Acabus.Utils;
using System;

namespace Acabus.Models
{
    /// <summary>
    /// Provee de una estructura para el manejo de los vehiculos.
    /// </summary>
    public sealed class Vehicle : Device
    {
        /// <summary>
        /// Campo que provee a la propiedad 'EconomicNumber'.
        /// </summary>
        private String _economicNumber;

        /// <summary>
        /// Obtiene el número económico de la unidad.
        /// </summary>
        public String EconomicNumber => _economicNumber;

        /// <summary>
        /// Campo que provee a la propiedad 'BusType'.
        /// </summary>
        private VehicleType _type;

        /// <summary>
        /// Obtiene o establece el tipo de autobus.
        /// </summary>
        public VehicleType BusType {
            get => _type;
            set {
                _type = value;
                OnPropertyChanged("BusType");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'Status'.
        /// </summary>
        private VehicleStatus _status;

        /// <summary>
        /// Obtiene o establece el estado de la unidad.
        /// </summary>
        [XmlAnnotation(Ignore = true)]
        public VehicleStatus Status {
            get => _status;
            set {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'Route'.
        /// </summary>
        private Route _route;

        /// <summary>
        /// Obtiene o establece la ruta asignada de la unidad.
        /// </summary>
        [XmlAnnotation(Ignore = true)]
        public Route Route {
            get => _route;
            set {
                _route = value;
                OnPropertyChanged("Route");
            }
        }

        /// <summary>
        /// Crea una instancia nueva de 'Vehicle' indicando el número económico.
        /// </summary>
        /// <param name="economicNumber">Número Económico de la unidad.</param>
        /// <param name="status">Estado del funcionamiento de la unidad.</param>
        public Vehicle(String economicNumber, VehicleStatus status = VehicleStatus.UNKNOWN) : base(0, DeviceType.VEHICLE, null)
        {
            this._economicNumber = economicNumber;
            this._status = status;
        }

        /// <summary>
        /// Obtiene la representación de una unidad a través de una cadena.
        /// </summary>
        /// <returns>El número económico de la unidad.</returns>
        public override string ToString()
        {
            return EconomicNumber;
        }

        public static Vehicle CreateVehicle(Route route, String economicNumber, VehicleType busType) => new Vehicle(economicNumber)
        {
            Enabled = true,
            BusType = busType,
            Route = route
        };

    }
}
