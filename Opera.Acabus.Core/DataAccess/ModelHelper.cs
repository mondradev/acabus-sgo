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

            return new[] { bid, bnumber, new[] { type, (byte)bname.Length }, bname, new[] { (byte)bassigned.Length }, bassigned }.Merge().ToArray();
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

            var nameLenght = bytes.Skip(11).Take(1).Single();
            var assignationLength = bytes.Skip(12 + nameLenght).Take(1).Single();

            var assignation = GetStringFromBytes(bytes, 12 + nameLenght + 1, assignationLength);
            string name = GetStringFromBytes(bytes, 12, nameLenght);

            return new Route(id, number, type)
            {
                Name = name,
                AssignedSection = assignation
            };
        }

        /// <summary>
        /// Obtiene los bytes de la cadena especificada.
        /// </summary>
        /// <param name="s">Cadena a extraer los bytes.</param>
        /// <returns>Una secuencias de bytes que representan a la cadena especificada.</returns>
        private static byte[] GetBytesFromString(string s)
            => String.IsNullOrEmpty(s) ? new byte[] { } : Encoding.UTF8.GetBytes(s);

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