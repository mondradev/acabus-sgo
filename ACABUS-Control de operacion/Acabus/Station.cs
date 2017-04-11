using ACABUS_Control_de_operacion.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace ACABUS_Control_de_operacion.Acabus
{
    /// <summary>
    /// Esta clase define la estructura de una estación perteneciente a una ruta troncal.
    /// </summary>
    public class Station
    {
        #region StaticFunctions

        /// <summary>
        /// Convierte un nodo XML con una estructura correspondiente a una
        /// estación que pertenece a una ruta troncal en una instancia de
        /// Station.
        /// </summary>
        /// <param name="stationXmlNode">Nodo XML que representa una estación.</param>
        /// <param name="trunk">Ruta troncal a la que pertenece la estación.</param>
        /// <returns>Una instancia Station que representa una estación.</returns>
        public static Station ToStation(XmlNode stationXmlNode, Trunk trunk)
        {
            try
            {
                if (!stationXmlNode.Name.Equals("Station")) return null;
                var station = new Station(trunk)
                {
                    Name = XmlUtils.GetAttribute(stationXmlNode, "Name"),
                    ID = XmlUtils.GetAttributeInt(stationXmlNode, "ID"),
                    Connected = XmlUtils.GetAttributeBool(stationXmlNode, "Connected")
                };
                return station;
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is ArgumentNullException)
                    Trace.WriteLine("Un nodo 'Station' debe tener un ID ", "ERROR");
            }
            return null;
        }

        #endregion

        public List<Device> Devices { get; set; }

        /// <summary>
        /// Obtiene la ruta troncal a la que pertence la estación.
        /// </summary>
        [XmlAnnotation(Ignore = true)] public Trunk Trunk { get; private set; }

        /// <summary>
        /// Obtiene el identificador de estación.
        /// </summary>
        public Int16 ID { get; private set; }

        /// <summary>
        /// Obtiene el nombre de la estación.
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// Indica si la estación esta conectada a la red.
        /// </summary>
        public Boolean Connected { get; set; }

        /// <summary>
        /// Crea una instancia de una estación indicando la ruta troncal 
        /// a la que pertence.
        /// </summary>
        /// <param name="trunk">Ruta troncal a la que pertenece la estación.</param>
        public Station(Trunk trunk)
        {
            Trunk = trunk;
            Devices = new List<Device>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        public void AddDevice(Device device)
        {
            Devices.Add(device);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idDevice"></param>
        /// <returns></returns>
        public Device GetDevice(Int16 idDevice)
        {
            foreach (var device in Devices)
                if (device.ID == idDevice) return device;
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        public void RemoveDevice(Device device)
        {
            Devices.Remove(device);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Int16 DeviceCount()
        {
            return (Int16)(Devices.Count);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Device[] GetDevices()
        {
            return Devices.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("Estación: {0}", Name);
        }

    }
}
