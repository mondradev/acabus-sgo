using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ACABUS_Control_de_operacion.Acabus
{
    internal class AcabusData
    {
        /// <summary>
        /// Archivo de configuración de las rutas.
        /// </summary>
        private const String FILE_NAME_CONFIG_XML = "Trunks.Config";

        /// <summary>
        /// Instancia que pertenece al documento XML.
        /// </summary>
        private static XmlDocument _xmlConfig = null;

        /// <summary>
        /// Lista de rutas troncales leidas desde el archivo XML.
        /// </summary>
        private static List<Trunk> _trunks;

        /// <summary>
        /// Ruta de acceso al archivo de plantilla del reporte semanal
        /// </summary>
        internal static readonly String TEMPLATE_WEEK_REPORT = Path.Combine(Environment.CurrentDirectory, "Resources\\reporte_semanal_acabus.template");

        /// <summary>
        /// Ruta de acceso al directorio de PostgreSQL
        /// </summary>
        internal const String PG_PATH = "/opt/PostgreSQL/9.3";

        internal const String PG_PATH_SERVER = "/opt/PostgresPlus/9.3AS";
        internal const String BD_SERVER_IP = "172.17.0.121";
        internal const String REPLICA_SERVER_IP = "172.17.0.125";
        internal const Int16 PG_PORT = 5432;
        internal const String PG_USERNAME = "postgres";
        internal const String PG_PASSWORD_SERVER = "admin";
        internal const String DATABASE_NAME = "SITM";
        internal const String DATABASE_REPLICA_NAME = "SITM_REPLICA";
        internal const String SSH_USERNAME_SERVER = "Administrador";
        internal const String SSH_PASSWORD_SERVER = "Administrador*2016";

        internal const String QUERY_DB_BACKUP = "SELECT datname FROM pg_database WHERE datname like 'SITM_%' AND datname not like 'SITM_A%' ORDER BY datname";
        internal const String QUERY_STATUS_COUNTER = "WITH TEMP_ACCE_SALI AS ( SELECT ID_EQUI, FCH_ACCE_SALI FROM SITM.SBOP_ACCE_SALI WHERE DATE(FCH_ACCE_SALI)>(DATE(NOW())-3) AND ID_EQUI IN (SELECT ID_EQUI FROM SITM.SBEQ_EQUI WHERE ID_VEHI IS NOT NULL) AND ID_TIPO_ACCE NOT IN (327,868) ), MAX_FCH_VALIDA AS ( SELECT ID_EQUI, MAX(FCH_ACCE_SALI) FCH_MAX FROM TEMP_ACCE_SALI GROUP BY ID_EQUI ORDER BY ID_EQUI ), TEMP_CONT_ACCE AS ( SELECT ID_EQUI, FCH_CONT_ACCE FROM SITM.SBOP_CONT_ACCE WHERE DATE(FCH_CONT_ACCE)>(DATE(NOW())-3) AND ID_EQUI IN (SELECT ID_EQUI FROM SITM.SBEQ_EQUI WHERE ID_VEHI IS NOT NULL) AND ID_TIPO_FLUJ=325 ) SELECT NO_ECON \"Numero Economico\", ESTADO_CONTADOR \"Estado del contador\", MAX(FCH_ACCE_SALI) \"ultima validacion\" FROM ( SELECT ID_EQUI,SUM(VAL) VAL, SUM(CONT) CONT, CASE WHEN SUM(VAL)>SUM(CONT) THEN CASE WHEN SUM(CONT)=0 THEN 'FALLA CONTADOR EN CERO' ELSE 'FALLA CONTADOR' END ELSE 'CONTADOR OPERANDO' END ESTADO_CONTADOR FROM ( SELECT ID_EQUI, DATE(FCH_ACCE_SALI) FECHA, COUNT(*) VAL, 0 CONT FROM TEMP_ACCE_SALI S WHERE DATE(FCH_ACCE_SALI) IN ( SELECT DATE(FCH_MAX) FROM MAX_FCH_VALIDA WHERE ID_EQUI = S.ID_EQUI ) GROUP BY ID_EQUI, FECHA UNION SELECT ID_EQUI, DATE(FCH_CONT_ACCE) FECHA, 0 VAL, COUNT(*) CONT FROM TEMP_CONT_ACCE C WHERE DATE(FCH_CONT_ACCE) IN ( SELECT DATE(FCH_MAX) FROM MAX_FCH_VALIDA WHERE ID_EQUI = C.ID_EQUI ) GROUP BY ID_EQUI, FECHA ) GROUP BY ID_EQUI) VC LEFT JOIN SITM.SBEQ_EQUI E ON E.ID_EQUI=VC.ID_EQUI LEFT JOIN TEMP_ACCE_SALI S ON VC.ID_EQUI=S.ID_EQUI WHERE ESTADO_CONTADOR LIKE '%FALLA%' GROUP BY NO_ECON, ESTADO_CONTADOR ORDER BY ESTADO_CONTADOR DESC, NO_ECON";
        internal const String QUERY_ALAR_TRUNK = "SELECT NUME_SERI \"Numero de serie\", DES_ALAR \"Descripcion de la alarma\", FCH_INI::TIME \"Hora\" FROM SITM.SBOP_EQUI_ALAR EA LEFT JOIN SITM.SBCT_ALAR A ON EA.ID_ALAR=A.ID_ALAR LEFT JOIN SITM.SBEQ_EQUI E ON E.ID_EQUI=EA.ID_EQUI WHERE DATE(FCH_INI)=DATE(NOW()) AND DES_ALAR NOT LIKE '%NO HAY CONEXION%' ORDER BY FCH_INI DESC";
        internal const String QUERY_LAST_CONNECTION_VEHI = "SELECT NO_ECON \"Numero Economico\", RECE.FCH_CREA \"ultima conexion\" FROM SITM.SFMO_RECE_NAVE AS RECE JOIN SITM.SFVH_VEHI AS VEHI ON RECE.ID_VEHI = VEHI.ID_VEHI WHERE (DATE(NOW())>DATE(RECE.FCH_CREA) OR NOW()::TIME - RECE.FCH_CREA::TIME > '00:10:00'::TIME) AND VEHI.NO_ECON NOT IN ({0}) ORDER BY VEHI.NO_ECON ASC, RECE.FCH_CREA DESC";
        internal static String TOTAL_SALE = String.Format("SELECT COUNT(*) AS VENTA FROM SITM_DISP.SBOP_TRAN WHERE  TIPO_TRAN = 'V' AND CAST(FCH_TRAN AS DATE)='{0:yyyy-MM-dd}'", DateTime.Now.AddDays(-1));
        internal static String PENDING_INFO_TO_SEND_DEVICE_RN = "SELECT NO_ECON \"Numero de serie\", 'Asignada por VPN' \"Direccion IP\", ULTIMO_DATO \"ultima trans.registrada\", PRIMERO_SIN_ENVIAR \"Primera trans. s/enviar\", CASE WHEN DATOS_PENDIENTES IS NULL THEN 0 ELSE DATOS_PENDIENTES END \"Trans.pendientes\", CASE WHEN PRIMERO_SIN_ENVIAR IS NULL THEN '00:00:00' ELSE (NOW()-PRIMERO_SIN_ENVIAR) END \"Tiempo s/replicar\" FROM ( SELECT D.ID_VEHI, MAX(FCH_CREA) ULTIMO_DATO, PRIMERO_SIN_ENVIAR, DATOS_PENDIENTES FROM SITM_DISP.SFMO_HIST_RECE_NAVE D LEFT JOIN ( SELECT ID_VEHI, MIN(FCH_ENVI) PRIMERO_SIN_ENVIAR, COUNT(*) DATOS_PENDIENTES FROM SITM_ENVI.SFMO_HIST_RECE_NAVE WHERE BOL_ENVI=FALSE GROUP BY ID_VEHI ) E ON E.ID_VEHI=D.ID_VEHI GROUP BY D.ID_VEHI, PRIMERO_SIN_ENVIAR, DATOS_PENDIENTES ) AS DATA_ENVI JOIN SITM_DISP.SFVH_VEHI E ON E.ID_VEHI=DATA_ENVI.ID_VEHI WHERE E.ID_ESTA=1";
        internal const String CARD_STOCK = "SELECT tarj_stck FROM sitm_disp.sbop_sum_tarj ORDER BY fch_oper DESC LIMIT 1";
        internal const String PENDING_INFO_TO_SEND_DEVICE_R_S = "SELECT NUME_SERI \"Numero de serie\", DIR_IP \"Direccion IP\", ULTIMO_DATO \"ultima trans. registrada\", PRIMERO_SIN_ENVIAR \"Primera trans. s/enviar\", CASE WHEN DATOS_PENDIENTES IS NULL THEN 0 ELSE DATOS_PENDIENTES END \"Trans. pendientes\", CASE WHEN PRIMERO_SIN_ENVIAR IS NULL THEN '00:00:00' ELSE (NOW()-PRIMERO_SIN_ENVIAR) END \"Tiempo s/replicar\" FROM ( SELECT D.ID_EQUI, MAX(FCH_TRAN) ULTIMO_DATO, PRIMERO_SIN_ENVIAR, DATOS_PENDIENTES FROM SITM_DISP.SBOP_TRAN D LEFT JOIN ( SELECT ID_EQUI, MIN(FCH_ENVI) PRIMERO_SIN_ENVIAR, COUNT(*) DATOS_PENDIENTES FROM SITM_ENVI.SBOP_TRAN WHERE BOL_ENVI=FALSE GROUP BY ID_EQUI ) E ON E.ID_EQUI=D.ID_EQUI GROUP BY D.ID_EQUI, PRIMERO_SIN_ENVIAR, DATOS_PENDIENTES ) AS DATA_ENVI JOIN SITM_DISP.SBEQ_EQUI E ON E.ID_EQUI=DATA_ENVI.ID_EQUI WHERE E.ID_ESTA=1";
        internal const String PENDING_INFO_TO_SEND_DEVICE_I_O = "SELECT COALESCE(NO_ECON, NUME_SERI) \"Numero de serie\", DIR_IP \"Direccion IP\", ULTIMO_DATO \"ultima trans. registrada\", PRIMERO_SIN_ENVIAR \"Primera trans. s/enviar\", CASE WHEN DATOS_PENDIENTES IS NULL THEN 0 ELSE DATOS_PENDIENTES END \"Trans. pendientes\", CASE WHEN PRIMERO_SIN_ENVIAR IS NULL THEN '00:00:00' ELSE (NOW()-PRIMERO_SIN_ENVIAR) END \"Tiempo s/replicar\" FROM ( SELECT D.ID_EQUI, MAX(FCH_ACCE_SALI) ULTIMO_DATO, PRIMERO_SIN_ENVIAR, DATOS_PENDIENTES FROM SITM_DISP.SBOP_ACCE_SALI D LEFT JOIN ( SELECT ID_EQUI, MIN(FCH_ENVI) PRIMERO_SIN_ENVIAR, COUNT(*) DATOS_PENDIENTES FROM SITM_ENVI.SBOP_ACCE_SALI WHERE BOL_ENVI=FALSE GROUP BY ID_EQUI ) E ON E.ID_EQUI=D.ID_EQUI GROUP BY D.ID_EQUI, PRIMERO_SIN_ENVIAR, DATOS_PENDIENTES ) AS DATA_ENVI JOIN SITM_DISP.SBEQ_EQUI E ON E.ID_EQUI=DATA_ENVI.ID_EQUI WHERE E.ID_ESTA=1";

        internal const string QUERY_FILES_ACCESS = "Queries\\Accesos_Semana.sql";
        internal const string QUERY_FILE_SALES = "Queries\\Ventas_Semana.sql";
        internal const string QUERY_FILE_RECHARGE = "Queries\\Recargas_Semana.sql";
        internal const string QUERY_FILE_ACCE_TRUNK = "Queries\\Accesos_Estaciones_Semana.sql";
        internal const string QUERY_FILE_ACCE_VEHI = "Queries\\Accesos_Vehiculos_Semana.sql";
        internal const string QUERY_FILE_ACC_CRED_VEHI = "Queries\\Accesos_Vehiculos_Cred_Semana.sql";
        internal const string QUERY_FILE_ACC_CRED_VEH_COST = "Queries\\Acce_Vehi_Cred_Cost_Semana.sql";
        internal const string QUERY_FILE_RECHARGE_CRED = "Queries\\Recargas_Credito_Semana.sql";

        /// <summary>
        /// Obtiene la lista de rutas troncales.
        /// </summary>
        public static List<Trunk> Trunks {
            get {
                if (_trunks == null)
                    _trunks = new List<Trunk>();
                return _trunks;
            }
        }

        /// <summary>
        /// Carga la configuración del XML en Trunk.Trunks.
        /// </summary>
        public static void LoadConfiguration()
        {
            Trunks.Clear();
            _xmlConfig = new XmlDocument();
            _xmlConfig.Load(FILE_NAME_CONFIG_XML);
            foreach (XmlNode trunkXmlNode in _xmlConfig.SelectSingleNode("Trunks"))
            {
                if (!trunkXmlNode.Name.Equals("Trunk"))
                    continue;
                var trunk = Trunk.ToTrunk(trunkXmlNode) as Trunk;
                Trunks.Add(trunk);
                LoadStations(trunk, trunkXmlNode.SelectSingleNode("Stations")?.ChildNodes);
            }
        }

        /// <summary>
        /// Carga las estaciones a partir de una lista de nodos
        /// XML.
        /// </summary>
        /// <param name="stationNodes">Lista de nodos XML que representan
        /// una estación cada uno.</param>
        private static void LoadStations(Trunk trunk, XmlNodeList stationNodes)
        {
            if (stationNodes != null)
                foreach (XmlNode stationXmlNode in stationNodes)
                {
                    if (!stationXmlNode.Name.Equals("Station"))
                        continue;
                    var station = Station.ToStation(stationXmlNode, trunk) as Station;
                    trunk.AddStation(station);
                    LoadDevices(station, stationXmlNode.SelectSingleNode("Devices")?.ChildNodes);
                }
        }

        /// <summary>
        /// Carga los dispositivos a partir de una lista de nodos XML.
        /// </summary>
        /// <param name="deviceNodes">Lista de nodos XML.</param>
        private static void LoadDevices(Station station, XmlNodeList deviceNodes)
        {
            if (deviceNodes != null)
                foreach (XmlNode deviceXmlNode in deviceNodes)
                {
                    var device = Device.ToDevice(deviceXmlNode, station) as Device;
                    station.AddDevice(device);
                }
        }
    }
}