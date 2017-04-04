using System;
using System.Windows.Forms;

namespace ACABUS_Control_de_operacion
{
    public partial class MainForm : Form
    {
        private SQLModule sqlAndReplica;
        private Monitor monitor;
        private StockCard stockCard;
        private NetworkingMonitor netMonitor;
        private DBKVRsExternos bdkvrExterns;

        public static MainForm Instance { get; set; }

        public MainForm()
        {
            InitializeComponent();
            Instance = this;
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
            if (stockCard == null || stockCard.IsDisposed)
                stockCard = new StockCard()
                {
                    MdiParent = this
                };
            stockCard.Show();
            stockCard.BringToFront();
        }

        private void DailyTaskButton_Click(object sender, EventArgs e)
        {
            if (sqlAndReplica == null || sqlAndReplica.IsDisposed)
                sqlAndReplica = new SQLModule()
                {
                    MdiParent = this
                };
            sqlAndReplica.Show();
            sqlAndReplica.BringToFront();
        }

        private void MonitorButton_Click(object sender, EventArgs e)
        {
            if (monitor == null || monitor.IsDisposed)
                monitor = new Monitor()
                {
                    MdiParent = this
                };
            monitor.Show();
            monitor.BringToFront();
        }

        private void MonitorLanButton_Click(object sender, EventArgs e)
        {
            if (netMonitor == null || netMonitor.IsDisposed)
                netMonitor = new NetworkingMonitor()
                {
                    MdiParent = this
                };
            netMonitor.Show();
            netMonitor.BringToFront();

        }

        #endregion

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void VisorDeEventosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (EventViewer.Instance == null || EventViewer.Instance.IsDisposed)
                EventViewer.Instance = new EventViewer();
            EventViewer.Instance.ShowDialog();
        }

        private void ConexiónBDKVRExternosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (bdkvrExterns == null || bdkvrExterns.IsDisposed)
                bdkvrExterns = new DBKVRsExternos();
            bdkvrExterns.ShowDialog();
        }
    }
}
