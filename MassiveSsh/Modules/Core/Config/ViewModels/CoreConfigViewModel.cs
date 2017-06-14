using Acabus.Models;
using Acabus.Utils.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using Acabus.DataAccess;
using System.Linq;
using System.Windows.Input;
using Acabus.Window;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Acabus.Modules.Core.Config.ViewModels
{
    public class CoreConfigViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Devices'.
        /// </summary>
        private ICollection<Device> _devices;

        /// <summary>
        /// Obtiene una lista de todos los dispositivos.
        /// </summary>
        public ICollection<Device> Devices {
            get {
                if (_devices == null)
                    _devices = new ObservableCollection<Device>();
                return _devices;
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'Stations'.
        /// </summary>
        private ICollection<Station> _stations;

        /// <summary>
        /// Obtiene una lista de todas las estaciones.
        /// </summary>
        public ICollection<Station> Stations {
            get {
                if (_stations == null)
                    _stations = new ObservableCollection<Station>();
                return _stations;
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'Vehicles'.
        /// </summary>
        private ICollection<Vehicle> _vehicles;

        /// <summary>
        /// Obtiene una lista de todos los autobuses.
        /// </summary>
        public ICollection<Vehicle> Vehicles {
            get {
                if (_vehicles == null)
                    _vehicles = new ObservableCollection<Vehicle>();
                return _vehicles;
            }
        }

        public ICommand DownloadDataCommand { get; }

        /// <summary>
        ///  
        /// </summary>
        public CoreConfigViewModel()
        {
            DownloadDataCommand = new CommandBase(parameter =>
            {
                Task.Run(() =>
                {
                    AcabusControlCenterViewModel.AddNotify("DESCARGANDO RUTAS...");
                    if (DownloadRoutes())
                    {
                        AcabusControlCenterViewModel.AddNotify("RUTAS DESCARGAS CORRECTAMENTE, DESCAGANDO VEHÍCULOS...");
                        if (DownloadVehicles())
                            AcabusControlCenterViewModel.AddNotify("VEHÍCULOS DESCARGADOS Y REASIGNADOS A SUS RUTAS CORRECTAMENTE.");
                        AcabusControlCenterViewModel.AddNotify("DESCARGANDO ESTACIONES...");
                        if (DownloadStations())
                        {
                            AcabusControlCenterViewModel.AddNotify("ESTACIONES DESCARGADAS CORRECTAMENTE, DESCARGANDO EQUIPOS ASIGNADOS...");
                            if (DownloadDevices())
                                AcabusControlCenterViewModel.AddNotify("EQUIPOS DESCARGADOS CORRECTAMENTE.");

                        }
                    }

                });
            });

            RefreshCommand = new CommandBase(parameter =>
            {
                Task.Run(() =>
                {
                    RefreshData();
                    AcabusControlCenterViewModel.AddNotify("LISTA DE EQUIPOS, ESTACIONES, VEHÍCULOS Y RUTAS ACTUALIZADAS.");
                });
            });

            BusReassingCommand = new CommandBase(parameter =>
            {
                Task.Run(() =>
                {
                    AcabusControlCenterViewModel.AddNotify("DESCAGANDO VEHÍCULOS Y REASIGNANDO...");
                    if (DownloadVehicles())
                        AcabusControlCenterViewModel.AddNotify("VEHÍCULOS DESCARGADOS Y REASIGNADOS.");
                });
              });

            Task.Run(() => RefreshData());
        }

        public ICommand BusReassingCommand { get; }

        private bool DownloadDevices()
        {
            try
            {
                var resultSet = AcabusData.ExecuteQueryInServerDB(AcabusData.DevicesQuery);
                var headerRead = false;
                foreach (var row in resultSet)
                {
                    if (!headerRead) { headerRead = true; continue; }


                    var id = UInt64.Parse(row[0].Trim());
                    var serialNumber = row[1].Trim();
                    var ipAddress = row[2].Trim();
                    var station = !String.IsNullOrEmpty(row[3].Trim())
                        ? Stations.FirstOrDefault(stationFind => stationFind.StationNumber == UInt16.Parse(row[3].Trim()))
                        : null;
                    var vehicle = Vehicles.FirstOrDefault(vehicleFind => vehicleFind.EconomicNumber == row[4].Trim());
                    var type = (DeviceType)Enum.Parse(typeof(DeviceType), row[5].Trim());
                    var enabled = row[6].Trim().Equals("1") ? true : false;
                    var sshEnabled = row[7].Trim().Equals("1") ? true : false;
                    var hasDatabase = row[8].Trim().Equals("1") ? true : false;
                    var canReplica = row[9].Trim().Equals("1") ? true : false;

                    Device device = new Device(id, type, station, serialNumber)
                    {
                        CanReplicate = canReplica,
                        Enabled = enabled,
                        HasDatabase = hasDatabase,
                        IP = ipAddress,
                        SshEnabled = sshEnabled,
                        Vehicle = vehicle
                    };

                    if (Devices.Contains(device))
                    {
                        var deviceReassign = Devices.FirstOrDefault(deviceTemp => device.ID == deviceTemp.ID);
                        deviceReassign.Station = station;
                        deviceReassign.Vehicle = vehicle;
                        AcabusData.Session.Update(device);
                        continue;
                    }
                    App.Current.Dispatcher.Invoke(() => Devices.Add(device));
                    AcabusData.Session.Save(device);
                }
                return true;
            }
            catch { return false; }
        }


        private bool DownloadStations()
        {
            try
            {
                var resultSet = AcabusData.ExecuteQueryInServerDB(AcabusData.StationsQuery);
                var headerRead = false;
                foreach (var row in resultSet)
                {
                    if (!headerRead) { headerRead = true; continue; }

                    var id = UInt16.Parse(row[0].Trim());
                    var name = row[1].Trim();
                    var isConnected = UInt16.Parse(row[2].Trim()) == 1 ? true : false;
                    var minPing = UInt16.Parse(row[3].Trim());
                    var maxPing = UInt16.Parse(row[4].Trim());
                    var stationNumber = UInt16.Parse(row[5].Trim());
                    var routeAsigned = Routes.FindRoute(route => route.ID == UInt16.Parse(row[6].Trim()));

                    Station station = new Station(routeAsigned, id, stationNumber)
                    {
                        IsConnected = isConnected,
                        Name = name,
                        PingMin = minPing,
                        PingMax = maxPing
                    };

                    if (Stations.Contains(station))
                    {
                        var stationReassign = Stations.FirstOrDefault(stationTemp => stationTemp.ID == station.ID);
                        stationReassign.Route = station.Route;
                        AcabusData.Session.Update(stationReassign);
                        continue;
                    }
                    App.Current.Dispatcher.Invoke(() => Stations.Add(station));
                    AcabusData.Session.Save(station);
                }
                return true;
            }
            catch { return false; }
        }


        private bool DownloadVehicles()
        {
            try
            {
                var resultSet = AcabusData.ExecuteQueryInServerDB(AcabusData.VehiclesQuery);
                var headerRead = false;
                foreach (var row in resultSet)
                {
                    if (!headerRead) { headerRead = true; continue; }

                    var id = UInt16.Parse(row[0].Trim());
                    var type = (VehicleType)UInt16.Parse(row[1].Trim());
                    var economicNumber = row[2].Trim();
                    var enabled = UInt16.Parse(row[3].Trim()) == 1 ? true : false;
                    var ipAddress = row[4].Trim();
                    var routeAsigned = Routes.FindRoute(route => route.ID == UInt16.Parse(row[5].Trim()));

                    Vehicle vehicle = new Vehicle(id, economicNumber, type)
                    {
                        Enabled = enabled,
                        Route = routeAsigned
                    };

                    if (Vehicles.Contains(vehicle))
                    {
                        var vehicleReassign = Vehicles.FirstOrDefault(vehicleTemp => vehicleTemp.ID == vehicle.ID);
                        vehicleReassign.Route = vehicle.Route;
                        AcabusData.Session.Update(vehicleReassign);
                        continue;
                    }
                    App.Current.Dispatcher.Invoke(() => Vehicles.Add(vehicle));
                    AcabusData.Session.Save(vehicle);
                }
                return true;
            }
            catch { return false; }
        }

        private bool DownloadRoutes()
        {
            try
            {
                var resultSet = AcabusData.ExecuteQueryInServerDB(AcabusData.RoutesQuery);
                var headerRead = false;
                foreach (var row in resultSet)
                {
                    if (!headerRead) { headerRead = true; continue; }

                    var id = UInt16.Parse(row[0]);
                    var number = UInt16.Parse(row[1]);
                    var type = (RouteType)Enum.Parse(typeof(RouteType), row[3], true);
                    Route route = new Route(id, number, type)
                    {
                        Name = row[2]?.Trim().ToUpper()
                    };

                    if (Routes.Contains(route)) continue;

                    App.Current.Dispatcher.Invoke(() => Routes.Add(route));
                    AcabusData.Session.Save(route);
                }
                return true;
            }
            catch { return false; }
        }

        private void RefreshData()
        {
            App.Current.Dispatcher.Invoke(() => Devices.Clear());
            foreach (var device in AcabusData.Session.GetObjects(typeof(Device)).Cast<Device>())
                App.Current.Dispatcher.Invoke(() => Devices.Add(device));

            App.Current.Dispatcher.Invoke(() => Stations.Clear());
            foreach (var station in AcabusData.Session.GetObjects(typeof(Station)).Cast<Station>())
                App.Current.Dispatcher.Invoke(() => Stations.Add(station));

            App.Current.Dispatcher.Invoke(() => Vehicles.Clear());
            foreach (var vehicle in AcabusData.Session.GetObjects(typeof(Vehicle)).Cast<Vehicle>())
                App.Current.Dispatcher.Invoke(() => Vehicles.Add(vehicle));

            App.Current.Dispatcher.Invoke(() => Routes.Clear());
            foreach (var route in AcabusData.Session.GetObjects(typeof(Route)).Cast<Route>())
                App.Current.Dispatcher.Invoke(() => Routes.Add(route));
        }

        /// <summary>
        /// Campo que provee a la propiedad 'Routes'.
        /// </summary>
        private ICollection<Route> _routes;

        /// <summary>
        /// Obtiene una lista de todas las rutas.
        /// </summary>
        public ICollection<Route> Routes {
            get {
                if (_routes == null)
                    _routes = new ObservableCollection<Route>();
                return _routes;
            }
        }

        public ICommand RefreshCommand { get; }
    }
}