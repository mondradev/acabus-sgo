using InnSyTech.Standard.Database.Linq;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Models;
using Opera.Acabus.Core.Models.Base;
using Opera.Acabus.Core.Services;
using Opera.Acabus.Server.Core;
using Opera.Acabus.Server.Core.Gui;
using Opera.Acabus.Server.Core.Utils;
using System;
using System.Linq;
using System.Net;

namespace Opera.Acabus.Server.Config
{
    /// <summary>
    /// Provee de funciones de consulta básica del sistema.
    /// </summary>
    public class ServerCoreFunctions : ServiceModuleBase
    {
        /// <summary>
        /// Obtiene el nombre del servicio.
        /// </summary>
        public override string ServiceName => "Server Core";

        /// <summary>
        /// Crea un autobus nuevo.
        /// </summary>
        public void CreateBus([ParameterField(12)] BusType type,
            [ParameterField(17)] String economicNumber, [ParameterField(13, true)] UInt16 idRoute,
            [ParameterField(36)] BusStatus status, [ParameterField(14, true)] UInt32 id, IAdaptiveMessageReceivedArgs args)
        {
            Route assignedRoute = AcabusDataContext.AllRoutes.FirstOrDefault(x => x.ID == idRoute);

            if (assignedRoute == null)
                throw new ServiceException(String.Format("Ruta [ID={0}] no encontrada", idRoute), AdaptiveMessageResponseCode.NOT_ACCEPTABLE, nameof(CreateBus), ServiceName);

            if (String.IsNullOrEmpty(economicNumber))
                throw new ServiceException("Número económico no especificado", AdaptiveMessageResponseCode.NOT_ACCEPTABLE, nameof(CreateBus), ServiceName);

            if (AcabusDataContext.AllBuses.Any(b => b.EconomicNumber == economicNumber))
                throw new ServiceException("El número económico especificado ya existe", AdaptiveMessageResponseCode.CONFLICT, nameof(CreateBus), ServiceName);

            if (id > 0 && AcabusDataContext.AllBuses.Any(b => b.ID == id))
                throw new ServiceException("Ya existe un autobus con el identificador especificado", AdaptiveMessageResponseCode.CONFLICT, nameof(CreateBus), ServiceName);

            Bus bus = new Bus(id, economicNumber)
            {
                Route = assignedRoute,
                Status = status,
                Type = type
            };

            bool created = AcabusDataContext.DbContext.Create(bus);

            if (!created || bus.ID <= 0)
                throw new ServiceException("No se logró crear la instancia de autobús", nameof(CreateBus), ServiceName);

            args.Data.SetResponse(String.Empty, AdaptiveMessageResponseCode.CREATED);
            args.Data[61] = bus.Serialize();

            ServerNotify.Notify(new PushAcabus(nameof(Bus), bus.ID, LocalSyncOperation.CREATE));
            Dispatcher.SendNotify("CORE: Nuevo autobús agregado " + bus);
        }

        /// <summary>
        /// Crea un equipo.
        /// </summary>
        public void CreateDevice([ParameterField(12)] DeviceType type,
            [ParameterField(17, true)] String serialNumber, [ParameterField(13, true)] UInt16 idStation,
            [ParameterField(36, true)] UInt16 idBus, [ParameterField(18, true)] String ipAddress, [ParameterField(14, true)] UInt32 id, IAdaptiveMessageReceivedArgs args)
        {
            if (idBus > 0 && idStation > 0)
                throw new ServiceException("No es posible asignar a dos ubicaciones un equipo", AdaptiveMessageResponseCode.BAD_REQUEST, nameof(CreateDevice), ServiceName);

            ILocation location = idBus > 0 ? (ILocation)AcabusDataContext.AllBuses.FirstOrDefault(x => x.ID == idBus) :
                idStation > 0 ? AcabusDataContext.AllStations.FirstOrDefault(x => x.ID == idStation) : null;

            if (location == null)
                throw new ServiceException("No se encontró la ubicación (autobús/estación) asignada.", AdaptiveMessageResponseCode.NOT_ACCEPTABLE, nameof(CreateDevice), ServiceName);

            if (!String.IsNullOrEmpty(serialNumber) && AcabusDataContext.AllDevices.FirstOrDefault(d => d.SerialNumber == serialNumber) != null)
                throw new ServiceException("Ya existe un equipo con ese número de serie.", AdaptiveMessageResponseCode.CONFLICT, nameof(CreateDevice), ServiceName);

            if (id > 0 && AcabusDataContext.AllDevices.FirstOrDefault(d => d.ID == id) != null)
                throw new ServiceException("Ya existe un equipo con el identificador especificado", AdaptiveMessageResponseCode.CONFLICT, nameof(CreateDevice), ServiceName);

            IPAddress.TryParse(ipAddress, out IPAddress address);

            Device device = new Device(id, serialNumber, type)
            {
                IPAddress = address ?? IPAddress.Parse("0.0.0.0"),
                Bus = location.GetType() == typeof(Bus) ? (Bus)location : null,
                Station = location.GetType() == typeof(Station) ? (Station)location : null
            };

            bool created = AcabusDataContext.DbContext.Create(device);

            if (!created || device.ID <= 0)
                throw new ServiceException("No se logró crear la instancia del equipo", nameof(CreateDevice), ServiceName);

            args.Data.SetResponse(string.Empty, AdaptiveMessageResponseCode.CREATED);
            args.Data[61] = device.Serialize();

            ServerNotify.Notify(new PushAcabus(nameof(Device), device.ID, LocalSyncOperation.CREATE));
            Dispatcher.SendNotify("CORE: Nuevo equipo agregado " + device);
        }

        /// <summary>
        /// Crea una ruta nueva.
        /// </summary>
        public void CreateRoute([ParameterField(12)] UInt16 number,
            [ParameterField(17)] String name, [ParameterField(13)] RouteType type,
            [ParameterField(18, true)] String assignedSection, [ParameterField(14, true)] UInt32 id, IAdaptiveMessageReceivedArgs args)
        {
            if (number == 0)
                throw new ServiceException("No se especificó el número de ruta", AdaptiveMessageResponseCode.BAD_REQUEST, nameof(CreateRoute), ServiceName);

            if (string.IsNullOrEmpty(name))
                throw new ServiceException("No se especificó el nombre de la ruta", AdaptiveMessageResponseCode.BAD_REQUEST, nameof(CreateRoute), ServiceName);

            if (AcabusDataContext.AllRoutes.FirstOrDefault(r => r.Name == name) != null)
                throw new ServiceException("Ya existe una ruta con el mismo nombre", AdaptiveMessageResponseCode.CONFLICT, nameof(CreateRoute), ServiceName);

            if (AcabusDataContext.AllRoutes.FirstOrDefault(r => r.Type == type && r.RouteNumber == number) != null)
                throw new ServiceException("Ya existe una ruta igual", AdaptiveMessageResponseCode.CONFLICT, nameof(CreateRoute), ServiceName);

            if (id > 0 && AcabusDataContext.AllRoutes.FirstOrDefault(r => r.ID == id) != null)
                throw new ServiceException("Ya existe una ruta con el identificador especificado", AdaptiveMessageResponseCode.CONFLICT, nameof(CreateRoute), ServiceName);

            Route route = new Route(id, number, type) { Name = name, AssignedSection = assignedSection };

            bool created = AcabusDataContext.DbContext.Create(route);

            if (!created || route.ID <= 0)
                throw new ServiceException("No se logró crear la instancia de la ruta", nameof(CreateRoute), ServiceName);

            args.Data.SetResponse(string.Empty, AdaptiveMessageResponseCode.CREATED);
            args.Data[61] = route.Serialize();

            ServerNotify.Notify(new PushAcabus(nameof(Route), route.ID, LocalSyncOperation.CREATE));
            Dispatcher.SendNotify("CORE: Nueva ruta agregada " + route);
        }

        /// <summary>
        /// Crea un personal nuevo.
        /// </summary>
        public void CreateStaff([ParameterField(12)] AssignableArea area,
            [ParameterField(17)] String name, IAdaptiveMessageReceivedArgs args)
        {
            if (string.IsNullOrEmpty(name))
                throw new ServiceException("No se especificó el nombre del empleado", AdaptiveMessageResponseCode.BAD_REQUEST, nameof(CreateStaff), ServiceName);

            Staff staff = new Staff()
            {
                Name = name,
                Area = area
            };

            bool created = AcabusDataContext.DbContext.Create(staff);

            if (!created || staff.ID <= 0)
                throw new ServiceException("No se logró crear la instancia de empleado", nameof(CreateStaff), ServiceName);

            args.Data.SetResponse(string.Empty, AdaptiveMessageResponseCode.CREATED);
            args.Data[61] = staff.Serialize();

            ServerNotify.Notify(new PushAcabus(nameof(Staff), staff.ID, LocalSyncOperation.CREATE));
            Dispatcher.SendNotify("CORE: Nuevo personal agregado " + staff);
        }

        /// <summary>
        /// Crea una estación nueva.
        /// </summary>
        /// <param name="number">Numero de la estación.</param>
        /// <param name="name">Nombre de la estación.</param>
        public void CreateStation([ParameterField(12)] UInt16 number,
            [ParameterField(17)] String name, [ParameterField(18, true)] String assignedSection,
            [ParameterField(13, true)] UInt16 routeID, [ParameterField(14, true)] uint id, [ParameterField(23)] Boolean isExternal,
            IAdaptiveMessageReceivedArgs args)
        {
            if (routeID == 0)
                throw new ServiceException("No se especificó la ruta a la que pertenece la estación", AdaptiveMessageResponseCode.BAD_REQUEST, nameof(CreateStation), ServiceName);

            Route route = AcabusDataContext.AllRoutes.FirstOrDefault(x => x.ID == routeID);

            if (route == null)
                throw new ServiceException("No se encontró la ruta especificada.", AdaptiveMessageResponseCode.NOT_ACCEPTABLE, nameof(CreateStation), ServiceName);

            if (String.IsNullOrEmpty(name))
                throw new ServiceException("No se especificó el nombre de la estación", AdaptiveMessageResponseCode.BAD_REQUEST, nameof(CreateStation), ServiceName);

            if (id > 0 && AcabusDataContext.AllStations.FirstOrDefault(s => s.ID == id) != null)
                throw new ServiceException("Ya existe una estación con el identificador especificado", AdaptiveMessageResponseCode.CONFLICT, nameof(CreateStation), ServiceName);

            Station station = new Station(id, number)
            {
                Name = name,
                AssignedSection = assignedSection,
                IsExternal = isExternal,
                Route = route
            };

            bool created = AcabusDataContext.DbContext.Create(station);

            if (!created || station.ID <= 0)
                throw new ServiceException("No se logró crear la instancia de estación", nameof(CreateStation), ServiceName);

            args.Data.SetResponse(string.Empty, AdaptiveMessageResponseCode.CREATED);
            args.Data[61] = station.Serialize();

            ServerNotify.Notify(new PushAcabus(nameof(Station), station.ID, LocalSyncOperation.CREATE));
            Dispatcher.SendNotify("CORE: Nueva estación agregada " + station);
        }

        /// <summary>
        /// Elimina el autobus especificado por el ID.
        /// </summary>
        /// <param name="id">Identificador del autobus</param>
        /// <param name="args"> Controlador de la petición </param>
        public void DeleteBus([ParameterField(14)] UInt64 id, IAdaptiveMessageReceivedArgs args)
        {
            Bus bus = AcabusDataContext.AllBuses.FirstOrDefault(x => x.ID == id);

            if (bus != null)
            {
                bus.SetAsDeleted();

                bool deleted = AcabusDataContext.DbContext.Update(bus);

                if (!deleted)
                    throw new ServiceException("No se logró eliminar el autobús [NoEconómico=" + bus.EconomicNumber + "]", nameof(DeleteBus), ServiceName);
            }

            ServerNotify.Notify(new PushAcabus(nameof(Bus), id, LocalSyncOperation.DELETE));
            args.Data.SetResponse(String.Empty, AdaptiveMessageResponseCode.OK);
        }

        /// <summary>
        /// Elimina el equipo especificado por el ID.
        /// </summary>
        /// <param name="id">Identificador del equipo.</param>
        /// <param name="args"> Controlador de la petición </param>
        public void DeleteDevice([ParameterField(14)] UInt64 id, IAdaptiveMessageReceivedArgs args)
        {
            Device device = AcabusDataContext.AllDevices.FirstOrDefault(x => x.ID == id);

            if (device != null)
            {
                device.SetAsDeleted();

                bool deleted = AcabusDataContext.DbContext.Update(device);

                if (!deleted)
                    throw new ServiceException(String.Format("No se logró eliminar el equipo [Serie={0}, Tipo={1}]", device.SerialNumber, device.Type.TranslateToSpanish()), nameof(DeleteDevice), ServiceName);
            }

            ServerNotify.Notify(new PushAcabus(nameof(Device), id, LocalSyncOperation.DELETE));
            args.Data.SetResponse(string.Empty, AdaptiveMessageResponseCode.OK);
        }

        /// <summary>
        /// Elimina la ruta especificada por el ID.
        /// </summary>
        /// <param name="id">Identificador de la ruta.</param>
        /// <param name="args"> Controlador de la petición </param>
        public void DeleteRoute([ParameterField(14)] UInt64 id, IAdaptiveMessageReceivedArgs args)
        {
            Route route = AcabusDataContext.AllRoutes.FirstOrDefault(x => x.ID == id);

            if (route != null)
            {
                route.SetAsDeleted();

                bool deleted = AcabusDataContext.DbContext.Update(route);

                if (!deleted)
                    throw new ServiceException(String.Format("No se logró eliminar la ruta [Nombre={0}]", route.Name), nameof(DeleteRoute), ServiceName);
            }

            ServerNotify.Notify(new PushAcabus(nameof(Route), id, LocalSyncOperation.DELETE));
            args.Data.SetResponse(string.Empty, AdaptiveMessageResponseCode.OK);
        }

        /// <summary>
        /// Elimina el personal especificado por el ID, si y solo si no está vinculada con alguna otra entidad.
        /// </summary>
        /// <param name="id">Identificador de la estación.</param>
        /// <param name="args"> Controlador de la petición </param>
        public void DeleteStaff([ParameterField(14)] UInt64 id, IAdaptiveMessageReceivedArgs args)
        {
            Staff staff = AcabusDataContext.AllStaff.FirstOrDefault(x => x.ID == id);

            if (staff != null)
            {
                staff.SetAsDeleted();

                bool deleted = AcabusDataContext.DbContext.Update(staff);

                if (!deleted)
                    throw new ServiceException(String.Format("No se logró eliminar al empleado [Nombre={0}]", staff.Name), nameof(DeleteStaff), ServiceName);
            }

            ServerNotify.Notify(new PushAcabus(nameof(Staff), id, LocalSyncOperation.DELETE));
            args.Data.SetResponse(string.Empty, AdaptiveMessageResponseCode.OK);
        }

        /// <summary>
        /// Elimina la estación especificada por el ID, si y solo si no está vinculada con alguna otra entidad.
        /// </summary>
        /// <param name="id">Identificador de la estación.</param>
        /// <param name="args"> Controlador de la petición </param>
        public void DeleteStation([ParameterField(14)] UInt64 id, IAdaptiveMessageReceivedArgs args)
        {
            Station station = AcabusDataContext.AllStations.FirstOrDefault(x => x.ID == id);

            if (station != null)
            {
                station.SetAsDeleted();

                bool deleted = AcabusDataContext.DbContext.Update(station);

                if (!deleted)
                    throw new ServiceException(String.Format("No se logró eliminar a la estación [Nombre={0}]", station.Name), nameof(DeleteStation), ServiceName);
            }

            ServerNotify.Notify(new PushAcabus(nameof(Station), id, LocalSyncOperation.DELETE));
            args.Data.SetResponse(String.Empty, AdaptiveMessageResponseCode.OK);
        }

        /// <summary>
        /// Obtiene los autobuses.
        /// </summary>
        /// <param name="args"> Controlador de la petición </param>
        public void DownloadBus([ParameterField(25, true)] DateTime? lastDownLoad, IAdaptiveMessageReceivedArgs args)
        {
            if (lastDownLoad == null)
                lastDownLoad = DateTime.MinValue;

            IQueryable<Bus> busQuery = AcabusDataContext.AllBuses.LoadReference(1)
                .Where(x => x.ModifyTime > lastDownLoad)
                .OrderBy(x => x.ModifyTime);

            AdaptiveMessageSocketHelper.SendCollection(busQuery.ToList(), args, (b, message) => message[61] = b.Serialize());
        }

        /// <summary>
        /// Obtiene todos los equipos.
        /// </summary>
        /// <param name="id">Identificador de la estación.</param>
        /// <param name="args"> Controlador de la petición </param>
        public void DownloadDevice([ParameterField(25, true)] DateTime? lastDownLoad, IAdaptiveMessageReceivedArgs args)
        {
            if (lastDownLoad == null)
                lastDownLoad = DateTime.MinValue;

            IQueryable<Device> deviceQuery = AcabusDataContext.AllDevices.LoadReference(1)
                .Where(x => x.ModifyTime > lastDownLoad)
                .OrderBy(x => x.ModifyTime);

            AdaptiveMessageSocketHelper.SendCollection(deviceQuery.ToList(), args, (d, message) => message[61] = d.Serialize());
        }

        /// <summary>
        /// Obtiene las rutas.
        /// </summary>
        /// <param name="args"> Controlador de la petición </param>
        public void DownloadRoute([ParameterField(25, true)] DateTime? lastDownLoad, IAdaptiveMessageReceivedArgs args)
        {
            if (lastDownLoad == null)
                lastDownLoad = DateTime.MinValue;

            IQueryable<Route> routeQuery = AcabusDataContext.AllRoutes
                .Where(x => x.ModifyTime > lastDownLoad)
                .OrderBy(x => x.ModifyTime);

            AdaptiveMessageSocketHelper.SendCollection(routeQuery.ToList(), args, (r, message) => message[61] = r.Serialize());
        }

        /// <summary>
        /// Obtiene el personal.
        /// </summary>
        /// <param name="args"> Controlador de la petición </param>
        public void DownloadStaff([ParameterField(25, true)] DateTime? lastDownLoad, IAdaptiveMessageReceivedArgs args)
        {
            if (lastDownLoad == null)
                lastDownLoad = DateTime.MinValue;

            IQueryable<Staff> staffQuery = AcabusDataContext.AllStaff
                .Where(x => x.ModifyTime > lastDownLoad)
                .OrderBy(x => x.ModifyTime);

            AdaptiveMessageSocketHelper.SendCollection(staffQuery.ToList(), args, (s, message) => message[61] = s.Serialize());
        }

        /// <summary>
        /// Obtiene las estaciones.
        /// </summary>
        /// <param name="args"> Controlador de la petición </param>
        public void DownloadStation([ParameterField(25, true)] DateTime? lastDownLoad, IAdaptiveMessageReceivedArgs args)
        {
            if (lastDownLoad == null)
                lastDownLoad = DateTime.MinValue;

            IQueryable<Station> stationQuery = AcabusDataContext.AllStations.LoadReference(1)
                .Where(x => x.ModifyTime > lastDownLoad)
                .OrderBy(x => x.ModifyTime);

            AdaptiveMessageSocketHelper.SendCollection(stationQuery.ToList(), args, (s, message) => message[61] = s.Serialize());
        }

        /// <summary>
        /// Obtiene el autobus con el ID especificado.
        /// </summary>
        /// <param name="id">Identificador del autobus.</param>
        /// <param name="args"> Controlador de la petición </param>
        public void GetBusByID([ParameterField(14)] UInt64 id, IAdaptiveMessageReceivedArgs args)
        {
            if (id <= 0)
                throw new ServiceException("No se especificó el identificador del autobús a descargar", AdaptiveMessageResponseCode.BAD_REQUEST, nameof(GetBusByID), ServiceName);

            Bus bus = AcabusDataContext.AllBuses.LoadReference(1).FirstOrDefault(x => x.ID == id);

            if (bus == null)
                throw new ServiceException("No existe el autobús especificado", AdaptiveMessageResponseCode.NOT_FOUND, nameof(GetBusByID), ServiceName);

            args.Data.SetResponse(String.Empty, AdaptiveMessageResponseCode.OK);

            args.Data[61] = bus.Serialize();
        }

        /// <summary>
        /// Obtiene el equipo con el ID especificado.
        /// </summary>
        /// <param name="id">Identificador del equipo.</param>
        /// <param name="args"> Controlador de la petición </param>
        public void GetDeviceByID([ParameterField(14)] UInt64 id, IAdaptiveMessageReceivedArgs args)
        {
            if (id <= 0)
                throw new ServiceException("No se especificó el identificador del equipo a descargar", AdaptiveMessageResponseCode.BAD_REQUEST, nameof(GetDeviceByID), ServiceName);

            Device device = AcabusDataContext.AllDevices.LoadReference(1).FirstOrDefault(x => x.ID == id);

            if (device == null)
                throw new ServiceException("No existe el equipo especificado", AdaptiveMessageResponseCode.NOT_FOUND, nameof(GetDeviceByID), ServiceName);

            args.Data.SetResponse(String.Empty, AdaptiveMessageResponseCode.OK);

            args.Data[61] = device.Serialize();
        }

        /// <summary>
        /// Obtiene la ruta con el ID especificado.
        /// </summary>
        /// <param name="id">Identificador de la ruta.</param>
        /// <param name="args"> Controlador de la petición </param>
        public void GetRouteByID([ParameterField(14)] UInt64 id, IAdaptiveMessageReceivedArgs args)
        {
            if (id <= 0)
                throw new ServiceException("No se especificó el identificador de la ruta a descargar", AdaptiveMessageResponseCode.BAD_REQUEST, nameof(GetRouteByID), ServiceName);

            Route route = AcabusDataContext.AllRoutes.FirstOrDefault(x => x.ID == id);

            if (route == null)
                throw new ServiceException("No existe la ruta especificada", AdaptiveMessageResponseCode.NOT_FOUND, nameof(GetRouteByID), ServiceName);

            args.Data.SetResponse(String.Empty, AdaptiveMessageResponseCode.OK);

            args.Data[61] = route.Serialize();
        }

        /// <summary>
        /// Obtiene el personal con el ID especificado.
        /// </summary>
        /// <param name="id">Identificador del personal.</param>
        /// <param name="args"> Controlador de la petición </param>
        public void GetStaffByID([ParameterField(14)] UInt64 id, IAdaptiveMessageReceivedArgs args)
        {
            if (id <= 0)
                throw new ServiceException("No se especificó el identificador del empleado a descargar", AdaptiveMessageResponseCode.BAD_REQUEST, nameof(GetStaffByID), ServiceName);

            Staff staff = AcabusDataContext.AllStaff.LoadReference(1).FirstOrDefault(x => x.ID == id);

            if (staff == null)
                throw new ServiceException("No existe el empleado especificada", AdaptiveMessageResponseCode.NOT_FOUND, nameof(GetStaffByID), ServiceName);

            args.Data.SetResponse(String.Empty, AdaptiveMessageResponseCode.OK);

            args.Data[61] = staff.Serialize();
        }

        /// <summary>
        /// Obtiene la estación con el ID especificado.
        /// </summary>
        /// <param name="id">Identificador de la estación.</param>
        /// <param name="args"> Controlador de la petición </param>
        public void GetStationByID([ParameterField(14)] UInt64 id, IAdaptiveMessageReceivedArgs args)
        {
            if (id <= 0)
                throw new ServiceException("No se especificó el identificador de la estación a descargar", AdaptiveMessageResponseCode.BAD_REQUEST, nameof(GetStationByID), ServiceName);

            Station station = AcabusDataContext.AllStations.LoadReference(1).FirstOrDefault(x => x.ID == id);

            if (station == null)
                throw new ServiceException("No existe la estación especificada", AdaptiveMessageResponseCode.NOT_FOUND, nameof(GetStationByID), ServiceName);

            args.Data.SetResponse(String.Empty, AdaptiveMessageResponseCode.OK);

            args.Data[61] = station.Serialize();
        }

        /// <summary>
        /// Actualiza el autobus especificado por el ID
        /// </summary>
        /// <param name="args"> Controlador de la petición </param>
        public void UpdateBus(IAdaptiveMessageReceivedArgs args)
        {
            Bus bus = DataHelper.GetBus(args.Data.GetBytes(61));

            if (bus == null)
                throw new ServiceException("No se logró deserializar la información del autobús", nameof(UpdateBus), ServiceName);

            bus.Commit();

            bool updated = AcabusDataContext.DbContext.Update(bus);

            if (!updated)
                throw new ServiceException("No se logró actualizar las propiedades del autobús.", nameof(UpdateBus), ServiceName);

            args.Data.SetResponse(string.Empty, AdaptiveMessageResponseCode.OK);

            ServerNotify.Notify(new PushAcabus(nameof(Bus), bus.ID, LocalSyncOperation.UPDATE));
        }

        /// <summary>
        /// Actualiza el equipo especificado por el ID
        /// </summary>
        /// <param name="id">Identificador del equipo.</param>
        /// <param name="args"> Controlador de la petición </param>
        public void UpdateDevice(IAdaptiveMessageReceivedArgs args)
        {
            Device device = DataHelper.GetDevice(args.Data.GetBytes(61));

            if (device == null)
                throw new ServiceException("No se logró deserializar la información del equipo", nameof(UpdateDevice), ServiceName);

            device.Commit();

            bool updated = AcabusDataContext.DbContext.Update(device);

            if (!updated)
                throw new ServiceException("No se logró actualizar las propiedades del equipo.", nameof(UpdateDevice), ServiceName);

            args.Data.SetResponse(string.Empty, AdaptiveMessageResponseCode.OK);

            ServerNotify.Notify(new PushAcabus(nameof(Device), device.ID, LocalSyncOperation.UPDATE));
        }

        /// <summary>
        /// Actualiza la ruta especificada por el ID
        /// </summary>
        /// <param name="args"> Controlador de la petición </param>
        public void UpdateRoute(IAdaptiveMessageReceivedArgs args)
        {
            Route route = DataHelper.GetRoute(args.Data.GetBytes(61));

            if (route == null)
                throw new ServiceException("No se logró deserializar la información de la ruta", nameof(UpdateRoute), ServiceName);

            route.Commit();

            bool updated = AcabusDataContext.DbContext.Update(route);

            if (!updated)
                throw new ServiceException("No se logró actualizar las propiedades de la ruta.", nameof(UpdateRoute), ServiceName);

            args.Data.SetResponse(string.Empty, AdaptiveMessageResponseCode.OK);

            ServerNotify.Notify(new PushAcabus(nameof(Route), route.ID, LocalSyncOperation.UPDATE));
        }

        /// <summary>
        /// Actualiza el personal especificado por el ID
        /// </summary>
        /// <param name="args"> Controlador de la petición </param>
        public void UpdateStaff(IAdaptiveMessageReceivedArgs args)
        {
            Staff staff = DataHelper.GetStaff(args.Data.GetBytes(61));

            if (staff == null)
                throw new ServiceException("No se logró deserializar la información del empleado", nameof(UpdateStaff), ServiceName);

            staff.Commit();

            bool updated = AcabusDataContext.DbContext.Update(staff);

            if (!updated)
                throw new ServiceException("No se logró actualizar las propiedades del empleado.", nameof(UpdateStaff), ServiceName);

            args.Data.SetResponse(string.Empty, AdaptiveMessageResponseCode.OK);

            ServerNotify.Notify(new PushAcabus(nameof(Staff), staff.ID, LocalSyncOperation.UPDATE));
        }

        /// <summary>
        /// Actualiza la estación especificada por el ID
        /// </summary>
        /// <param name="args"> Controlador de la petición </param>
        public void UpdateStation(IAdaptiveMessageReceivedArgs args)
        {
            Station station = DataHelper.GetStation(args.Data.GetBytes(61));

            if (station == null)
                throw new ServiceException("No se logró deserializar la información de la estación", nameof(UpdateStation), ServiceName);

            station.Commit();

            bool updated = AcabusDataContext.DbContext.Update(station);

            if (!updated)
                throw new ServiceException("No se logró actualizar las propiedades de la estación.", nameof(UpdateStation), ServiceName);

            args.Data.SetResponse(string.Empty, AdaptiveMessageResponseCode.OK);

            ServerNotify.Notify(new PushAcabus(nameof(Staff), station.ID, LocalSyncOperation.UPDATE));
        }
    }
}