using InnSyTech.Standard.Database;
using InnSyTech.Standard.Net.Messenger.Iso8583;
using InnSyTech.Standard.SecureShell;
using InnSyTech.Standard.Utils;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;

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
        private static readonly string RESOURCES_DIRECTORY = Path.Combine(Environment.CurrentDirectory, "Resources");

        /// <summary>
        /// Campo que provee a la propiedad <see cref="AllRoutes"/>.
        /// </summary>
        private static IEnumerable<Route> _allRoutes;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="CC"/>.
        /// </summary>
        private static Station _cc;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Credentials" />.
        /// </summary>
        private static ICollection<Credential> _credentials;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ITStaff"/>.
        /// </summary>
        private static IEnumerable<ITStaff> _itStaff;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ModulesNames"/>.
        /// </summary>
        private static ICollection<Tuple<String, String, String>> _modulesNames;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Queries" />.
        /// </summary>
        private static ICollection<Tuple<String, String>> _queries;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Session" />.
        /// </summary>
        private static DbSession _session;

        /// <summary>
        /// Constructor estático de <see cref="AcabusData"/>.
        /// </summary>
        static AcabusData()
        {
            if (!Directory.Exists(RESOURCES_DIRECTORY))
                Directory.CreateDirectory(RESOURCES_DIRECTORY);

            FillList(ref _credentials, SettingToCredential, "Credentials", "Credential");
            FillList(ref _queries, SettingToQuery, "SqlSentences", "Tuple");
            FillList(ref _modulesNames, SettingToModuleName, "ModulesNames", "Tuple");

            _session = DbManager.CreateSession(typeof(SQLiteConnection), new SQLiteConfiguration());
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
        /// Obtiene la consulta para la descarga de los autobuses desde el servidor de base de datos.
        /// </summary>
        public static String BusQuery
            => Queries.FirstOrDefault(query => query.Item1 == "DownloadBuses").Item2;

        /// <summary>
        /// Obtiene o establece una instancia de estación que representa al centro de control.
        /// </summary>
        public static Station CC => _cc;

        /// <summary>
        /// Obtiene una lista de las credenciales de acceso
        /// </summary>
        public static ICollection<Credential> Credentials
            => _credentials ?? (_credentials = new ObservableCollection<Credential>());

        /// <summary>
        /// Obtiene la consulta para la descarga de los equipos desde el servidor de base de datos.
        /// </summary>
        public static String DeviceQuery
            => Queries.FirstOrDefault(query => query.Item1 == "DownloadDevices").Item2;

        /// <summary>
        /// Obtiene la lista de todos los miembros del personal registrados.
        /// </summary>
        public static IEnumerable<ITStaff> ITStaff => _itStaff;

        /// <summary>
        /// Obtiene una lista de los nombres de los modulos de esta aplicación.
        /// </summary>
        public static ICollection<Tuple<String, String, String>> ModulesNames
            => _modulesNames ?? (_modulesNames = new ObservableCollection<Tuple<String, String, String>>());

        /// <summary>
        /// Obtiene una lista de los autobuses sin operar.
        /// </summary>
        public static IEnumerable<Bus> OffDutyBus
            => AllBuses.Where(bus => bus.Status != BusStatus.OPERATIONAL);

        /// <summary>
        /// Obtiene una lista de las sentencias SQL utilizadas para la aplicación.
        /// </summary>
        public static ICollection<Tuple<String, String>> Queries
            => _queries ?? (_queries = new ObservableCollection<Tuple<String, String>>());

        /// <summary>
        /// Obtiene la consulta para la descarga de las rutas desde el servidor de base de datos.
        /// </summary>
        public static String RouteQuery
            => Queries.FirstOrDefault(query => query.Item1 == "DownloadRoutes").Item2;

        /// <summary>
        /// Obtiene de la sesión de la base de datos de la aplicación.
        /// </summary>
        public static DbSession Session
            => _session;

        /// <summary>
        /// Obtiene la consulta para la descarga de las estaciones desde el servidor de base de datos.
        /// </summary>
        public static String StationQuery
            => Queries.FirstOrDefault(query => query.Item1 == "DownloadStations").Item2;

        /// <summary>
        /// Ejecuta una sentencia SQL en el servidor de base de datos.
        /// </summary>
        /// <param name="query">Sentencia Sql de selección.</param>
        /// <returns>Una matriz con el resultado de la consulta.</returns>
        public static String[][] ExecuteQueryInServerDB(String query)
        {
            var credential = Credentials.FirstOrDefault(cred => cred.Username == "postgres" && cred.Type == "psql");
            var credentialSsh = Credentials.FirstOrDefault(cred => cred.Username == "Administrador" && cred.Type == "ssh");

            if (query.ToUpper().Contains("UPDATE")
                || query.ToUpper().Contains("DELETE")
                || query.ToUpper().Contains("TRUNCATE")
                || query.ToUpper().Contains("DROP")
                || query.ToUpper().Contains("CREATE")
                || query.ToUpper().Contains("ALTER")) return null;

            SshPsql psql = SshPsql.CreateConnection(
                "",
                "172.17.0.121",
                5432,
                credential.Username,
                credential.Password,
                "SITM",
                credentialSsh.Username,
                credentialSsh.Password);

            var response = psql.ExecuteQuery(query);
            return response.Skip(1).ToArray();
        }

        /// <summary>
        /// Convierte las configuraciones guardadas en el archivo de configuración de la aplicación
        /// en instancias del tipo representado.
        /// </summary>
        /// <typeparam name="T">Tipo destino de la configuración.</typeparam>
        /// <param name="list">Colección de elementos leidos desde la configuración.</param>
        /// <param name="converterFunction">Función para convertir la configuración en instancia.</param>
        /// <param name="sectionName">Nombre de la sección de configuraciones.</param>
        /// <param name="settingName">Nombre de los elementos de configuración.</param>
        public static void FillList<T>(ref ICollection<T> list, Func<ISetting, T> converterFunction,
            String sectionName, String settingName)
        {
            if (list is null) list = new ObservableCollection<T>();

            var settings = XmlConfiguration.GetSettings(settingName, sectionName);
            foreach (ISetting setting in settings)
                list.Add(converterFunction.Invoke(setting));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Message ProcessingRequest(Message request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Recarga los catálogos de la base de datos.
        /// </summary>
        public static void ReloadData() => LoadFromDatabase();

        /// <summary>
        /// Guarda la configuración que se encuentra actualmente en la aplicación.
        /// </summary>
        public static void SaveConfig()
        {
            XmlConfiguration.SaveSettings(_credentials, "Credentials");
            XmlConfiguration.SaveSettings(_queries, "SqlSentences");
        }

        /// <summary>
        /// Realiza una consulta a la base de datos de la aplicación y obtiene los catálogos.
        /// </summary>
        private static void LoadFromDatabase()
        {
            _allRoutes = Session.GetObjects<Route>();
            _itStaff = Session.GetObjects<ITStaff>()
                                .OrderBy(tiStaff => tiStaff.Name);

            _cc = AllStations.FirstOrDefault(station => station.Name.Contains("CENTRO DE CONTROL"));
        }

        /// <summary>
        /// Convierte una instancia <see cref="ISetting"/> en una instancia <see cref="Credential"/>.
        /// </summary>
        /// <param name="arg">Instancia <see cref="ISetting"/> a convertir.</param>
        /// <returns>Una instancia <see cref="Credential"/>.</returns>
        private static Credential SettingToCredential(ISetting arg)
            => new Credential(
                arg["Username"].ToString(),
                arg["Password"].ToString(),
                arg["Type"].ToString(),
                Boolean.Parse(arg["IsRoot"].ToString())
            );

        /// <summary>
        /// Convierte una instancia <see cref="ISetting"/> en una instancia <see
        /// cref="Tuple{String,String,String}"/> donde el primer valor es el nombre del módulo, el
        /// segundo el nombre completo de la clase del módulo y el tercero el nombre del ensamblado
        /// que lo contiene.
        /// </summary>
        /// <param name="arg">Instancia <see cref="ISetting"/> a convertir.</param>
        /// <returns>
        /// Una instancia de <see cref="Tuple{String, String, String}"/> que representa un módulo.
        /// </returns>
        private static Tuple<String, String, String> SettingToModuleName(ISetting arg)
            => Tuple.Create(arg["Item1"].ToString(), arg["Item2"].ToString(), arg["Item3"].ToString());

        /// <summary>
        /// Convierte una instancia <see cref="ISetting"/> en una <see cref="Tuple{String, String}"/>
        /// donde el primero elemento es el nombre de la etiqueta de la sentencia y el segundo es la
        /// sentencia Sql.
        /// </summary>
        /// <param name="arg">Instancia <see cref="ISetting"/> a convertir.</param>
        /// <returns>Una <see cref="Tuple{String, String}"/> que representa un sentencia Sql.</returns>
        private static Tuple<string, string> SettingToQuery(ISetting arg)
            => Tuple.Create(arg["Item1"].ToString(), arg["Item2"].ToString());

        /// <summary>
        /// Define la configuración básica de la base de datos local.
        /// </summary>
        private class SQLiteConfiguration : IDbConfiguration
        {
            /// <summary>
            /// Obtiene la cadena de conexión a la base de datos local.
            /// </summary>
            public string ConnectionString => "Data Source=Resources/acabus_data.dat;Password=acabus*data*dat";

            /// <summary>
            /// Obtiene el nombre de la función para obtener el último insertado.
            /// </summary>
            public string LastInsertFunctionName => "last_insert_rowid";

            /// <summary>
            /// Obtiene el número de transacciones por conexión.
            /// </summary>
            public int TransactionPerConnection => 1;
        }
    }
}