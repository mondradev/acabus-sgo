using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
using Opera.Acabus.Core.Models;
using Opera.Acabus.Core.Services;
using System;
using System.Collections.Generic;

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
        /// Obtiene el equipo correspondiente al ID especificado.
        /// </summary>
        /// <param name="id">Identificador del equipo.</param>
        public static Device GetDeviceByID(UInt16 id)
        {
            IMessage message = _client.CreateMessage();

            message[AdaptiveMessageFieldID.FunctionName.ToInt32()] = nameof(GetDeviceByID);
            message[12] = id;

            Device response = null;

            _client.SendMessage(message, x =>
            {
                if (x.GetInt32(3) == 200)
                    response = ModelHelper.GetDevice(x.GetBytes(60));
            });

            return response;
        }

        /// <summary>
        /// Obtiene todos los equipos.
        /// </summary>
        /// <param name="type">Tipo del equipo.</param>
        public static List<Device> GetDevices()
        {
            IMessage message = _client.CreateMessage();

            message[AdaptiveMessageFieldID.FunctionName.ToInt32()] = nameof(GetDevices);

            List<Device> devices = new List<Device>();

            _client.SendMessage(message, (IAdaptiveMsgEnumerator x) =>
            {
                if (x.Current.GetInt32(3) == 200)
                    devices.Add(ModelHelper.GetDevice(x.Current.GetBytes(60)));
            }).Wait();

            return devices;
        }

        /// <summary>
        /// Obtiene los equipos que corresponden a la estación especificada.
        /// </summary>
        /// <param name="id">Identificador de la estación.</param>
        public static List<Device> GetDevicesByStation(UInt16 idStation)
        {
            IMessage message = _client.CreateMessage();

            message[AdaptiveMessageFieldID.FunctionName.ToInt32()] = nameof(GetDevicesByStation);
            message[12] = idStation;

            List<Device> devices = new List<Device>();

            _client.SendMessage(message, (IAdaptiveMsgEnumerator x) =>
            {
                if (x.Current.GetInt32(3) == 200)
                    devices.Add(ModelHelper.GetDevice(x.Current.GetBytes(60)));
            }).Wait();

            return devices;
        }

        /// <summary>
        /// Obtiene los equipos que corresponden al tipo especificado
        /// </summary>
        /// <param name="type">Tipo del equipo.</param>
        public static List<Device> GetDevicesByType(DeviceType type)
        {
            IMessage message = _client.CreateMessage();

            message[AdaptiveMessageFieldID.FunctionName.ToInt32()] = nameof(GetDevicesByType);
            message[12] = (int)type;

            List<Device> devices = new List<Device>();

            _client.SendMessage(message, (IAdaptiveMsgEnumerator x) =>
            {
                if (x.Current.GetInt32(3) == 200)
                    devices.Add(ModelHelper.GetDevice(x.Current.GetBytes(60)));
            }).Wait();

            return devices;
        }

        /// <summary>
        /// Obtiene la ruta especificada por el ID.
        /// </summary>
        /// <param name="id">Identificador de la ruta.</param>
        /// <returns>La ruta obtenida desde servidor.</returns>
        public static Route GetRouteByID(UInt16 id)
        {
            IMessage message = _client.CreateMessage();

            message[AdaptiveMessageFieldID.FunctionName.ToInt32()] = nameof(GetRouteByID);
            message[12] = id;

            Route resRoute = null;

            _client.SendMessage(message, x =>
            {
                if (x.GetInt32(3) == 200)
                {
                    resRoute = ModelHelper.GetRoute(x.GetBytes(60));
                }
            }).Wait();

            return resRoute;
        }

        /// <summary>
        /// Obtiene las rutas registradas en el servidor.
        /// </summary>
        /// <returns>Una secuencia de rutas.</returns>
        public static List<Route> GetRoutes()
        {
            IMessage message = _client.CreateMessage();

            message[AdaptiveMessageFieldID.FunctionName.ToInt32()] = nameof(GetRoutes);

            List<Route> routes = new List<Route>();

            _client.SendMessage(message, (IAdaptiveMsgEnumerator x) =>
            {
                if (x.Current.GetInt32(3) == 200)
                    routes.Add(ModelHelper.GetRoute(x.Current.GetBytes(60)));
            }).Wait();

            return routes;
        }

        /// <summary>
        /// Obtiene una estación que coincida con el ID especificado.
        /// </summary>
        /// <param name="id">Identificador de la estación.</param>
        /// <returns>Una instancia de estación.</returns>
        public static Station GetStationByID(UInt16 id)
        {
            IMessage message = _client.CreateMessage();

            message[AdaptiveMessageFieldID.FunctionName.ToInt32()] = nameof(GetStationByID);
            message[12] = id;

            Station resStation = null;

            _client.SendMessage(message, x =>
            {
                if (x.GetInt32(3) == 200)
                {
                    resStation = ModelHelper.GetStation(x.GetBytes(60));
                }
            }).Wait();

            return resStation;
        }

        /// <summary>
        /// Obtiene una secuencia de las estaciones registras en el servidor.
        /// </summary>
        /// <returns>Una secuencia de estaciones.</returns>
        public static List<Station> GetStations()
        {
            IMessage message = _client.CreateMessage();

            message[AdaptiveMessageFieldID.FunctionName.ToInt32()] = nameof(GetStations);

            List<Station> stations = new List<Station>();

            _client.SendMessage(message, (IAdaptiveMsgEnumerator x) =>
            {
                if (x.Current.GetInt32(3) == 200)
                    stations.Add(ModelHelper.GetStation(x.Current.GetBytes(60)));
            }).Wait();

            return stations;
        }

        /// <summary>
        /// Obtiene una secuencia de las estaciones que pertecen a la ruta especificada por el ID.
        /// </summary>
        /// <param name="id">Identificador de la ruta.</param>
        /// <returns>Secuencia de estaciones.</returns>
        public static List<Station> GetStationsByRoute(UInt16 id)
        {
            IMessage message = _client.CreateMessage();

            message[AdaptiveMessageFieldID.FunctionName.ToInt32()] = nameof(GetStationsByRoute);
            message[12] = id;

            List<Station> stations = new List<Station>();

            _client.SendMessage(message, (IAdaptiveMsgEnumerator x) =>
            {
                if (x.Current.GetInt32(3) == 200)
                    stations.Add(ModelHelper.GetStation(x.Current.GetBytes(60)));
            }).Wait();

            return stations;
        }
    }
}