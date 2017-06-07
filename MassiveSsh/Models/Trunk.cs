using Acabus.Utils;
using InnSyTech.Standard.Database;
using System;
using System.Collections.ObjectModel;

namespace Acabus.Models
{
    [XmlAnnotation(Name = "Route")]
    public sealed class Trunk : Route
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Stations'.
        /// </summary>
        private ObservableCollection<Station> _stations;

        /// <summary>
        /// Obtiene una lista de las estaciones de asignadas a la ruta.
        /// </summary>
        [Column(IsIgnored = true)]
        public ObservableCollection<Station> Stations {
            get {
                if (_stations == null)
                    _stations = new ObservableCollection<Station>();
                return _stations;
            }
        }

        /// <summary>
        /// Crea una instancia de ruta troncal.
        /// </summary>
        /// <param name="id">Identificador de la ruta troncal.</param>
        /// <param name="routeNumber">El número de la ruta troncal.</param>
        public Trunk(UInt16 id, UInt16 routeNumber) : base(id, routeNumber, RouteType.TRUNK) { }

        /// <summary>
        /// Añade una estación a la ruta.
        /// </summary>
        /// <param name="station">Estación por agregar.</param>
        public void AddStation(Station station)
        {
            Stations.Add(station);
        }

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
        /// Obtiene el número de estaciones en la ruta.
        /// </summary>
        /// <returns>El número de estaciones asignadas a la ruta actual.</returns>
        public UInt16 StationCount() => (UInt16)(Stations.Count);

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
        /// Crea una instancia de 'Trunk' proporcionando su ID.
        /// </summary>
        /// <param name="id">ID de la ruta troncal.</param>
        /// <returns>Una instancia de ruta troncal.</returns>
        public static Trunk CreateTrunk(UInt16 id, UInt16 routeNumber) => new Trunk(id, routeNumber);

    }
}
