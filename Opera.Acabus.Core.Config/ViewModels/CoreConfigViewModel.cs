using InnSyTech.Standard.Database;
using InnSyTech.Standard.Mvvm;
using InnSyTech.Standard.Utils;
using Opera.Acabus.Core.Config.Views;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opera.Acabus.Core.Config.ViewModels
{
    /// <summary>
    /// Define la estructura del modelo de la vista de <see cref="Views.CoreConfigView"/>.
    /// </summary>
    public class CoreConfigViewModel : ViewModelBase
    {
        /// <summary>
        /// Crea una instancia de <see cref="CoreConfigViewModel"/>.
        /// </summary>
        public CoreConfigViewModel()
        {
            DownloadDataCommand = new Command(parameter =>
            {
                Task.Run(() =>
                {
                    Dispatcher.SendNotify("DESCARGANDO RUTAS...");
                    if (DownloadRoutes())
                    {
                        OnPropertyChanged(nameof(Routes));
                        Dispatcher.SendNotify("RUTAS DESCARGAS CORRECTAMENTE, DESCAGANDO AUTOBUSES...");
                        if (DownloadBuses())
                        {
                            Dispatcher.SendNotify("AUTOBUSES DESCARGADOS Y REASIGNADOS A SUS RUTAS CORRECTAMENTE.");
                            OnPropertyChanged(nameof(Buses));
                        }
                        else
                            Dispatcher.SendNotify("ERROR: FALLA AL DESCARGAR LOS AUTOBUSES");
                        Dispatcher.SendNotify("DESCARGANDO ESTACIONES...");
                        if (DownloadStations())
                        {
                            OnPropertyChanged(nameof(Stations));
                            Dispatcher.SendNotify("ESTACIONES DESCARGADAS CORRECTAMENTE, DESCARGANDO EQUIPOS ASIGNADOS...");
                            if (DownloadDevices())
                            {
                                Dispatcher.SendNotify("EQUIPOS DESCARGADOS CORRECTAMENTE.");
                                OnPropertyChanged(nameof(Devices));
                            }
                            else
                                Dispatcher.SendNotify("ERROR: FALLA AL DESCARGAR LOS EQUIPOS");
                        }
                        else
                            Dispatcher.SendNotify("ERROR: FALLA AL DESCARGAR LAS ESTACIONES");
                    }
                    else
                        Dispatcher.SendNotify("ERROR: FALLA AL DESCARGAR LAS RUTAS");
                });
            });

            RefreshCommand = new Command(parameter =>
            {
                Task.Run(() =>
                {
                    OnPropertyChanged(nameof(Routes));
                    OnPropertyChanged(nameof(Buses));
                    OnPropertyChanged(nameof(Stations));
                    OnPropertyChanged(nameof(Devices));
                    Dispatcher.SendNotify("LISTA DE EQUIPOS, ESTACIONES, AUTOBUSES Y RUTAS ACTUALIZADAS.");
                });
            });

            BusReassingCommand = new Command(parameter =>
            {
                switch (parameter.ToString())
                {
                    case "0":
                        Task.Run(() =>
                        {
                            Dispatcher.SendNotify("DESCAGANDO AUTOBUSES Y REASIGNANDO...");
                            if (DownloadBuses())
                                Dispatcher.SendNotify("VEHÍCULOS AUTOBUSES Y REASIGNADOS.");
                            OnPropertyChanged(nameof(Routes));
                            OnPropertyChanged(nameof(Buses));
                        });
                        break;

                    case "1":
                        Dispatcher.RequestShowDialog(new ManualReassignRouteView(), RefreshCommand.Execute);
                        break;
                }
            });

            ShowAddDeviceCommand = new Command(parameter => Dispatcher.RequestShowDialog(new AddDeviceView(), RefreshCommand.Execute));

            Task.Run(() =>
            {
                OnPropertyChanged(nameof(Routes));
                OnPropertyChanged(nameof(Buses));
                OnPropertyChanged(nameof(Stations));
                OnPropertyChanged(nameof(Devices));
            });
        }

        /// <summary>
        /// Obtiene una lista de todos los autobuses.
        /// </summary>
        public IEnumerable<Bus> Buses => AcabusDataContext.AllBuses;

        /// <summary>
        /// Obtiene el comando para reasignar ruta a los autobuses.
        /// </summary>
        public ICommand BusReassingCommand { get; }

        /// <summary>
        /// Obtiene una lista de todos los dispositivos.
        /// </summary>
        public IEnumerable<Device> Devices => AcabusDataContext.AllDevices;

        /// <summary>
        /// Obtiene el comando para descargar la información del servidor.
        /// </summary>
        public ICommand DownloadDataCommand { get; }

        /// <summary>
        /// Obtiene el comando para actualizar la información desde la base de datos local.
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// Obtiene una lista de todas las rutas.
        /// </summary>
        public IEnumerable<Route> Routes => AcabusDataContext.AllRoutes;

        /// <summary>
        /// Obtiene el comando para mostrar el formulario para añadir equipos.
        /// </summary>
        public ICommand ShowAddDeviceCommand { get; }

        /// <summary>
        /// Obtiene una lista de el personal del área de TI.
        /// </summary>
        public IEnumerable<Staff> Staff => AcabusDataContext.AllStaff;

        /// <summary>
        /// Obtiene una lista de todas las estaciones.
        /// </summary>
        public IEnumerable<Station> Stations => AcabusDataContext.AllStations;

        /// <summary>
        /// Carga los valor al momento de mostrar el módulo de configuración.
        /// </summary>
        /// <param name="parameter">Parametros de carga.</param>
        protected override void OnLoad(object parameter)
        {
            OnPropertyChanged(nameof(Routes));
            OnPropertyChanged(nameof(Buses));
            OnPropertyChanged(nameof(Stations));
            OnPropertyChanged(nameof(Devices));
        }

        /// <summary>
        /// Descarga toda la información de los autobuses desde el servidor.
        /// </summary>
        /// <returns>Un valor <see cref="true"/> si la descarga se completó.</returns>
        private bool DownloadBuses()
        {
            try
            {
                var serverCC = AcabusDataContext.ConfigContext["serverCC"];

                if (serverCC == null) return false;

                string busQuery = serverCC?.GetSetting("downloadFromServer")?.ToString("buses");
                string connectionString = serverCC?.ToString("connectionString");

                Type dbConnectionType = TypeHelper.LoadFromDll("Libraries/Npgsql.dll", "Npgsql.NpgsqlConnection");

                if (string.IsNullOrEmpty(busQuery)) return false;
                if (string.IsNullOrEmpty(connectionString)) return false;
                if (dbConnectionType == null) return false;

                var session = DbFactory.CreateSession(dbConnectionType, new PsqlDialect(connectionString));

                var resultSet = session.Batch(busQuery);

                foreach (var row in resultSet)
                {
                    var id = default(ulong);
                    var type = default(BusType);
                    var economicNumber = null as string;
                    var idRouteAssigned = default(ulong);

                    try
                    {
                        id = UInt64.Parse(row["id"].ToString().Trim());
                        type = (BusType)UInt16.Parse(row["type"].ToString().Trim());
                        economicNumber = row["economicNumber"].ToString().Trim();
                        idRouteAssigned = UInt64.Parse(row["idRoute"].ToString().Trim());
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        Trace.WriteLine("La consulta para la descarga de los autobuses debe devolver los campos {id: ulong, type: ushort, economicNumber: string, idRoute: ulong}", "ERROR");
                        throw ex;
                    }

                    var routeAssigned = AcabusDataContext.AllRoutes.Where(r => r.ID == idRouteAssigned).FirstOrDefault();

                    Bus vehicle = new Bus(id, economicNumber)
                    {
                        Status = BusStatus.OPERATIONAL,
                        Type = type,
                        Route = routeAssigned
                    };

                    Bus vehicleReassign = null;

                    if ((vehicleReassign = Buses.FirstOrDefault(vehicleTemp => vehicleTemp.ID == vehicle.ID)) != null)
                    {
                        vehicleReassign.Route?.Buses.Remove(vehicle);

                        if (vehicle.Route != null && !vehicle.Route.Buses.Contains(vehicle))
                            vehicle.Route.Buses.Add(vehicle);

                        AcabusDataContext.DbContext.Update(vehicle);

                        continue;
                    }

                    routeAssigned.Buses.Add(vehicle);

                    AcabusDataContext.DbContext.Create(vehicle);
                }
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Descarga toda la información de los dispositivos desde el servidor.
        /// </summary>
        /// <returns>Un valor <see cref="true"/> si la descarga se completó.</returns>
        private bool DownloadDevices()
        {
            try
            {
                var serverCC = AcabusDataContext.ConfigContext["serverCC"];

                if (serverCC == null) return false;

                string deviceQuery = serverCC?.GetSetting("downloadFromServer")?.ToString("devices");
                string connectionString = serverCC?.ToString("connectionString");

                Type dbConnectionType = TypeHelper.LoadFromDll("Libraries/Npgsql.dll", "Npgsql.NpgsqlConnection");

                if (string.IsNullOrEmpty(deviceQuery)) return false;
                if (string.IsNullOrEmpty(connectionString)) return false;
                if (dbConnectionType == null) return false;

                var session = DbFactory.CreateSession(dbConnectionType, new PsqlDialect(connectionString));

                var resultSet = session.Batch(deviceQuery);

                foreach (var row in resultSet)
                {
                    var id = default(ulong);
                    var serialNumber = null as String;
                    var ipAddress = null as String;
                    var idStation = default(ulong);
                    var idBus = default(ulong);
                    var type = default(DeviceType);
                    var bus = null as Bus;
                    var station = null as Station;
                    try
                    {
                        id = UInt64.Parse(row["id"].ToString().Trim());
                        serialNumber = row["serial"].ToString().Trim();
                        ipAddress = row["ip"].ToString().Trim();
                        idStation = UInt64.Parse(row["idStation"].ToString().Trim());
                        idBus = UInt64.Parse(row["idBus"].ToString().Trim());
                        type = (DeviceType)Enum.Parse(typeof(DeviceType), row["deviceType"].ToString().Trim());

                        station = Stations.FirstOrDefault(stationFind => stationFind.StationNumber == idStation);
                        bus = Buses.FirstOrDefault(vehicleFind => vehicleFind.ID == idBus);
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        Trace.WriteLine("La consulta para la descarga de los equipos debe devolver los campos {id: ulong, serial: string, idStation: ulong, idBus: ulong, deviceType: string}", "ERROR");
                        throw ex;
                    }

                    Device device = new Device(id, serialNumber, type)
                    {
                        Bus = bus,
                        IPAddress = IPAddress.Parse(ipAddress),
                        Station = station
                    };

                    var deviceReassign = null as Device;

                    if ((deviceReassign = Devices.FirstOrDefault(deviceTemp => device.ID == deviceTemp.ID)) != null)
                    {
                        deviceReassign.Station?.Devices.Remove(device);
                        deviceReassign.Bus?.Devices.Remove(device);

                        if (bus != null && !bus.Devices.Contains(device))
                            bus.Devices.Add(device);

                        if (station != null && !station.Devices.Contains(device))
                            station.Devices.Add(device);

                        AcabusDataContext.DbContext.Update(device);

                        continue;
                    }

                    station?.Devices.Add(device);
                    bus?.Devices.Add(device);

                    AcabusDataContext.DbContext.Create(device);
                }
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Descarga toda la información de las rutas desde el servidor.
        /// </summary>
        /// <returns>Un valor <see cref="true"/> si la descarga se completó.</returns>
        private bool DownloadRoutes()
        {
            try
            {
                var serverCC = AcabusDataContext.ConfigContext["serverCC"];

                if (serverCC == null) return false;

                string routeQuery = serverCC?.GetSetting("downloadFromServer")?.ToString("routes");
                string connectionString = serverCC?.ToString("connectionString");

                Type dbConnectionType = TypeHelper.LoadFromDll("Libraries/Npgsql.dll", "Npgsql.NpgsqlConnection");

                if (string.IsNullOrEmpty(routeQuery)) return false;
                if (string.IsNullOrEmpty(connectionString)) return false;
                if (dbConnectionType == null) return false;

                var session = DbFactory.CreateSession(dbConnectionType, new PsqlDialect(connectionString));

                var resultSet = session.Batch(routeQuery);

                foreach (var row in resultSet)
                {
                    var id = UInt16.Parse(row["id"].ToString());
                    var number = UInt16.Parse(row["number"].ToString());
                    var type = (RouteType)Enum.Parse(typeof(RouteType), row["type"].ToString(), true);

                    Route route = new Route(id, number, type)
                    {
                        Name = row["name"].ToString().Trim().ToUpper()
                    };

                    if (Routes.Contains(route)) continue;

                    AcabusDataContext.DbContext.Create(route);
                }
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Descarga toda la información de las estaciones desde el servidor.
        /// </summary>
        /// <returns>Un valor <see cref="true"/> si la descarga se completó.</returns>
        private bool DownloadStations()
        {
            try
            {
                var serverCC = AcabusDataContext.ConfigContext["serverCC"];

                if (serverCC == null) return false;

                string stationQuery = serverCC?.GetSetting("downloadFromServer")?.ToString("stations");
                string connectionString = serverCC?.ToString("connectionString");

                Type dbConnectionType = TypeHelper.LoadFromDll("Libraries/Npgsql.dll", "Npgsql.NpgsqlConnection");

                if (string.IsNullOrEmpty(stationQuery)) return false;
                if (string.IsNullOrEmpty(connectionString)) return false;
                if (dbConnectionType == null) return false;

                var session = DbFactory.CreateSession(dbConnectionType, new PsqlDialect(connectionString));

                var resultSet = session.Batch(stationQuery);

                foreach (var row in resultSet)
                {
                    var id = default(ulong);
                    var name = null as String;
                    var stationNumber = default(ushort);
                    var idRoute = default(ulong);
                    var routeAssigned = null as Route;

                    try
                    {
                        id = UInt64.Parse(row["id"].ToString().Trim());
                        name = row["name"].ToString().Trim();
                        stationNumber = UInt16.Parse(row["number"].ToString().Trim());
                        idRoute = UInt64.Parse(row["idRoute"].ToString().Trim());
                        routeAssigned = AcabusDataContext.AllRoutes.FirstOrDefault(route => route.ID == idRoute);
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        Trace.WriteLine("La consulta para la descarga de las estaciones debe devolver los campos {id: ulong, name: string, number: ushort, idRoute: ulong}", "ERROR");
                        throw ex;
                    }

                    Station station = new Station(id, stationNumber)
                    {
                        Name = name,
                        Route = routeAssigned
                    };

                    var stationReassign = null as Station;

                    if ((stationReassign = Stations.FirstOrDefault(statioTemp => statioTemp.ID == station.ID)) != null)
                    {
                        stationReassign.Route?.Stations.Remove(station);

                        if (station.Route != null && !station.Route.Stations.Contains(station))
                            station.Route.Stations.Add(station);

                        AcabusDataContext.DbContext.Update(station);

                        continue;
                    }

                    routeAssigned.Stations.Add(station);

                    AcabusDataContext.DbContext.Create(station);
                }
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Dialecto utilizado para la comunicación de solo lectura de los equipos conectados.
        /// </summary>
        internal class PsqlDialect : DbDialectBase
        {
            /// <summary>
            /// Crea una instancia nueva de <see cref="PsqlDialect"/>.
            /// </summary>
            /// <param name="connection">Cadena de conexión a la base de datos de los equipos.</param>
            public PsqlDialect(String connection) : base(connection) { }

            /// <summary>
            /// Obtiene el convertidor de fecha de la base de datos.
            /// </summary>
            public override IDbConverter DateTimeConverter => null;

            /// <summary>
            /// Obtiene la función que permite obtener el ultimo insertado.
            /// </summary>
            public override string LastInsertFunctionName => "";

            /// <summary>
            /// Obtiene el número de transacciones por conexión permitidas.
            /// </summary>
            public override int TransactionPerConnection => 10;
        }
    }
}