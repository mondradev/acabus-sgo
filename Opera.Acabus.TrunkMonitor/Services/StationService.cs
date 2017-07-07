using Opera.Acabus.Core.Models;
using Opera.Acabus.TrunkMonitor.Models;
using Opera.Acabus.TrunkMonitor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Opera.Acabus.TrunkMonitor.Service
{
    public static class StationService
    {
        ///<summary>
        /// Lleva el control de todas las instancias <see cref="Station"/> y su estado de enlace.
        ///</summary>
        private static Dictionary<Station, LinkState> _stationLinkState
            = new Dictionary<Station, LinkState>();

        ///<summary>
        /// Lleva el control de todas las instancias <see cref="Station"/> y su latencia.
        ///</summary>
        private static Dictionary<Station, Int16> _stationPing
            = new Dictionary<Station, short>();

        /// <summary>
        /// Calcula <see cref="LinkState"/> a partir de la latencia obtenida durante <see cref="DoPingLinkDevice(Station)"/>.
        /// </summary>
        /// <param name="station">Estación a obtener su estado de enlace.</param>
        /// <returns>Estado de enlace de la estación.</returns>
        public static LinkState CalculateLinkState(this Station station)
            => LinkStateExtensions.CalculateLinkState(
                station.GetPing(),
                station.GetMaximunPing(),
                station.GetMaximunAcceptablePing());

        /// <summary>
        /// Realiza un ping a todos los equipos de la estación y calcula la latencia promedio de esta.
        /// </summary>
        /// <param name="station">Estación a realizar el ping.</param>
        /// <returns>Latencia promedio de la estación.</returns>
        public static Int16 DoPing(this Station station)
        {
            var ping = 0;
            var nDevice = 0;
            foreach (var device in station.Devices)
            {
                var pingTemp = DeviceService.DoPing(device);
                if (device.GetState() != LinkState.DISCONNECTED)
                {
                    ping += pingTemp;
                    nDevice++;
                }
            }

            if (_stationPing.ContainsKey(station))
                _stationPing.Add(station, (Int16)(ping / nDevice));
            else
                _stationPing[station] = (Int16)(ping / nDevice);

            var linkState = station.CalculateLinkState();

            if (_stationLinkState.ContainsKey(station))
                _stationLinkState.Add(station, linkState);
            else
                _stationLinkState[station] = linkState;

            return station.GetPing();
        }

        /// <summary>
        /// Realiza un ping a un equipo del tipo <see cref="DeviceType.SW"/> o uno aleatorio y
        /// obtiene su latencia.
        /// </summary>
        /// <param name="station">Estación a realizar el ping.</param>
        /// <returns>La latencia de la estación.</returns>
        public static Int16 DoPingLinkDevice(this Station station)
        {
            var linkDevice = station.Devices.FirstOrDefault(device => device.Type == DeviceType.SW);
            if (linkDevice == null)
                linkDevice = station.Devices.First();
            if (linkDevice == null) return -1;
            var ping = DeviceService.DoPing(linkDevice);
            return ping;
        }

        /// <summary>
        /// Obtiene la latencia obtenida durante <see cref="DoPing(Station)"/>.
        /// </summary>
        /// <param name="station">Estación a obtener su propiedad.</param>
        /// <return>Latencia producida por <see cref="DoPing(Station)"/>.</return>
        public static Int16 GetPing(this Station station)
        {
            if (!_stationPing.ContainsKey(station))
                return -1;
            return _stationPing[station];
        }

        /// <summary>
        /// Obtiene el estado del enlace obtenida durante <see cref="DoPing(Station)"/>.
        /// </summary>
        /// <param name="station">Estación a obtener su propiedad.</param>
        /// <return>El estado del enlace producida por <see cref="DoPing(Station)"/>.</return>
        public static LinkState GetState(this Station station)
        {
            if (!_stationLinkState.ContainsKey(station))
                return LinkState.DISCONNECTED;
            return _stationLinkState[station];
        }
    }
}