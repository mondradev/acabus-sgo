using InnSyTech.Standard.Configuration;
using InnSyTech.Standard.Database;
using InnSyTech.Standard.Net.Messenger.Iso8583;
using InnSyTech.Standard.Utils;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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
        private static readonly string _resourcesDirectory;

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
            _resourcesDirectory = Path.Combine(Environment.CurrentDirectory, "Resources");

            if (!Directory.Exists(_resourcesDirectory))
                Directory.CreateDirectory(_resourcesDirectory);

            _configContext = new Configuration()
            {
                Filename = Path.Combine(_resourcesDirectory, "app.conf")
            };

            Type dbType = TypeHelper.LoadFromDll(
                ConfigContext["connectionDb"]?["assembly"]?.ToString(),
                ConfigContext["connectionDb"]?["type"]?.ToString()
            );

            Type dialectType = TypeHelper.LoadFromDll(
               ConfigContext["connectionDb"]?["assemblyDialect"]?.ToString(),
               ConfigContext["connectionDb"]?["typeDialect"]?.ToString()
            );


            if (dialectType != null && dbType != null)
            {
                if (!dialectType.IsAssignableFrom(typeof(DbDialectBase)))
                    throw new InvalidOperationException($"El tipo '{dialectType.FullName}' no deriva de '{typeof(DbDialectBase).FullName}'");

                _dbContext = DbFactory.CreateSession(
                    dbType,
                    (DbDialectBase)Activator.CreateInstance(dialectType, ConfigContext["connectionDb"]?["connectionString"]?.ToString())
                );
            }
        }

        /// <summary>
        /// Obtiene una lista de autobuses desde la base de datos.
        /// </summary>
        public static IEnumerable<Bus> AllBuses
            => DbContext?.Read<Bus>();

        /// <summary>
        /// Obtiene una lista de equipos desde la base de datos.
        /// </summary>
        public static IEnumerable<Device> AllDevices
            => DbContext?.Read<Device>();

        /// <summary>
        /// Obtiene una lista de rutas desde la base de datos.
        /// </summary>
        public static IEnumerable<Route> AllRoute
            => DbContext?.Read<Route>();

        /// <summary>
        /// Obtiene una lista del personal desde la base de datos.
        /// </summary>
        public static IEnumerable<Staff> AllStaff
            => DbContext?.Read<Staff>();

        /// <summary>
        /// Obtiene una lista de estaciones desde la base de datos.
        /// </summary>
        public static IEnumerable<Station> AllStations
            => DbContext?.Read<Station>();

        /// <summary>
        /// Obtiene el controlador de las configuraciones.
        /// </summary>
        public static Configuration ConfigContext => _configContext;

        /// <summary>
        /// Obtiene la sesión a la base de datos.
        /// </summary>
        public static IDbSession DbContext => _dbContext;
        
    }
}