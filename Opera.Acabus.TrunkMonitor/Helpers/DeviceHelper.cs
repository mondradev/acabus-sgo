﻿using InnSyTech.Standard.Net;
using Opera.Acabus.Core.Models;
using Opera.Acabus.TrunkMonitor.Models;
using System;
using System.Collections.Generic;

namespace Opera.Acabus.TrunkMonitor.Helpers
{
    /// <summary>
    /// Provee a las instancias de <see cref="Device"/> el manejo de enlaces de comunicación <see
    /// cref="Link"/> y sus propiedades adheridas.
    /// </summary>
    public static class DeviceHelper
    { ///<summary>
      /// Lleva el control de todas las instancias <see cref="Device"/> y su estado de enlace.
      ///</summary>
        private static Dictionary<Device, DeviceStateInfo> _deviceStateInfo
            = new Dictionary<Device, DeviceStateInfo>();

        /// <summary>
        /// Calcula <see cref="LinkState"/> a partir de la latencia obtenida durante <see cref="DoPingLinkDevice(Station)"/>.
        /// </summary>
        /// <param name="device">Estación a obtener su estado de enlace.</param>
        /// <returns>Estado de enlace de la estación.</returns>
        public static LinkState CalculateLinkState(this Device device)
            => LinkStateHelper.CalculateLinkState(
                device.GetPing(),
                device.Station.GetMaximunPing(),
                device.Station.GetMaximunAcceptablePing());

        /// <summary>
        /// Realiza un eco al dispositivo obteniendo el tiempo de este.
        /// </summary>
        /// <param name="device">Dispositivo a realizar eco.</param>
        /// <returns>Duración del eco.</returns>
        public static Int16 DoPing(this Device device)
        {
            var info = device.GetStateInfo();
            var ping = ConnectionTCP.SendToPing(device.IPAddress.ToString(), 3);
            var linkState = device.CalculateLinkState();

            info.Ping = ping;
            info.State = linkState;

            return device.GetPing();
        }

        /// <summary>
        /// Obtiene la latencia obtenida durante <see cref="DoPing(Device)"/>.
        /// </summary>
        /// <param name="device">Dispositivo a obtener su propiedad.</param>
        /// <return>Latencia producida por <see cref="DoPing(Device)"/>.</return>
        public static Int16 GetPing(this Device device)
            => device.GetStateInfo().Ping;

        /// <summary>
        /// Obtiene el estado del enlace obtenida durante <see cref="DoPing(Device)"/>.
        /// </summary>
        /// <param name="device">Dispositivo a obtener su propiedad.</param>
        /// <return>El estado del enlace producida por <see cref="DoPing(Device)"/>.</return>
        public static LinkState GetState(this Device device)
            => device.GetStateInfo().State;

        /// <summary>
        /// Obtiene la información de estado del dispositivo.
        /// </summary>
        /// <param name="device">
        /// Instancia de <see cref="Device"/> a obtener su información de estado.
        /// </param>
        /// <returns>La información del estado del dispositivo.</returns>
        public static DeviceStateInfo GetStateInfo(this Device device)
        {
            lock (_deviceStateInfo)
                if (!_deviceStateInfo.ContainsKey(device))
                    _deviceStateInfo.Add(device, new DeviceStateInfo(device));

            return _deviceStateInfo[device];
        }
    }
}