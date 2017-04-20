using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace MassiveSsh.Acabus
{
    internal class AcabusData
    {
        /// <summary>
        /// Archivo de configuración de las rutas.
        /// </summary>
        private static String FILE_NAME_CONFIG_XML = Path.Combine(Environment.CurrentDirectory, "Resources\\Trunks.Config");

        /// <summary>
        /// Archivo de historial de comandos de la consola Ssh.
        /// </summary>
        internal static readonly String SSH_HISTORY_FILENAME = Path.Combine(Environment.CurrentDirectory, "Resources\\ssh_history");

        /// <summary>
        /// Instancia que pertenece al documento XML.
        /// </summary>
        private static XmlDocument _xmlConfig = null;

        /// <summary>
        /// Lista de rutas troncales leidas desde el archivo XML.
        /// </summary>
        private static List<Trunk> _trunks;

        internal static string UsernameSsh { get; private set; }
        internal static string PasswordSsh { get; private set; }
        internal static string CmdCreateBackup { get; private set; }

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
            _xmlConfig = new XmlDocument();
            _xmlConfig.Load(FILE_NAME_CONFIG_XML);

            UsernameSsh = GetProperty("Username", "Credential-Ssh");
            PasswordSsh = GetProperty("Password", "Credential-Ssh");
            CmdCreateBackup = GetProperty("Backup", "Command-Ssh");

            Trunks.Clear();
            foreach (XmlNode trunkXmlNode in _xmlConfig.SelectSingleNode("Acabus").SelectSingleNode("Trunks"))
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

        /// <summary>
        /// Obtiene el valor de una propiedad especificada.
        /// </summary>
        /// <param name="name">Nombre de la propiedad.</param>
        /// <param name="type">Tipo de la propiedad.</param>
        /// <returns>Valor de la propiedad.</returns>
        public static String GetProperty(String name, String type)
        {
            foreach (var item in _xmlConfig.SelectSingleNode("Acabus").SelectSingleNode("Settings").SelectNodes("Property"))
            {
                String nameAttr = Utils.XmlUtils.GetAttribute(item as XmlNode, "name");
                String typeAttr = Utils.XmlUtils.GetAttribute(item as XmlNode, "type");
                if (nameAttr.Equals(name) && typeAttr.Equals(type))
                    return Utils.XmlUtils.GetAttribute(item as XmlNode, "value");
            }
            return null;
        }
    }
}
