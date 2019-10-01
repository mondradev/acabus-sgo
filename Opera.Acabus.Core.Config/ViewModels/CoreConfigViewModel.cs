using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Linq;
using InnSyTech.Standard.Mvvm;
using InnSyTech.Standard.Utils;
using Opera.Acabus.Core.Config.Views;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Models;
using Opera.Acabus.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
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
                        Dispatcher.SendNotify("RUTAS DESCARGAS CORRECTAMENTE, DESCAGANDO AUTOBUSES...");
                        if (DownloadBuses())
                            Dispatcher.SendNotify("AUTOBUSES DESCARGADOS Y REASIGNADOS A SUS RUTAS CORRECTAMENTE.");
                        else
                            Dispatcher.SendNotify("ERROR: FALLA AL DESCARGAR LOS AUTOBUSES");
                        Dispatcher.SendNotify("DESCARGANDO ESTACIONES...");
                        if (DownloadStations())
                        {
                            Dispatcher.SendNotify("ESTACIONES DESCARGADAS CORRECTAMENTE, DESCARGANDO EQUIPOS ASIGNADOS...");
                            if (DownloadDevices())
                                Dispatcher.SendNotify("EQUIPOS DESCARGADOS CORRECTAMENTE.");
                            else
                                Dispatcher.SendNotify("ERROR: FALLA AL DESCARGAR LOS EQUIPOS");
                        }
                        else
                            Dispatcher.SendNotify("ERROR: FALLA AL DESCARGAR LAS ESTACIONES");
                    }
                    else
                        Dispatcher.SendNotify("ERROR: FALLA AL DESCARGAR LAS RUTAS");

                    ReadFromDataBase();
                });
            });

            RefreshCommand = new Command(parameter =>
            {
                Task.Run(() =>
                {
                    ReadFromDataBase();
                    Dispatcher.SendNotify("LISTA DE EQUIPOS, ESTACIONES, AUTOBUSES Y RUTAS ACTUALIZADAS.");
                });
            });

            BusReassingCommand = new Command(parameter => Dispatcher.RequestShowDialog(new ManualReassignRouteView(), p => RefreshBus()));

            ShowAddDeviceCommand = new Command(parameter => Dispatcher.RequestShowDialog(new AddDeviceView(), p => RefreshDevice()));

            ShowAddStationCommand = new Command(parameter => Dispatcher.RequestShowDialog(new AddStationView(), p => RefreshStation()));

            ShowAddRouteCommand = new Command(parameter => Dispatcher.RequestShowDialog(new AddRouteView(), p => RefreshRoutes()));

            ShowAddBusCommand = new Command(parameter => Dispatcher.RequestShowDialog(new AddBusView(), p => RefreshBus()));

            ShowAddStaffCommand = new Command(parameter => Dispatcher.RequestShowDialog(new AddStaffView(), p => RefreshStaff()));
        }

        /// <summary>
        /// Obtiene una lista de todos los autobuses
        /// </summary>
        public ICollection<Bus> AllBuses { get; private set; }

        /// <summary>
        /// Obtiene una lista de todos los equipos.
        /// </summary>
        public ICollection<Device> AllDevices { get; private set; }

        /// <summary>
        /// Obtiene una lista de todas las rutas.
        /// </summary>
        public ICollection<Route> AllRoutes { get; private set; }

        /// <summary>
        /// Obtiene una lista de todo el personal empleado.
        /// </summary>
        public ICollection<Staff> AllStaff { get; private set; }

        /// <summary>
        /// Obtiene una lista de todas las estaciones.
        /// </summary>
        public ICollection<Station> AllStations { get; private set; }

        /// <summary>
        /// Obtiene el comando para reasignar ruta a los autobuses.
        /// </summary>
        public ICommand BusReassingCommand { get; }

        /// <summary>
        /// Obtiene el comando para descargar la información del servidor.
        /// </summary>
        public ICommand DownloadDataCommand { get; }

        /// <summary>
        /// Obtiene si los controles están activos. Estos se deshabilitan cuando está cargando la base de datos.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Obtiene el comando para actualizar la información desde la base de datos local.
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// Obtiene el comando para mostrar el formulario para añadir autobuses.
        /// </summary>
        public ICommand ShowAddBusCommand { get; }

        /// <summary>
        /// Obtiene el comando para mostrar el formulario para añadir equipos.
        /// </summary>
        public ICommand ShowAddDeviceCommand { get; }

        /// <summary>
        /// Obtiene el comando para mostrar el formulario para añadir rutas.
        /// </summary>
        public ICommand ShowAddRouteCommand { get; }

        /// <summary>
        /// Obtiene el comando para mostrar el formulario para añadir personal.
        /// </summary>
        public ICommand ShowAddStaffCommand { get; }

        /// <summary>
        /// Obtiene el comando para mostrar el formulario para añadir estaciones.
        /// </summary>
        public ICommand ShowAddStationCommand { get; }

        /// <summary>
        /// Carga los valores al momento de mostrar el módulo de configuración.
        /// </summary>
        /// <param name="parameter">Parametros de carga.</param>
        protected override void OnLoad(object parameter)
        {
            RegisterHandlers(ServerContext.GetLocalSync("Bus"), RefreshBus);
            RegisterHandlers(ServerContext.GetLocalSync("Station"), RefreshStation);
            RegisterHandlers(ServerContext.GetLocalSync("Device"), RefreshDevice);
            RegisterHandlers(ServerContext.GetLocalSync("Route"), RefreshRoutes);
            RegisterHandlers(ServerContext.GetLocalSync("Staff"), RefreshStaff);

            Task.Run(ReadFromDataBase);
        }

        /// <summary>
        /// Descarga los valores del módulo al momento de ocultar la vista.
        /// </summary>
        /// <param name="parameter">Parametros de descarga</param>
        protected override void OnUnload(object parameter)
        {
            IsActive = false;
            AllDevices = null;
            AllBuses = null;
            AllStations = null;
            AllRoutes = null;
            AllStaff = null;

            OnPropertyChanged(nameof(IsActive));
            OnPropertyChanged(nameof(AllBuses));
            OnPropertyChanged(nameof(AllDevices));
            OnPropertyChanged(nameof(AllRoutes));
            OnPropertyChanged(nameof(AllStaff));
            OnPropertyChanged(nameof(AllStations));

            UnRegisterHandlers(ServerContext.GetLocalSync("Bus"), RefreshBus);
            UnRegisterHandlers(ServerContext.GetLocalSync("Station"), RefreshStation);
            UnRegisterHandlers(ServerContext.GetLocalSync("Device"), RefreshDevice);
            UnRegisterHandlers(ServerContext.GetLocalSync("Route"), RefreshRoutes);
            UnRegisterHandlers(ServerContext.GetLocalSync("Staff"), RefreshStaff);
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

                string busQuery = serverCC?.GetSetting("downloadFromServer")?.GetSetting("buses").ToString("value");
                string connectionString = serverCC?.ToString("connectionString");

                Type dbConnectionType = TypeHelper.LoadFromDll("Libraries/Npgsql.dll", "Npgsql.NpgsqlConnection");

                if (string.IsNullOrEmpty(busQuery)) return false;
                if (string.IsNullOrEmpty(connectionString)) return false;
                if (dbConnectionType == null) return false;

                var session = DbFactory.CreateSession(dbConnectionType, new PsqlDialectAux(connectionString));

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
                        economicNumber = row["economicnumber"].ToString().Trim();
                        idRouteAssigned = UInt64.Parse(row["idroute"].ToString().Trim());
                    }
                    catch (Exception ex)
                    {
                        if (ex is KeyNotFoundException || ex is FormatException)
                            Trace.WriteLine("La consulta para la descarga de los autobuses debe devolver los campos {id: ulong, type: ushort, economicnumber: string, idroute: ulong}", "ERROR");
                        throw ex;
                    }

                    var routeAssigned = AllRoutes.Where(r => r.ID == idRouteAssigned).FirstOrDefault();

                    Bus vehicle = new Bus(id, economicNumber)
                    {
                        Status = BusStatus.OPERATIONAL,
                        Type = type,
                        Route = routeAssigned
                    };

                    Bus vehicleReassign = null;

                    if ((vehicleReassign = AllBuses.FirstOrDefault(vehicleTemp => vehicleTemp.ID == vehicle.ID)) != null)
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
            catch (Exception ex) { Trace.WriteLine(ex.PrintMessage().JoinLines()); return false; }
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

                string deviceQuery = serverCC?.GetSetting("downloadFromServer")?.GetSetting("devices").ToString("value");
                string connectionString = serverCC?.ToString("connectionString");

                Type dbConnectionType = TypeHelper.LoadFromDll("Libraries/Npgsql.dll", "Npgsql.NpgsqlConnection");

                if (string.IsNullOrEmpty(deviceQuery)) return false;
                if (string.IsNullOrEmpty(connectionString)) return false;
                if (dbConnectionType == null) return false;

                var session = DbFactory.CreateSession(dbConnectionType, new PsqlDialectAux(connectionString));

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
                        idStation = UInt64.Parse(row["idstation"]?.ToString().Trim() ?? "0");
                        idBus = UInt64.Parse(row["idbus"]?.ToString().Trim() ?? "0");
                        type = (DeviceType)Enum.Parse(typeof(DeviceType), row["type"].ToString().Trim());

                        station = AllStations.FirstOrDefault(stationFind => stationFind.ID == idStation);
                        bus = AllBuses.FirstOrDefault(vehicleFind => vehicleFind.ID == idBus);
                    }
                    catch (Exception ex)
                    {
                        if (ex is KeyNotFoundException || ex is FormatException)
                            Trace.WriteLine("La consulta para la descarga de los equipos debe devolver los campos {id: ulong, serial: string, idstation: ulong, idbus: ulong, type: string}", "ERROR");
                        throw ex;
                    }

                    Device device = new Device(id, serialNumber, type)
                    {
                        Bus = bus,
                        IPAddress = IPAddress.Parse(ipAddress),
                        Station = station
                    };

                    var deviceReassign = null as Device;

                    if ((deviceReassign = AllDevices.FirstOrDefault(deviceTemp => device.ID == deviceTemp.ID)) != null)
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
            catch (Exception ex) { Trace.WriteLine(ex.PrintMessage().JoinLines()); return false; }
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

                string routeQuery = serverCC?.GetSetting("downloadFromServer")?.GetSetting("routes").ToString("value");
                string connectionString = serverCC?.ToString("connectionString");

                Type dbConnectionType = TypeHelper.LoadFromDll("Libraries/Npgsql.dll", "Npgsql.NpgsqlConnection");

                if (string.IsNullOrEmpty(routeQuery)) return false;
                if (string.IsNullOrEmpty(connectionString)) return false;
                if (dbConnectionType == null) return false;

                var session = DbFactory.CreateSession(dbConnectionType, new PsqlDialectAux(connectionString));

                var resultSet = session.Batch(routeQuery);

                foreach (var row in resultSet)
                {
                    var id = default(ulong);
                    var number = default(ushort);
                    var type = default(RouteType);
                    var name = null as String;

                    try
                    {
                        id = UInt16.Parse(row["id"].ToString());
                        number = UInt16.Parse(row["number"].ToString());
                        type = (RouteType)Enum.Parse(typeof(RouteType), row["type"].ToString(), true);
                        name = row["name"].ToString().Trim().ToUpper();
                    }
                    catch (Exception ex)
                    {
                        if (ex is KeyNotFoundException || ex is FormatException)
                            Trace.WriteLine("La consulta para la descarga de los equipos debe devolver los campos {id: ulong, name: string, number: ushort, type: string}", "ERROR");
                        throw ex;
                    }
                    Route route = new Route(id, number, type)
                    {
                        Name = name
                    };

                    if (AllRoutes.Contains(route)) continue;

                    AcabusDataContext.DbContext.Create(route);
                }
                return true;
            }
            catch (Exception ex) { Trace.WriteLine(ex.PrintMessage().JoinLines()); return false; }
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

                string stationQuery = serverCC?.GetSetting("downloadFromServer")?.GetSetting("stations").ToString("value");
                string connectionString = serverCC?.ToString("connectionString");

                Type dbConnectionType = TypeHelper.LoadFromDll("Libraries/Npgsql.dll", "Npgsql.NpgsqlConnection");

                if (string.IsNullOrEmpty(stationQuery)) return false;
                if (string.IsNullOrEmpty(connectionString)) return false;
                if (dbConnectionType == null) return false;

                var session = DbFactory.CreateSession(dbConnectionType, new PsqlDialectAux(connectionString));

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
                        idRoute = UInt64.Parse(row["idroute"].ToString().Trim());
                        routeAssigned = AllRoutes.FirstOrDefault(route => route.ID == idRoute);
                    }
                    catch (Exception ex)
                    {
                        if (ex is KeyNotFoundException || ex is FormatException)
                            Trace.WriteLine("La consulta para la descarga de las estaciones debe devolver los campos {id: ulong, name: string, number: ushort, idroute: ulong}", "ERROR");
                        throw ex;
                    }

                    Station station = new Station(id, stationNumber)
                    {
                        Name = name,
                        Route = routeAssigned
                    };

                    var stationReassign = null as Station;

                    if ((stationReassign = AllStations.FirstOrDefault(statioTemp => statioTemp.ID == station.ID)) != null)
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
            catch (Exception ex) { Trace.WriteLine(ex.PrintMessage().JoinLines()); return false; }
        }

        /// <summary>
        /// Lee toda la información desde la base de datos.
        /// </summary>
        private void ReadFromDataBase()
        {
            /// Lectura desde la base de datos.

            AllDevices = AcabusDataContext.AllDevices.LoadReference(1).Where(d => d.Active).ToList();
            AllBuses = AcabusDataContext.AllBuses.LoadReference(1).Where(b => b.Active).ToList();
            AllStations = AcabusDataContext.AllStations.LoadReference(1).Where(s => s.Active).ToList();
            AllRoutes = AcabusDataContext.AllRoutes.LoadReference(1).Where(r => r.Active).ToList();
            AllStaff = AcabusDataContext.AllStaff.Where(s => s.Active == true && s.Name != "SISTEMA").ToList();

            foreach (var s in AllStations)
                AcabusDataContext.DbContext.LoadRefences(s, nameof(Station.Devices), AllDevices);

            foreach (var b in AllBuses)
                AcabusDataContext.DbContext.LoadRefences(b, nameof(Bus.Devices), AllDevices);

            foreach (var r in AllRoutes)
            {
                AcabusDataContext.DbContext.LoadRefences(r, nameof(Route.Buses), AllBuses);
                AcabusDataContext.DbContext.LoadRefences(r, nameof(Route.Stations), AllStations);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                IsActive = true;
                OnPropertyChanged(nameof(IsActive));
                OnPropertyChanged(nameof(AllBuses));
                OnPropertyChanged(nameof(AllDevices));
                OnPropertyChanged(nameof(AllRoutes));
                OnPropertyChanged(nameof(AllStaff));
                OnPropertyChanged(nameof(AllStations));
            });
        }

        /// <summary>
        /// Actualiza la tabla de autobuses.
        /// </summary>
        private void RefreshBus(object sender = null, LocalSyncArgs args = null)
        {
            IsActive = false;
            Application.Current.Dispatcher.Invoke(() => { OnPropertyChanged(nameof(IsActive)); });

            AllBuses = AcabusDataContext.AllBuses.LoadReference(1).Where(x => x.Active).ToList();

            if (AllDevices != null)
                foreach (var b in AllBuses)
                    AcabusDataContext.DbContext.LoadRefences(b, nameof(Bus.Devices), AllDevices);

            if (AllRoutes != null)
                foreach (var r in AllRoutes)
                    AcabusDataContext.DbContext.LoadRefences(r, nameof(Route.Buses), AllBuses);

            Application.Current.Dispatcher.Invoke(() =>
            {
                IsActive = true;
                OnPropertyChanged(nameof(IsActive));
                OnPropertyChanged(nameof(AllBuses));
                OnPropertyChanged(nameof(AllRoutes));
            });
        }

        /// <summary>
        /// Actualiza la tabla de equipos.
        /// </summary>
        private void RefreshDevice(object sender = null, LocalSyncArgs args = null)
        {
            IsActive = false;
            Application.Current.Dispatcher.Invoke(() => { OnPropertyChanged(nameof(IsActive)); });

            AllDevices = AcabusDataContext.AllDevices.LoadReference(1).Where(x => x.Active).ToList();

            if (AllStations != null)
                foreach (var s in AllStations)
                    AcabusDataContext.DbContext.LoadRefences(s, nameof(Station.Devices), AllDevices);

            if (AllBuses != null)
                foreach (var b in AllBuses)
                    AcabusDataContext.DbContext.LoadRefences(b, nameof(Bus.Devices), AllDevices);

            IsActive = true;

            Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(IsActive));
                OnPropertyChanged(nameof(AllDevices));
                OnPropertyChanged(nameof(AllStations));
                OnPropertyChanged(nameof(AllBuses));
            });
        }

        /// <summary>
        /// Actualiza la tabla de rutas.
        /// </summary>
        private void RefreshRoutes(object sender = null, LocalSyncArgs args = null)
        {
            IsActive = false;
            Application.Current.Dispatcher.Invoke(() => { OnPropertyChanged(nameof(IsActive)); });

            AllRoutes = AcabusDataContext.AllRoutes.LoadReference(1).Where(x => x.Active).ToList();

            if (AllStations != null && AllBuses != null)
                foreach (var r in AllRoutes)
                {
                    AcabusDataContext.DbContext.LoadRefences(r, nameof(Route.Stations), AllStations);
                    AcabusDataContext.DbContext.LoadRefences(r, nameof(Route.Buses), AllBuses);
                }

            IsActive = true;

            Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(IsActive));
                OnPropertyChanged(nameof(AllBuses));
                OnPropertyChanged(nameof(AllStations));
                OnPropertyChanged(nameof(AllRoutes));
            });
        }

        /// <summary>
        /// Actualiza la tabla de personal.
        /// </summary>
        private void RefreshStaff(object sender = null, LocalSyncArgs args = null)
        {
            IsActive = false;
            Application.Current.Dispatcher.Invoke(() => { OnPropertyChanged(nameof(IsActive)); });

            AllStaff = AcabusDataContext.AllStaff.Where(s => s.Active == true && s.Name != "SISTEMA").ToList();

            IsActive = true;

            Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(IsActive));
                OnPropertyChanged(nameof(AllStaff));
            });
        }

        /// <summary>
        /// Actualiza la tabla de estaciones.
        /// </summary>
        private void RefreshStation(object sender = null, LocalSyncArgs args = null)
        {
            IsActive = false;
            Application.Current.Dispatcher.Invoke(() => { OnPropertyChanged(nameof(IsActive)); });

            AllStations = AcabusDataContext.AllStations.LoadReference(1).Where(x => x.Active).ToList();

            if (AllDevices != null)
                foreach (var s in AllStations)
                    AcabusDataContext.DbContext.LoadRefences(s, nameof(Station.Devices), AllDevices);

            if (AllRoutes != null)
                foreach (var r in AllRoutes)
                {
                    AcabusDataContext.DbContext.LoadRefences(r, nameof(Route.Stations), AllStations);
                }

            IsActive = true;

            Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(IsActive));
                OnPropertyChanged(nameof(AllStations));
                OnPropertyChanged(nameof(AllRoutes));
            });
        }

        /// <summary>
        /// Registra todos los eventos de los diferentes monitores de sincronización.
        /// </summary>
        /// <param name="entityLocalSync">Monitor de sincronización de entidad.</param>
        /// <param name="handler">Método que se ejecuta al desencadenarse algún evento.</param>
        private void RegisterHandlers(IEntityLocalSync entityLocalSync, LocalSyncHandler handler)
        {
            entityLocalSync.Created += handler;
            entityLocalSync.Updated += handler;
            entityLocalSync.Deleted += handler;
        }

        /// <summary>
        /// Remueve el registro de todos los eventos de los diferentes monitores de sincronización.
        /// </summary>
        /// <param name="entityLocalSync">Monitor de sincronización de entidad.</param>
        /// <param name="handler">Método que se ejecuta al desencadenarse algún evento.</param>
        private void UnRegisterHandlers(IEntityLocalSync entityLocalSync, LocalSyncHandler handler)
        {
            entityLocalSync.Created -= handler;
            entityLocalSync.Updated -= handler;
            entityLocalSync.Deleted -= handler;
        }

        /// <summary>
        /// Dialecto utilizado para la comunicación de solo lectura de los equipos conectados.
        /// </summary>
        private class PsqlDialectAux : DbDialectBase
        {
            /// <summary>
            /// Crea una instancia nueva de <see cref="PsqlDialectAux"/>.
            /// </summary>
            /// <param name="connection">Cadena de conexión a la base de datos de los equipos.</param>
            public PsqlDialectAux(String connection) : base(connection) { }

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
            public override int TransactionPerConnection => 1;
        }
    }
}