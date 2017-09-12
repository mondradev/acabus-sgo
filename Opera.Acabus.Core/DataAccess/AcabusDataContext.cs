﻿using InnSyTech.Standard.Configuration;
using InnSyTech.Standard.Database;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using InnSyTech.Standard.Net.Messenger.Iso8583;

namespace Opera.Acabus.Core.DataAccess
{
    /// <summary>
    /// Esta clase provee de datos al nucleo del sistema de ACABUS SGO, así como las conexiones a los
    /// diferentes equipos asociados con la operación. Cada transferencia de datos por red, archivos
    /// o base de datos debe ser gestionada desde aqui para centralizar las comunicaciones.
    /// </summary>
    public static class AcabusDataContext
    {
        /// <summary>
        /// Nombre del directorio de recursos.
        /// </summary>
        private static readonly string RESOURCES_DIRECTORY = Path.Combine(Environment.CurrentDirectory, "Resources");

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ConfigContext" />.
        /// </summary>
        private static Configuration _configContext;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="DbContext" />.
        /// </summary>
        private static IDbSession _dbContext;

        /// <summary>
        /// Constructor estático de <see cref="AcabusDataContext"/>.
        /// </summary>
        static AcabusDataContext()
        {
            if (!Directory.Exists(RESOURCES_DIRECTORY))
                Directory.CreateDirectory(RESOURCES_DIRECTORY);

            _configContext = new Configuration()
            {
                Filename = Path.Combine(RESOURCES_DIRECTORY, "app.conf")
            };
        }

        /// <summary>
        /// Obtiene una lista de autobuses desde la base de datos.
        /// </summary>
        public static IEnumerable<Bus> AllBuses
            => DbContext.Read<Bus>();

        /// <summary>
        /// Obtiene una lista de equipos desde la base de datos.
        /// </summary>
        public static IEnumerable<Device> AllDevices
            => DbContext.Read<Device>();

        /// <summary>
        /// Obtiene una lista de rutas desde la base de datos.
        /// </summary>
        public static IEnumerable<Route> AllRoute
            => DbContext.Read<Route>();

        /// <summary>
        /// Obtiene una lista del personal desde la base de datos.
        /// </summary>
        public static IEnumerable<Staff> AllStaff
            => DbContext.Read<Staff>();

        /// <summary>
        /// Obtiene una lista de estaciones desde la base de datos.
        /// </summary>
        public static IEnumerable<Station> AllStations
            => DbContext.Read<Station>();

        /// <summary>
        /// Obtiene el controlador de las configuraciones.
        /// </summary>
        public static Configuration ConfigContext => _configContext;

        /// <summary>
        /// Obtiene la sesión a la base de datos.
        /// </summary>
        public static IDbSession DbContext => _dbContext;

        /// <summary>
        /// Procesa un mensaje y devuelve el resultado según su contenido.
        /// </summary>
        /// <param name="request">Mensaje que representa la petición.</param>
        /// <returns>El mensaje de respuesta a la petición.</returns>
        public static AppMessage ProcessingRequest(AppMessage request)
        {
            throw new NotImplementedException();
        }
    }
}