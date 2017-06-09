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
        /// Campo que provee a la propiedad 'CC'.
        /// </summary>
        private static Station _cc;

        /// <summary>
        /// Campo que provee a la propiedad 'CmdCreateBackup'.
        /// </summary>
        private static String _cmdCreateBackup;

        /// <summary>
        /// Campo que provee a la propiedad 'CommonFaults'.
        /// </summary>
        private static ObservableCollection<String> _commonFaults;

        /// <summary>
        /// Campo que provee a la propiedad 'Companies'.
        /// </summary>
        private static ObservableCollection<String> _companies;

        /// <summary>
        /// Campo que provee a la propiedad 'DevicesQuery'.
        /// </summary>
        private static String _devicesQuery;

        /// <summary>
        /// Campo que provee a la propiedad 'FirstFolio'.
        /// </summary>
        private static UInt64 _firstFolio;

        /// <summary>
        /// Campo que provee a la propiedad 'LinesQuery'.
        /// </summary>
        private static String _linesQuery;

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
        /// Campo que provee a la propiedad 'Routes'.
        /// </summary>
        private static ObservableCollection<Route> _routes;

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
        /// Campo que provee a la propiedad 'TrunksQuery'.
        /// </summary>
        private static String _trunksQuery;

        /// <summary>
        /// Campo que provee a la propiedad 'VehicleAsignQuery'.
        /// </summary>
        private static String _vehicleAsignQuery;

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
            Session = DbManager.CreateSession(typeof(SQLiteConnection), new SQLiteConfiguration());
            InitAcabusData();
            //foreach (var vehi in FindVehicles(vehicle => vehicle.BusType == VehicleType.ARTICULATED))
            //{
            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.PCA, String.Format("PCA01{0:D3}01", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "PC ABORDO",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });
            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.DSPB, String.Format("DSPB01{0:D3}01", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "DISPLAY BUS",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });
            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.DSPB, String.Format("DSPB01{0:D3}02", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "DISPLAY BUS",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });
            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.MON, String.Format("MON01{0:D3}02", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "MONITOR",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });
            //}
            //foreach (var vehi in FindVehicles(vehicle => vehicle.BusType == VehicleType.STANDARD))
            //{
            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.PCA, String.Format("PCA02{0:D3}01", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "PC ABORDO",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });
            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.CAM, String.Format("CAM02{0:D3}01", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "CAMARA",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });
            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.CAM, String.Format("CAM02{0:D3}02", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "CAMARA",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });
            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.TA, String.Format("TA02{0:D3}01", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "TORNIQUETE ABORDO",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });
            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.MON, String.Format("MON02{0:D3}01", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "MONITOR",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });

            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.NVRA, String.Format("NVRA02{0:D3}01", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "NVR ABORDO",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });

            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.CONT, String.Format("CONT02{0:D3}01", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "CONTADOR DE PASAJEROS",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });

            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.DSPB, String.Format("DSPB02{0:D3}01", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "DISPLAY BUS",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });
            //}
            //foreach (var vehi in FindVehicles(vehicle => vehicle.BusType == VehicleType.CONVENTIONAL))
            //{
            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.PCA, String.Format("PCA03{0:D3}01", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "PC ABORDO",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });
            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.CAM, String.Format("CAM03{0:D3}01", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "CAMARA",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });
            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.CAM, String.Format("CAM03{0:D3}02", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "CAMARA",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });
            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.TA, String.Format("TA03{0:D3}01", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "TORNIQUETE ABORDO",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });
            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.MON, String.Format("MON03{0:D3}01", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "MONITOR",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });

            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.NVRA, String.Format("NVRA03{0:D3}01", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "NVR ABORDO",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });

            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.CONT, String.Format("CONT03{0:D3}01", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "CONTADOR DE PASAJEROS",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });

            //    Session.Save(new DeviceBus(default(UInt16), DeviceType.DSPB, String.Format("DSPB03{0:D3}01", Regex.Match(vehi.EconomicNumber, "[0-9]{3}").Value))
            //    {
            //        Description = "DISPLAY BUS",
            //        CanReplicate = false,
            //        Enabled = true,
            //        HasDatabase = false,
            //        IP = "0.0.0.0",
            //        SshEnabled = false,
            //        Vehicle = vehi
            //    });
            //}
        }

        /// <summary>
        /// Obtiene la sentencia SQL para consultar los vehículos sin conexión.
        /// </summary>
        public static String BusDisconnectedQuery => _busDisconnectedQuery;

        /// <summary>
        /// Obtiene o establece una instancia de estación que representa al centro de control.
        /// </summary>
        public static Station CC => _cc;

        /// <summary>
        /// Obtiene un comando bash para generar un respaldo de base de datos en PostgreSQL 9.3
        /// </summary>
        public static String CmdCreateBackup => _cmdCreateBackup;

        /// <summary>
        /// Obtiene una lista de las fallas comunes de la operación.
        /// </summary>
        public static ObservableCollection<String> CommonFaults {
            get {
                if (_commonFaults == null)
                    _commonFaults = new ObservableCollection<String>();
                return _commonFaults;
            }
        }

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
        /// Obtiene la sentencia SQL utilizada para la descarga de los datos de los equipos en operación.
        /// </summary>
        public static String DevicesQuery => _devicesQuery;

        /// <summary>
        /// Obtiene el folio que será el primero en utilizarse para reportes CCTV.
        /// </summary>
        public static UInt64 FirstFolio => _firstFolio;

        /// <summary>
        /// Obtiene la sentencia SQL utilizada para la descarga de los datos de las lineas validas.
        /// </summary>
        public static String LinesQuery => _linesQuery;

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
        /// Obtiene una lista de las rutas.
        /// </summary>
        public static ObservableCollection<Route> Routes {
            get {
                if (_routes == null)
                    _routes = new ObservableCollection<Route>();
                return _routes;
            }
        }

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
        /// Obtiene una lista de las rutas troncales.
        /// </summary>
        public static ObservableCollection<Route> Trunks
            => (ObservableCollection<Route>)Util.Where(Routes, (route) => route.RouteType == RouteType.TRUNK);

        /// <summary>
        /// Obtiene la sentencia SQL utilizada para la descarga de los datos de las rutas troncales.
        /// </summary>
        public static String TrunksQuery => _trunksQuery;

        /// <summary>
        /// Obtiene la sentencia SQL utilizada para la descarga de la última asignación de ruta.
        /// </summary>
        public static String VehicleAsignQuery => _vehicleAsignQuery;

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
        /// Obtiene un dispositivo que cumple con la condiciones establecidas por el predicado.
        /// </summary>
        /// <param name="predicate">Predicado que evaluará a los dispositivos.</param>
        /// <returns>Un dispositivo que cumple el predicado.</returns>
        public static Device FindDevice(Predicate<Device> predicate)
        {
            foreach (Trunk trunk in Trunks)
                foreach (var station in trunk.Stations)
                    foreach (var device in station.Devices)
                        if (predicate.Invoke(device))
                            return device;
            return null;
        }

        /// <summary>
        /// Obtiene los dispositivos que cumple con la condiciones establecidas por el predicado.
        /// </summary>
        /// <param name="predicate">Predicado que evaluará a los dispositivos.</param>
        /// <returns>Un vector unidimensional de dispositivos que cumple el predicado.</returns>
        public static IEnumerable<Device> FindDevices(Predicate<Device> predicate)
        {
            List<Device> devices = new List<Device>();
            foreach (Trunk trunk in Trunks)
                foreach (var station in trunk.Stations)
                    foreach (var device in station.Devices)
                        if (predicate.Invoke(device))
                            devices.Add(device);
            return devices.ToArray();
        }

        /// <summary>
        /// Obtiene la ruta que cumple con la condiciones establecidas por el predicado.
        /// </summary>
        /// <param name="predicate">Predicado que evaluará a los dispositivos.</param>
        /// <returns>Una ruta que cumple el predicado.</returns>
        public static Route FindRoute(this IEnumerable<Route> routes, Predicate<Route> predicate)
        {
            foreach (Route route in routes)
                if (predicate.Invoke(route))
                    return route;
            return null;
        }

        /// <summary>
        /// Obtiene una estación que cumple con las condiciones establecidas por el predicado.
        /// </summary>
        /// <param name="predicate">Predicado que evaluará a las estaciones.</param>
        /// <returns>Una estación que cumple el predicado.</returns>
        public static Station FindStation(Predicate<Station> predicate)
        {
            foreach (Trunk trunk in Trunks)
                foreach (var station in trunk.Stations)
                    if (predicate.Invoke(station))
                        return station;
            return null;
        }

        /// <summary>
        /// Obtiene las estaciones que cumple con las condiciones establecidas por el predicado.
        /// </summary>
        /// <param name="predicate">Predicado que evaluará a las estaciones.</param>
        /// <returns>Un vector unidimensional de estaciones que cumple el predicado.</returns>
        public static Station[] FindStations(Predicate<Station> predicate)
        {
            List<Station> stations = new List<Station>();
            foreach (Trunk trunk in Trunks)
                foreach (var station in trunk.Stations)
                    if (predicate.Invoke(station))
                        stations.Add(station);
            return stations.ToArray();
        }

        /// <summary>
        /// Obtiene la ruta troncal que cumple con la condiciones establecidas por el predicado.
        /// </summary>
        /// <param name="predicate">Predicado que evaluará a los dispositivos.</param>
        /// <returns>Una ruta troncal que cumple el predicado.</returns>
        public static Trunk FindTrunk(this IEnumerable<Route> trunks, Predicate<Trunk> predicate)
        {
            foreach (Trunk trunk in trunks)
                if (trunk.RouteType == RouteType.TRUNK && predicate.Invoke(trunk))
                    return trunk;
            return null;
        }

        /// <summary>
        /// Obtiene el vehículo que cumple con la condiciones establecidas por el predicado.
        /// </summary>
        /// <param name="predicate">Predicado que evaluará a los vehículos.</param>
        /// <returns>Un vehículo que cumple el predicado.</returns>
        public static Vehicle FindVehicle(Predicate<Vehicle> predicate)
        {
            foreach (Route route in Routes)
                foreach (var vehicle in route.Vehicles)
                    if (predicate.Invoke(vehicle))
                        return vehicle;
            return null;
        }

        /// <summary>
        /// Obtiene los vehículos que cumple con la condiciones establecidas por el predicado.
        /// </summary>
        /// <param name="predicate">Predicado que evaluará a los vehículos.</param>
        /// <returns>Un vector unidimensional de vehículos que cumple el predicado.</returns>
        public static IEnumerable<Vehicle> FindVehicles(Predicate<Vehicle> predicate)
        {
            List<Vehicle> vehicles = new List<Vehicle>();
            foreach (Route route in Routes)
                foreach (var vehicle in route.Vehicles)
                    if (predicate.Invoke(vehicle))
                        vehicles.Add(vehicle);
            return vehicles.ToArray();
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
        /// Carga la lista de unidades fuera de servicio.
        /// </summary>
        public static void LoadOffDutyVehiclesSettings()
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

            _trunksQuery = GetProperty("Trunks", "Command-Sql");
            _stationsQuery = GetProperty("Stations", "Command-Sql");
            _linesQuery = GetProperty("Lines", "Command-Sql");
            _devicesQuery = GetProperty("Devices", "Command-Sql");
            _routesQuery = GetProperty("Routes", "Command-Sql");
            _vehiclesQuery = GetProperty("Vehicles", "Command-Sql");
            _vehicleAsignQuery = GetProperty("VehicleAsign", "Command-Sql");

            _firstFolio = UInt64.Parse(GetProperty("FirstFolio", "Setting"));
            _timeMaxLowPriorityBus = TimeSpan.Parse(GetProperty("TimeMaxLowPriorityBus", "Setting"));
            _timeMaxMediumPriorityBus = TimeSpan.Parse(GetProperty("TimeMaxMediumPriorityBus", "Setting"));

            _timeMaxLowPriorityIncidence = TimeSpan.Parse(GetProperty("TimeMaxLowPriorityIncidence", "Setting"));
            _timeMaxMediumPriorityIncidence = TimeSpan.Parse(GetProperty("TimeMaxMediumPriorityIncidence", "Setting"));

            _timeMaxLowPriorityIncidenceBus = TimeSpan.Parse(GetProperty("TimeMaxLowPriorityIncidenceBus", "Setting"));
            _timeMaxMediumPriorityIncidenceBus = TimeSpan.Parse(GetProperty("TimeMaxMediumPriorityIncidenceBus", "Setting"));
        }

        /// <summary>
        /// Carga la información leida del nodo Trunks del documento de configuración en XML.
        /// </summary>
        public static void ReadRoutesData()
        {
            Routes.Clear();

            foreach (XmlNode routeXmlNode in _xmlConfig.SelectSingleNode("Acabus").SelectSingleNode("Routes").SelectNodes("Route"))
            {
                var route = ToRoute(routeXmlNode) as Route;
                Session.Save(route);
                //Routes.Add(route);
                LoadStations(route as Trunk, routeXmlNode.SelectSingleNode("Stations")?.SelectNodes("Station"));
                LoadVehicles(route, routeXmlNode.SelectSingleNode("Vehicles")?.SelectNodes("Vehicle"));
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
        /// Permite guarda la información modificada en las listas de configuración.
        /// </summary>
        public static void SaveXml()
        {
            var data = XmlUtils.ToXml(Routes, "Routes");
            var root = _xmlConfig.SelectSingleNode("Acabus");
            var oldTrunks = root.SelectSingleNode("Routes");
            var newNode = data.SelectSingleNode("Routes");
            oldTrunks.InnerXml = newNode.InnerXml;
            File.Delete(CONFIG_FILENAME);
            File.WriteAllText(CONFIG_FILENAME, _xmlConfig.OuterXml);
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
                var device = new Device(default(UInt16),
                    (DeviceType)Enum.Parse(typeof(DeviceType), XmlUtils.GetAttribute(deviceXmlNode, "Type")),
                    station, XmlUtils.GetAttribute(deviceXmlNode, "NumeSeri", (DeviceType)Enum.Parse(typeof(DeviceType), XmlUtils.GetAttribute(deviceXmlNode, "Type")) + station?.Trunk?.RouteNumber.ToString("D2") + station?.StationNumber.ToString("D2") + XmlUtils.GetAttributeInt(deviceXmlNode, "ID").ToString("D2")))
                {
                    IP = XmlUtils.GetAttribute(deviceXmlNode, "IP", "0.0.0.0"),
                    Enabled = XmlUtils.GetAttributeBool(deviceXmlNode, "Enabled"),
                    HasDatabase = XmlUtils.GetAttributeBool(deviceXmlNode, "HasDataBase"),
                    SshEnabled = XmlUtils.GetAttributeBool(deviceXmlNode, "SshEnabled"),
                    CanReplicate = XmlUtils.GetAttributeBool(deviceXmlNode, "CanReplicate")
                };
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
        /// Convierte un nodo XML con la estructura correspondiente a un
        /// elemento de ruta a una instancia Route.
        /// </summary>
        /// <param name="routeNode">Nodo XML a parsear.</param>
        /// <returns>Una instancia de ruta troncal correspondiente al nodo
        /// pasado como argumento.</returns>
        public static Route ToRoute(XmlNode routeNode)
        {
            try
            {
                var id = UInt16.Parse(XmlUtils.GetAttribute(routeNode, "ID"));
                var routeNumber = XmlUtils.GetAttribute(routeNode, "RouteNumber");
                Route routeTemp = null;
                RouteType type = (RouteType)Enum.Parse(typeof(RouteType), XmlUtils.GetAttribute(routeNode, "Type"));
                switch (type)
                {
                    case RouteType.ALIM:
                        routeTemp = new Route(id, String.IsNullOrEmpty(routeNumber) ? id : UInt16.Parse(routeNumber));
                        break;

                    case RouteType.TRUNK:
                        routeTemp = Trunk.CreateTrunk(id, String.IsNullOrEmpty(routeNumber) ? id : UInt16.Parse(routeNumber));
                        break;

                    default:
                        break;
                }
                routeTemp.Name = XmlUtils.GetAttribute(routeNode, "Name");
                routeTemp.Section = XmlUtils.GetAttribute(routeNode, "Section");

                return routeTemp;
            }
            catch (Exception) { }
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
                var stationNumber = XmlUtils.GetAttribute(stationXmlNode, "StationNumber");
                var id = UInt16.Parse(XmlUtils.GetAttribute(stationXmlNode, "ID"));
                var station = Station.CreateStation(trunk, id, String.IsNullOrEmpty(stationNumber) ? id : UInt16.Parse(stationNumber));
                station.Name = XmlUtils.GetAttribute(stationXmlNode, "Name");
                station.IsConnected = XmlUtils.GetAttributeBool(stationXmlNode, "IsConnected");
                station.PingMin = (UInt16)XmlUtils.GetAttributeInt(stationXmlNode, "PingMin");
                station.PingMax = (UInt16)XmlUtils.GetAttributeInt(stationXmlNode, "PingMax");
                station.Section = XmlUtils.GetAttribute(stationXmlNode, "Section");

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
        /// Carga la configuración del XML en Trunk.Trunks.
        /// </summary>
        private static void InitAcabusData()
        {
            if (_loadedData) return;
            try
            {
                _xmlConfig = new XmlDocument();
                _xmlConfig.Load(CONFIG_FILENAME);

                LoadSettings();
                LoadTrunkSettings();
                LoadTechniciansSettings();
                LoadOffDutyVehiclesSettings();
                LoadCommonFaultsSettings();
                LoadCompaniesSettings();

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
        /// Carga las fallas comunes de la operación.
        /// </summary>
        private static void LoadCommonFaultsSettings()
        {
            CommonFaults.Clear();
            foreach (XmlNode faultXmlNode in _xmlConfig.SelectSingleNode("Acabus").SelectSingleNode("Faults")?.SelectNodes("Fault"))
                CommonFaults.Add(XmlUtils.GetAttribute(faultXmlNode, "Description"));
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
        /// Carga los dispositivos a partir de una lista de nodos XML.
        /// </summary>
        /// <param name="deviceNodes">Lista de nodos XML.</param>
        private static void LoadDevices(Station station, XmlNodeList deviceNodes)
        {
            if (deviceNodes != null)
                foreach (XmlNode deviceXmlNode in deviceNodes)
                {
                    var device = ToDevice(deviceXmlNode, station) as Device;
                    // Session.Save(device);
                    station.AddDevice(device);
                }
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
                    //Session.Save(station);
                    trunk.AddStation(station);
                    LoadDevices(station, stationXmlNode.SelectSingleNode("Devices").SelectNodes("Device"));
                }
        }

        /// <summary>
        /// Carga una lista de los técnicos.
        /// </summary>
        private static void LoadTechniciansSettings()
        {
            foreach (XmlNode technicianXmlNode in _xmlConfig.SelectSingleNode("Acabus").SelectSingleNode("Technicians")?.SelectNodes("Technician"))
                Technicians.Add(XmlUtils.GetAttribute(technicianXmlNode, "Name"));
        }

        private static void LoadTrunkSettings()
        {
            ReadRoutesData();
            ReadCCData();
            // ReadLinksData();
        }

        /// <summary>
        /// Carga los vehículos a partir de una lista de nodos
        /// XML.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="vehicleNode"></param>
        private static void LoadVehicles(Route route, XmlNodeList vehicleNode)
        {
            if (vehicleNode != null)
                foreach (XmlNode vehicleXmlNode in vehicleNode)
                {
                    var vehicle = ToVehicle(vehicleXmlNode, route) as Vehicle;
                    //Session.Save(vehicle);
                    route.AddVehicle(vehicle);
                }
        }

        /// <summary>
        /// Carga la información leida del nodo CC del documento de configuración en XML.
        /// </summary>
        private static void ReadCCData()
        {
            _cc = FindStation((station) => station.Name.Contains("CENTRO DE CONTROL"));
        }

        /// <summary>
        /// Carga la información leida del nodo Links del documento de configuración en XML.
        /// </summary>
        private static void ReadLinksData()
        {
            foreach (var xmlNode in _xmlConfig.SelectSingleNode("Acabus").SelectSingleNode("Links").SelectNodes("Link"))
            {
                var stationA = XmlUtils.GetAttribute(xmlNode as XmlNode, "StationA");
                var stationB = XmlUtils.GetAttribute(xmlNode as XmlNode, "StationB");
                var link = Link.CreateLink(
                    FindStation((station) => station.GetViaID().Equals(stationA)),
                    FindStation((station) => station.GetViaID().Equals(stationB))
                    );
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

        /// <summary>
        /// Convierte un nodo XML con una estructura correspondiente a un
        /// vehículo que pertenece a una ruta.
        /// </summary>
        /// <param name="vehicleXmlNode">Node XML que representa un vehículo.</param>
        /// <param name="route">Instancia de ruta al que está asignado.</param>
        /// <returns>Una instancia de vehículo.</returns>
        private static Vehicle ToVehicle(XmlNode vehicleXmlNode, Route route)
        {
            try
            {
                var vehicle = Vehicle.CreateVehicle(route,
                    XmlUtils.GetAttribute(vehicleXmlNode, "EconomicNumber"),
                    (VehicleType)Enum.Parse(typeof(VehicleType), XmlUtils.GetAttribute(vehicleXmlNode, "BusType")));

                vehicle.IP = XmlUtils.GetAttribute(vehicleXmlNode, "IP", "0.0.0.0");
                vehicle.Enabled = XmlUtils.GetAttributeBool(vehicleXmlNode, "Enabled");

                return vehicle;
            }
            catch { }
            return null;
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