using InnSyTech.Standard.Database.Linq;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;
using Opera.Acabus.Server.Core.Utils;
using System;
using System.Linq;
using System.Net;

namespace Opera.Acabus.Server.Gui
{
    /// <summary>
    /// Provee de funciones de consulta básica del sistema.
    /// </summary>
    public static class ServerCoreFunctions
    {
        /// <summary>
        /// Crea un autobus nuevo.
        /// </summary>
        public static void CreateBus([ParameterField(12)] BusType type,
            [ParameterField(17)] String economicNumber, [ParameterField(13)] UInt16 idRoute,
            [ParameterField(36)] BusStatus status, IMessage message)
        {
            Bus bus = new Bus(0, economicNumber)
            {
                Route = idRoute == 0 ? null : AcabusDataContext.AllRoutes.FirstOrDefault(x => x.ID == idRoute),
                Status = status,
                Type = type
            };

            bool res = AcabusDataContext.DbContext.Create(bus);

            message.SetBoolean(22, res);
            message[14] = bus.ID;
        }

        /// <summary>
        /// Crea un equipo.
        /// </summary>
        public static void CreateDevice([ParameterField(12)] DeviceType type,
            [ParameterField(17)] String serialNumber, [ParameterField(13)] UInt16 idStation,
            [ParameterField(36)] UInt16 idBus, [ParameterField(18)] String ipAddress, IMessage message)
        {
            Device device = new Device(0, serialNumber, type)
            {
                IPAddress = IPAddress.Parse(ipAddress),
                Bus = idBus == 0 ? null : AcabusDataContext.AllBuses.FirstOrDefault(x => x.ID == idBus),
                Station = idStation == 0 ? null : AcabusDataContext.AllStations.FirstOrDefault(x => x.ID == idStation)
            };

            bool res = AcabusDataContext.DbContext.Create(device);

            message.SetBoolean(22, res);
            message[14] = device.ID;
        }

        /// <summary>
        /// Crea una ruta nueva.
        /// </summary>
        /// <param name="number">Numero de la ruta.</param>
        /// <param name="name">Nombre de la ruta.</param>
        /// <param name="type">Tipo de la ruta. ALIM = 1 o TRUNK = 2.</param>
        public static void CreateRoute([ParameterField(12)] UInt16 number,
            [ParameterField(17)] String name, [ParameterField(13)] RouteType type,
            [ParameterField(18)] String assignedSection, IMessage message)
        {
            Route route = new Route(0, number, type) { Name = name, AssignedSection = assignedSection };
            bool res = AcabusDataContext.DbContext.Create(route);

            message.SetBoolean(22, res);
            message[14] = route.ID;
        }

        /// <summary>
        /// Crea un personal nuevo.
        /// </summary>
        public static void CreateStaff([ParameterField(12)] AssignableArea area,
            [ParameterField(17)] String name, [ParameterField(23)] Boolean active,
            IMessage message)
        {
            Staff staff = new Staff(0)
            {
                Active = active,
                Name = name,
                Area = area
            };

            bool res = AcabusDataContext.DbContext.Create(staff);

            message.SetBoolean(22, res);
            message[14] = staff.ID;
        }

        /// <summary>
        /// Crea una estación nueva.
        /// </summary>
        /// <param name="number">Numero de la estación.</param>
        /// <param name="name">Nombre de la estación.</param>
        public static void CreateStation([ParameterField(12)] UInt16 number,
            [ParameterField(17)] String name, [ParameterField(18)] String assignedSection,
            [ParameterField(13)] UInt16 routeID, [ParameterField(23)] Boolean isExternal,
            IMessage message)
        {
            Station station = new Station(0, number)
            {
                Name = name,
                AssignedSection = assignedSection,
                IsExternal = isExternal,
                Route = routeID == 0 ? null : AcabusDataContext.AllRoutes.FirstOrDefault(x => x.ID == routeID)
            };

            bool res = AcabusDataContext.DbContext.Create(station);

            message.SetBoolean(22, res);
            message[14] = station.ID;
        }

        /// <summary>
        /// Obtiene los autobuses.
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetBus(IMessage message)
        {
            IQueryable<Bus> busQuery = AcabusDataContext.AllBuses.LoadReference(1);

            Helpers.Enumerating(message, busQuery.ToList().Count,
                  i => message[61] = busQuery.ToList()[i].Serialize());
        }

        /// <summary>
        /// Obtiene todos los equipos.
        /// </summary>
        /// <param name="id">Identificador de la estación.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetDevices(IMessage message)
        {
            IQueryable<Device> devices = AcabusDataContext.AllDevices.LoadReference(1);

            Helpers.Enumerating(message, devices.ToList().Count,
                i => message[61] = devices.ToList()[i].Serialize());
        }

        /// <summary>
        /// Obtiene las rutas.
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetRoutes(IMessage message)
        {
            IQueryable<Route> routeQuery = AcabusDataContext.AllRoutes;

            Helpers.Enumerating(message, routeQuery.ToList().Count,
                  i => message[61] = routeQuery.ToList()[i].Serialize());
        }

        /// <summary>
        /// Obtiene el personal.
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetStaff(IMessage message)
        {
            IQueryable<Staff> staffQuery = AcabusDataContext.AllStaff;

            Helpers.Enumerating(message, staffQuery.ToList().Count,
                  i => message[61] = staffQuery.ToList()[i].Serialize());
        }

        /// <summary>
        /// Obtiene las estaciones.
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetStations(IMessage message)
        {
            /***
                7, FieldType.Binary, 1, false, "Es enumerable"
                8, FieldType.Numeric, 100, true, "Registros totales del enumerable"
                9, FieldType.Numeric, 100, true, "Posición del enumerable"
                10, FieldType.Numeric, 1, false, "Operaciones del enumerable (Siguiente|Inicio)"
             */

            IQueryable<Station> stationsQuery = AcabusDataContext.AllStations.LoadReference(1);

            Helpers.Enumerating(message, stationsQuery.ToList().Count,
                i => message[61] = stationsQuery.ToList()[i].Serialize());
        }
    }
}