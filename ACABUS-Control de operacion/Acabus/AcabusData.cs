using System;
using System.Collections.Generic;
using System.Xml;

namespace ACABUS_Control_de_operacion.Acabus
{
    internal class AcabusData
    {
        /// <summary>
        /// Archivo de configuración de las rutas.
        /// </summary>
        private static String FILE_NAME_CONFIG_XML = "Trunks.Config";

        /// <summary>
        /// Instancia que pertenece al documento XML.
        /// </summary>
        private static XmlDocument _xmlConfig = null;

        /// <summary>
        /// Lista de rutas troncales leidas desde el archivo XML.
        /// </summary>
        private static List<Trunk> _trunks;

        /// <summary>
        /// Ruta de acceso al directorio de PostgreSQL
        /// </summary>
        public static String PG_PATH = "/opt/PostgreSQL/9.3";

        /// <summary>
        /// Obtiene la lista de rutas troncales.
        /// </summary>
        public static List<Trunk> Trunks {
            get {
                if (_trunks == null)
                    _trunks = new List<Trunk>();
                return _trunks;
            }
        }

        /// <summary>
        /// Carga la configuración del XML en Trunk.Trunks.
        /// </summary>
        public static void LoadConfiguration()
        {
            Trunks.Clear();
            _xmlConfig = new XmlDocument();
            _xmlConfig.Load(FILE_NAME_CONFIG_XML);
            foreach (XmlNode trunkXmlNode in _xmlConfig.SelectSingleNode("Trunks"))
            {
                if (!trunkXmlNode.Name.Equals("Trunk"))
                    continue;
                var trunk = Trunk.ToTrunk(trunkXmlNode) as Trunk;
                Trunks.Add(trunk);
                LoadStations(trunk, trunkXmlNode.SelectSingleNode("Stations")?.ChildNodes);
            }
        }

        /// <summary>
        /// Carga las estaciones a partir de una lista de nodos
        /// XML.
        /// </summary>
        /// <param name="stationNodes">Lista de nodos XML que representan
        /// una estación cada uno.</param>
        private static void LoadStations(Trunk trunk, XmlNodeList stationNodes)
        {
            if (stationNodes != null)
                foreach (XmlNode stationXmlNode in stationNodes)
                {
                    if (!stationXmlNode.Name.Equals("Station"))
                        continue;
                    var station = Station.ToStation(stationXmlNode, trunk) as Station;
                    trunk.AddStation(station);
                    LoadDevices(station, stationXmlNode.SelectSingleNode("Devices")?.ChildNodes);
                }
        }

        /// <summary>
        /// Carga los dispositivos a partir de una lista de nodos XML.
        /// </summary>
        /// <param name="deviceNodes">Lista de nodos XML.</param>
        private static void LoadDevices(Station station, XmlNodeList deviceNodes)
        {
            if (deviceNodes != null)
                foreach (XmlNode deviceXmlNode in deviceNodes)
                {
                    var device = Device.ToDevice(deviceXmlNode, station) as Device;
                    station.AddDevice(device);
                }
        }
    }
}
