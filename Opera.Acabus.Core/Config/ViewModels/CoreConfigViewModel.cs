using InnSyTech.Standard.Mvvm;
using InnSyTech.Standard.Utils;
using Opera.Acabus.Core.Config.Views;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// Campo que provee a la propiedad 'Vehicles'.
        /// </summary>
        private ICollection<Bus> _buses;

        /// <summary>
        /// Campo que provee a la propiedad 'Devices'.
        /// </summary>
        private ICollection<Device> _devices;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ITStaff" />.
        /// </summary>
        private ICollection<ITStaff> _itStaff;

        /// <summary>
        /// Campo que provee a la propiedad 'Routes'.
        /// </summary>
        private ICollection<Route> _routes;

        /// <summary>
        /// Campo que provee a la propiedad 'Stations'.
        /// </summary>
        private ICollection<Station> _stations;

        /// <summary>
        /// Crea una instancia de <see cref="CoreConfigViewModel"/>.
        /// </summary>
        public CoreConfigViewModel()
        {
            DownloadDataCommand = new Command(parameter =>
            {
                Task.Run(() =>
                {
                    AcabusData.SendNotify("DESCARGANDO RUTAS...");
                    if (DownloadRoutes())
                    {
                        AcabusData.ReloadData();
                        AcabusData.SendNotify("RUTAS DESCARGAS CORRECTAMENTE, DESCAGANDO AUTOBUSES...");
                        if (DownloadBuses())
                        {
                            AcabusData.SendNotify("AUTOBUSES DESCARGADOS Y REASIGNADOS A SUS RUTAS CORRECTAMENTE.");
                            AcabusData.ReloadData();
                        }
                        else
                            AcabusData.SendNotify("ERROR: FALLA AL DESCARGAR LOS AUTOBUSES");
                        AcabusData.SendNotify("DESCARGANDO ESTACIONES...");
                        if (DownloadStations())
                        {
                            AcabusData.ReloadData();
                            AcabusData.SendNotify("ESTACIONES DESCARGADAS CORRECTAMENTE, DESCARGANDO EQUIPOS ASIGNADOS...");
                            if (DownloadDevices())
                            {
                                AcabusData.SendNotify("EQUIPOS DESCARGADOS CORRECTAMENTE.");
                                RefreshData();
                            }
                            else
                                AcabusData.SendNotify("ERROR: FALLA AL DESCARGAR LOS EQUIPOS");
                        }
                        else
                            AcabusData.SendNotify("ERROR: FALLA AL DESCARGAR LAS ESTACIONES");
                    }
                    else
                        AcabusData.SendNotify("ERROR: FALLA AL DESCARGAR LAS RUTAS");
                });
            });

            RefreshCommand = new Command(parameter =>
            {
                Task.Run(() =>
                {
                    RefreshData();
                    AcabusData.SendNotify("LISTA DE EQUIPOS, ESTACIONES, AUTOBUSES Y RUTAS ACTUALIZADAS.");
                });
            });

            BusReassingCommand = new Command(parameter =>
            {
                switch (parameter.ToString())
                {
                    case "0":
                        Task.Run(() =>
                        {
                            AcabusData.SendNotify("DESCAGANDO AUTOBUSES Y REASIGNANDO...");
                            if (DownloadBuses())
                                AcabusData.SendNotify("VEHÍCULOS AUTOBUSES Y REASIGNADOS.");
                        });
                        break;

                    case "1":
                        AcabusData.RequestShowDialog(new ManualReassignRouteView(), RefreshCommand.Execute);
                        break;
                }
            });

            ShowAddDeviceCommand = new Command(parameter => AcabusData.RequestShowDialog(new AddDeviceView(), RefreshCommand.Execute));

            Task.Run(() => RefreshData());
        }

        /// <summary>
        /// Obtiene una lista de todos los autobuses.
        /// </summary>
        public ICollection<Bus> Buses {
            get {
                if (_buses == null)
                    _buses = new ObservableCollection<Bus>();
                return _buses;
            }
        }

        /// <summary>
        /// Obtiene el comando para reasignar ruta a los autobuses.
        /// </summary>
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

        /// <summary>
        /// Obtiene el comando para descargar la información del servidor.
        /// </summary>
        public ICommand DownloadDataCommand { get; }

        /// <summary>
        /// Obtiene una lista de el personal del área de TI.
        /// </summary>
        public ICollection<ITStaff> ITStaff
            => _itStaff ?? (_itStaff = new ObservableCollection<ITStaff>());

        /// <summary>
        /// Obtiene el comando para actualizar la información desde la base de datos local.
        /// </summary>
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
        /// Obtiene el comando para mostrar el formulario para añadir equipos.
        /// </summary>
        public ICommand ShowAddDeviceCommand { get; }

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
        /// Descarga toda la información de los autobuses desde el servidor.
        /// </summary>
        /// <returns>Un valor <see cref="true"/> si la descarga se completó.</returns>
        private bool DownloadBuses()
        {
            try
            {
                if (String.IsNullOrEmpty(AcabusData.BusQuery)) return false;

                var resultSet = AcabusData.ExecuteQueryInServerDB(AcabusData.BusQuery);

                foreach (var row in resultSet)
                {
                    var id = UInt64.Parse(row[0].Trim());
                    var type = (BusType)UInt16.Parse(row[1].Trim());
                    var economicNumber = row[2].Trim();
                    var routeAsigned = AcabusData.AllRoutes.FirstOrDefault(route => route.ID == UInt16.Parse(row[3].Trim()));

                    Bus vehicle = new Bus(id, economicNumber)
                    {
                        Status = BusStatus.OPERATIONAL,
                        Type = type,
                        Route = routeAsigned
                    };

                    if (Buses.Contains(vehicle))
                    {
                        var vehicleReassign = Buses.FirstOrDefault(vehicleTemp => vehicleTemp.ID == vehicle.ID);
                        vehicleReassign.Route = vehicle.Route;
                        AcabusData.Session.Update(ref vehicleReassign);
                        continue;
                    }
                    Application.Current.Dispatcher.Invoke(() => Buses.Add(vehicle));
                    AcabusData.Session.Save(ref vehicle);
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
                if (String.IsNullOrEmpty(AcabusData.DeviceQuery)) return false;

                var resultSet = AcabusData.ExecuteQueryInServerDB(AcabusData.DeviceQuery);

                foreach (var row in resultSet)
                {
                    var id = UInt64.Parse(row[0].Trim());
                    var serialNumber = row[1].Trim();
                    var ipAddress = row[2].Trim();
                    var station = !String.IsNullOrEmpty(row[3].Trim())
                        ? Stations.FirstOrDefault(stationFind => stationFind.StationNumber == UInt16.Parse(row[3].Trim()))
                        : null;
                    var bus = Buses.FirstOrDefault(vehicleFind => vehicleFind.EconomicNumber == row[4].Trim());
                    var type = (DeviceType)Enum.Parse(typeof(DeviceType), row[5].Trim());

                    Device device = new Device(id, serialNumber, type)
                    {
                        Bus = bus,
                        IPAddress = IPAddress.Parse(ipAddress),
                        Station = station
                    };

                    if (Devices.Contains(device))
                    {
                        var deviceReassign = Devices.FirstOrDefault(deviceTemp => device.ID == deviceTemp.ID);
                        deviceReassign.Station = station;
                        deviceReassign.Bus = bus;
                        AcabusData.Session.Update(ref device);
                        continue;
                    }
                    Application.Current.Dispatcher.Invoke(() => Devices.Add(device));
                    AcabusData.Session.Save(ref device);
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
                if (String.IsNullOrEmpty(AcabusData.RouteQuery)) return false;

                var resultSet = AcabusData.ExecuteQueryInServerDB(AcabusData.RouteQuery);

                foreach (var row in resultSet)
                {
                    var id = UInt16.Parse(row[0]);
                    var number = UInt16.Parse(row[1]);
                    var type = (RouteType)Enum.Parse(typeof(RouteType), row[3], true);
                    Route route = new Route(id, number, type)
                    {
                        Name = row[2]?.Trim().ToUpper()
                    };

                    if (Routes.Contains(route)) continue;

                    Application.Current.Dispatcher.Invoke(() => Routes.Add(route));
                    AcabusData.Session.Save(ref route);
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
                if (String.IsNullOrEmpty(AcabusData.StationQuery)) return false;

                var resultSet = AcabusData.ExecuteQueryInServerDB(AcabusData.StationQuery);

                foreach (var row in resultSet)
                {
                    var id = UInt16.Parse(row[0].Trim());
                    var name = row[1].Trim();
                    var stationNumber = UInt16.Parse(row[2].Trim());
                    var routeAsigned = AcabusData.AllRoutes.FirstOrDefault(route => route.ID == UInt16.Parse(row[3].Trim()));

                    Station station = new Station(id, stationNumber)
                    {
                        Name = name,
                        Route = routeAsigned
                    };

                    if (Stations.Contains(station))
                    {
                        var stationReassign = Stations.FirstOrDefault(stationTemp => stationTemp.ID == station.ID);
                        stationReassign.Route = station.Route;
                        AcabusData.Session.Update(ref stationReassign);
                        continue;
                    }
                    Application.Current.Dispatcher.Invoke(() => Stations.Add(station));
                    AcabusData.Session.Save(ref station);
                }
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Actualiza la información desde la base de datos local.
        /// </summary>
        private void RefreshData()
        {
            Application.Current.Dispatcher.Invoke(() => Devices.Clear());
            Application.Current.Dispatcher.Invoke(() => Buses.Clear());
            Application.Current.Dispatcher.Invoke(() => Stations.Clear());
            Application.Current.Dispatcher.Invoke(() => Routes.Clear());
            Application.Current.Dispatcher.Invoke(() => ITStaff.Clear());

            AcabusData.ReloadData();

            IEnumerable<Route> routes = AcabusData.AllRoutes;

            foreach (var route in routes)
                Application.Current.Dispatcher.Invoke(() => Routes.Add(route));

            foreach (var station in routes.Select(route => route.Stations).Merge())
                Application.Current.Dispatcher.Invoke(() => Stations.Add(station));

            foreach (var vehicle in routes.Select(route => route.Buses).Merge())
                Application.Current.Dispatcher.Invoke(() => Buses.Add(vehicle));

            foreach (var device in Extensions.Merge(new[] {
                Stations.Select(station => station.Devices).Merge(),
                Buses.Select(vehicle => vehicle.Devices).Merge()
            }))
                Application.Current.Dispatcher.Invoke(() => Devices.Add(device));

            foreach (var itStaff in AcabusData.ITStaff)
                Application.Current.Dispatcher.Invoke(() => ITStaff.Add(itStaff));
        }
    }
}