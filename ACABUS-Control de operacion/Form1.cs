using System;
using System.Windows.Forms;

namespace ACABUS_Control_de_operacion
{
    public partial class frmMain : Form
    {
        private SQLModule sqlAndReplica;
        private Monitor monitor;
        private StockCard stockCard;

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
            if (stockCard == null)
                stockCard = new StockCard()
                {
                    MdiParent = this
                };
            stockCard.Show();
        }

        private void DailyTaskButton_Click(object sender, EventArgs e)
        {
            if (sqlAndReplica == null)
                sqlAndReplica = new SQLModule()
                {
                    MdiParent = this
                };
            sqlAndReplica.Show();
        }

        private void MonitorButton_Click(object sender, EventArgs e)
        {
            if (monitor == null)
                monitor = new Monitor()
                {
                    MdiParent = this
                };
            monitor.Show();
        }


        #endregion

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
