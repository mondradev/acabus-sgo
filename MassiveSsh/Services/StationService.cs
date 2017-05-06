using Acabus.Models;
using System;

namespace Acabus.Services
{
    public static class StationService
    {
        public static Int16 DoPing(Station station)
        {
            var ping = 0;
            var nDevice = 0;
            foreach (var device in station.Devices)
            {
                var pingTemp = DeviceService.DoPing(device);
                if (device.State != StateValue.DISCONNECTED)
                {
                    ping += pingTemp;
                    nDevice++;
                }
            }

            station.State = Util.GetConnectionState((Int16)(ping / nDevice), station.PingMin, station.PingMax);

            return (Int16)(ping / nDevice);
        }

        public static Int16 DoPingLinkDevice(Station station)
        {
            var linkDevice = station.FindDevice((device) => device.Type == DeviceType.SW);
            if (linkDevice == null)
                linkDevice = station.Devices?[0];
            if (linkDevice == null) return -1;
            var ping = DeviceService.DoPing(linkDevice);
            return ping;
        }
    }
}
