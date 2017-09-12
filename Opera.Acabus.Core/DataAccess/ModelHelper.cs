using InnSyTech.Standard.Utils;
using Opera.Acabus.Core.Models;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace Opera.Acabus.Core.DataAccess
{
    /// <summary>
    /// Provee métodos auxiliares para la manipulación de los modelos utilizados en el <see cref="Core"/>.
    /// </summary>
    public static class ModelHelper
    {
        /// <summary>
        /// Obtiene una instancia <see cref="Bus"/> desde un vector de bytes.
        /// </summary>
        /// <param name="bytes">Vector de bytes que contiene la instancia <see cref="Bus"/>.</param>
        /// <returns>Una instancia <see cref="Bus"/>.</returns>
        public static Bus GetBus(Byte[] bytes)
        {
            var id = BitConverter.ToUInt64(bytes, 0);
            var type = (BusType)bytes.Skip(8).Take(1).Single();
            var status = (BusStatus)bytes.Skip(9).Take(1).Single();
            var economicLength = BitConverter.ToUInt16(bytes, 10);

            var economicNumber = GetStringFromBytes(bytes, 12, economicLength);
            var routeID = BitConverter.ToUInt64(bytes, 12 + economicLength);

            return new Bus(id, economicNumber)
            {
                Route = AcabusDataContext.AllRoute?.SingleOrDefault(r => r.ID == routeID),
                Status = status,
                Type = type
            };
        }

        /// <summary>
        /// Obtiene los bytes que conforman una instancia <see cref="Staff"/>.
        /// </summary>
        /// <param name="staff">Instancia a convertir a bytes.</param>
        /// <returns>Un vector de bytes que representan la instancia <see cref="Staff"/></returns>
        public static Byte[] GetBytes(this Staff staff)
        {
            var id = staff.ID; // 8 bytes
            var type = (byte)staff.Area;
            var name = staff.Name;

            var bid = BitConverter.GetBytes(id);
            var bname = GetBytesFromString(name);
            var bnameLength = BitConverter.GetBytes((UInt16)bname.Length);

            return new[] { bid, new[] { type }, bnameLength, bname }.Merge().ToArray();
        }

        /// <summary>
        /// Obtiene los bytes que conforman una instancia <see cref="Bus"/>.
        /// </summary>
        /// <param name="bus">Instancia a convertir a bytes.</param>
        /// <returns>Un vector de bytes que representan la instancia <see cref="Bus"/></returns>
        public static Byte[] GetBytes(this Bus bus)
        {
            var id = bus.ID; // 8 bytes
            var route = bus.Route?.ID;
            var status = (byte)bus.Status;
            var type = (byte)bus.Type;
            var economic = bus.EconomicNumber;

            var bid = BitConverter.GetBytes(id);
            var broute = BitConverter.GetBytes(route ?? 0L);
            var beconomic = GetBytesFromString(economic);
            var beconomicLength = BitConverter.GetBytes((UInt16)beconomic.Length);

            return new[] { bid, new[] { type, status }, beconomicLength, beconomic, broute }.Merge().ToArray();
        }

        /// <summary>
        /// Obtiene los bytes que conforman una instancia <see cref="Device"/>.
        /// </summary>
        /// <param name="device">Instancia a convertir a bytes.</param>
        /// <returns>Un vector de bytes que representan la instancia <see cref="Device"/></returns>
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
            var bserial = GetBytesFromString(serial);

            return new[] { bid, bstation, bbus, bip, new byte[] { type }, bserial }.Merge().ToArray();
        }

        /// <summary>
        /// Obtiene los bytes que conforman una instancia <see cref="Route"/>.
        /// </summary>
        /// <param name="route">Instancia a convertir a bytes.</param>
        /// <returns>Un vector de bytes que representan la instancia <see cref="Route"/></returns>
        public static Byte[] GetBytes(this Route route)
        {
            var id = route.ID; // 8 bytes
            var name = route.Name; // n bytes
            var assignedSection = route.AssignedSection; // n bytes
            var type = (byte)route.Type; // 1 byte
            var number = route.RouteNumber; // 2 bytes

            if (!String.IsNullOrEmpty(assignedSection) && assignedSection.Length > 255)
                assignedSection = assignedSection?.Substring(0, 255);

            if (!String.IsNullOrEmpty(name) && name.Length > 255)
                name = name?.Substring(0, 255);

            var bid = BitConverter.GetBytes(id);
            var bnumber = BitConverter.GetBytes(number);
            var bassigned = GetBytesFromString(assignedSection);
            var bname = GetBytesFromString(name);
            var bassignedLength = BitConverter.GetBytes((UInt16)bassigned.Length);
            var bnameLength = BitConverter.GetBytes((UInt16)bname.Length);

            return new[] { bid, bnumber, new[] { type }, bnameLength, bname, bassignedLength, bassigned }.Merge().ToArray();
        }

        /// <summary>
        /// Obtiene los bytes que conforman una instancia <see cref="Station"/>.
        /// </summary>
        /// <param name="station">Instancia a convertir a bytes.</param>
        /// <returns>Un vector de bytes que representan la instancia <see cref="Station"/></returns>
        public static Byte[] GetBytes(this Station station)
        {
            var id = station.ID; // 8 bytes
            var name = station.Name; // n bytes
            var assignedSection = station.AssignedSection; // n bytes
            var number = station.StationNumber; // 2 bytes
            var route = station.Route?.ID;

            var bid = BitConverter.GetBytes(id);
            var bnumber = BitConverter.GetBytes(number);
            var bassigned = GetBytesFromString(assignedSection);
            var bname = GetBytesFromString(name);
            var bassignedLength = BitConverter.GetBytes((UInt16)bassigned.Length);
            var bnameLength = BitConverter.GetBytes((UInt16)bname.Length);
            var broute = BitConverter.GetBytes(route ?? 0L);

            return new[] { bid, bnumber, bnameLength, bname, bassignedLength, bassigned, broute }.Merge().ToArray();
        }

        /// <summary>
        /// Obtiene una instancia <see cref="Device"/> desde un vector de bytes.
        /// </summary>
        /// <param name="bytes">Vector de bytes que contiene la instancia <see cref="Device"/>.</param>
        /// <returns>Una instancia <see cref="Device"/>.</returns>
        public static Device GetDevice(Byte[] bytes)
        {
            var id = BitConverter.ToUInt64(bytes, 0);
            var bstation = BitConverter.ToUInt64(bytes, 8);
            var bbus = BitConverter.ToUInt64(bytes, 16);
            var ip = new IPAddress(bytes.Skip(24).Take(4).ToArray());
            var type = (DeviceType)bytes.Skip(28).Take(1).Single();
            var serial = GetStringFromBytes(bytes, 29, bytes.Length - 29);

            return new Device(id, serial, type)
            {
                IPAddress = ip,
                Bus = AcabusDataContext.AllBuses?.SingleOrDefault(b => b.ID == bbus),
                Station = AcabusDataContext.AllStations?.SingleOrDefault(s => s.ID == bstation)
            };
        }

        /// <summary>
        /// Obtiene una instancia <see cref="Route"/> desde un vector de bytes.
        /// </summary>
        /// <param name="bytes">Vector de bytes que contiene la instancia <see cref="Route"/>.</param>
        /// <returns>Una instancia <see cref="Route"/>.</returns>
        public static Route GetRoute(Byte[] bytes)
        {
            var id = BitConverter.ToUInt64(bytes, 0);
            var number = BitConverter.ToUInt16(bytes, 8);
            var type = (RouteType)bytes.Skip(10).Take(1).Single();

            var nameLenght = BitConverter.ToUInt16(bytes, 11);
            var assignationLength = BitConverter.ToUInt16(bytes, 13 + nameLenght);

            string name = GetStringFromBytes(bytes, 13, nameLenght);
            var assignation = GetStringFromBytes(bytes, 13 + nameLenght + 2, assignationLength);

            return new Route(id, number, type)
            {
                Name = name,
                AssignedSection = assignation
            };
        }

        /// <summary>
        /// Obtiene una instancia <see cref="Staff"/> desde un vector de bytes.
        /// </summary>
        /// <param name="bytes">Vector de bytes que contiene la instancia <see cref="Staff"/>.</param>
        /// <returns>Una instancia <see cref="Staff"/>.</returns>
        public static Staff GetStaff(Byte[] bytes)
        {
            var id = BitConverter.ToUInt64(bytes, 0);
            var area = (AssignableArea)bytes.Skip(8).Take(1).Single();
            var nameLength = BitConverter.ToUInt16(bytes, 9);
            var name = GetStringFromBytes(bytes, 11, nameLength);

            return new Staff(id)
            {
                Name = name,
                Area = area
            };
        }

        /// <summary>
        /// Obtiene una instancia <see cref="Station"/> desde un vector de bytes.
        /// </summary>
        /// <param name="bytes">Vector de bytes que contiene la instancia <see cref="Station"/>.</param>
        /// <returns>Una instancia <see cref="Station"/>.</returns>
        public static Station GetStation(Byte[] bytes)
        {
            var id = BitConverter.ToUInt64(bytes, 0);
            var number = BitConverter.ToUInt16(bytes, 8);

            var nameLenght = BitConverter.ToUInt16(bytes, 10);
            var assignationLength = BitConverter.ToUInt16(bytes, 12 + nameLenght);

            string name = GetStringFromBytes(bytes, 12, nameLenght);
            var assignation = GetStringFromBytes(bytes, 12 + nameLenght + 2, assignationLength);

            var routeID = BitConverter.ToUInt64(bytes, 12 + nameLenght + 2 + assignationLength);

            return new Station(id, number)
            {
                Name = name,
                AssignedSection = assignation,
                Route = AcabusDataContext.AllRoute?.SingleOrDefault(r => r.ID == routeID)
            };
        }

        /// <summary>
        /// Obtiene los bytes de la cadena especificada.
        /// </summary>
        /// <param name="s">Cadena a extraer los bytes.</param>
        /// <returns>Una secuencias de bytes que representan a la cadena especificada.</returns>
        private static byte[] GetBytesFromString(string s)
        {
            if (String.IsNullOrEmpty(s))
                return new byte[] { };

            if (s.Length > 255)
                s = s.Substring(0, 255);

            return Encoding.UTF8.GetBytes(s);
        }

        /// <summary>
        /// Obtiene una cadena a partir de una secuencia de bytes.
        /// </summary>
        /// <param name="bytes">Secuencia de bytes que contiene la cadena a obtener.</param>
        /// <param name="startIndex">Indice en la secuencia donde comienza los bytes de la cadena.</param>
        /// <param name="count">Número de bytes que componen la cadena.</param>
        /// <returns>La cadena obtenida de la secuencia especificada.</returns>
        private static string GetStringFromBytes(byte[] bytes, int startIndex, int count)
        {
            var s = Encoding.UTF8.GetString(bytes, startIndex, count);
            s = String.IsNullOrEmpty(s) ? null : s;
            return s;
        }
    }
}