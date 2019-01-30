using InnSyTech.Standard.Database.Linq;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Models;
using Opera.Acabus.Core.Models.Base;
using Opera.Acabus.Core.Services;
using Opera.Acabus.Server.Core;
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
            [ParameterField(17)] String economicNumber, [ParameterField(13, true)] UInt16 idRoute,
            [ParameterField(36)] BusStatus status, IMessage message)
        {
            Bus bus = new Bus(0, economicNumber)
            {
                Route = idRoute == 0 ? null : AcabusDataContext.AllRoutes.FirstOrDefault(x => x.ID == idRoute),
                Status = status,
                Type = type
            };

            bool res = AcabusDataContext.DbContext.Create(bus);

            if (res)
                ServerNotify.Notify(new PushAcabus(nameof(Bus), bus.ID, LocalSyncOperation.CREATE));

            message.SetBoolean(22, res);
            message[61] = bus.Serialize();

            if (res)
                Dispatcher.SendNotify("CORE: Nuevo autobús agregado " + bus);
        }

        /// <summary>
        /// Crea un equipo.
        /// </summary>
        public static void CreateDevice([ParameterField(14)] DeviceType type,
            [ParameterField(17, true)] String serialNumber, [ParameterField(13, true)] UInt16 idStation,
            [ParameterField(36, true)] UInt16 idBus, [ParameterField(18, true)] String ipAddress, IMessage message)
        {
            Device device = new Device(0, serialNumber, type)
            {
                IPAddress = IPAddress.Parse(ipAddress),
                Bus = idBus == 0 ? null : AcabusDataContext.AllBuses.FirstOrDefault(x => x.ID == idBus),
                Station = idStation == 0 ? null : AcabusDataContext.AllStations.FirstOrDefault(x => x.ID == idStation)
            };

            bool res = true;

            if (!String.IsNullOrEmpty(device.SerialNumber) && AcabusDataContext.AllDevices.Where(x => x.Type == type)
                .ToList().Any(x => x.SerialNumber.Equals(device.SerialNumber)))
            {
                message[61] = AcabusDataContext.AllDevices.FirstOrDefault(x => x.SerialNumber.Equals(device.SerialNumber)).Serialize();
                message[AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32()] = String.Format("SERVIDOR: El número de serie {0} ya existe", serialNumber);
            }
            else
            {
                res = AcabusDataContext.DbContext.Create(device);

                if (res)
                    ServerNotify.Notify(new PushAcabus(nameof(Device), device.ID, LocalSyncOperation.CREATE));

                message[61] = device.Serialize();
            }

            message.SetBoolean(22, res);

            if (res)
                Dispatcher.SendNotify("CORE: Nuevo dispositivo agregado " + device);
        }

        /// <summary>
        /// Crea una ruta nueva.
        /// </summary>
        /// <param name="number">Numero de la ruta.</param>
        /// <param name="name">Nombre de la ruta.</param>
        /// <param name="type">Tipo de la ruta. ALIM = 1 o TRUNK = 2.</param>
        public static void CreateRoute([ParameterField(12)] UInt16 number,
            [ParameterField(17)] String name, [ParameterField(13)] RouteType type,
            [ParameterField(18, true)] String assignedSection, IMessage message)
        {
            Route route = new Route(0, number, type) { Name = name, AssignedSection = assignedSection };
            bool res = AcabusDataContext.DbContext.Create(route);

            if (res)
                ServerNotify.Notify(new PushAcabus(nameof(Route), route.ID, LocalSyncOperation.CREATE));

            message.SetBoolean(22, res);
            message[61] = route.Serialize();

            if (res)
                Dispatcher.SendNotify("CORE: Nueva ruta agregada " + route);
        }

        /// <summary>
        /// Crea un personal nuevo.
        /// </summary>
        public static void CreateStaff([ParameterField(12)] AssignableArea area,
            [ParameterField(17)] String name, IMessage message)
        {
            Staff staff = new Staff()
            {
                Name = name,
                Area = area
            };

            bool res = AcabusDataContext.DbContext.Create(staff);

            if (res)
                ServerNotify.Notify(new PushAcabus(nameof(Staff), staff.ID, LocalSyncOperation.CREATE));

            message.SetBoolean(22, res);
            message[61] = staff.Serialize();

            if (res)
                Dispatcher.SendNotify("CORE: Nuevo personal agregado " + staff);
        }

        /// <summary>
        /// Crea una estación nueva.
        /// </summary>
        /// <param name="number">Numero de la estación.</param>
        /// <param name="name">Nombre de la estación.</param>
        public static void CreateStation([ParameterField(12)] UInt16 number,
            [ParameterField(17)] String name, [ParameterField(18, true)] String assignedSection,
            [ParameterField(13, true)] UInt16 routeID, [ParameterField(23)] Boolean isExternal,
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

            if (res)
                ServerNotify.Notify(new PushAcabus(nameof(Station), station.ID, LocalSyncOperation.CREATE));

            message.SetBoolean(22, res);
            message[61] = station.Serialize();

            if (res)
                Dispatcher.SendNotify("CORE: Nueva estación agregada " + station);
        }

        /// <summary>
        /// Elimina el autobus especificado por el ID, si y solo si no está vinculada con alguna otra entidad.
        /// </summary>
        /// <param name="id">Identificador del autobus</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void DeleteBus([ParameterField(14)] UInt64 id, IMessage message)
        {
            try
            {
                Bus bus = AcabusDataContext.AllBuses.FirstOrDefault(x => x.ID == id);
                bus.Delete();

                bool deleted = AcabusDataContext.DbContext.Update(bus);

                if (!deleted)
                    throw new InvalidOperationException("Existen entidades vinculadas al autobus que intenta eliminar.");

                message.SetBoolean(22, deleted);

                if (deleted)
                    ServerNotify.Notify(new PushAcabus(nameof(Bus), id, LocalSyncOperation.DELETE));
            }
            catch (Exception ex)
            {
                message[AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()] = 400;
                message[AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32()] = ex.Message;
                message.SetBoolean(22, false);
            }
        }

        /// <summary>
        /// Elimina el equipo especificado por el ID, si y solo si no está vinculada con alguna otra entidad.
        /// </summary>
        /// <param name="id">Identificador del equipo.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void DeleteDevice([ParameterField(14)] UInt64 id, IMessage message)
        {
            try
            {
                Device device = AcabusDataContext.AllDevices.FirstOrDefault(x => x.ID == id);
                device.Delete();

                bool deleted = AcabusDataContext.DbContext.Update(device);

                if (!deleted)
                    throw new InvalidOperationException("Existen entidades vinculadas al equipo que intenta eliminar.");

                message.SetBoolean(22, deleted);

                if (deleted)
                    ServerNotify.Notify(new PushAcabus(nameof(Device), id, LocalSyncOperation.DELETE));
            }
            catch (Exception ex)
            {
                message[AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()] = 400;
                message[AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32()] = ex.Message;
                message.SetBoolean(22, false);
            }
        }

        /// <summary>
        /// Elimina la ruta especificada por el ID, si y solo si no está vinculada con alguna otra entidad.
        /// </summary>
        /// <param name="id">Identificador de la ruta.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void DeleteRoute([ParameterField(14)] UInt64 id, IMessage message)
        {
            try
            {
                Route route = AcabusDataContext.AllRoutes.FirstOrDefault(x => x.ID == id);
                route.Delete();

                bool deleted = AcabusDataContext.DbContext.Update(route);

                if (!deleted)
                    throw new InvalidOperationException("Existen entidades vinculadas a la ruta que intenta eliminar.");

                message.SetBoolean(22, deleted);

                if (deleted)
                    ServerNotify.Notify(new PushAcabus(nameof(Route), id, LocalSyncOperation.DELETE));
            }
            catch (Exception ex)
            {
                message[AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()] = 400;
                message[AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32()] = ex.Message;
                message.SetBoolean(22, false);
            }
        }

        /// <summary>
        /// Elimina el personal especificado por el ID, si y solo si no está vinculada con alguna otra entidad.
        /// </summary>
        /// <param name="id">Identificador de la estación.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void DeleteStaff([ParameterField(14)] UInt64 id, IMessage message)
        {
            try
            {
                Staff staff = AcabusDataContext.AllStaff.FirstOrDefault(x => x.ID == id);
                staff.Delete();

                bool deleted = AcabusDataContext.DbContext.Update(staff);

                if (!deleted)
                    throw new InvalidOperationException("Existen entidades vinculadas al personal que intenta eliminar.");

                message.SetBoolean(22, deleted);

                if (deleted)
                    ServerNotify.Notify(new PushAcabus(nameof(Staff), id, LocalSyncOperation.DELETE));
            }
            catch (Exception ex)
            {
                message[AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()] = 400;
                message[AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32()] = ex.Message;
                message.SetBoolean(22, false);
            }
        }

        /// <summary>
        /// Elimina la estación especificada por el ID, si y solo si no está vinculada con alguna otra entidad.
        /// </summary>
        /// <param name="id">Identificador de la estación.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void DeleteStation([ParameterField(14)] UInt64 id, IMessage message)
        {
            try
            {
                Station station = AcabusDataContext.AllStations.FirstOrDefault(x => x.ID == id);
                station.Delete();

                bool deleted = AcabusDataContext.DbContext.Update(station);

                if (!deleted)
                    throw new InvalidOperationException("Existen entidades vinculadas a la estación que intenta eliminar.");

                message.SetBoolean(22, deleted);

                if (deleted)
                    ServerNotify.Notify(new PushAcabus(nameof(Station), id, LocalSyncOperation.DELETE));
            }
            catch (Exception ex)
            {
                message[AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()] = 400;
                message[AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32()] = ex.Message;
                message.SetBoolean(22, false);
            }
        }

        /// <summary>
        /// Obtiene los autobuses.
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void DownloadBus([ParameterField(25, true)] DateTime? lastDownLoad, IMessage message)
        {
            if (lastDownLoad == null)
                lastDownLoad = DateTime.MinValue;

            IQueryable<Bus> busQuery = AcabusDataContext.AllBuses.LoadReference(1)
                .Where(x => x.ModifyTime > lastDownLoad)
                .OrderBy(x => x.ModifyTime);

            ServerHelper.Enumerating(message, busQuery.ToList().Count,
                  i => message[61] = busQuery.ToList()[i].Serialize());
        }

        /// <summary>
        /// Obtiene todos los equipos.
        /// </summary>
        /// <param name="id">Identificador de la estación.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void DownloadDevice([ParameterField(25, true)] DateTime? lastDownLoad, IMessage message)
        {
            if (lastDownLoad == null)
                lastDownLoad = DateTime.MinValue;

            IQueryable<Device> devices = AcabusDataContext.AllDevices.LoadReference(1)
                .Where(x => x.ModifyTime > lastDownLoad)
                .OrderBy(x => x.ModifyTime);

            ServerHelper.Enumerating(message, devices.ToList().Count,
                i => message[61] = devices.ToList()[i].Serialize());
        }

        /// <summary>
        /// Obtiene las rutas.
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void DownloadRoute([ParameterField(25, true)] DateTime? lastDownLoad, IMessage message)
        {
            if (lastDownLoad == null)
                lastDownLoad = DateTime.MinValue;

            IQueryable<Route> routeQuery = AcabusDataContext.AllRoutes
                .Where(x => x.ModifyTime > lastDownLoad)
                .OrderBy(x => x.ModifyTime);

            ServerHelper.Enumerating(message, routeQuery.ToList().Count,
                  i => message[61] = routeQuery.ToList()[i].Serialize());
        }

        /// <summary>
        /// Obtiene el personal.
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void DownloadStaff([ParameterField(25, true)] DateTime? lastDownLoad, IMessage message)
        {
            if (lastDownLoad == null)
                lastDownLoad = DateTime.MinValue;

            IQueryable<Staff> staffQuery = AcabusDataContext.AllStaff
                .Where(x => x.ModifyTime > lastDownLoad)
                .OrderBy(x => x.ModifyTime);

            ServerHelper.Enumerating(message, staffQuery.ToList().Count,
                  i => message[61] = staffQuery.ToList()[i].Serialize());
        }

        /// <summary>
        /// Obtiene las estaciones.
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void DownloadStation([ParameterField(25, true)] DateTime? lastDownLoad, IMessage message)
        {
            /***
                7, FieldType.Binary, 1, false, "Es enumerable"
                8, FieldType.Numeric, 100, true, "Registros totales del enumerable"
                9, FieldType.Numeric, 100, true, "Posición del enumerable"
                10, FieldType.Numeric, 1, false, "Operaciones del enumerable (Siguiente|Inicio)"
             */

            if (lastDownLoad == null)
                lastDownLoad = DateTime.MinValue;

            IQueryable<Station> stationsQuery = AcabusDataContext.AllStations.LoadReference(1)
                .Where(x => x.ModifyTime > lastDownLoad)
                .OrderBy(x => x.ModifyTime);

            ServerHelper.Enumerating(message, stationsQuery.ToList().Count,
                i => message[61] = stationsQuery.ToList()[i].Serialize());
        }

        /// <summary>
        /// Obtiene el autobus con el ID especificado.
        /// </summary>
        /// <param name="id">Identificador del autobus.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetBusByID([ParameterField(14)] UInt64 id, IMessage message)
        {
            Bus bus = AcabusDataContext.AllBuses.LoadReference(1).FirstOrDefault(x => x.ID == id);
            message[61] = bus.Serialize();
        }

        /// <summary>
        /// Obtiene el equipo con el ID especificado.
        /// </summary>
        /// <param name="id">Identificador del equipo.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetDeviceByID([ParameterField(14)] UInt64 id, IMessage message)
        {
            Device device = AcabusDataContext.AllDevices.LoadReference(1).FirstOrDefault(x => x.ID == id);
            message[61] = device.Serialize();
        }

        /// <summary>
        /// Obtiene la ruta con el ID especificado.
        /// </summary>
        /// <param name="id">Identificador de la ruta.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetRouteByID([ParameterField(14)] UInt64 id, IMessage message)
        {
            Route route = AcabusDataContext.AllRoutes.FirstOrDefault(x => x.ID == id);
            message[61] = route.Serialize();
        }

        /// <summary>
        /// Obtiene el personal con el ID especificado.
        /// </summary>
        /// <param name="id">Identificador del personal.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetStaffByID([ParameterField(14)] UInt64 id, IMessage message)
        {
            Staff staff = AcabusDataContext.AllStaff.LoadReference(1).FirstOrDefault(x => x.ID == id);
            message[61] = staff.Serialize();
        }

        /// <summary>
        /// Obtiene la estación con el ID especificado.
        /// </summary>
        /// <param name="id">Identificador de la estación.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetStationByID([ParameterField(14)] UInt64 id, IMessage message)
        {
            Station station = AcabusDataContext.AllStations.LoadReference(1).FirstOrDefault(x => x.ID == id);
            message[61] = station.Serialize();
        }

        /// <summary>
        /// Actualiza el autobus especificado por el ID
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void UpdateBus( IMessage message)
        {
            try
            {
                Bus bus = DataHelper.GetBus(message.GetBytes(61));
                bus.Commit();

                bool updated = AcabusDataContext.DbContext.Update(bus);

                if (!updated)
                    throw new InvalidOperationException("No se logró actualizar el autobus, verifique las relaciones.");

                message.SetBoolean(22, updated);

                if (updated)
                    ServerNotify.Notify(new PushAcabus(nameof(Bus), bus.ID, LocalSyncOperation.UPDATE));
            }
            catch (Exception ex)
            {
                message[AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()] = 400;
                message[AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32()] = ex.Message;
                message.SetBoolean(22, false);
            }
        }

        /// <summary>
        /// Actualiza el equipo especificado por el ID
        /// </summary>
        /// <param name="id">Identificador del equipo.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void UpdateDevice(IMessage message)
        {
            try
            {
                Device device =  DataHelper.GetDevice(message.GetBytes(61));
                device.Commit();

                bool updated = AcabusDataContext.DbContext.Update(device);

                if (!updated)
                    throw new InvalidOperationException("No se logró actualizar el equipo, verifique las relaciones.");

                message.SetBoolean(22, updated);

                if (updated)
                    ServerNotify.Notify(new PushAcabus(nameof(Device), device.ID, LocalSyncOperation.UPDATE));
            }
            catch (Exception ex)
            {
                message[AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()] = 400;
                message[AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32()] = ex.Message;
                message.SetBoolean(22, false);
            }
        }

        /// <summary>
        /// Actualiza la ruta especificada por el ID
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void UpdateRoute(IMessage message)
        {
            try
            {
                Route route = DataHelper.GetRoute(message.GetBytes(61));
                route.Commit();

                bool updated = AcabusDataContext.DbContext.Update(route);

                if (!updated)
                    throw new InvalidOperationException("No se logró actualizar la ruta, verifique las relaciones.");

                message.SetBoolean(22, updated);

                if (updated)
                    ServerNotify.Notify(new PushAcabus(nameof(Route), route.ID, LocalSyncOperation.UPDATE));
            }
            catch (Exception ex)
            {
                message[AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()] = 400;
                message[AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32()] = ex.Message;
                message.SetBoolean(22, false);
            }
        }

        /// <summary>
        /// Actualiza el personal especificado por el ID
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void UpdateStaff(IMessage message)
        {
            try
            {
                Staff staff = DataHelper.GetStaff(message.GetBytes(61));
                staff.Commit();

                bool updated = AcabusDataContext.DbContext.Update(staff);

                if (!updated)
                    throw new InvalidOperationException("No se logró actualizar el personal, verifique las relaciones.");

                message.SetBoolean(22, updated);

                if (updated)
                    ServerNotify.Notify(new PushAcabus(nameof(Staff), staff.ID, LocalSyncOperation.UPDATE));
            }
            catch (Exception ex)
            {
                message[AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()] = 400;
                message[AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32()] = ex.Message;
                message.SetBoolean(22, false);
            }
        }

        /// <summary>
        /// Actualiza la estación especificada por el ID
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void UpdateStation(IMessage message)
        {
            try
            {
                Station station = DataHelper.GetStation(message.GetBytes(61));
                station.Commit();

                bool updated = AcabusDataContext.DbContext.Update(station);

                if (!updated)
                    throw new InvalidOperationException("No se logró actualizar la estación, verifique las relaciones.");

                message.SetBoolean(22, updated);

                if (updated)
                    ServerNotify.Notify(new PushAcabus(nameof(Station), station.ID, LocalSyncOperation.UPDATE));
            }
            catch (Exception ex)
            {
                message[AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()] = 400;
                message[AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32()] = ex.Message;
                message.SetBoolean(22, false);
            }
        }
    }
}