using InnSyTech.Standard.Configuration;
using InnSyTech.Standard.Database;
using InnSyTech.Standard.Net.Messenger.Iso8583;
using InnSyTech.Standard.SecureShell;
using InnSyTech.Standard.Utils;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Opera.Acabus.Core.DataAccess
{
    /// <summary>
    /// Esta clase provee de datos al nucleo del sistema de ACABUS SGO, así como las conexiones a los
    /// diferentes equipos asociados con la operación. Cada transferencia de datos por red, archivos
    /// o base de datos debe ser gestionada desde aqui para centralizar las comunicaciones.También
    /// ofrece funciones para realizar la comunicación con la ventana principal.
    /// </summary>
    public static class AcabusData
    {
        /// <summary>
        /// Nombre del directorio de recursos.
        /// </summary>
        private static readonly string RESOURCES_DIRECTORY;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="AllRoutes"/>.
        /// </summary>
        private static IEnumerable<Route> _allRoutes;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="CC"/>.
        /// </summary>
        private static Station _cc;

        /// <summary>
        /// Define el archivo de configuración de la aplicación.
        /// </summary>
        private static readonly string CONFIG_FILENAME;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ITStaff"/>.
        /// </summary>
        private static IEnumerable<ITStaff> _itStaff;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Session" />.
        /// </summary>
        private static DbSession _session;

        /// <summary>
        /// Constructor estático de <see cref="AcabusData"/>.
        /// </summary>
        static AcabusData()
        {
            CONFIG_FILENAME = Path.Combine(Environment.CurrentDirectory, "acabus.config");

            ConfigurationManager.SetConfigurationFile(CONFIG_FILENAME);
            Message.SetTemplate(CONFIG_FILENAME);

            RESOURCES_DIRECTORY = Path.Combine(Environment.CurrentDirectory,
                ConfigurationManager.Settings["Directory", "Resources"].ToString());

            if (!Directory.Exists(RESOURCES_DIRECTORY))
                Directory.CreateDirectory(RESOURCES_DIRECTORY);

            Assembly assemblyFile = Assembly.LoadFrom(ConfigurationManager.Settings["assembly", "LocalDb"].ToString());

            _session = DbManager.CreateSession(
                assemblyFile.GetType(ConfigurationManager.Settings["type", "LocalDb"].ToString()),
                new DbConfiguration()
                );

            LoadFromDatabase();
        }

        /// <summary>
        /// Obtiene una lista de todos los vehículos registrados en la base de datos.
        /// </summary>
        public static IEnumerable<Bus> AllBuses
            => AllRoutes.Select(route => route.Buses)
                .Merge().OrderBy(bus => bus.EconomicNumber);

        /// <summary>
        /// Obtiene una lista de todos los dispositivos registrados en la base de datos.
        /// </summary>
        public static IEnumerable<Device> AllDevices => new[]
        {
            AllStations.Select(station=> station.Devices).Merge(),
            AllBuses.Select(bus => bus.Devices).Merge()
        }.Merge();

        /// <summary>
        /// Obtiene una lista de las rutas registradas en la base de datos.
        /// </summary>
        public static IEnumerable<Route> AllRoutes => _allRoutes;

        /// <summary>
        /// Obtiene una lista de todos las estaciones registrados en la base de datos.
        /// </summary>
        public static IEnumerable<Station> AllStations => AllRoutes.Select(route => route.Stations).Merge();

        /// <summary>
        /// Obtiene o establece una instancia de estación que representa al centro de control.
        /// </summary>
        public static Station CC => _cc;

        /// <summary>
        /// Obtiene la lista de todos los miembros del personal registrados.
        /// </summary>
        public static IEnumerable<ITStaff> ITStaff => _itStaff;

        /// <summary>
        /// Obtiene una lista de los autobuses sin operar.
        /// </summary>
        public static IEnumerable<Bus> OffDutyBus
            => AllBuses.Where(bus => bus.Status != BusStatus.OPERATIONAL);

        /// <summary>
        /// Obtiene de la sesión de la base de datos de la aplicación.
        /// </summary>
        public static DbSession Session
            => _session;

        /// <summary>
        /// Ejecuta una sentencia SQL en el servidor de base de datos.
        /// </summary>
        /// <param name="query">Sentencia Sql de selección.</param>
        /// <returns>Una matriz con el resultado de la consulta.</returns>
        public static String[][] ExecuteQueryInServerDB(String query)
        {
            var credential = ConfigurationManager.Settings.GetSettings("dbcredential", "DbServer")?
                .Cast<Credential>().FirstOrDefault(c => c.HasPermission(Permissions.READ));

            var credentialSsh = ConfigurationManager.Settings.GetSettings("sshcredential", "DbServer")?.FirstOrDefault() as Credential;

            var dbName = ConfigurationManager.Settings["dbname", "DbServer"]?.ToString();

            var ipAddress = AllDevices.FirstOrDefault(d => d.Type == DeviceType.DB)?.IPAddress.ToString()
                ?? ConfigurationManager.Settings["ipaddress", "DbServer"]?.ToString();

            var pgPath = ConfigurationManager.Settings["pgpath", "DbServer"]?.ToString();

            var pgPort = UInt16.Parse(ConfigurationManager.Settings["pgport", "DbServer"]?.ToString() ?? "5432");

            if (query.ToUpper().Contains("UPDATE")
                || query.ToUpper().Contains("DELETE")
                || query.ToUpper().Contains("TRUNCATE")
                || query.ToUpper().Contains("DROP")
                || query.ToUpper().Contains("CREATE")
                || query.ToUpper().Contains("ALTER")) return null;

            SshPsql psql = SshPsql.CreateConnection(
                pgPath,
                ipAddress,
                pgPort,
                credential.Username,
                credential.Password,
                dbName,
                credentialSsh.Username,
                credentialSsh.Password);

            var response = psql.ExecuteQuery(query);
            return response.Skip(1).ToArray();
        }

        /// <summary>
        /// Processa una petición y devuelve una respuesta a la misma.
        /// </summary>
        /// <param name="request">Petición realizada por un cliente remoto.</param>
        /// <returns>Una respuesta a la petición realizada.</returns>
        public static Message ProcessingRequest(Message request)
        {
            Message response = request;

            if (!Authenticate(response.GetField(61).ToString(), response.GetField(62).ToString()))
                response.AddField(60, MessagesError.NO_AUTHENTICATED);
        
            return response;
        }
        /// <summary>
        /// Autentica un usuario para poder realizar la petición.
        /// </summary>
        /// <param name="username">Nombre de usuario.</param>
        /// <param name="password">Clave del usuario.</param>
        /// <returns>Un true en caso de ser un usuario válido.</returns>
        private static bool Authenticate(string username, string password)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Recarga los catálogos de la base de datos.
        /// </summary>
        public static void ReloadData() => LoadFromDatabase();

        /// <summary>
        /// Realiza una consulta a la base de datos de la aplicación y obtiene los catálogos.
        /// </summary>
        private static void LoadFromDatabase()
        {
            _allRoutes = Session.GetObjects<Route>();
            _itStaff = Session.GetObjects<ITStaff>()
                                .OrderBy(tiStaff => tiStaff.Name);

            _cc = AllStations.FirstOrDefault(station => station.Name.Contains(ConfigurationManager.Settings["name", "CCO"].ToString()));
        }

        /// <summary>
        /// Define la configuración básica de la base de datos local.
        /// </summary>
        private class DbConfiguration : IDbDialect
        {
            /// <summary>
            /// Obtiene la cadena de conexión a la base de datos local.
            /// </summary>
            public string ConnectionString => ConfigurationManager.Settings["connectionString", "LocalDb"]?.ToString();

            /// <summary>
            /// Obtiene el nombre de la función para obtener el último insertado.
            /// </summary>
            public string LastInsertFunctionName => ConfigurationManager.Settings["lastInsertFunction", "LocalDb"]?.ToString();

            /// <summary>
            /// Obtiene el número de transacciones por conexión.
            /// </summary>
            public int TransactionPerConnection => Int32.Parse(ConfigurationManager.Settings["transactionPerConnection", "LocalDb"]?.ToString() ?? "1");
        }
    }
}