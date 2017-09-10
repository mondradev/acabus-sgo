using InnSyTech.Standard.Database;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
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
        private static readonly string RESOURCES_DIRECTORY = Path.Combine(Environment.CurrentDirectory, "Resources");

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
        /// Obtiene la sesión a la base de datos.
        /// </summary>
        public static IDbSession DbContext => _dbContext;
    }
}