using InnSyTech.Standard.Utils;
using Opera.Acabus.Core.Models;
using Opera.Acabus.TrunkMonitor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static Opera.Acabus.TrunkMonitor.Models.StationStateInfo;

namespace Opera.Acabus.TrunkMonitor.Helpers
{
    /// <summary>
    /// Provee a las instancias de <see cref="Station"/> el manejo de enlaces de comunicación <see
    /// cref="Link"/> y sus propiedades adheridas.
    /// </summary>
    public static class StationHelper
    {
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
            var info = station.GetStateInfo();

            ICollection<Link> links = info?.Links;

            if (links.Any(l => l.ID == link.ID && l != link))
                links.Remove(links.Single(l => l.ID == link.ID && l != link));

            if (!links.Contains(link))
                links.Add(link);
        }

        /// <summary>
        /// Calcula <see cref="LinkState"/> a partir de la latencia obtenida durante <see cref="DoPingLinkDevice(Station)"/>.
        /// </summary>
        /// <param name="station">Estación a obtener su estado de enlace.</param>
        /// <returns>Estado de enlace de la estación.</returns>
        public static LinkState CalculateLinkState(this Station station)
            => LinkStateHelper.CalculateLinkState(
                station.GetPing(),
                station.GetMaximunPing(),
                station.GetMaximunAcceptablePing());

        /// <summary>
        /// Verifica si todos los equipos de la estación están conectados.
        /// </summary>
        /// <param name="station">Estación a realizar la revisión.</param>
        public static void CheckDevice(this Station station)
        {
            var info = station.GetStateInfo();
            var devices = station.Devices.Where(d => new[] {
                DeviceType.APP,
                DeviceType.CDE,
                DeviceType.DB,
                DeviceType.KVR,
                DeviceType.NVR,
                DeviceType.PDE,
                DeviceType.PMR,
                DeviceType.SW,
                DeviceType.TD,
                DeviceType.TOR,
                DeviceType.TS,
                DeviceType.TSI
            }.Contains(d.Type));

            foreach (var device in devices)
                Task.Run(() =>
                {
                    var message = new StationMessage(device, $"Equipo desconectado: {device}", Priority.HIGH);

                    if (device.DoPing(2) < 0)
                    {
                        if (!info.Messages.Contains(message))
                            info.Messages.Add(message);
                    }
                    else
                        info.Messages.Remove(message);
                });
        }

        /// <summary>
        /// Verifica el estado de replica de los equipo del tipo <see cref="DeviceType.KVR"/>, <see
        /// cref="DeviceType.PMR"/>, <see cref="DeviceType.TD"/>, <see cref="DeviceType.TSI"/> y <see
        /// cref="DeviceType.TS"/>. En caso de haber problemas de replica se envían mensajes a las
        /// alertas de estación.
        /// </summary>
        /// <param name="stationToCheck">Estación a verificar el equipo replicado.</param>
        public static void VerifyReplica(this Station stationToCheck)
        {
            if (stationToCheck == null)
                return;

            if (stationToCheck.Devices.Count == 0)
                return;

            var info = stationToCheck.GetStateInfo();

            foreach (var device in stationToCheck.Devices.Where(d => new[] {
                DeviceType.KVR,
                DeviceType.PMR,
                DeviceType.TD,
                DeviceType.TS,
                DeviceType.TSI
            }.Contains(d.Type)))
            {
                Task.Run(() =>
                {
                    if (device.DoPing() < 0)
                        return;

                    var message = new StationMessage(device, $"Pendiente por replicar {device}", Priority.MEDIUM);

                    if (device.PendingReplica())
                    {
                        if (!info.Messages.Contains(message))
                            info.Messages.Add(message);
                    }
                    else
                        info.Messages.Remove(message);
                });
            }
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

            if (linkDevice == null && station.IsExternal)
                linkDevice = station.Devices.FirstOrDefault(d => d.Type == DeviceType.KVR);

            if (linkDevice == null)
                linkDevice = station.Devices
                    .GetRandom(d => new[] {
                        DeviceType.CDE,
                        DeviceType.KVR,
                        DeviceType.PMR,
                        DeviceType.TD,
                        DeviceType.TS,
                        DeviceType.TSI
                    }.Contains(d.Type));

            if (linkDevice == null) return -1;

            var ping = linkDevice.DoPing();

            return ping;
        }

        /// <summary>
        /// Obtiene todos los enlaces de esta estación.
        /// </summary>
        /// <param name="station">Instancia <see cref="Station"/> que contiene los enlaces.</param>
        /// <returns>Una lista de <see cref="Link"/>.</returns>
        public static ObservableCollection<Link> GetLinks(this Station station)
        {
            var info = station.GetStateInfo();
            return info.Links as ObservableCollection<Link>;
        }

        /// <summary>
        /// Obtiene la máxima latencia aceptable de la estación.
        /// </summary>
        /// <param name="station">Estación a obtener su propiedad.</param>
        /// <returns>La latencia máxima aceptable.</returns>
        public static UInt16 GetMaximunAcceptablePing(this Station station)
            => station.GetStateInfo().MaximunAcceptablePing;

        /// <summary>
        /// Obtiene el limite de latencia optima de la estación.
        /// </summary>
        /// <param name="station">Estación a obtener su propiedad.</param>
        /// <returns>El limite de latencia optima.</returns>
        public static UInt16 GetMaximunPing(this Station station)
            => station.GetStateInfo().MaximunPing;

        /// <summary>
        /// Obtiene la latencia obtenida durante <see cref="CheckDevice(Station)"/>.
        /// </summary>
        /// <param name="station">Estación a obtener su propiedad.</param>
        /// <return>Latencia producida por <see cref="CheckDevice(Station)"/>.</return>
        public static Int16 GetPing(this Station station)
            => station.GetStateInfo().Ping;

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
            {
                if (!_stationStateInfo.ContainsKey(station))
                    _stationStateInfo.Add(station, new StationStateInfo(station));

                return _stationStateInfo[station];
            }
        }

        /// <summary>
        /// Remueve el enlace de la estación.
        /// </summary>
        /// <param name="station">Estación a la que se quitará el enlace.</param>
        /// <param name="link">Enlace a remover.</param>
        public static bool RemoveLink(this Station station, Link link)
        {
            var info = station.GetStateInfo();
            var links = info.Links;

            if (links.Contains(link))
                return links.Remove(link);

            return false;
        }

        /// <summary>
        /// Establce la máxima latencia aceptable de la estación.
        /// </summary>
        /// <param name="station">Estación a obtener su propiedad.</param>
        /// <param name="maxAcceptablePing">La latencia máxima aceptable.</param>
        public static void SetMaximunAcceptablePing(this Station station, UInt16 maxAcceptablePing)
            => station.GetStateInfo().MaximunAcceptablePing = maxAcceptablePing;

        /// <summary>
        /// Establece el limite de latencia optima de la estación.
        /// </summary>
        /// <param name="station">Estación a obtener su propiedad.</param>
        /// <param name="maxPing">El limite de latencia optima.</param>
        public static void SetMaximunPing(this Station station, UInt16 maxPing)
            => station.GetStateInfo().MaximunPing = maxPing;
    }
}