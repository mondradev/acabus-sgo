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
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        #region MENU

        #region MENU ARCHIVO

        private void mbrArcExi_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        #endregion

        #region BARRA DE HERRAMIENTAS

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            StockCard stockCard = new StockCard()
            {
                MdiParent = this
            };
            stockCard.Show();
        }

        private void DailyTaskButton_Click(object sender, EventArgs e)
        {
            SQLModule sqlAndReplica = new SQLModule()
            {
                MdiParent = this
            };
            sqlAndReplica.Show();
        }

        private void MonitorButton_Click(object sender, EventArgs e)
        {
            Monitor monitor = new Monitor()
            {
                MdiParent = this
            };
            monitor.Show();
        }

        #endregion


    }
}
