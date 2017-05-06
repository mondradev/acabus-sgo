using Acabus.Models;
using Acabus.Utils;
using System;

namespace Acabus.Services
{
    public sealed class DeviceService
    {
        public static Int16 DoPing(Device device)
        {
            device.Ping = ConnectionTCP.SendToPing(device.IP, 3);

            device.State = Util.GetConnectionState(device.Ping, device.Station.PingMin, device.Station.PingMax);

            return device.Ping;
        }
    }
}
