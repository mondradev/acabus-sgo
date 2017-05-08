using Acabus.Models;
using Acabus.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Acabus.DataAccess
{
    internal sealed class AcabusData
    {
        /// <summary>
        /// Archivo de configuración de las rutas.
        /// </summary>
        private static String CONFIG_FILENAME = Path.Combine(Environment.CurrentDirectory, "Resources\\Trunks.Config");

        /// <summary>
        /// Archivo de historial de comandos de la consola Ssh.
        /// </summary>
        public static readonly String SSH_HISTORY_FILENAME = Path.Combine(Environment.CurrentDirectory, "Resources\\ssh_history.dat");

        /// <summary>
        /// Archivo donde se guarda la lista de unidades fuera de servicio.
        /// </summary>
        public static readonly String OFF_DUTY_VEHICLES_FILENAME = Path.Combine(Environment.CurrentDirectory, "Resources\\vehicles_{0:yyyyMMdd}.dat");

        /// <summary>
        /// Instancia que pertenece al documento XML.
        /// </summary>
        private static XmlDocument _xmlConfig = null;

        /// <summary>
        /// Campo que provee a la propiedad 'CmdCreateBackup'.
        /// </summary>
        private static String _cmdCreateBackup;

        /// <summary>
        /// Obtiene un comando bash para generar un respaldo de base de datos en PostgreSQL 9.3
        /// </summary>
        public static String CmdCreateBackup => _cmdCreateBackup;

        /// <summary>
        /// Campo que provee a la propiedad 'Trunks'.
        /// </summary>
        private static ObservableCollection<Trunk> _trunks;

        /// <summary>
        /// Obtiene una lista de las rutas troncales.
        /// </summary>
        public static ObservableCollection<Trunk> Trunks {
            get {
                if (_trunks == null)
                    _trunks = new ObservableCollection<Trunk>();
                return _trunks;
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'OffDutyVehicles'.
        /// </summary>
        private static ObservableCollection<Vehicle> _offDutyVehicles;

        /// <summary>
        /// Obtiene una lista de las unidades en taller o sin energía.
        /// </summary>
        public static ObservableCollection<Vehicle> OffDutyVehicles {
            get {
                if (_offDutyVehicles == null)
                    _offDutyVehicles = new ObservableCollection<Vehicle>();
                return _offDutyVehicles;
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'CC'.
        /// </summary>
        private static Station _cc;

        /// <summary>
        /// Indica si la información ya fue cargada.
        /// </summary>
        private static Boolean _loadedData;

        /// <summary>
        /// Obtiene o establece una instancia de estación que representa al centro de control.
        /// </summary>
        public static Station CC => _cc;

        /// <summary>
        /// Campo que provee a la propiedad 'PGPort'.
        /// </summary>
        private static UInt16 _pgPort;

        /// <summary>
        /// Obtiene el puerto utilizado para la conexión al motor de PostgreSQL.
        /// </summary>
        public static UInt16 PGPort => _pgPort;

        /// <summary>
        /// Campo que provee a la propiedad 'PGDatabaseName'.
        /// </summary>
        private static String _pgDatabaseName;

        /// <summary>
        /// Obtiene el nombre predeterminado de la base de datos.
        /// </summary>
        public static String PGDatabaseName => _pgDatabaseName;

        /// <summary>
        /// Campo que provee a la propiedad 'PGPathPlus'.
        /// </summary>
        private static String _pgPathPlus;

        /// <summary>
        /// Obtiene la ruta de la instalación de PostgreSQL en un sistema Linux.
        /// </summary>
        public static String PGPathPlus => _pgPathPlus;

        /// <summary>
        /// Campo que provee a la propiedad 'TrunkAlertQuery'.
        /// </summary>
        private static String _trunkAlertQuery;

        /// <summary>
        /// Obtiene la sentencia SQL para consultar las alarmas de vía.
        /// </summary>
        public static String TrunkAlertQuery => _trunkAlertQuery;

        /// <summary>
        /// Campo que provee a la propiedad 'BusDisconnectedQuery'.
        /// </summary>
        private static String _busDisconnectedQuery;

        /// <summary>
        /// Obtiene la sentencia SQL para consultar los vehículos sin conexión.
        /// </summary>
        public static String BusDisconnectedQuery => _busDisconnectedQuery;

        /// <summary>
        /// Crea una instancia de 'AcabusData' y realiza la carga del archivo de configuración.
        /// </summary>
        public AcabusData()
        {
            LoadConfiguration();
        }

        /// <summary>
        /// Carga la configuración del XML en Trunk.Trunks.
        /// </summary>
        public static void LoadConfiguration()
        {
            if (_loadedData) return;
            try
            {

                _xmlConfig = new XmlDocument();
                _xmlConfig.Load(CONFIG_FILENAME);

                LoadSettings();
                LoadTrunk();
                LoadCC();
                LoadLinks();
                LoadOffDutyVehicles();

                _loadedData = true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message, "ERROR");
            }
        }

        /// <summary>
        /// Carga la lista de unidades fuera de servicio.
        /// </summary>
        public static void LoadOffDutyVehicles()
        {
            var filename = String.Format(OFF_DUTY_VEHICLES_FILENAME, DateTime.Now);

            OffDutyVehicles.Clear();
            try
            {
                if (!File.Exists(filename)) return;

                var lines = File.ReadAllLines(filename);

                foreach (var line in lines)
                {
                    var economicNumber = line.Split('|')?[0];
                    var status = line.Split('|')?[1];

                    OffDutyVehicles.Add(new Vehicle(economicNumber, (VehicleStatus)Enum.Parse(typeof(VehicleStatus), status)));
                }
            }
            catch (IOException)
            {
                Trace.WriteLine("Ocurrió un error al intentar leer el archivo de la lista de vehículos.", "ERROR");
            }
        }

        /// <summary>
        /// Guarda toda la información de los vehículos en fuera de servicio.
        /// </summary>
        public static void SaveOffDutyVehiclesList()
        {
            var filename = String.Format(OFF_DUTY_VEHICLES_FILENAME, DateTime.Now);

            File.Delete(filename);
            try
            {
                foreach (Vehicle vehicle in OffDutyVehicles)
                    File.AppendAllText(filename, String.Format("{0}|{1}\n", vehicle.EconomicNumber, vehicle.Status));
            }
            catch (IOException)
            {
                Trace.WriteLine("Ocurrió un problema al intentar guardar la lista de vehículos.", "ERROR");
            }
        }

        /// <summary>
        /// Obtiene una credencial de acceso desde el archivo de configuración.
        /// </summary>
        public static Credential GetCredential(String alias, String type, Boolean isRoot = false)
        {
            foreach (XmlNode credentialXmlNode in _xmlConfig.SelectSingleNode("Acabus").SelectSingleNode("Credentials").SelectNodes("Credential"))
            {
                if (XmlUtils.GetAttribute(credentialXmlNode, "Alias").Equals(alias)
                    && XmlUtils.GetAttribute(credentialXmlNode, "Type").Equals(type)
                    && XmlUtils.GetAttributeBool(credentialXmlNode, "IsRoot").Equals(isRoot))
                    return ToCredential(credentialXmlNode);
            }
            return null;
        }

        /// <summary>
        /// Convierte un nodo XML que representa una credencial en una instancia de Credencial.
        /// </summary>
        /// <param name="credentialXmlNode">Node XML que representa la credencial.</param>
        /// <returns>Una instancia nueva de credencial.</returns>
        private static Credential ToCredential(XmlNode credentialXmlNode)
        {
            var username = XmlUtils.GetAttribute(credentialXmlNode, "Username");
            var password = XmlUtils.GetAttribute(credentialXmlNode, "Password");
            var type = XmlUtils.GetAttribute(credentialXmlNode, "Type");
            var isRoot = XmlUtils.GetAttributeBool(credentialXmlNode, "IsRoot");

            return new Credential(username, password, type, isRoot);
        }

        /// <summary>
        /// Carga la información leida del nodo CC del documento de configuración en XML.
        /// </summary>
        private static void LoadCC()
        {
            var ccNode = _xmlConfig.SelectSingleNode("Acabus").SelectSingleNode("CC");
            if (ccNode != null)
            {
                _cc = ToStation(ccNode, null);
                LoadDevices(CC, ccNode.SelectSingleNode("Devices").SelectNodes("Device"));
            }
        }

        /// <summary>
        /// Carga la información leida del nodo Links del documento de configuración en XML.
        /// </summary>
        private static void LoadLinks()
        {
            foreach (var xmlNode in _xmlConfig.SelectSingleNode("Acabus").SelectSingleNode("Links").SelectNodes("Link"))
            {
                var stationA = XmlUtils.GetAttribute(xmlNode as XmlNode, "StationA");
                var stationB = XmlUtils.GetAttribute(xmlNode as XmlNode, "StationB");
                var link = Link.CreateLink(
                    stationA.Equals("CC") ? CC : FindStation((station) => station.GetViaID().Equals(stationA)),
                    stationB.Equals("CC") ? CC : FindStation((station) => station.GetViaID().Equals(stationB))
                    );
            }
        }

        /// <summary>
        /// Carga la información leida del nodo Settings del documento de configuración en XML.
        /// </summary>
        public static void LoadSettings()
        {
            _cmdCreateBackup = GetProperty("Backup", "Command-Ssh");
            _pgDatabaseName = GetProperty("DBName", "PGSetting");
            _pgPathPlus = GetProperty("PathPlus", "PGSetting");
            _pgPort = UInt16.Parse(GetProperty("Port", "PGSetting"));
            _trunkAlertQuery = GetProperty("TrunkAlert", "Command-Sql");
            _busDisconnectedQuery = GetProperty("BusDisconnected", "Command-Sql");
        }

        /// <summary>
        /// Carga la información leida del nodo Trunks del documento de configuración en XML.
        /// </summary>
        public static void LoadTrunk()
        {
            Trunks.Clear();

            foreach (XmlNode trunkXmlNode in _xmlConfig.SelectSingleNode("Acabus").SelectSingleNode("Trunks").SelectNodes("Trunk"))
            {
                var trunk = ToTrunk(trunkXmlNode) as Trunk;
                Trunks.Add(trunk);
                LoadStations(trunk, trunkXmlNode.SelectSingleNode("Stations").SelectNodes("Station"));
            }
        }

        /// <summary>
        /// Obtiene el valor de una propiedad que tiene un vinculo a otra.
        /// Ej. Una propiedad de "Contraseña" tiene un vinculo con una propiedad de "Nombre de usuario".
        /// </summary>
        /// <param name="name">Nombre de la propiedad.</param>
        /// <param name="type">Tipo de la propiedad.</param>
        /// <param name="propertyLink">Propiedad que establece el vinculo.</param>
        /// <param name="linkValue">Valor del vinculo.</param>
        /// <returns></returns>
        private static string GetPropertyLinked(string name, string type, string propertyLink, string linkValue)
        {
            return XmlUtils.GetAttribute(GetPropertiesNode(name, type).Find((node) => XmlUtils.GetAttribute(node, propertyLink).Equals(linkValue)), "Value");
        }

        /// <summary>
        /// Obtiene todos los valores que corresponden a las propiedades con el nombre y tipo especificado.
        /// </summary>
        /// <param name="name">Nombre de la propiedad.</param>
        /// <param name="type">Tipo de la propiedad.</param>
        /// <returns>Una lista de valores de las propiedades.</returns>
        private static List<String> GetProperties(string name, string type)
        {
            List<String> values = new List<string>();
            GetPropertiesNode(name, type).ForEach((node) => values.Add(XmlUtils.GetAttribute(node, "Value")));
            return values;
        }

        /// <summary>
        /// Obtiene todos los nodos de propiedades que tienen el nombre y el tipo especificado.
        /// </summary>
        /// <param name="name">Nombre de la propiedad.</param>
        /// <param name="type">Tipo de la propiedad.</param>
        /// <returns>Una lista de nodos de propiedades.</returns>
        private static List<XmlNode> GetPropertiesNode(string name, string type)
        {
            List<XmlNode> nodes = null;
            foreach (var item in _xmlConfig.SelectSingleNode("Acabus").SelectSingleNode("Settings").SelectNodes("Property"))
            {
                if (nodes == null) nodes = new List<XmlNode>();
                if (XmlUtils.GetAttribute(item as XmlNode, "Name").Equals(name)
                    && XmlUtils.GetAttribute(item as XmlNode, "Type").Equals(type))
                    nodes.Add(item as XmlNode);
            }
            return nodes;
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
                    var station = ToStation(stationXmlNode, trunk) as Station;
                    trunk.AddStation(station);
                    LoadDevices(station, stationXmlNode.SelectSingleNode("Devices").SelectNodes("Device"));
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
                    var device = ToDevice(deviceXmlNode, station) as Device;
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
                String nameAttr = XmlUtils.GetAttribute(item as XmlNode, "Name");
                String typeAttr = XmlUtils.GetAttribute(item as XmlNode, "Type");
                if (nameAttr.Equals(name) && typeAttr.Equals(type))
                    return XmlUtils.GetAttribute(item as XmlNode, "Value");
            }
            return null;
        }



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
                var station = Station.CreateStation(trunk, (UInt16)XmlUtils.GetAttributeInt(stationXmlNode, "ID"));
                station.Name = XmlUtils.GetAttribute(stationXmlNode, "Name");
                station.IsConnected = XmlUtils.GetAttributeBool(stationXmlNode, "IsConnected");
                station.PingMin = (UInt16)XmlUtils.GetAttributeInt(stationXmlNode, "PingMin");
                station.PingMax = (UInt16)XmlUtils.GetAttributeInt(stationXmlNode, "PingMax");

                return station;
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is ArgumentNullException)
                    Trace.WriteLine("Un nodo 'Station' debe tener un ID ", "ERROR");
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
                var device = Device.CreateDevice(station,
                    (UInt16)XmlUtils.GetAttributeInt(deviceXmlNode, "ID"),
                    ParseType(XmlUtils.GetAttribute(deviceXmlNode, "Type")),
                    XmlUtils.GetAttributeBool(deviceXmlNode, "CanReplicate"));

                device.IP = XmlUtils.GetAttribute(deviceXmlNode, "IP");
                device.Enabled = XmlUtils.GetAttributeBool(deviceXmlNode, "Enabled");
                device.HasDataBase = XmlUtils.GetAttributeBool(deviceXmlNode, "HasDataBase");
                device.SshEnabled = XmlUtils.GetAttributeBool(deviceXmlNode, "SshEnabled");
                device.CanReplicate = XmlUtils.GetAttributeBool(deviceXmlNode, "CanReplicate");

                if (device.Type == DeviceType.KVR)
                {
                    var kvr = Kvr.FromDeviceToKvr(ref device);
                    kvr.MaxCard = (UInt16)XmlUtils.GetAttributeInt(deviceXmlNode, "MaxCard");
                    kvr.MinCard = (UInt16)XmlUtils.GetAttributeInt(deviceXmlNode, "MinCard");
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
        /// Hace la conversión de una cadena valida a una 
        /// instancia tipo de equipo.
        /// </summary>
        /// <param name="value">Cadena a convertir</param>
        /// <returns>Una instancia de tipo</returns>
        private static DeviceType ParseType(String value)
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
                case "APP":
                    return DeviceType.APP;
                case "PDE":
                    return DeviceType.PDE;
                case "DB":
                    return DeviceType.DB;
            }
            return DeviceType.NONE;
        }

        /// <summary>
        /// Convierte un nodo XML con la estructura correspondiente a un
        /// elemento de ruta troncal a una instancia Trunk.
        /// </summary>
        /// <param name="trunkNode">Nodo XML a parsear.</param>
        /// <returns>Una instancia de ruta troncal correspondiente al nodo
        /// pasado como argumento.</returns>
        public static Trunk ToTrunk(XmlNode trunkNode)
        {
            try
            {
                Trunk trunkTemp = Trunk.CreateTrunk(UInt16.Parse(XmlUtils.GetAttribute(trunkNode, "ID")));
                trunkTemp.Name = XmlUtils.GetAttribute(trunkNode, "Name");

                return trunkTemp;
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is ArgumentNullException)
                    Trace.WriteLine("Un nodo 'Trunk' debe tener un ID ", "ERROR");

            }
            return null;
        }

        /// <summary>
        /// Obtiene una estación que cumple con las condiciones establecidas por el predicado.
        /// </summary>
        /// <param name="predicate">Predicado que evaluará a las estaciones.</param>
        /// <returns>Una estación que cumple el predicado.</returns>
        public static Station FindStation(Predicate<Station> predicate)
        {
            foreach (var trunk in Trunks)
                foreach (var station in trunk.Stations)
                    if (predicate.Invoke(station))
                        return station;
            return null;
        }

        /// <summary>
        /// Obtiene un dispositivo que cumple con la condiciones establecidas por el predicado.
        /// </summary>
        /// <param name="predicate">Predicado que evaluará a los dispositivos.</param>
        /// <returns>Un dispositivo que cumple el predicado.</returns>
        public static Device FindDevice(Predicate<Device> predicate)
        {
            foreach (var trunk in Trunks)
                foreach (var station in trunk.Stations)
                    foreach (var device in station.Devices)
                        if (predicate.Invoke(device))
                            return device;
            return null;
        }


    }
}
