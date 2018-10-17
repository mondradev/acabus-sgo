using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
using Opera.Acabus.Core.Models;
using Opera.Acabus.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Opera.Acabus.Core.DataAccess
{
    /// <summary>
    /// Proporciona las funciones para comunicarse con el núcleo del servidor.
    /// </summary>
    public static class ServerContext
    {
        /// <summary>
        /// Cliente que controla la interacción con el servidor.
        /// </summary>
        private static readonly AppClient _client;

        /// <summary>
        /// Inicializa la clase estátitca.
        /// </summary>
        static ServerContext()
        {
            _client = new AppClient();
        }

        /// <summary>
        /// Crea un autobus en el lado del servidor.
        /// </summary>
        /// <param name="bus">Instancia del autobus a crear.</param>
        /// <returns>Un valor true si el autobus fue creado.</returns>
        public static bool CreateBus(ref Bus bus)
        {
            IMessage message = _client.CreateMessage();

            message[AdaptiveMessageFieldID.FunctionName.ToInt32()] = nameof(CreateStation);
            message[12] = bus.Type;
            message[17] = bus.EconomicNumber;
            message[13] = bus.Route?.ID ?? 0;
            message[36] = bus.Status;

            Boolean res = false;
            Bus busRes = bus;

            _client.SendMessage(message, x =>
            {
                if (x.GetInt32(3) == 200)
                {
                    res = x.GetBoolean(22);
                    if (res)
                        busRes = new Bus(x.GetUInt64(14), busRes.EconomicNumber)
                        {
                            Status = busRes.Status,
                            Type = busRes.Type,
                            Route = busRes.Route
                        };
                }
            }).Wait();

            bus = busRes;

            return res;
        }

        /// <summary>
        /// Crea un equipo en el lado del servidor.
        /// </summary>
        /// <param name="device">Instancia del equipo a crear.</param>
        /// <returns>Un valor true si el equipo fue creado.</returns>
        public static bool CreateDevice(ref Device device)
        {
            IMessage message = _client.CreateMessage();

            message[AdaptiveMessageFieldID.FunctionName.ToInt32()] = nameof(CreateStation);
            message[12] = device.Type;
            message[17] = device.SerialNumber;
            message[13] = device.Station?.ID ?? 0;
            message[36] = device.Bus?.ID ?? 0;
            message[18] = device.IPAddress.ToString();

            Boolean res = false;
            Device deviceRes = device;

            _client.SendMessage(message, x =>
            {
                if (x.GetInt32(3) == 200)
                {
                    res = x.GetBoolean(22);
                    if (res)
                        deviceRes = new Device(x.GetUInt64(14), deviceRes.SerialNumber, deviceRes.Type)
                        {
                            Bus = deviceRes.Bus,
                            Station = deviceRes.Station,
                            IPAddress = deviceRes.IPAddress
                        };
                }
            }).Wait();

            device = deviceRes;

            return res;
        }

        /// <summary>
        /// Crea una ruta nueva y devuelve true en caso de lograrlo.
        /// </summary>
        /// <param name="route">Instancia ruta a crear en el lado del servidor.</param>
        /// <returns>Un valor true si se creó correctamente.</returns>
        public static bool CreateRoute(ref Route route)
        {
            IMessage message = _client.CreateMessage();

            message[AdaptiveMessageFieldID.FunctionName.ToInt32()] = nameof(CreateRoute);
            message[12] = route.RouteNumber;
            message[17] = route.Name;
            message[13] = (int)route.Type;
            message[18] = route.AssignedSection;

            Boolean res = false;
            Route routeRes = route;

            _client.SendMessage(message, x =>
            {
                if (x.GetInt32(3) == 200)
                {
                    res = x.GetBoolean(22);
                    if (res)
                        routeRes = new Route(x.GetUInt64(14), routeRes.RouteNumber, routeRes.Type)
                        {
                            AssignedSection = routeRes.AssignedSection,
                            Name = routeRes.Name
                        };
                }
            }).Wait();

            route = routeRes;

            return res;
        }

        /// <summary>
        /// Crea una estación en el lado del servidor.
        /// </summary>
        /// <param name="station">Instancia Station a crear.</param>
        /// <returns>Un valor true si la estación fue creada,</returns>
        public static bool CreateStation(ref Station station)
        {
            IMessage message = _client.CreateMessage();

            message[AdaptiveMessageFieldID.FunctionName.ToInt32()] = nameof(CreateStation);
            message[12] = station.StationNumber;
            message[17] = station.Name;
            message[13] = station.Route?.ID ?? 0;
            message[18] = station.AssignedSection;

            message.SetBoolean(23, station.IsExternal);

            Boolean res = false;
            Station stationRes = station;

            _client.SendMessage(message, x =>
            {
                if (x.GetInt32(3) == 200)
                {
                    res = x.GetBoolean(22);
                    if (res)
                        stationRes = new Station(x.GetUInt64(14), stationRes.StationNumber)
                        {
                            AssignedSection = stationRes.AssignedSection,
                            Name = stationRes.Name,
                            Route = stationRes.Route
                        };
                }
            }).Wait();

            station = stationRes;

            return res;
        }

        /// <summary>
        /// Sincroniza los autobuses con la base de datos local.
        /// </summary>
        public static void SyncBus()
        {
            List<Bus> buses = new List<Bus>();
            IMessage message = _client.CreateMessage();

            message[AdaptiveMessageFieldID.FunctionName.ToInt32()] = "GetBus";

            _client.SendMessage(message, (IAdaptiveMsgEnumerator x) =>
            {
                if (x.Current.GetInt32(3) == 200)
                    buses.Add(ModelHelper.GetBus(x.Current.GetBytes(61)));
            }).Wait();

            buses.ForEach(x =>
            {
                bool r = AcabusDataContext.AllBuses.ToList().Any(y => y.ID == x.ID);

                if (r)
                    return;

                r = AcabusDataContext.DbContext.Create(x);

                if (!r)
                    throw new Exception("No se logró guardar el autobus " + x);
            });
        }

        /// <summary>
        /// Sincroniza los autobuses con la base de datos local.
        /// </summary>
        public static void SyncDevices()
        {
            List<Device> devices = new List<Device>();
            IMessage message = _client.CreateMessage();

            message[AdaptiveMessageFieldID.FunctionName.ToInt32()] = "GetDevices";

            _client.SendMessage(message, (IAdaptiveMsgEnumerator x) =>
            {
                if (x.Current.GetInt32(3) == 200)
                    devices.Add(ModelHelper.GetDevice(x.Current.GetBytes(61)));
            }).Wait();

            devices.ForEach(x =>
            {
                bool r = AcabusDataContext.AllDevices.ToList().Any(y => y.ID == x.ID);

                if (r)
                    return;

                r = AcabusDataContext.DbContext.Create(x);

                if (!r)
                    throw new Exception("No se logró guardar el equipo " + x);
            });
        }

        /// <summary>
        /// Sincroniza las rutas con la base de datos local.
        /// </summary>
        public static void SyncRoutes()
        {
            List<Route> routes = new List<Route>();
            IMessage message = _client.CreateMessage();

            message[AdaptiveMessageFieldID.FunctionName.ToInt32()] = "GetRoutes";

            _client.SendMessage(message, (IAdaptiveMsgEnumerator x) =>
            {
                if (x.Current.GetInt32(3) == 200)
                    routes.Add(ModelHelper.GetRoute(x.Current.GetBytes(61)));
            }).Wait();

            routes.ForEach(x =>
            {
                bool r = AcabusDataContext.AllRoutes.ToList().Any(y => y.ID == x.ID);

                if (r)
                    return;

                r = AcabusDataContext.DbContext.Create(x);

                if (!r)
                    throw new Exception("No se logró guardar la ruta " + x);
            });
        }

        /// <summary>
        /// Sincroniza el personal con la base de datos local.
        /// </summary>
        public static void SyncStaff()
        {
            List<Staff> staff = new List<Staff>();
            IMessage message = _client.CreateMessage();

            message[AdaptiveMessageFieldID.FunctionName.ToInt32()] = "GetStaff";

            _client.SendMessage(message, (IAdaptiveMsgEnumerator x) =>
            {
                if (x.Current.GetInt32(3) == 200)
                    staff.Add(ModelHelper.GetStaff(x.Current.GetBytes(61)));
            }).Wait();

            staff.ForEach(x =>
            {
                bool r = AcabusDataContext.AllStaff.ToList().Any(y => y.ID == x.ID);

                if (r)
                    return;

                r = AcabusDataContext.DbContext.Create(x);

                if (!r)
                    throw new Exception("No se logró guardar el miembro del personal " + x);
            });
        }

        /// <summary>
        /// Sincroniza las estaciones con la base de datos local.
        /// </summary>
        public static void SyncStations()
        {
            List<Station> stations = new List<Station>();
            IMessage message = _client.CreateMessage();

            message[AdaptiveMessageFieldID.FunctionName.ToInt32()] = "GetStations";

            _client.SendMessage(message, (IAdaptiveMsgEnumerator x) =>
            {
                if (x.Current.GetInt32(3) == 200)
                    stations.Add(ModelHelper.GetStation(x.Current.GetBytes(61)));
            }).Wait();

            stations.ForEach(x =>
            {
                bool r = AcabusDataContext.AllStations.ToList().Any(y => y.ID == x.ID);

                if (r)
                    return;

                r = AcabusDataContext.DbContext.Create(x);

                if (!r)
                    throw new Exception("No se logró guardar la estación " + x);
            });
        }
    }
}