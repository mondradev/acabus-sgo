using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Acabus.Models
{
    [Entity(TableName = "Routes")]
    public class Route : AssignableSection
    {
        /// <summary>
        /// Campo que provee a la propiedad 'ID'.
        /// </summary>
        protected UInt64 _id;

        /// <summary>
        /// Campo que provee a la propiedad 'Type'.
        /// </summary>
        protected RouteType _routeType;

        /// <summary>
        /// Campo que provee a la propiedad 'RouteNumber'.
        /// </summary>
        private UInt16 _routeNumber;

        /// <summary>
        /// Campo que provee a la propiedad 'Stations'.
        /// </summary>
        private ObservableCollection<Station> _stations;

        /// <summary>
        /// Campo que provee a la propiedad 'Vehicles'.
        /// </summary>
        private ObservableCollection<Vehicle> _vehicles;

        /// <summary>
        /// Crea una instancia básica de <see cref="Route"/>.
        /// </summary>
        public Route()
        {
        }

        /// <summary>
        /// Crea una nueva instancia de ruta especificando su identificador.
        /// </summary>
        /// <param name="id">Identificador de la ruta.</param>
        /// <param name="routeNumber">Número de la ruta.</param>
        /// <param name="type">Tipo de ruta.</param>
        public Route(UInt16 id, UInt16 routeNumber, RouteType type = RouteType.ALIM)
        {
            _id = id;
            _routeType = type;
            _routeNumber = routeNumber;
        }

        /// <summary>
        /// Obtiene el ID de la ruta.
        /// </summary>
        [Column(IsPrimaryKey = true, IsAutonumerical = true)]
        public UInt64 ID {
            get => _id;
            protected set {
                _id = value;
                OnPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Obtiene número de la ruta.
        /// </summary>
        public UInt16 RouteNumber {
            get => _routeNumber;
            protected set {
                _routeNumber = value;
                OnPropertyChanged("RouteNumber");
            }
        }

        /// <summary>
        /// Obtiene el tipo de ruta.
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<RouteType>))]
        public RouteType RouteType {
            get => _routeType;
            protected set {
                _routeType = value;
                OnPropertyChanged("Type");
            }
        }

        /// <summary>
        /// Obtiene una lista de las estaciones de asignadas a la ruta.
        /// </summary>
        [Column(ForeignKeyName = "Fk_Route_ID")]
        public ObservableCollection<Station> Stations {
            get {
                if (_stations == null)
                    _stations = new ObservableCollection<Station>();
                return _stations;
            }
        } /// <summary>

        /// <summary>
        /// Obtiene una lista de los vehículos asignados a esa ruta.
        /// </summary>
        [Column(ForeignKeyName = "Fk_Route_ID")]
        public ObservableCollection<Vehicle> Vehicles {
            get {
                if (_vehicles == null)
                    _vehicles = new ObservableCollection<Vehicle>();
                return _vehicles;
            }
        }

        /// <summary>
        /// Compara dos instancias de <see cref="Route"/> y determina si son diferentes
        /// </summary>
        public static bool operator !=(Route routeA, Route routeB) => !(routeA == routeB);

        /// <summary>
        /// Compara dos instancias de <see cref="Route"/> y determina si son iguales.
        /// </summary>
        public static bool operator ==(Route routeA, Route routeB)
            => !(routeA is null)
            && !(routeB is null)
            && routeA.ID == routeB.ID
            && routeA.RouteNumber == routeB.RouteNumber
            && routeA.RouteType == routeB.RouteType;

        /// Añade una estación a la ruta.
        /// </summary>
        /// <param name="station">Estación por agregar.</param>
        public void AddStation(Station station)
        {
            Stations.Add(station);
        }

        /// <summary>
        /// Añade un vehículo a la ruta.
        /// </summary>
        /// <param name="vehicle">Vehículo a agregar.</param>
        public void AddVehicle(Vehicle vehicle)
        {
            if (!Vehicles.Contains(vehicle))
                Vehicles.Add(vehicle);
        }

        /// <summary>
        /// Obtiene el número de dispositivos en la ruta troncal
        /// </summary>
        /// <returns>Número de dispositivos</returns>
        public int CountDevices()
        {
            int count = 0;
            foreach (var item in Stations)
                count += item.DeviceCount();
            return count;
        }

        /// <summary>
        /// Compara si la instancia es igual a la actual.
        /// </summary>
        /// <param name="obj">Una instancia a comparar.</param>
        /// <returns>Un valor verdadero si ambas instancias son iguales.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != GetType()) return false;

            return this == (Route)obj;
        }

        /// <summary>
        /// Obtiene el primer vehículo que cumple con las condiciones expresadas por el predicado.
        /// </summary>
        /// <param name="predicate">Predicado para evaluar a los vehículos.</param>
        /// <returns>El vehículo que cumple con el predicato.</returns>
        public Vehicle FindVehicle(Predicate<Vehicle> predicate)
        {
            foreach (Vehicle vehicle in Vehicles)
                if (predicate.Invoke(vehicle))
                    return vehicle;
            return null;
        }

        /// <summary>
        /// Obtiene todos los vehículos que cumplen con las condiciones expresadas por el predicado.
        /// </summary>
        /// <param name="predicate">Predicado para evaluar a los vehículos</param>
        /// <returns>Un arreglo unidimensional de vehículos que cumplen con el predicato.</returns>
        public Vehicle[] FindVehicles(Predicate<Vehicle> predicate)
        {
            List<Vehicle> vehicles = new List<Vehicle>();
            foreach (Vehicle vehicle in Vehicles)
                if (predicate.Invoke(vehicle))
                    vehicles.Add(vehicle);
            return vehicles.ToArray();
        }

        /// <summary>
        /// Obtiene el código de la ruta actual.
        /// </summary>
        /// <returns>Una cadena que representa una instancia.</returns>
        public String GetCodeRoute() => String.Format("R{0}{1}", Enum.GetName(typeof(RouteType), RouteType)?[0], RouteNumber);

        /// <summary>
        /// Obtiene un código hash de la instancia.
        /// </summary>
        /// <returns>Código hash de la instancia actual.</returns>
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Obtiene la estación con el ID especificado.
        /// </summary>
        /// <param name="idStation">ID de la estación a buscar.</param>
        /// <returns>Una estación que coincida con el número de estación.</returns>
        public Station GetStation(UInt16 idStation)
        {
            foreach (var item in Stations)
                if (item.ID == idStation)
                    return item;
            return null;
        }

        /// <summary>
        /// Obtiene el vehículo que coincide con el número económico especificado.
        /// </summary>
        /// <param name="economicNumber">Número económico del vehículo a buscar</param>
        /// <returns>Un vehículo de la ruta.</returns>
        public Vehicle GetVehicle(String economicNumber)
        {
            foreach (var item in Vehicles)
                if (item.EconomicNumber == economicNumber)
                    return item;
            return null;
        }

        /// <summary>
        /// Obtiene el número de estaciones en la ruta.
        /// </summary>
        /// <returns>El número de estaciones asignadas a la ruta actual.</returns>
        public UInt16 StationCount() => (UInt16)(Stations.Count);

        /// <summary>
        /// Representa en una cadena la instancia actual.
        /// </summary>
        /// <returns>Una cadena que representa una instancia Route.</returns>
        public override String ToString() => String.Format("RUTA {0}{1} - {2}",
                                                            Enum.GetName(typeof(RouteType), RouteType)?[0],
                                                            RouteNumber.ToString("D2"),
                                                            Name);

        /// <summary>
        /// Obtiene el número total de vehículos en la ruta.
        /// </summary>
        /// <returns>El número total de vehículos.</returns>
        public UInt16 VehicleCount() => (UInt16)(Vehicles.Count);
    }
}