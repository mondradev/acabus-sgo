using Acabus.Models;
using System;

namespace Acabus.Services
{
    public static class LinkService
    {
        public static Int16 DoPing(this Link link)
        {
            var pingA = StationService.DoPingLinkDevice(link.StationA);
            var pingB = StationService.DoPingLinkDevice(link.StationB);
            link.Ping = pingA > pingB ? pingA : pingB;

            link.State = StateValueExtension.GetConnectionState(pingA, link.StationA.PingMin, link.StationB.PingMax)
                .AndConnectionStete(StateValueExtension.GetConnectionState(pingB, link.StationB.PingMin, link.StationB.PingMax));

            if (pingA < 0 || pingB < 0)
            {
                link.Ping = -1;
                return -1;
            }

            return link.Ping;
        }
    }
}
