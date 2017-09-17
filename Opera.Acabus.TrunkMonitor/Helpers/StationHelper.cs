using Opera.Acabus.Core.Models;
using Opera.Acabus.TrunkMonitor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Opera.Acabus.TrunkMonitor.Helpers
{
    /// <summary>
    /// Provee a las instancias de <see cref="Station"/> el manejo de enlaces de comunicación <see cref="Link"/>.
    /// </summary>
    public static class StationHelper
    {
        ///<summary>
        /// Lleva el control de todas las instancias <see cref="Station"/> y su limite de latencia aceptable.
        ///</summary>
        private static Dictionary<Station, UInt16> _stationMaximunAcceptablePing
            = new Dictionary<Station, ushort>();

        ///<summary>
        /// Lleva el control de todas las instancias <see cref="Station"/> y su limite de latencia optima.
        ///</summary>
        private static Dictionary<Station, UInt16> _stationMaximunPing
            = new Dictionary<Station, ushort>();

        /// <summary>
        /// Lleva el control de todas las instancias <see cref="Station"/> y su vinculo con una lista
        /// de <see cref="Link"/>;
        /// </summary>
        private static Dictionary<Station, ObservableCollection<Link>> _stations
            = new Dictionary<Station, ObservableCollection<Link>>();

        ///<summary>
        /// Lleva el control de todas las instancias <see cref="Station"/> y su información de estado.
        ///</summary>
        private static Dictionary<Station, StationStateInfo> _stationStateInfo
            = new Dictionary<Station, StationStateInfo>();

        /// <summary>
        /// Agrega un enlace a esta estación.
        /// </summary>
        /// <param name="station">Estación a la que se añadirá el enlace nuevo.</param>
        /// <param name="link">Enlace nuevo que se añadirá.</param>
        public static void AddLink(this Station station, Link link)
        {
            lock (_stations)
                if (!_stations.ContainsKey(station))
                    _stations.Add(station, new ObservableCollection<Link>());

            ObservableCollection<Link> links = _stations[station];

            if (links.Any(l => l.ID == link.ID && l != link))
                links.Remove(links.Single(l => l.ID == link.ID && l != link));

            if (!links.Contains(link))
                links.Add(link);
        }

        /// <summary>
        /// Obtiene todos los enlaces de esta estación.
        /// </summary>
        /// <param name="station">Instancia <see cref="Station"/> que contiene los enlaces.</param>
        /// <returns>Una lista de <see cref="Link"/>.</returns>
        public static ObservableCollection<Link> GetLinks(this Station station)
        {
            lock (_stations)
                if (!_stations.ContainsKey(station))
                    _stations.Add(station, new ObservableCollection<Link>());
            return _stations[station];
        }

        /// <summary>
        /// Obtiene la máxima latencia aceptable de la estación.
        /// </summary>
        /// <param name="station">Estación a obtener su propiedad.</param>
        /// <returns>La latencia máxima aceptable.</returns>
        public static UInt16 GetMaximunAcceptablePing(this Station station)
        {
            lock (_stationMaximunAcceptablePing)
                if (!_stationMaximunAcceptablePing.ContainsKey(station))
                    _stationMaximunAcceptablePing.Add(station, 600);

            return _stationMaximunAcceptablePing[station];
        }

        /// <summary>
        /// Obtiene el limite de latencia optima de la estación.
        /// </summary>
        /// <param name="station">Estación a obtener su propiedad.</param>
        /// <returns>El limite de latencia optima.</returns>
        public static UInt16 GetMaximunPing(this Station station)
        {
            lock (_stationMaximunPing)
                if (!_stationMaximunPing.ContainsKey(station))
                    _stationMaximunPing.Add(station, 90);

            return _stationMaximunPing[station];
        }

        /// <summary>
        /// Obtiene la información de estado de esta estación.
        /// </summary>
        /// <param name="station">
        /// Instancia de <see cref="Station"/> a obtener su información de estado.
        /// </param>
        /// <returns>La información del estado de la estación.</returns>
        public static StationStateInfo GetStateInfo(this Station station)
        {
            lock (_stationStateInfo)
                if (!_stationStateInfo.ContainsKey(station))
                    _stationStateInfo.Add(station, new StationStateInfo(station));

            return _stationStateInfo[station];
        }

        /// <summary>
        /// Remueve el enlace de la estación.
        /// </summary>
        /// <param name="station">Estación a la que se quitará el enlace.</param>
        /// <param name="link">Enlace a remover.</param>
        public static bool RemoveLink(this Station station, Link link)
        {
            lock (_stations)
                if (!_stations.ContainsKey(station))
                    _stations.Add(station, new ObservableCollection<Link>());

            if (_stations[station].Contains(link))
                return _stations[station].Remove(link);

            return false;
        }

        /// <summary>
        /// Establce la máxima latencia aceptable de la estación.
        /// </summary>
        /// <param name="station">Estación a obtener su propiedad.</param>
        /// <param name="maxAcceptablePing">La latencia máxima aceptable.</param>
        public static void SetMaximunAcceptablePing(this Station station, UInt16 maxAcceptablePing)
        {
            lock (_stationMaximunAcceptablePing)
                if (!_stationMaximunAcceptablePing.ContainsKey(station))
                    _stationMaximunAcceptablePing.Add(station, maxAcceptablePing);
                else _stationMaximunAcceptablePing[station] = maxAcceptablePing;
        }

        /// <summary>
        /// Establece el limite de latencia optima de la estación.
        /// </summary>
        /// <param name="station">Estación a obtener su propiedad.</param>
        /// <param name="maxPing">El limite de latencia optima.</param>
        public static void SetMaximunPing(this Station station, UInt16 maxPing)
        {
            lock (_stationMaximunPing)
                if (!_stationMaximunPing.ContainsKey(station))
                    _stationMaximunPing.Add(station, maxPing);
                else _stationMaximunPing[station] = maxPing;
        }
    }
}