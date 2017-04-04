using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ACABUS_Control_de_operacion
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
        /// <param name="station">Nodo XML que representa una estación.</param>
        /// <param name="trunk">Ruta troncal a la que pertenece la estación.</param>
        /// <returns>Una instancia Station que representa una estación.</returns>
        public static Station ToStation(XmlNode station, Trunk trunk)
        {
            if (!station.Name.Equals("Station")) return null;
            var stationTemp = new Station(trunk)
            {
                Name = station.Attributes["name"].Value,
                ID = Int32.Parse(station.Attributes["id"].Value),
                Connected = Boolean.Parse(station.Attributes["connected"].Value)
            };
            return stationTemp;
        }

        #endregion

        /// <summary>
        /// Obtiene la ruta troncal a la que pertence la estación.
        /// </summary>
        public Trunk Trunk { get; private set; }

        /// <summary>
        /// Obtiene el identificador de estación.
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// Obtiene el nombre de la estación.
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// Obtiene la lista de dispositivos pertenecientes a la estación.
        /// </summary>
        public List<Device> Devices { get; private set; }

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
            Devices = new List<Device>();
            Trunk = trunk;
        }

        /// <summary>
        /// Carga los dispositivos a partir de una lista de nodos XML.
        /// </summary>
        /// <param name="childNodes">Lista de nodos XML.</param>
        public void LoadDevices(XmlNodeList childNodes)
        {
            Devices.Clear();
            foreach (XmlNode device in childNodes)
            {
                var deviceTemp = Device.ToDevice(device, this) as Device;
                Devices.Add(deviceTemp);
            }
        }
    }
}
