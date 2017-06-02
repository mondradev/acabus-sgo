using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Acabus.Models
{
    public class Route : Location
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Type'.
        /// </summary>
        protected RouteType _type;

        /// <summary>
        /// Obtiene el tipo de ruta.
        /// </summary>
        public RouteType Type => _type;

        /// <summary>
        /// Campo que provee a la propiedad 'ID'.
        /// </summary>
        protected UInt16 _id;

        /// <summary>
        /// Obtiene el ID de la ruta.
        /// </summary>
        public UInt16 ID => _id;

        /// <summary>
        /// Campo que provee a la propiedad 'RouteNumber'.
        /// </summary>
        private UInt16 _routeNumber;

        /// <summary>
        /// Obtiene número de la ruta.
        /// </summary>
        public UInt16 RouteNumber => _routeNumber;

        /// <summary>
        /// Representa en una cadena la instancia actual.
        /// </summary>
        /// <returns>Una cadena que representa una instancia Route.</returns>
        public override String ToString() => String.Format("RUTA {0}{1} - {2}",
                                                            Enum.GetName(typeof(RouteType), Type)?[0],
                                                            RouteNumber.ToString("D2"),
                                                            Name);

        /// <summary>
        /// Obtiene el código de la ruta actual.
        /// </summary>
        /// <returns>Una cadena que representa una instancia.</returns>
        public String GetCodeRoute() => String.Format("R{0}{1}", Enum.GetName(typeof(RouteType), Type)?[0], RouteNumber);

        /// <summary>
        /// Campo que provee a la propiedad 'Vehicles'.
        /// </summary>
        private ObservableCollection<Vehicle> _vehicles;

        /// <summary>
        /// Obtiene una lista de los vehículos asignados a esa ruta.
        /// </summary>
        public ObservableCollection<Vehicle> Vehicles {
            get {
                if (_vehicles == null)
                    _vehicles = new ObservableCollection<Vehicle>();
                return _vehicles;
            }
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
            _type = type;
            _routeNumber = routeNumber;
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
        /// Añade un vehículo a la ruta.
        /// </summary>
        /// <param name="vehicle">Vehículo a agregar.</param>
        public void AddVehicle(Vehicle vehicle)
        {
            if (!Vehicles.Contains(vehicle))
                Vehicles.Add(vehicle);
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
        /// Obtiene el número total de vehículos en la ruta.
        /// </summary>
        /// <returns>El número total de vehículos.</returns>
        public UInt16 VehicleCount() => (UInt16)(Vehicles.Count);

    }
}
