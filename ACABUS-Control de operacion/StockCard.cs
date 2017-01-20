using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ACABUS_Control_de_operacion
{
    public partial class StockCard : Form
    {
        public StockCard()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
        }

        #region BARRA DE HERRAMIENTAS

        private void tsbExecute_Click(object sender, EventArgs e)
        {
            proccessRequest();
        }

        #endregion

        #region FUNCIONES

        private void proccessRequest()
        {
            Configuration config = new Configuration();
            config._informationKVR();
            
            Dictionary<string, string> KVRs = loadConfigurationEquip();
            Dictionary<string, string> SQL = loadConfigurationPostgreSQL();

            this.dgvResult.Columns.Add("KVR", "Id KVR");
            this.dgvResult.Columns.Add("Vta", "Total de Ventas");
            this.dgvResult.Columns.Add("Sto", "Total de Stock");
            this.dgvResult.Columns.Add("Sum", "Total de Suministro");

            foreach (KeyValuePair<string, string> entry in KVRs)
            {
                if (isAvaibleIP(entry.Value))
                {
                    string[] row = new string[] { entry.Key,
                        consultSales(entry.Value, SQL),
                        consultStock(entry.Value, SQL),
                        "3" };
                    dgvResult.Rows.Add(row);
                }
                Application.DoEvents();
            }
          
        }

        private Dictionary<string, string> loadConfigurationEquip()
        {
            Configuration config = new Configuration();
            return config.informationKVR();
        }

        private Dictionary<string, string> loadConfigurationPostgreSQL()
        {
            Configuration config = new Configuration();
            return config.informationSQL();
        }

        private bool isAvaibleIP(string strIP)
        {
            ConnectionTCP cnnTCP = new ConnectionTCP();
            if (cnnTCP.sendToPing(strIP, 3))
                return true;
            return false;
        }

        private string consultStock(string strEquip, Dictionary<string, string> dicConfig)
        {
            PostgreSQL SQL = new PostgreSQL();
            return SQL.stockCard(dicConfig, strEquip, PostgreSQL.queryConsult.totalCard);
        }

        private static string consultSales(string strEquip, Dictionary<string, string> dicConfig)
        {
            PostgreSQL SQL = new PostgreSQL();
            return SQL.stockCard(dicConfig, strEquip, PostgreSQL.queryConsult.totalVta);
        }

        #endregion

    }
}
