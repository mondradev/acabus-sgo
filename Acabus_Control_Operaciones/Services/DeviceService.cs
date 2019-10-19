using Acabus.Models;
using Acabus.Utils;
using System;

namespace Acabus.Services
{
    public static class DeviceService
    {
        public static Int16 DoPing(this Device device)
        {
            device.Ping = ConnectionTCP.SendToPing(device.IP, 3);

            device.State = StateValueExtension.GetConnectionState(device.Ping, device.Station.PingMin, device.Station.PingMax);

            return device.Ping;
        }
    }
}
