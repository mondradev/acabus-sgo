using Npgsql;
using System;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace ACABUS_Control_de_operacion
{
    public class PostgreSQL
    {

        private const String _CONNECTION = "Server={0};Port={1};User Id={2};Password={3};Database={4};";
        private const String _connectionBySsh = "PGPASSWORD='{0}' /opt/PostgreSQL/9.3/bin/psql -U {1} -d {2} -p {3}  -F ',' -R '|' --no-align -c \"{4}\" | grep -E '[\\||,|0-9A-Za-z]'";

        private Boolean _customCommand;
        private String _customConnectionBySsh;

        public String ConnectionBySsh {
            get {
                if (_customCommand && !String.IsNullOrEmpty(this._customConnectionBySsh))
                    return this._customConnectionBySsh;
                return _connectionBySsh;
            }
        }

        public String Username { get; private set; }
        public String Passoword { get; private set; }
        public String Host { get; private set; }
        public String DataBase { get; private set; }
        public Int16 Port { get; private set; }
        public Int16 TimeOut { get; set; }
        public Int16 LimitOfAttempts { get; set; }

        public static PostgreSQL CreateConnection(String host, Int16 port, String username, String password, String database)
        {
            return new PostgreSQL()
            {
                Host = host,
                Port = port,
                Username = username,
                Passoword = password,
                DataBase = database,
                LimitOfAttempts = 5,
                TimeOut = 600
            };
        }

        public String[][] ExecuteQuery(String statement)
        {
            String response = null;
            using (NpgsqlConnection connection = InitializeConnection())
            {
                if (connection != null) connection.Open();
                else return null;

                if (connection.State == ConnectionState.Open)
                {
                    NpgsqlCommand command = new NpgsqlCommand(statement, connection)
                    {
                        CommandTimeout = TimeOut
                    };
                    NpgsqlDataReader reader = command.ExecuteReader();
                    StringBuilder header = new StringBuilder();
                    Boolean readHeader = false;
                    Int64 rowsCount = 0;
                    StringBuilder rows = new StringBuilder();
                    while (reader.Read())
                    {
                        if (rowsCount > 0) rows.Append('|');
                        for (Int16 i = 0; i < reader.FieldCount; i++)
                        {
                            if (!readHeader)
                            {
                                header.Append(reader.GetName(i));
                                if (i + 1 < reader.FieldCount)
                                    header.Append(",");
                            }
                            rows.Append(reader[i].ToString());
                            if (i + 1 < reader.FieldCount) rows.Append(",");
                        }
                        rowsCount++;
                        readHeader = true;
                    }
                    response = header.Append("|").Append(rows.ToString()).ToString();
                }
            }
            return String.IsNullOrEmpty(response) ? null : ProcessResponse(response);
        }

        private NpgsqlConnection InitializeConnection()
        {
            string connection = String.Format(_CONNECTION,
                        Host, Port, Username, Passoword, DataBase);
            return new NpgsqlConnection(connection);
        }

        public String[][] ExcuteQueryBySsh(String query, String usernameSsh, String passwordSsh)
        {
            Int16 attempts = 0;
            Int16 limitOfAttempts = LimitOfAttempts;
            String response = "";
            while (attempts < limitOfAttempts)
            {
                try
                {
                    using (Ssh ssh = new Ssh(Host, usernameSsh, passwordSsh))
                    {
                        ssh.TimeOut = TimeOut;
                        if (ssh.IsConnected())
                        {
                            response = ssh.SendCommand(String.Format(this.ConnectionBySsh, Passoword, Username, DataBase, Port, query));
                            if (String.IsNullOrEmpty(response))
                                throw new Exception(String.Format("El host {0} no respondió a tiempo", Host));
                            break;

                        }
                    }
                }
                catch (Exception ex)
                {
                    attempts++;
                    response = "Error al obtener información";
                    Trace.WriteLine(ex.Message);
                    Trace.WriteLine(String.Format("El host {0} intentará nuevamente realizar la consulta: intento {1}/{2}", Host, attempts + 1, limitOfAttempts));
                }
            }
            return String.IsNullOrEmpty(response) ? null : ProcessResponse(response);
        }

        public void SetConnectionBySsh(String connectionString)
        {
            this._customConnectionBySsh = connectionString;
            this._customCommand = true;
        }

        private String[][] ProcessResponse(string response)
        {
            int columnsCount = 0;
            String[] rows = response.Split('|');
            int rowsCount = rows.Length;
            if (rowsCount > 0)
            {
                columnsCount = rows[0].Split(',').Length;
            }
            if (columnsCount > 0 && rowsCount > 0)
            {
                String[][] tempData = new String[rowsCount][];
                for (int i = 0; i < rowsCount; i++)
                {
                    if (Regex.IsMatch(rows[i], "[0-9]{1,}\\srow"))
                    {
                        rowsCount--;
                        break;
                    }
                    tempData[i] = rows[i].Split(',');
                }
                String[][] data = new String[rowsCount][];
                for (int i = 0; i < rowsCount; i++)
                {
                    data[i] = tempData[i];
                }
                return data;
            }
            return null;
        }

        public static String TOTAL_SALE = String.Format("SELECT COUNT(*) AS VENTA FROM SITM_DISP.SBOP_TRAN WHERE  TIPO_TRAN = 'V' AND CAST(FCH_TRAN AS DATE)='{0:yyyy-MM-dd}'", DateTime.Now.AddDays(-1));
        public static String CARD_STOCK = "SELECT tarj_stck FROM sitm_disp.sbop_sum_tarj ORDER BY fch_oper DESC LIMIT 1";
        public static String PENDING_INFO_TO_SEND_DEVICE_R_S = "SELECT NUME_SERI, DIR_IP, ULTIMO_DATO, PRIMERO_SIN_ENVIAR, CASE WHEN DATOS_PENDIENTES IS NULL THEN 0 ELSE DATOS_PENDIENTES END DATOS_PENDIENTES, CASE WHEN PRIMERO_SIN_ENVIAR IS NULL THEN '00:00:00' ELSE (NOW()-PRIMERO_SIN_ENVIAR) END SIN_REPLICAR FROM ( SELECT D.ID_EQUI, MAX(FCH_TRAN) ULTIMO_DATO, PRIMERO_SIN_ENVIAR, DATOS_PENDIENTES FROM SITM_DISP.SBOP_TRAN D LEFT JOIN ( SELECT ID_EQUI, MIN(FCH_ENVI) PRIMERO_SIN_ENVIAR, COUNT(*) DATOS_PENDIENTES FROM SITM_ENVI.SBOP_TRAN WHERE BOL_ENVI=FALSE GROUP BY ID_EQUI ) E ON E.ID_EQUI=D.ID_EQUI GROUP BY D.ID_EQUI, PRIMERO_SIN_ENVIAR, DATOS_PENDIENTES ) AS DATA_ENVI JOIN SITM_DISP.SBEQ_EQUI E ON E.ID_EQUI=DATA_ENVI.ID_EQUI WHERE E.ID_ESTA=1";
        public static String PENDING_INFO_TO_SEND_DEVICE_I_O = "SELECT NUME_SERI, DIR_IP, ULTIMO_DATO, PRIMERO_SIN_ENVIAR, CASE WHEN DATOS_PENDIENTES IS NULL THEN 0 ELSE DATOS_PENDIENTES END DATOS_PENDIENTES, CASE WHEN PRIMERO_SIN_ENVIAR IS NULL THEN '00:00:00' ELSE (NOW()-PRIMERO_SIN_ENVIAR) END SIN_REPLICAR FROM ( SELECT D.ID_EQUI, MAX(FCH_ACCE_SALI) ULTIMO_DATO, PRIMERO_SIN_ENVIAR, DATOS_PENDIENTES FROM SITM_DISP.SBOP_ACCE_SALI D LEFT JOIN ( SELECT ID_EQUI, MIN(FCH_ENVI) PRIMERO_SIN_ENVIAR, COUNT(*) DATOS_PENDIENTES FROM SITM_ENVI.SBOP_ACCE_SALI WHERE BOL_ENVI=FALSE GROUP BY ID_EQUI ) E ON E.ID_EQUI=D.ID_EQUI GROUP BY D.ID_EQUI, PRIMERO_SIN_ENVIAR, DATOS_PENDIENTES ) AS DATA_ENVI JOIN SITM_DISP.SBEQ_EQUI E ON E.ID_EQUI=DATA_ENVI.ID_EQUI WHERE E.ID_ESTA=1";
    }
}
