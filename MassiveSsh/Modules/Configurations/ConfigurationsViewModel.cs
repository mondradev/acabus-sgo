using Acabus.Models;
using System;
using System.Text.RegularExpressions;
using static Acabus.DataAccess.AcabusData;

namespace Acabus.Modules.Configurations
{
    public class ConfigurationsViewModel
    {
        public static void DownloadAllTrunks()
        {
            var responseTrunks = ExecuteQueryInServerDB(TrunksQuery);
            var responseStations = ExecuteQueryInServerDB(StationsQuery);
            var responseLines = ExecuteQueryInServerDB(LinesQuery);
            var responseDevice = ExecuteQueryInServerDB(DevicesQuery);

            for (int i = 1; i < responseTrunks.Length; i++)
            {
                var trunkData = responseTrunks[i];
                var id = UInt16.Parse(trunkData[0]);
                Trunk trunkTemp = new Trunk(id, UInt16.Parse(Regex.Match(trunkData[1], "T{1}[0-9]{1,}")?.Value.Substring(1)))
                {
                    Name = trunkData[1].Substring(trunkData[1].IndexOf('-') + 1).Trim()
                };

                var oldTrunk = Trunks.FindTrunk((trunk) => trunk.RouteNumber == trunkTemp.RouteNumber);
                if (oldTrunk != null)
                {
                    foreach (var item in oldTrunk.Stations)
                        trunkTemp.AddStation(item);
                    oldTrunk.Stations.Clear();
                    Trunks.Remove(oldTrunk);
                    Trunks.Add(trunkTemp);
                }
                else
                    Trunks.Add(trunkTemp);

                for (int j = 1; j < responseLines.Length; j++)
                {
                    var lineData = responseLines[j];
                    var idFkTrunk = UInt16.Parse(lineData[0]);
                    var idLine = UInt16.Parse(lineData[3]);

                    if (id != idFkTrunk) continue;

                    var idFkStation = UInt16.Parse(lineData[1]);

                    for (int k = 1; k < responseStations.Length; k++)
                    {
                        var stationData = responseStations[k];
                        var idStation = UInt16.Parse(stationData[0]);

                        if (idFkStation != idStation) continue;

                        Station stationTemp = new Station(trunkTemp, idStation, UInt16.Parse(lineData[2]))
                        {
                            IsConnected = true,
                            PingMin = 100,
                            PingMax = 600,
                            Name = stationData[1]
                        };

                        var oldStation = FindStation((station) => station.StationNumber == stationTemp.StationNumber);
                        if (oldStation != null)
                        {
                            foreach (var item in oldStation.Devices)
                                if (item.Type == DeviceType.SW || item.Type == DeviceType.NVR || item.Type == DeviceType.PDE
                                    || item.Type == DeviceType.DB || item.Type == DeviceType.APP)
                                    stationTemp.AddDevice(item);
                            oldStation.Devices.Clear();
                            trunkTemp.Stations.Remove(oldStation);
                            trunkTemp.AddStation(stationTemp);
                        }
                        else
                            trunkTemp.AddStation(stationTemp);

                        for (int l = 1; l < responseDevice.Length; l++)
                        {
                            var deviceData = responseDevice[l];
                            var idFkLine = UInt16.Parse(deviceData[2]);

                            if (idFkLine != idLine) continue;

                            var type = (DeviceType)Enum.Parse(typeof(DeviceType), Regex.Match(deviceData[0], "[A-Z]{2,}")?.Value);
                            if (type == DeviceType.NONE) continue;

                            var idDevice = UInt16.Parse(deviceData[0].Substring(deviceData[0].Length - 2));

                            Device deviceTemp = new Device(idDevice, type, stationTemp)
                            {
                                IP = deviceData[1],
                                CanReplicate = type != DeviceType.CDE,
                                HasDataBase = type != DeviceType.CDE,
                                Enabled = true,
                                SshEnabled = true
                            };
                            if (type == DeviceType.KVR)
                                Kvr.FromDeviceToKvr(ref deviceTemp);

                            stationTemp.AddDevice(deviceTemp);
                        }
                    }
                }
            }

            foreach (Device device in CC.Devices)
                FindStation((station) => station.Name.Contains("CENTRO DE CONTROL"))?.AddDevice(device);

            SaveXml();
        }

        public static void DownloadAllVehicles()
        {
            var responseRoutes = ExecuteQueryInServerDB(RoutesQuery);
            var responseVehicles = ExecuteQueryInServerDB(VehiclesQuery);
            var responseVehiclesAsign = ExecuteQueryInServerDB(VehicleAsignQuery);

            for (int i = 1; i < responseRoutes.Length; i++)
            {
                var routeData = responseRoutes[i];
                var id = UInt16.Parse(routeData[0]);
                var caption = Regex.Match(routeData[1], "RUTA [T|A]{1}[0-9]{1,}")?.Value;
                var type = caption.Contains("RUTA T") ? RouteType.TRUNK : RouteType.ALIM;
                var routeNumber = Regex.Match(caption, "[0-9]{1,}")?.Value;
                var name = routeData[1].Substring(routeData[1].IndexOf('-') + 1).Trim();
                Route routeTemp = null;
                switch (type)
                {
                    case RouteType.ALIM:
                        routeTemp = new Route(id, UInt16.Parse(routeNumber))
                        {
                            Name = name
                        };
                        break;

                    case RouteType.TRUNK:
                        routeTemp = new Trunk(id, ushort.Parse(routeNumber))
                        {
                            Name = name
                        };
                        break;
                }
                if (Routes.FindRoute((route)
                    => route.RouteNumber == routeTemp.RouteNumber
                        && route.Type == routeTemp.Type) == null)
                    Routes.Add(routeTemp);
            }

            Routes.ForEachRoute((route) => route.Vehicles.Clear());

            for (int i = 1; i < responseVehicles.Length; i++)
            {
                var vehicleData = responseVehicles[i];
                var economicNumber = vehicleData[1];
                var type = vehicleData[2];

                Vehicle vehicleTemp = new Vehicle(economicNumber)
                {
                    BusType = (VehicleType)UInt16.Parse(type),
                    Enabled = true
                };

                Boolean asigned = false;
                for (int j = 1; j < responseVehiclesAsign.Length; j++)
                {
                    var asignData = responseVehiclesAsign[j];
                    var caption = Regex.Match(asignData[1], "RUTA [T|A]{1}[0-9]{1,}")?.Value;
                    var routeType = caption.Contains("RUTA T") ? RouteType.TRUNK : RouteType.ALIM;
                    var routeNumber = UInt16.Parse(Regex.Match(caption, "[0-9]{1,}")?.Value);
                    var fkEconomicNumber = asignData[0];

                    if (fkEconomicNumber != economicNumber) continue;

                    var routeAsigned = Routes.FindRoute((route) => route.RouteNumber == routeNumber && route.Type == routeType);
                    routeAsigned.AddVehicle(vehicleTemp);
                    vehicleTemp.Route = routeAsigned;
                    asigned = true;
                    break;
                }

                if (asigned) continue;

                Routes.FindTrunk((trunk) => trunk.RouteNumber == 2)?.AddVehicle(vehicleTemp);
                vehicleTemp.Route = Routes.FindTrunk((trunk) => trunk.RouteNumber == 2);
            }

            SaveXml();
        }
    }
}