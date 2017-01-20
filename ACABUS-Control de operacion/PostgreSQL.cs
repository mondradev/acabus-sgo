using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACABUS_Control_de_operacion
{
    class PostgreSQL
    {
        public enum queryConsult
        {
            totalCard,
            totalVta,
            queryTran,
            queryVal
        }

        public string selectQuery(queryConsult query)
        {
            string strQuery = null;

            switch (query)
            {
                case queryConsult.totalCard:
                    strQuery = "SELECT tarj_stck FROM sitm_disp.sbop_sum_tarj ORDER BY fch_oper DESC LIMIT 1;";
                    break;
                case queryConsult.queryTran:
                    strQuery = "SELECT (SELECT COUNT(*) FROM SITM_ENVI.SBOP_TRAN WHERE BOL_ENVI=FALSE)||','||(SELECT MAX(fch_envi) FROM SITM_ENVI.SBOP_TRAN)||','||(SELECT CASE WHEN MAX(fch_envi) IS NULL THEN '1900/01/01' ELSE MAX(fch_envi) END FROM SITM_ENVI.SBOP_TRAN WHERE BOL_ENVI=TRUE)";
                    break;
                case queryConsult.queryVal:
                    strQuery = "SELECT (SELECT COUNT(*) FROM SITM_ENVI.SBOP_ACCE_SALI WHERE BOL_ENVI=FALSE)||','||(SELECT MAX(fch_envi) FROM SITM_ENVI.SBOP_ACCE_SALI)||','||(SELECT CASE WHEN MAX(fch_envi) IS NULL THEN '1900/01/01' ELSE MAX(fch_envi) END FROM SITM_ENVI.SBOP_ACCE_SALI WHERE BOL_ENVI=TRUE)";
                    break;
                case queryConsult.totalVta:
                    DateTime dt = DateTime.Now.AddDays(-1);
                    strQuery= String.Format("SELECT COUNT(*) AS VENTA FROM SITM_DISP.SBOP_TRAN WHERE  TIPO_TRAN = 'V' AND CAST(FCH_TRAN AS DATE)='{0:yyyy-MM-dd}';", dt);
                    break;
                default:
                    break;
            }

            return strQuery;
        }

        public string stockCard(Dictionary<string, string> dicConfig, string strIP, queryConsult query)
        {
            string strCon = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                        strIP,
                        dicConfig["Port"].ToString(),
                        dicConfig["Id"].ToString(),
                        dicConfig["Password"].ToString(),
                        dicConfig["Database"].ToString());

            string strQuery = selectQuery(query);

            return executeQuery001(strQuery, strCon);
        }

        private string executeQuery001(string strQuery, string strConn)
        {
            NpgsqlConnection conn = new NpgsqlConnection(strConn);
            conn.Open();

            if (conn != null && conn.State == ConnectionState.Open)
            {
                NpgsqlCommand com = new NpgsqlCommand(strQuery, conn);
                NpgsqlDataReader dRead = com.ExecuteReader();
                while (dRead.Read())
                {
                    if (!String.IsNullOrEmpty(dRead[0].ToString()))
                        return dRead[0].ToString();
                }
            }
            conn.Close();
            return null;
        }
    }
}
