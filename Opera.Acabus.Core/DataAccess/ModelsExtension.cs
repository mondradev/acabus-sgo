using InnSyTech.Standard.Utils;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Opera.Acabus.Core.DataAccess
{
    public static class ModelsExtension
    {

        public static Byte[] GetBytes(this Device device)
        {
            var id = device.ID; // 8 bytes
            var serial = device.SerialNumber; // n bytes
            var ip = device.IPAddress; // 4 bytes
            var type = (byte)device.Type; // 1 byte
            var station = device.Station?.ID; // 8 bytes
            var bus = device.Bus?.ID;  // 8 bytes

            var bid = BitConverter.GetBytes(id);
            var bstation = BitConverter.GetBytes(station ?? 0L);
            var bbus = BitConverter.GetBytes(bus ?? 0L);
            var bip = ip.GetAddressBytes();
            var bserial = Encoding.UTF8.GetBytes(serial);

            return new[] { bid, bstation, bbus, bip, new byte[] { type }, bserial }.Merge().ToArray();
        }

        public static Device GetDevice(Byte[] bytes)
        {
            var id = BitConverter.ToUInt64(bytes.Take(8).ToArray(), 0);
            var bstation = BitConverter.ToUInt64(bytes.Skip(8).Take(8).ToArray(), 0);
            var bbus = BitConverter.ToUInt64(bytes.Skip(16).Take(8).ToArray(), 0);
            var ip = new IPAddress(bytes.Skip(24).Take(4).ToArray());
            var type = (DeviceType)bytes.Skip(28).Take(1).Single();
            var serial = Encoding.UTF8.GetString(bytes.Skip(29).ToArray());

            return new Device(id, serial, type)
            {
                IPAddress = ip,
                Bus = AcabusData.AllBuses?.FirstOrDefault(b => b.ID == bbus),
                Station = AcabusData.AllStations?.FirstOrDefault(s => s.ID == bstation)
            };
        }

    }
}
