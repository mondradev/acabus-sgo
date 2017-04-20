using MassiveSsh.Utils;
using System;
using System.Diagnostics;
using System.Xml;

namespace MassiveSsh.Acabus
{
    /// <summary>
    /// Esta clase define la estructura básica de un equipo
    /// en ruta troncal.
    /// </summary>
    public class Device
    {
        /// <summary>
        /// Define los tipos de equipos disponibles.
        /// </summary>
        public enum DeviceType
        {
            /// <summary>
            /// Kiosko de venta y recarga.
            /// </summary>
            KVR,
            /// <summary>
            /// Torniquete de E/S.
            /// </summary>
            TOR,
            /// <summary>
            /// Torniquete Doble E/S.
            /// </summary>
            TD,
            /// <summary>
            /// Torniquete Simple de E/S.
            /// </summary>
            TS,
            /// <summary>
            /// Puerta para personas de movilidad reducida.
            /// </summary>
            PMR,
            /// <summary>
            /// Grabador de video en red
            /// </summary>
            NVR,
            /// <summary>
            /// Switch de estación
            /// </summary>
            SW,
            /// <summary>
            /// Concentrador de estación
            /// </summary>
            CDE
        }

        /// <summary>
        /// Hace la conversión de una cadena valida a una 
        /// instancia tipo de equipo.
        /// </summary>
        /// <param name="value">Cadena a convertir</param>
        /// <returns>Una instancia de tipo</returns>
        private static DeviceType? ParseType(String value)
        {
            switch (value)
            {
                case "KVR":
                    return DeviceType.KVR;
                case "PMR":
                    return DeviceType.PMR;
                case "TOR":
                    return DeviceType.TOR;
                case "TS":
                    return DeviceType.TS;
                case "TD":
                    return DeviceType.TD;
                case "NVR":
                    return DeviceType.NVR;
                case "SW":
                    return DeviceType.SW;
                case "CDE":
                    return DeviceType.CDE;
            }
            return null;
        }

        /// <summary>
        /// Convierte un nodo XML con una estructura correspondiente
        /// a un equipo de estación a una instancia Device.
        /// </summary>
        /// <param name="deviceXmlNode">Nodo XML que representa un equipo.</param>
        /// <param name="station">Estación a la que pertenece el equipo.</param>
        /// <returns>Una instancia de un equipo de estación.</returns>
        public static Device ToDevice(XmlNode deviceXmlNode, Station station)
        {
            try
            {
                if (!deviceXmlNode.Name.Equals("Device"))
                    return null;
                var device = new Device(station)
                {
                    ID = Int16.Parse(XmlUtils.GetAttribute(deviceXmlNode, "ID")),
                    IP = XmlUtils.GetAttribute(deviceXmlNode, "IP"),
                    Type = ParseType(XmlUtils.GetAttribute(deviceXmlNode, "Type")),
                    Status = XmlUtils.GetAttributeBool(deviceXmlNode, "Status"),
                    HasDataBase = XmlUtils.GetAttributeBool(deviceXmlNode, "HasDataBase"),
                    SshEnabled = XmlUtils.GetAttributeBool(deviceXmlNode, "SshEnabled")
                };
                if (device.Type == DeviceType.KVR)
                {
                    var kvr = Kvr.ToKVR(device);
                    kvr.MaxCard = XmlUtils.GetAttributeInt(deviceXmlNode, "MaxCard");
                    kvr.MinCard = XmlUtils.GetAttributeInt(deviceXmlNode, "MinCard");
                    kvr.IsExtern = XmlUtils.GetAttributeBool(deviceXmlNode, "IsExtern");
                    return kvr;
                }
                return device;
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is ArgumentNullException)
                    Trace.WriteLine("Un nodo 'Device' debe tener un ID ", "ERROR");
            }
            return null;
        }

        /// <summary>
        /// Obtiene el identificador del equipo en la 
        /// estación.
        /// </summary>
        public Int16 ID { get; protected set; }

        /// <summary>
        /// Obtiene el tipo de equipo
        /// </summary>
        public DeviceType? Type { get; protected set; }

        /// <summary>
        /// Obtiene la dirección IP del equipo
        /// </summary>
        public String IP { get; protected set; }

        /// <summary>
        /// Obtiene el estado del dispositivo.
        /// </summary>
        public Boolean Status { get; protected set; }

        /// <summary>
        /// Indica si tiene una base de datos
        /// </summary>
        public Boolean HasDataBase { get; set; }

        /// <summary>
        /// Indica si la consola segura está disponible.
        /// </summary>
        public Boolean SshEnabled { get; set; }

        /// <summary>
        /// Obtiene la estación a la que pertenece el 
        /// equipo.
        /// </summary>
        [XmlAnnotation(Ignore = true)] public Station Station { get; protected set; }

        /// <summary>
        /// Crea una instancia nueva de un equipo.
        /// </summary>
        /// <param name="station">Estación a la que pertence
        /// el equipo.</param>
        public Device(Station station)
        {
            this.Station = station;
            SshEnabled = false;
        }

        /// <summary>
        /// Una cadena que representa a este equipo.
        /// </summary>
        /// <returns>Un número de serie que identifica al equipo.</returns>
        public override String ToString()
        {
            return GetNumeSeri();
        }

        /// <summary>
        /// Obtiene el número de serie del equipo.
        /// </summary>
        /// <returns>El número de serie del equipo.</returns>
        public String GetNumeSeri()
        {
            var type = Type.ToString();
            var trunkID = this.Station.Trunk.ID.ToString("D2");
            var stationID = this.Station.ID.ToString("D2");
            var deviceID = ID.ToString("D2");
            return String.Format("{0}{1}{2}{3}", type, trunkID, stationID, deviceID);
        }


    }
}
