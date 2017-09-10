using Acabus.Models;
using Acabus.Utils;
using Acabus.Utils.SecureShell;
using InnSyTech.Standard.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Acabus.DataAccess
{
    /// <summary>
    /// Esta clase permite el acceso a todas las configuraciones básicas y especificas de
    /// la aplicación <c>AcabusControlCenter</c>.
    /// </summary>
    internal static partial class AcabusData
    {
        /// <summary>
        /// Archivo donde se guarda la lista de unidades fuera de servicio.
        /// </summary>
        public static readonly String OFF_DUTY_VEHICLES_FILENAME = Path.Combine(Environment.CurrentDirectory, "Resources\\vehicles_{0:yyyyMMdd}.dat");

        /// <summary>
        /// Archivo de historial de comandos de la consola Ssh.
        /// </summary>
        public static readonly String SSH_HISTORY_FILENAME = Path.Combine(Environment.CurrentDirectory, "Resources\\ssh_history.dat");

        /// <summary>
        /// Archivo de configuración de las rutas.
        /// </summary>
        private static readonly String CONFIG_FILENAME = Path.Combine(Environment.CurrentDirectory, "Resources\\Trunks.Config");

        /// <summary>
        /// Campo que provee a la propiedad 'BusDisconnectedQuery'.
        /// </summary>
        private static String _busDisconnectedQuery;

        /// <summary>
        /// Campo que provee a la propiedad 'CmdCreateBackup'.
        /// </summary>
        private static String _cmdCreateBackup;

        /// <summary>
        /// Campo que provee a la propiedad 'Companies'.
        /// </summary>
        private static ObservableCollection<String> _companies;

        /// <summary>
        /// Campo que provee a la propiedad 'CountersFailingQuery'
        /// </summary>
        private static string _countersFailingQuery;

        /// <summary>
        /// Campo que provee a la propiedad 'DevicesQuery'.
        /// </summary>
        private static String _devicesQuery;

        /// <summary>
        /// Campo que provee a la propiedad 'FirstFolio'.
        /// </summary>
        private static UInt64 _firstFolio;

        /// <summary>
        /// Indica si la información ya fue cargada.
        /// </summary>
        private static Boolean _loadedData;

        /// <summary>
        /// Campo que provee a la propiedad 'Modules'.
        /// </summary>
        private static ObservableCollection<String> _modules;

        /// <summary>
        /// Campo que provee a la propiedad 'OffDutyVehicles'.
        /// </summary>
        private static ObservableCollection<Vehicle> _offDutyVehicles;

        /// <summary>
        /// Campo que provee a la propiedad 'PGDatabaseName'.
        /// </summary>
        private static String _pgDatabaseName;

        /// <summary>
        /// Campo que provee a la propiedad 'PGPathPlus'.
        /// </summary>
        private static String _pgPathPlus;

        /// <summary>
        /// Campo que provee a la propiedad 'PGPort'.
        /// </summary>
        private static UInt16 _pgPort;

        /// <summary>
        /// Campo que provee a la propiedad 'RoutesQuery'.
        /// </summary>
        private static String _routesQuery;

        /// <summary>
        /// Campo que provee a la propiedad 'StationsQuery'.
        /// </summary>
        private static String _stationsQuery;

        /// <summary>
        /// Campo que provee a la propiedad 'Technicians'.
        /// </summary>
        private static ObservableCollection<String> _technicians;

        /// <summary>
        /// Campo que provee a la propiedad 'TimeMaxLowPriorityBus'.
        /// </summary>
        private static TimeSpan _timeMaxLowPriorityBus;

        /// <summary>
        /// Campo que provee a la propiedad 'TimeMaxLowPriorityIncidence'.
        /// </summary>
        private static TimeSpan _timeMaxLowPriorityIncidence;

        /// <summary>
        /// Campo que provee a la propiedad 'TimeMaxLowPriorityIncidenceBus'.
        /// </summary>
        private static TimeSpan _timeMaxLowPriorityIncidenceBus;

        /// <summary>
        /// Campo que provee a la propiedad 'TimeMaxMediumPriorityBus'.
        /// </summary>
        private static TimeSpan _timeMaxMediumPriorityBus;

        /// <summary>
        /// Campo que provee a la propiedad 'TimeMaxMediumPriorityIncidence'.
        /// </summary>
        private static TimeSpan _timeMaxMediumPriorityIncidence;

        /// <summary>
        /// Campo que provee a la propiedad 'TimeMaxMediumPriorityIncidenceBus'.
        /// </summary>
        private static TimeSpan _timeMaxMediumPriorityIncidenceBus;

        /// <summary>
        /// Campo que provee a la propiedad 'TrunkAlertQuery'.
        /// </summary>
        private static String _trunkAlertQuery;

        /// <summary>
        /// Campo que provee a la propiedad 'VehiclesQuery'.
        /// </summary>
        private static String _vehiclesQuery;

        /// <summary>
        /// Instancia que pertenece al documento XML.
        /// </summary>
        private static XmlDocument _xmlConfig = null;

        /// <summary>
        /// Crea una instancia de 'AcabusData' y realiza la carga del archivo de configuración.
        /// </summary>
        static AcabusData()
        {
            Session = DbFactory_temp.CreateSession(typeof(SQLiteConnection), new SQLiteConfiguration());
            InitAcabusData();
        }

        /// <summary>
        /// Obtiene la sentencia SQL para consultar los vehículos sin conexión.
        /// </summary>
        public static String BusDisconnectedQuery => _busDisconnectedQuery;

        /// <summary>
        /// Obtiene un comando bash para generar un respaldo de base de datos en PostgreSQL 9.3
        /// </summary>
        public static String CmdCreateBackup => _cmdCreateBackup;

        /// <summary>
        /// Obtiene una lista de las empresas involucradas en la operación.
        /// </summary>
        public static ObservableCollection<String> Companies {
            get {
                if (_companies == null)
                    _companies = new ObservableCollection<String>();
                return _companies;
            }
        }

        /// <summary>
        /// Obtiene la sentencia SQL utilizada para la revisión de contadores.
        /// </summary>
        public static string CountersFailingQuery => _countersFailingQuery;

        /// <summary>
        /// Obtiene la sentencia SQL utilizada para la descarga de los datos de los equipos en operación.
        /// </summary>
        public static String DevicesQuery => _devicesQuery;

        /// <summary>
        /// Obtiene el folio que será el primero en utilizarse para reportes CCTV.
        /// </summary>
        public static UInt64 FirstFolio => _firstFolio;

        /// <summary>
        /// Obtiene una lista de los modulos instalados en la aplicación.
        /// </summary>
        public static ObservableCollection<String> Modules {
            get {
                if (_modules == null)
                    _modules = new ObservableCollection<String>();
                return _modules;
            }
        }

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
        /// Obtiene el nombre predeterminado de la base de datos.
        /// </summary>
        public static String PGDatabaseName => _pgDatabaseName;

        /// <summary>
        /// Obtiene la ruta de la instalación de PostgreSQL en un sistema Linux.
        /// </summary>
        public static String PGPathPlus => _pgPathPlus;

        /// <summary>
        /// Obtiene el puerto utilizado para la conexión al motor de PostgreSQL.
        /// </summary>
        public static UInt16 PGPort => _pgPort;

        /// <summary>
        /// Obtiene la sentencia SQL utilizada para la descarga de los datos de las rutas.
        /// </summary>
        public static String RoutesQuery => _routesQuery;

        public static DbSession Session { get; set; }

        /// <summary>
        /// Obtiene la sentencia SQL utilizada para la descarga de los datos de las estaciones.
        /// </summary>
        public static String StationsQuery => _stationsQuery;

        /// <summary>
        /// Obtiene una lista de los técnicos.
        /// </summary>
        public static ObservableCollection<String> Technicians {
            get {
                if (_technicians == null)
                    _technicians = new ObservableCollection<String>();
                return _technicians;
            }
        }

        /// <summary>
        /// Obtiene el tiempo máximo de una prioridad baja para alarma de autobus.
        /// </summary>
        public static TimeSpan TimeMaxLowPriorityBus => _timeMaxLowPriorityBus;

        /// <summary>
        /// Obtiene el tiempo máximo para baja prioridad en una incidencia.
        /// </summary>
        public static TimeSpan TimeMaxLowPriorityIncidence => _timeMaxLowPriorityIncidence;

        /// <summary>
        /// Obtiene el tiempo máximo para baja prioridad de una incidencia de autobus.
        /// </summary>
        public static TimeSpan TimeMaxLowPriorityIncidenceBus => _timeMaxLowPriorityIncidenceBus;

        /// <summary>
        /// Obtiene el tiempo máximo de una prioridad media para alarma de autobus.
        /// </summary>
        public static TimeSpan TimeMaxMediumPriorityBus => _timeMaxMediumPriorityBus;

        /// <summary>
        /// Obtiene el tiempo máximo para media prioridad de una incidencia.
        /// </summary>
        public static TimeSpan TimeMaxMediumPriorityIncidence => _timeMaxMediumPriorityIncidence;

        /// <summary>
        /// Obtiene el tiempo máximo para media prioridad de una incidencia de autobus.
        /// </summary>
        public static TimeSpan TimeMaxMediumPriorityIncidenceBus => _timeMaxMediumPriorityIncidenceBus;

        /// <summary>
        /// Obtiene la sentencia SQL para consultar las alarmas de vía.
        /// </summary>
        public static String TrunkAlertQuery => _trunkAlertQuery;

        /// <summary>
        /// Obtiene la sentencia SQL utilizada para la descarga de los datos de los vehículos.
        /// </summary>
        public static String VehiclesQuery => _vehiclesQuery;

        /// <summary>
        /// Ejecuta una consulta en el servidor de base de datos.
        /// </summary>
        /// <param name="query">Consulta a ejecutar.</param>
        /// <returns>Un arreglo bidimensional que contiene el resultado de la consulta realizada.</returns>
        public static String[][] ExecuteQueryInServerDB(String query)
        {
            var credentialDBServer = GetCredential("Server", "DataBase");
            var credentialSshServer = GetCredential("Server", "Ssh");

            if (query.ToUpper().Contains("UPDATE")
                || query.ToUpper().Contains("DELETE")
                || query.ToUpper().Contains("TRUNCATE")
                || query.ToUpper().Contains("DROP")
                || query.ToUpper().Contains("CREATE")
                || query.ToUpper().Contains("ALTER")) return null;

            SshPostgreSQL psql = SshPostgreSQL.CreateConnection(
                PGPathPlus,
                "172.17.0.121",
                PGPort,
                credentialDBServer.Username,
                credentialDBServer.Password,
                PGDatabaseName,
                credentialSshServer.Username,
                credentialSshServer.Password);

            var response = psql.ExecuteQuery(query);
            return response;
        }

        /// <summary>
        /// Ejecuta una función por cada ruta que exista dentro de la lista.
        /// </summary>
        /// <param name="routes">Lista de rutas.</param>
        /// <param name="action">Acción a ejecutar en la ruta.</param>
        public static void ForEachRoute(this IEnumerable<Route> routes, Action<Route> action)
        {
            foreach (var route in routes)
                action.Invoke(route);
        }

        /// <summary>
        /// Obtiene una credencial de acceso desde el archivo de configuración.
        /// </summary>
        public static Credential GetCredential(String alias, String type, Boolean isRoot = false)
        {
            XmlNodeList xmlNodeList = _xmlConfig?.SelectSingleNode("Acabus")?.SelectSingleNode("Credentials")?.SelectNodes("Credential");

            if (xmlNodeList == null)
                return null;

            foreach (XmlNode credentialXmlNode in xmlNodeList)
            {
                if (XmlUtils.GetAttribute(credentialXmlNode, "Alias").Equals(alias)
                    && XmlUtils.GetAttribute(credentialXmlNode, "Type").Equals(type)
                    && XmlUtils.GetAttributeBool(credentialXmlNode, "IsRoot").Equals(isRoot))
                    return ToCredential(credentialXmlNode);
            }
            return null;
        }

        /// <summary>
        /// Obtiene el valor de una propiedad especificada.
        /// </summary>
        /// <param name="name">Nombre de la propiedad.</param>
        /// <param name="type">Tipo de la propiedad.</param>
        /// <returns>Valor de la propiedad.</returns>
        public static String GetProperty(String name, String type)
        {
            LoadXmlConfig();
            XmlNodeList property = _xmlConfig?.SelectSingleNode("Acabus")?.SelectSingleNode("Settings")?.SelectNodes("Property");

            if (property == null)
                return null;

            foreach (var item in property)
            {
                String nameAttr = XmlUtils.GetAttribute(item as XmlNode, "Name");
                String typeAttr = XmlUtils.GetAttribute(item as XmlNode, "Type");
                if (nameAttr.Equals(name) && typeAttr.Equals(type))
                    return XmlUtils.GetAttribute(item as XmlNode, "Value");
            }
            return null;
        }

        /// <summary>
        /// Carga la lista de unidades fuera de servicio.
        /// </summary>
        public static void LoadOffDutyVehiclesSettings()
        {
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
            _countersFailingQuery = GetProperty("CountersFailing", "Command-Sql");

            _stationsQuery = GetProperty("Stations", "Command-Sql");
            _devicesQuery = GetProperty("Devices", "Command-Sql");
            _routesQuery = GetProperty("Routes", "Command-Sql");
            _vehiclesQuery = GetProperty("Vehicles", "Command-Sql");

            _firstFolio = UInt64.Parse(GetProperty("FirstFolio", "Setting"));
            _timeMaxLowPriorityBus = TimeSpan.Parse(GetProperty("TimeMaxLowPriorityBus", "Setting"));
            _timeMaxMediumPriorityBus = TimeSpan.Parse(GetProperty("TimeMaxMediumPriorityBus", "Setting"));

            _timeMaxLowPriorityIncidence = TimeSpan.Parse(GetProperty("TimeMaxLowPriorityIncidence", "Setting"));
            _timeMaxMediumPriorityIncidence = TimeSpan.Parse(GetProperty("TimeMaxMediumPriorityIncidence", "Setting"));

            _timeMaxLowPriorityIncidenceBus = TimeSpan.Parse(GetProperty("TimeMaxLowPriorityIncidenceBus", "Setting"));
            _timeMaxMediumPriorityIncidenceBus = TimeSpan.Parse(GetProperty("TimeMaxMediumPriorityIncidenceBus", "Setting"));
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
        /// Permite guarda la información modificada en las listas de configuración.
        /// </summary>
        public static void SaveXml()
        {
            File.Delete(CONFIG_FILENAME);
            File.WriteAllText(CONFIG_FILENAME, _xmlConfig.OuterXml);
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
        /// Carga la configuración del XML en Route.Trunks.
        /// </summary>
        private static void InitAcabusData()
        {
            if (_loadedData) return;
            try
            {
                LoadXmlConfig();

                LoadSettings();
                LoadTechniciansSettings();
                LoadCompaniesSettings();

                Acabus.Modules.Core.DataAccess.AcabusData.ReloadData();

                LoadModuleSettings();

                _loadedData = true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Ocurrió un problema al cargar la configuración", "ERROR");
                Trace.WriteLine(ex.Message, "DEBUG");
            }
        }

        /// <summary>
        /// Carga los nombres de las empresas involucradas en la operación.
        /// </summary>
        private static void LoadCompaniesSettings()
        {
            Companies.Clear();
            foreach (XmlNode companyXmlNode in _xmlConfig.SelectSingleNode("Acabus").SelectSingleNode("Companies")?.SelectNodes("Company"))
                Companies.Add(XmlUtils.GetAttribute(companyXmlNode, "Name"));
        }

        /// <summary>
        /// Carga los nombres de los modulos disponibles en la aplicación.
        /// </summary>
        private static void LoadModuleSettings()
        {
            Modules.Clear();
            foreach (XmlNode moduleXmlNode in _xmlConfig.SelectSingleNode("Acabus").SelectSingleNode("Modules")?.SelectNodes("Module"))
                Modules.Add(XmlUtils.GetAttribute(moduleXmlNode, "Class"));
        }

        /// <summary>
        /// Carga una lista de los técnicos.
        /// </summary>
        private static void LoadTechniciansSettings()
        {
            foreach (XmlNode technicianXmlNode in _xmlConfig.SelectSingleNode("Acabus").SelectSingleNode("Technicians")?.SelectNodes("Technician"))
                Technicians.Add(XmlUtils.GetAttribute(technicianXmlNode, "Name"));
        }

        private static void LoadXmlConfig()
        {
            try
            {
                _xmlConfig = new XmlDocument();
                _xmlConfig.Load(CONFIG_FILENAME);
            } catch(Exception ex)
            {
                Trace.WriteLine(ex.StackTrace, "ERROR");
            }
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

        private class SQLiteConfiguration : IDbConfiguration
        {
            public string ConnectionString => "Data Source=Resources/acabus_data.dat;Password=acabus*data*dat";

            public string LastInsertFunctionName => "last_insert_rowid";

            public int TransactionPerConnection => 1;
        }

        #region BasicOperations

        private static void FillList<T>(ref ICollection<T> list, Func<XmlNode, T> converterFunction, String collectionNode, String nodesName, XmlNode rootNode = null)
        {
            rootNode = rootNode is null ? _xmlConfig.SelectSingleNode("Acabus") : rootNode;

            foreach (XmlNode xmlNode in rootNode?.SelectSingleNode(collectionNode)?.SelectNodes(nodesName))
                list.Add(converterFunction.Invoke(xmlNode));
        }

        #endregion BasicOperations
    }
}