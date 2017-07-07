using Opera.Acabus.TrunkMonitor.Models;
using Opera.Acabus.TrunkMonitor.Utils;
using System;

namespace Opera.Acabus.TrunkMonitor.Service
{
    /// <summary>
    /// Provee de funciones de servicio las instancias <see cref="Link"/>.
    /// </summary>
    public static class LinkService
    {
        /// <summary>
        /// Realiza un ping ambos extremos del enlace y obtiene la latencia generada.
        /// </summary>
        /// <param name="link">Enlace a realizar el ping.</param>
        /// <returns>La latencia del enlace de comunicación.</returns>
        public static Int16 DoPing(this Link link)
        {
            var pingA = link.StationA.DoPingLinkDevice();
            var pingB = link.StationB.DoPingLinkDevice();
            link.Ping = pingA > pingB ? pingA : pingB;

            link.State = LinkStateExtensions.CalculateLinkState(
                    pingA, 
                    link.StationA.GetMaximunPing(), 
                    link.StationA.GetMaximunAcceptablePing())
                .And(LinkStateExtensions.CalculateLinkState(
                    pingB, 
                    link.StationB.GetMaximunPing(),
                    link.StationB.GetMaximunAcceptablePing())
                    );

            if (pingA < 0 || pingB < 0)
            {
                link.Ping = -1;
                return -1;
            }

            return link.Ping;
        }
    }
}
