﻿using InnSyTech.Standard.Net.Notifications.Push;
using Opera.Acabus.Core.Services;
using Opera.Acabus.Core.Services.ModelServices;
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
        /// Almacena todos los sincronizadores.
        /// </summary>
        private static readonly Dictionary<String, IEntityLocalSync> _entityLocalSyncs;

        /// <summary>
        ///
        /// </summary>
        private static readonly PushService<PushAcabus> _pushService;

        /// <summary>
        /// Inicializador estático de la clase.
        /// </summary>
        static ServerContext()
        {
            _entityLocalSyncs = new Dictionary<string, IEntityLocalSync>();
            _pushService = new PushService<PushAcabus>();

            _pushService.Notified += OnNotify;

            RegisterLocalSync(new RouteLocalSync());
            RegisterLocalSync(new BusLocalSync());
            RegisterLocalSync(new StationLocalSync());
            RegisterLocalSync(new DeviceLocalSync());
            RegisterLocalSync(new StaffLocalSync());
        }

        /// <summary>
        /// Obtiene el monitor de sincronización de la entidad especificada.
        /// </summary>
        /// <param name="entityName">Nombre de la entidad.</param>
        /// <returns>El monitor de sincronización.</returns>
        public static IEntityLocalSync GetLocalSync(String entityName)
            => _entityLocalSyncs[entityName] ?? throw new ArgumentException("No existe el monitor de sincronización.");

        /// <summary>
        /// Registra un monitor de sincronización para recibir las actualizaciones de su entidad.
        /// </summary>
        /// <param name="localSync">Instancia del monitor.</param>
        public static void RegisterLocalSync(IEntityLocalSync localSync)
        {
            if (_entityLocalSyncs.ContainsKey(localSync.EntityName))
                throw new InvalidOperationException("Ya existe un IEntityLocalSync para la entidad '" + localSync.EntityName + "'.");

            _entityLocalSyncs.Add(localSync.EntityName, localSync);
        }

        /// <summary>
        /// Se invoca cuando el evento <see cref="PushService{T}.Notified"/> se desencadena.
        /// </summary>
        /// <param name="args">Argumentos del evento.</param>
        private static void OnNotify(PushArgs args)
        {
            PushAcabus push = args.Data as PushAcabus;
            IEntityLocalSync localSync = _entityLocalSyncs[push.EntityName];

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
    }
}