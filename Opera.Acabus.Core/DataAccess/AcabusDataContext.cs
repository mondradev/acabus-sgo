using InnSyTech.Standard.Configuration;
using InnSyTech.Standard.Database;
using InnSyTech.Standard.Utils;
using Opera.Acabus.Core.Models;
using Opera.Acabus.Core.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        /// Campo que provee a la propiedad <see cref="ModulesLoaded" />.
        /// </summary>
        private static List<IModuleInfo> _modulesLoaded;

        /// <summary>
        /// Constructor estático de <see cref="AcabusDataContext"/>.
        /// </summary>
        static AcabusDataContext()
        {
            _resourcesDirectory = Path.Combine(Environment.CurrentDirectory, "Resources");

            string[] commands = Environment.GetCommandLineArgs();
            string configFile = Path.Combine(_resourcesDirectory, "app.conf");

            if (commands.Length > 0)
            {
                var enumerator = commands.GetEnumerator();
                enumerator.Reset();

                while (enumerator.MoveNext())
                    if (enumerator.Current.Equals("--config") && enumerator.MoveNext())
                    {
                        configFile = enumerator.Current.ToString();
                        break;
                    }
            }

            if (!Directory.Exists(_resourcesDirectory))
                Directory.CreateDirectory(_resourcesDirectory);

            ConfigContext = new Configuration()
            {
                Filename = configFile
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
                if (!typeof(DbDialectBase).IsAssignableFrom(dialectType))
                    throw new InvalidOperationException($"El tipo '{dialectType.FullName}' no deriva de '{typeof(DbDialectBase).FullName}'");

                DbContext = DbFactory.CreateSession(
                    dbType,
                    (DbDialectBase)Activator.CreateInstance(dialectType, ConfigContext["connectionDb"]?["connectionString"]?.ToString())
                );
            }

            AssignableSection = ConfigContext["AssignableSections"]
                .GetSettings("AssignableSection").Select(x => x.ToString("Description"));

            ServerContext.Init();
        }

        /// <summary>
        /// Obtiene una lista de autobuses desde la base de datos.
        /// </summary>
        public static IQueryable<Bus> AllBuses
            => DbContext?.Read<Bus>().Where(x => x.Active);

        /// <summary>
        /// Obtiene una lista de equipos desde la base de datos.
        /// </summary>
        public static IQueryable<Device> AllDevices
            => DbContext?.Read<Device>().Where(x => x.Active);

        /// <summary>
        /// Obtiene una lista de rutas desde la base de datos.
        /// </summary>
        public static IQueryable<Route> AllRoutes
            => DbContext?.Read<Route>().Where(x => x.Active);

        /// <summary>
        /// Obtiene una lista del personal desde la base de datos.
        /// </summary>
        public static IQueryable<Staff> AllStaff
            => DbContext?.Read<Staff>().Where(x => x.Active);

        /// <summary>
        /// Obtiene una lista de estaciones desde la base de datos.
        /// </summary>
        public static IQueryable<Station> AllStations
            => DbContext?.Read<Station>().Where(x => x.Active);

        /// <summary>
        /// Obtiene una lista de las secciones que pueden ser asignadas para dar mantenimiento o servicio.
        /// </summary>
        public static IEnumerable<string> AssignableSection { get; }

        /// <summary>
        /// Obtiene el controlador de las configuraciones.
        /// </summary>
        public static Configuration ConfigContext { get; private set; }

        /// <summary>
        /// Obtiene la sesión a la base de datos.
        /// </summary>
        public static IDbSession DbContext { get; private set; }

        /// <summary>
        /// Obtiene una lista de los módulos cargados en el sistema.
        /// </summary>
        public static List<IModuleInfo> ModulesLoaded
            => _modulesLoaded ?? (_modulesLoaded = new List<IModuleInfo>());

        /// <summary>
        /// Obtiene el módulo cargado previamente en el sistema.
        /// </summary>
        /// <param name="moduleCodeName">Nombre del módulo cargado.</param>
        /// <param name="service">Servicio obtenido de la consulta.</param>
        /// <returns>Un valor true si el servicio está cargado.</returns>
        public static bool GetService(String moduleCodeName, out dynamic service)
        {
            var serviceModule = _modulesLoaded.FirstOrDefault(m => m.CodeName.Equals(moduleCodeName));
            service = serviceModule;

            return service != null;
        }
    }
}