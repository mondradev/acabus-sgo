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

        public String Username { get; protected set; }
        public String Passoword { get; protected set; }
        public String Host { get; protected set; }
        public String DataBase { get; protected set; }
        public Int16 Port { get; protected set; }

        public static PostgreSQL CreateConnection(String host, Int16 port, String username, String password, String database)
        {
            return new PostgreSQL()
            {
                Host = host,
                Port = port,
                Username = username,
                Passoword = password,
                DataBase = database
            };
        }

        protected PostgreSQL() { }

        public virtual String[][] ExecuteQuery(String statement)
        {
            try
            {
                String response = null;
                if (String.IsNullOrEmpty(statement)) return null;
                using (NpgsqlConnection connection = InitializeConnection())
                {
                    if (connection != null) connection.Open();
                    else return null;

                    if (connection.State == ConnectionState.Open)
                    {
                        NpgsqlCommand command = new NpgsqlCommand(statement, connection);
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
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message, "ERROR");
            }
            return new String[][] { };
        }

        private NpgsqlConnection InitializeConnection()
        {
            string connection = String.Format(_CONNECTION,
                        Host, Port, Username, Passoword, DataBase);
            return new NpgsqlConnection(connection);
        }

        protected String[][] ProcessResponse(string response)
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
        public static String PENDING_INFO_TO_SEND_DEVICE_R_S = "SELECT NUME_SERI \"Número de serie\", DIR_IP \"Dirección IP\", ULTIMO_DATO \"Última trans. registrada\", PRIMERO_SIN_ENVIAR \"Primera trans. s/enviar\", CASE WHEN DATOS_PENDIENTES IS NULL THEN 0 ELSE DATOS_PENDIENTES END \"Trans. pendientes\", CASE WHEN PRIMERO_SIN_ENVIAR IS NULL THEN '00:00:00' ELSE (NOW()-PRIMERO_SIN_ENVIAR) END \"Tiempo s/replicar\" FROM ( SELECT D.ID_EQUI, MAX(FCH_TRAN) ULTIMO_DATO, PRIMERO_SIN_ENVIAR, DATOS_PENDIENTES FROM SITM_DISP.SBOP_TRAN D LEFT JOIN ( SELECT ID_EQUI, MIN(FCH_ENVI) PRIMERO_SIN_ENVIAR, COUNT(*) DATOS_PENDIENTES FROM SITM_ENVI.SBOP_TRAN WHERE BOL_ENVI=FALSE GROUP BY ID_EQUI ) E ON E.ID_EQUI=D.ID_EQUI GROUP BY D.ID_EQUI, PRIMERO_SIN_ENVIAR, DATOS_PENDIENTES ) AS DATA_ENVI JOIN SITM_DISP.SBEQ_EQUI E ON E.ID_EQUI=DATA_ENVI.ID_EQUI WHERE E.ID_ESTA=1";
        public static String PENDING_INFO_TO_SEND_DEVICE_I_O = "SELECT NUME_SERI \"Número de serie\", DIR_IP \"Dirección IP\", ULTIMO_DATO \"Última trans. registrada\", PRIMERO_SIN_ENVIAR \"Primera trans. s/enviar\", CASE WHEN DATOS_PENDIENTES IS NULL THEN 0 ELSE DATOS_PENDIENTES END \"Trans. pendientes\", CASE WHEN PRIMERO_SIN_ENVIAR IS NULL THEN '00:00:00' ELSE (NOW()-PRIMERO_SIN_ENVIAR) END \"Tiempo s/replicar\" FROM ( SELECT D.ID_EQUI, MAX(FCH_ACCE_SALI) ULTIMO_DATO, PRIMERO_SIN_ENVIAR, DATOS_PENDIENTES FROM SITM_DISP.SBOP_ACCE_SALI D LEFT JOIN ( SELECT ID_EQUI, MIN(FCH_ENVI) PRIMERO_SIN_ENVIAR, COUNT(*) DATOS_PENDIENTES FROM SITM_ENVI.SBOP_ACCE_SALI WHERE BOL_ENVI=FALSE GROUP BY ID_EQUI ) E ON E.ID_EQUI=D.ID_EQUI GROUP BY D.ID_EQUI, PRIMERO_SIN_ENVIAR, DATOS_PENDIENTES ) AS DATA_ENVI JOIN SITM_DISP.SBEQ_EQUI E ON E.ID_EQUI=DATA_ENVI.ID_EQUI WHERE E.ID_ESTA=1";
    }
}
