using InnSyTech.Standard.Net.Notifications.Push;
using Opera.Acabus.Core.Services;
using Opera.Acabus.Core.Services.ModelServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Opera.Acabus.Core.DataAccess
{
    /// <summary>
    /// Proporciona las funciones para comunicarse con el núcleo del servidor.
    /// </summary>
    public static class ServerContext
    {
        /// <summary>
        /// Referencia para el bloqueo del hilo.
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// Almacena todos los sincronizadores.
        /// </summary>
        private static readonly Dictionary<String, LocalSyncStatus> _entityLocalSyncs;

        /// <summary>
        ///
        /// </summary>
        private static readonly PushService<PushAcabus> _pushService;

        /// <summary>
        /// Inicializador estático de la clase.
        /// </summary>
        static ServerContext()
        {
            int serverPort = (Int32)(AcabusDataContext.ConfigContext["Server"]?.ToInteger("Push_Port") ?? 5501);
            IPAddress serverIP = IPAddress.Parse(AcabusDataContext.ConfigContext["Server"]?.ToString("IP") ?? "127.0.0.1");

            _entityLocalSyncs = new Dictionary<string, LocalSyncStatus>();

            if (IsServer())
                return;

            _pushService = new PushService<PushAcabus>(serverIP, serverPort);

            _pushService.Notified += OnNotify;
        }

        /// <summary>
        /// Identifica si las peticiones son a un nodo remoto o a si mismo, para obtener la IP adecuada.
        /// </summary>
        /// <param name="ipAddress">Dirección IP a evaluar</param>
        /// <returns>La IP más adecuada para las peticiones.</returns>
        public static IPAddress GetAddress(IPAddress ipAddress)
        {
            var hostName = Dns.GetHostName();
            var ipHost = Dns.GetHostEntry(hostName);
            var listIP = ipHost.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork);

            if (listIP.Any(x => x.Equals(ipAddress)))
                return IPAddress.Loopback;

            return ipAddress;
        }

        /// <summary>
        /// Obtiene el monitor de sincronización de la entidad especificada.
        /// </summary>
        /// <param name="entityName">Nombre de la entidad.</param>
        /// <returns>El monitor de sincronización.</returns>
        public static IEntityLocalSync GetLocalSync(String entityName)
            => _entityLocalSyncs[entityName].Entity ?? throw new ArgumentException($"No existe el monitor de sincronización [Entidad={entityName}]");

        /// <summary>
        /// Inicializa los servicios con el servidor.
        /// </summary>
        public static void Init()
        {
            RegisterLocalSync(new RouteLocalSync());
            RegisterLocalSync(new BusLocalSync());
            RegisterLocalSync(new StationLocalSync());
            RegisterLocalSync(new DeviceLocalSync());
            RegisterLocalSync(new StaffLocalSync());
        }

        /// <summary>
        /// Registra un monitor de sincronización para recibir las actualizaciones de su entidad.
        /// </summary>
        /// <param name="localSync">Instancia del monitor.</param>
        public static void RegisterLocalSync(IEntityLocalSync localSync)
        {
            if (_entityLocalSyncs.ContainsKey(localSync.EntityName))
                throw new InvalidOperationException($"Ya existe la instancia IEntityLocalSync [Entidad={localSync.EntityName}]");

            _entityLocalSyncs.Add(localSync.EntityName,
                new LocalSyncStatus(localSync)
                {
                    IsSyncronized = false
                });

            Task.Run(() =>
            {
                var monitor = _entityLocalSyncs[localSync.EntityName];

                while (!monitor.IsSyncronized)
                {
                    lock (_lock)
                    {
                        try
                        {
                            foreach (String entityName in localSync.Dependencies)
                            {
                                while (true)
                                {
                                    LocalSyncStatus localSyncsStatus = _entityLocalSyncs[entityName];
                                    IEntityLocalSync dependency = localSyncsStatus.Entity;

                                    if (dependency == null || !localSyncsStatus.IsSyncronized)
                                        Monitor.Wait(_lock);
                                    else break;
                                }
                            }

                            localSync.Pull();

                            _entityLocalSyncs[localSync.EntityName].IsSyncronized = true;

                            Monitor.Pulse(_lock);
                        }
                        catch (Exception reason)
                        {
                            Trace.TraceWarning($"Fallo al sincronizar [Entidad={localSync.EntityName}, Razón={reason.Message}]");
                            Thread.Sleep(10000);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Indentifica si el equipo actual es el mismo a donde se envían las peticiones remotas.
        /// </summary>
        /// <returns>Un valor de true si el servidor y el cliente son los mismos.</returns>
        public static bool IsServer()
        {
            var server = IPAddress.Parse(AcabusDataContext.ConfigContext["Server"]?.ToString("IP") ?? "127.0.0.1");
            var hostName = Dns.GetHostName();
            var ipHost = Dns.GetHostEntry(hostName);
            var listIP = ipHost.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork);

            listIP = listIP.Concat(new IPAddress[] { IPAddress.IPv6Loopback, IPAddress.Loopback });

            if (listIP.Any(x => x.Equals(server)))
                return true;

            return false;
        }

        /// <summary>
        /// Se invoca cuando el evento <see cref="PushService{T}.Notified"/> se desencadena.
        /// </summary>
        /// <param name="args">Argumentos del evento.</param>
        private static void OnNotify(PushArgs args)
        {
            PushAcabus push = args.Data as PushAcabus;

            try
            {
                IEntityLocalSync localSync = GetLocalSync(push.EntityName);

                switch (push.Operation)
                {
                    case LocalSyncOperation.CREATE:
                    case LocalSyncOperation.UPDATE:
                        localSync.DownloadByID(push.ID);
                        break;

                    case LocalSyncOperation.DELETE:
                        localSync.LocalDeleteByID(push.ID);
                        break;
                }
            }
            catch (Exception reason)
            {
                Trace.TraceError($"Fallo al notificar cambio [Entidad={push.EntityName}, Operación={push.Operation}, Razón={reason.Message}]");
            }
        }

        /// <summary>
        /// Estructura que permite especificar el estado de la sincronización de los datos para una entidad.
        /// </summary>
        private class LocalSyncStatus
        {
            /// <summary>
            /// Obtiene la entidad en sincronización
            /// </summary>
            public IEntityLocalSync Entity { get; }

            /// <summary>
            /// Indica si la entidad finalizó la sincronización.
            /// </summary>
            public bool IsSyncronized { get; set; }

            /// <summary>
            /// Crea una nueva instancia de estado.
            /// </summary>
            /// <param name="entity">Entidad a sincronizar.</param>
            public LocalSyncStatus(IEntityLocalSync entity)
            {
                Entity = entity;
            }
        }
    }
}