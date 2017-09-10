using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Configurations.Config.Views;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Opera.Acabus.Configurations.Config.ViewModels
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
                    RefreshData();
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

            Task.Run(() => RefreshData());
        }

        /// <summary>
        /// Obtiene una lista de todos los autobuses.
        /// </summary>
        public IEnumerable<Bus> Buses => AcabusData.AllBuses;

        /// <summary>
        /// Obtiene el comando para reasignar ruta a los autobuses.
        /// </summary>
        public ICommand BusReassingCommand { get; }

        /// <summary>
        /// Obtiene una lista de todos los dispositivos.
        /// </summary>
        public IEnumerable<Device> Devices => AcabusData.AllDevices;

        /// <summary>
        /// Obtiene el comando para descargar la información del servidor.
        /// </summary>
        public ICommand DownloadDataCommand { get; }

        /// <summary>
        /// Obtiene una lista de el personal del área de TI.
        /// </summary>
        public IEnumerable<ITStaff> ITStaff => AcabusData.ITStaff as ICollection<ITStaff>;

        /// <summary>
        /// Obtiene el comando para actualizar la información desde la base de datos local.
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// Obtiene una lista de todas las rutas.
        /// </summary>
        public ICollection<Route> Routes => AcabusData.AllRoutes as ICollection<Route>;

        /// <summary>
        /// Obtiene el comando para mostrar el formulario para añadir equipos.
        /// </summary>
        public ICommand ShowAddDeviceCommand { get; }

        /// <summary>
        /// Obtiene una lista de todas las estaciones.
        /// </summary>
        public IEnumerable<Station> Stations => AcabusData.AllStations;

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
                        vehicleReassign.Route?.Buses.Remove(vehicle);
                        vehicleReassign.Route = vehicle.Route;

                        if (vehicle.Route != null && !vehicle.Route.Buses.Contains(vehicle))
                            vehicle.Route.Buses.Add(vehicle);

                        AcabusData.Session.Update(ref vehicleReassign);
                        continue;
                    }
                    routeAsigned.Buses.Add(vehicle);
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
                        deviceReassign.Station?.Devices.Remove(device);
                        deviceReassign.Bus?.Devices.Remove(device);
                        deviceReassign.Station = station;
                        deviceReassign.Bus = bus;

                        if (bus != null && !bus.Devices.Contains(device))
                            bus.Devices.Add(device);

                        if (station != null && !station.Devices.Contains(device))
                            station.Devices.Add(device);

                        AcabusData.Session.Update(ref device);
                        continue;
                    }
                    station?.Devices.Add(device);
                    bus?.Devices.Add(device);
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
                    var routeAssigned = AcabusData.AllRoutes.FirstOrDefault(route => route.ID == UInt16.Parse(row[3].Trim()));

                    Station station = new Station(id, stationNumber)
                    {
                        Name = name,
                        Route = routeAssigned
                    };

                    if (Stations.Contains(station))
                    {
                        var stationReassign = Stations.FirstOrDefault(stationTemp => stationTemp.ID == station.ID);
                        stationReassign.Route?.Stations.Remove(station);
                        stationReassign.Route = station.Route;

                        if (station.Route != null && !station.Route.Stations.Contains(station))
                            station.Route.Stations.Add(station);

                        AcabusData.Session.Update(ref stationReassign);
                        continue;
                    }
                    routeAssigned.Stations.Add(station);
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
            AcabusData.ReloadData();
        }
    }
}