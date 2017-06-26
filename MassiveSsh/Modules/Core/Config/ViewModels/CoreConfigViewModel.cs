using Acabus.DataAccess;
using Acabus.Models;
using Acabus.Modules.Core.Config.Views;
using Acabus.Utils;
using Acabus.Utils.Mvvm;
using Acabus.Window;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Acabus.Modules.Core.Config.ViewModels
{
    public class CoreConfigViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Devices'.
        /// </summary>
        private ICollection<Device> _devices;

        /// <summary>
        /// Campo que provee a la propiedad 'Routes'.
        /// </summary>
        private ICollection<Route> _routes;

        /// <summary>
        /// Campo que provee a la propiedad 'Stations'.
        /// </summary>
        private ICollection<Station> _stations;

        /// <summary>
        /// Campo que provee a la propiedad 'Vehicles'.
        /// </summary>
        private ICollection<Vehicle> _vehicles;

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
                        DataAccess.AcabusData.ReloadData();
                        AcabusControlCenterViewModel.AddNotify("RUTAS DESCARGAS CORRECTAMENTE, DESCAGANDO VEHÍCULOS...");
                        if (DownloadVehicles())
                        {
                            AcabusControlCenterViewModel.AddNotify("VEHÍCULOS DESCARGADOS Y REASIGNADOS A SUS RUTAS CORRECTAMENTE.");
                            DataAccess.AcabusData.ReloadData();
                        }
                        else
                            AcabusControlCenterViewModel.AddNotify("ERROR: FALLA AL DESCARGAR LOS VEHÍCULOS");
                        AcabusControlCenterViewModel.AddNotify("DESCARGANDO ESTACIONES...");
                        if (DownloadStations())
                        {
                            DataAccess.AcabusData.ReloadData();
                            AcabusControlCenterViewModel.AddNotify("ESTACIONES DESCARGADAS CORRECTAMENTE, DESCARGANDO EQUIPOS ASIGNADOS...");
                            if (DownloadDevices())
                            {
                                AcabusControlCenterViewModel.AddNotify("EQUIPOS DESCARGADOS CORRECTAMENTE.");
                                RefreshData();
                            }
                            else
                                AcabusControlCenterViewModel.AddNotify("ERROR: FALLA AL DESCARGAR LOS EQUIPOS");
                        }
                        else
                            AcabusControlCenterViewModel.AddNotify("ERROR: FALLA AL DESCARGAR LAS ESTACIONES");
                    }
                    else
                        AcabusControlCenterViewModel.AddNotify("ERROR: FALLA AL DESCARGAR LAS RUTAS");
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
                switch (parameter.ToString())
                {
                    case "0":
                        Task.Run(() =>
                        {
                            AcabusControlCenterViewModel.AddNotify("DESCAGANDO VEHÍCULOS Y REASIGNANDO...");
                            if (DownloadVehicles())
                                AcabusControlCenterViewModel.AddNotify("VEHÍCULOS DESCARGADOS Y REASIGNADOS.");
                        });
                        break;
                    case "1":
                        AcabusControlCenterViewModel.ShowDialog(new ManualReassignRouteView(), RefreshCommand.Execute);
                        break;
                }

            });

            AddDeviceCommand = new CommandBase(parameter => AcabusControlCenterViewModel.ShowDialog(new AddDeviceView(), RefreshCommand.Execute));

            Task.Run(() => RefreshData());
        }

        public ICommand AddDeviceCommand { get; }

        public ICommand BusReassingCommand { get; }

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

        public ICommand DownloadDataCommand { get; }

        public ICommand RefreshCommand { get; }

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
        /// Obtiene una lista de todos los autobuses.
        /// </summary>
        public ICollection<Vehicle> Vehicles {
            get {
                if (_vehicles == null)
                    _vehicles = new ObservableCollection<Vehicle>();
                return _vehicles;
            }
        }

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
                        AcabusData.Session.Update(ref device);
                        continue;
                    }
                    App.Current.Dispatcher.Invoke(() => Devices.Add(device));
                    AcabusData.Session.Save(ref device);
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
                    AcabusData.Session.Save(ref route);
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
                    var routeAsigned = DataAccess.AcabusData.AllRoutes.FirstOrDefault(route => route.ID == UInt16.Parse(row[6].Trim()));

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
                        AcabusData.Session.Update(ref stationReassign);
                        continue;
                    }
                    App.Current.Dispatcher.Invoke(() => Stations.Add(station));
                    AcabusData.Session.Save(ref station);
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
                    var routeAsigned = DataAccess.AcabusData.AllRoutes.FirstOrDefault(route => route.ID == UInt16.Parse(row[5].Trim()));

                    Vehicle vehicle = new Vehicle(id, economicNumber, type)
                    {
                        Enabled = enabled,
                        Route = routeAsigned
                    };

                    if (Vehicles.Contains(vehicle))
                    {
                        var vehicleReassign = Vehicles.FirstOrDefault(vehicleTemp => vehicleTemp.ID == vehicle.ID);
                        vehicleReassign.Route = vehicle.Route;
                        AcabusData.Session.Update(ref vehicleReassign);
                        continue;
                    }
                    App.Current.Dispatcher.Invoke(() => Vehicles.Add(vehicle));
                    AcabusData.Session.Save(ref vehicle);
                }
                return true;
            }
            catch { return false; }
        }

        private void RefreshData()
        {
            App.Current.Dispatcher.Invoke(() => Devices.Clear());
            App.Current.Dispatcher.Invoke(() => Vehicles.Clear());
            App.Current.Dispatcher.Invoke(() => Stations.Clear());
            App.Current.Dispatcher.Invoke(() => Routes.Clear());

            DataAccess.AcabusData.ReloadData();

            IEnumerable<Route> routes = DataAccess.AcabusData.AllRoutes;

            foreach (var route in routes)
                App.Current.Dispatcher.Invoke(() => Routes.Add(route));

            foreach (var station in routes.Select(route => route.Stations).Combine())
                App.Current.Dispatcher.Invoke(() => Stations.Add(station));

            foreach (var vehicle in routes.Select(route => route.Vehicles).Combine())
                App.Current.Dispatcher.Invoke(() => Vehicles.Add(vehicle));

            foreach (var device in Util.Combine(new[] {
                Stations.Select(station => station.Devices).Combine(),
                Vehicles.Select(vehicle => vehicle.Devices).Combine()
            }))
                App.Current.Dispatcher.Invoke(() => Devices.Add(device));
        }
    }
}