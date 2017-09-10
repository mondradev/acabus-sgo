using InnSyTech.Standard.Utils;
using Opera.Acabus.Core.Models;
using Opera.Acabus.TrunkMonitor.Models;
using Opera.Acabus.TrunkMonitor.Utils;
using System;
using System.Collections.Generic;

namespace Opera.Acabus.TrunkMonitor.Service
{
    public static class DeviceService
    { ///<summary>
      /// Lleva el control de todas las instancias <see cref="Station"/> y su estado de enlace.
      ///</summary>
        private static Dictionary<Device, LinkState> _deviceLinkState
            = new Dictionary<Device, LinkState>();

        ///<summary>
        /// Lleva el control de todas las instancias <see cref="Station"/> y su latencia.
        ///</summary>
        private static Dictionary<Device, Int16> _devicePing
            = new Dictionary<Device, short>();

        /// <summary>
        /// Calcula <see cref="LinkState"/> a partir de la latencia obtenida durante <see cref="DoPingLinkDevice(Station)"/>.
        /// </summary>
        /// <param name="device">Estación a obtener su estado de enlace.</param>
        /// <returns>Estado de enlace de la estación.</returns>
        public static LinkState CalculateLinkState(this Device device)
            => LinkStateExtensions.CalculateLinkState(
                device.GetPing(),
                device.Station.GetMaximunPing(),
                device.Station.GetMaximunAcceptablePing());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static Int16 DoPing(this Device device)
        {

            var ping = ConnectionTCP.SendToPing(device.IPAddress.ToString(), 3);
            lock (_devicePing)
                if (!_devicePing.ContainsKey(device))
                    _devicePing.Add(device, ping);
                else
                    _devicePing[device] = ping;

            var linkState = device.CalculateLinkState();

            lock (_deviceLinkState)
                if (!_deviceLinkState.ContainsKey(device))
                    _deviceLinkState.Add(device, linkState);
                else
                    _deviceLinkState[device] = linkState;


            return device.GetPing();

        }

        /// <summary>
        /// Obtiene la latencia obtenida durante <see cref="DoPing(Device)"/>.
        /// </summary>
        /// <param name="station">Estación a obtener su propiedad.</param>
        /// <return>Latencia producida por <see cref="DoPing(Device)"/>.</return>
        public static Int16 GetPing(this Device station)
        {
            if (!_devicePing.ContainsKey(station))
                return -1;
            return _devicePing[station];
        }

        /// <summary>
        /// Obtiene el estado del enlace obtenida durante <see cref="DoPing(Device)"/>.
        /// </summary>
        /// <param name="station">Estación a obtener su propiedad.</param>
        /// <return>El estado del enlace producida por <see cref="DoPing(Device)"/>.</return>
        public static LinkState GetState(this Device station)
        {
            if (!_deviceLinkState.ContainsKey(station))
                return LinkState.DISCONNECTED;
            return _deviceLinkState[station];
        }
    }
}