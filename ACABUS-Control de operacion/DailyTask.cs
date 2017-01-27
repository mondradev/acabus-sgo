using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace ACABUS_Control_de_operacion {
    public partial class DailyTask : Form {
        List<Device> _replicaDown = new List<Device>();

        public DailyTask() {
            InitializeComponent();
            Trunk.LoadConfiguration();
            this.WindowState = FormWindowState.Maximized;
        }

        private void checkReplicaButton_Click(object sender, EventArgs e) {
            this.resultTable.Rows.Clear();
            this.resultTable.Columns.Clear();
            this._replicaDown.Clear();

            Dictionary<string, string> SQL = loadConfigurationPostgreSQL();

            this.resultTable.Columns.Add("numeSeri", "Número Serie");
            this.resultTable.Columns.Add("lastTran", "Última transacción");
            this.resultTable.Columns.Add("lastSend", "Último enviado");
            this.resultTable.Columns.Add("countTran", "Transacciones pendientes");
            this.resultTable.Columns.Add("timeoutReplica", "Tiempo sin replicar");

            this.taskProgressBar.Maximum = Trunk.Trunks[0].CountDevices();
            this.taskProgressBar.Value = 0;

            this.currentTaskLabel.Text = "Tarea actual: Verificando réplica";

            foreach (Trunk trunk in Trunk.Trunks)
                foreach (Station station in trunk.Stations)
                    foreach (Device entry in station.Devices) {
                        string[] rowArray = new string[] { entry.GetNumeSeri() };
                        this.resultTable.Rows.Add(rowArray);
                        Thread task = new Thread(() => {
                            if (isAvaibleIP(entry.IP)) {
                                String[] response;
                                try {
                                    response = entry.Type == Device.DeviceType.KVR ? QueryTran(entry.IP, SQL).Split(',') : QueryVal(entry.IP, SQL).Split(',');
                                    var row = GetRow(this.resultTable, entry.GetNumeSeri()) as DataGridViewRow;

                                    DateTime lastTran = DateTime.Parse(response[1]);
                                    DateTime lastTranSend = DateTime.Parse(response[2]);
                                    TimeSpan dif = lastTran - lastTranSend;
                                    row.Cells[1].Value = lastTran.ToString();
                                    row.Cells[2].Value = lastTranSend.ToString();
                                    row.Cells[3].Value = response[0].Trim();
                                    row.Cells[4].Value = dif.ToString();
                                    if (dif > TimeSpan.Parse("00:10:00.000")) {
                                        this._replicaDown.Add(entry);
                                    }
                                    this.BeginInvoke(new Action(delegate {
                                        this.taskProgressBar.Value++;
                                        Trace.WriteLine(String.Format("Progreso: {0}%", (int)((float)this.taskProgressBar.Value / (float)this.taskProgressBar.Maximum * 100)));
                                    }));
                                }
                                catch (Exception) { }
                            }
                        });
                        task.Start();
                        Application.DoEvents();
                    }
        }

        private String QueryVal(string iP, Dictionary<string, string> sQL) {
            PostgreSQL SQL = new PostgreSQL();
            try {
                return SQL.stockCard(sQL, iP, PostgreSQL.queryConsult.queryVal);
            }
            catch (Exception ex1) {
                Trace.WriteLine(ex1.Message);
                Trace.WriteLine(String.Format("Intentando por SSH {0}", iP));
                return QueryBySSH(iP, SQL.selectQuery(PostgreSQL.queryConsult.queryVal));
            }
        }

        private DataGridViewRow GetRow(DataGridView resultTable, string key) {
            foreach (DataGridViewRow row in resultTable.Rows) {
                if (row.Cells[0].Value.Equals(key))
                    return row;
            }
            return null;
        }

        private Dictionary<string, string> loadConfigurationEquip() {
            Configuration config = new Configuration();
            return config.informationKVR();
        }

        private Dictionary<string, string> loadConfigurationPostgreSQL() {
            Configuration config = new Configuration();
            return config.informationSQL();
        }


        private bool isAvaibleIP(string strIP) {
            ConnectionTCP cnnTCP = new ConnectionTCP();
            if (cnnTCP.sendToPing(strIP, 3))
                return true;
            return false;
        }

        private string QueryTran(string strEquip, Dictionary<string, string> dicConfig) {
            PostgreSQL SQL = new PostgreSQL();
            try {
                return SQL.stockCard(dicConfig, strEquip, PostgreSQL.queryConsult.queryTran);
            }
            catch (Exception ex1) {
                Trace.WriteLine(ex1.Message);
                Trace.WriteLine(String.Format("Intentando por SSH {0}", strEquip));
                return QueryBySSH(strEquip, SQL.selectQuery(PostgreSQL.queryConsult.queryTran));
            }
        }

        private String QueryBySSH(String ip, String query) {
            int time = 0;
            int maxTime = 10;
            String response = "";
            while (time < maxTime) {
                try {
                    using (Ssh ssh = new Ssh(ip, "teknei", "4c4t3k")) {
                        if (ssh.IsConnected()) {
                            response = ssh.SendCommand(String.Format("PGPASSWORD='4c4t3k' /opt/PostgreSQL/9.3/bin/psql -U postgres -d SITM -c \"{0}\" | grep ','", query));
                            if (String.IsNullOrEmpty(response))
                                throw new Exception(String.Format("{0}: Sin respuesta", ip));
                            break;

                        }
                    }
                }
                catch (Exception) {
                    time++;
                    response = "Error al obtener información,,,";
                    Trace.WriteLine(String.Format("{0}: Intentando por SSH de nuevo", ip));
                }
            }
            return response;
        }


        List<Device> _replicaDownRetry = new List<Device>();

        private void restartReplicaButton_Click(object sender, EventArgs e) {

            if (this._replicaDown.Count > 0 || this._replicaDownRetry.Count > 0) {
                if (this._replicaDownRetry.Count > 0) {
                    this._replicaDown.Clear();
                    this._replicaDown.AddRange(_replicaDownRetry);
                    _replicaDownRetry.Clear();
                }
                this.currentTaskLabel.Text = "Tarea actual: Reiniciando réplica";
                this.taskProgressBar.Maximum = this._replicaDown.Count;
                this.taskProgressBar.Value = 0;

                this.resultTable.Rows.Clear();
                this.resultTable.Columns.Clear();

                Dictionary<string, string> SQL = loadConfigurationPostgreSQL();

                this.resultTable.Columns.Add("numeSeri", "Número Serie");
                this.resultTable.Columns.Add("countTranB", "Trans. a/reiniciar");
                this.resultTable.Columns.Add("countTranA", "Trans. d/reiniciar");
                int timeMax = 10;
                foreach (var entry in this._replicaDown) {
                    var ip = entry.IP;
                    string[] rowArray = new string[] { entry.GetNumeSeri() };
                    this.resultTable.Rows.Add(rowArray);
                    new Thread(() => {
                        int time = 0;
                        DataGridViewRow row = GetRow(resultTable, entry.GetNumeSeri());
                        String[] response = entry.Type == Device.DeviceType.KVR ? QueryTran(ip, SQL).Split(',') : QueryVal(ip, SQL).Split(',');
                        String pendingBefore = response[0].Trim();
                        row.Cells[1].Value = pendingBefore;
                        while (time < timeMax)
                            try {
                                using (Ssh ssh = new Ssh(ip, "root", "t43ck4n&3u12")) {
                                    if (ssh.IsConnected()) {
                                        ssh.SendCommand("rm /home/teknei/SITM/CONFIG/PID_SAVE.txt");
                                        ssh.SendCommand("killall java");
                                    }
                                }
                                using (Ssh ssh = new Ssh(ip, "teknei", "4c4t3k")) {
                                    if (ssh.IsConnected()) {
                                        ssh.SendCommand("killall java");
                                        ssh.SendCommand("sh /home/teknei/SITM/SHELL/JAR_CONFIG.sh start");
                                        Thread.Sleep(1000);
                                        Trace.WriteLine(ip + ": " + ssh.SendCommand("ps -fea | grep 'SNAPSHOT.jar'"));
                                    }
                                }
                                this.BeginInvoke(new Action(delegate {
                                    this.taskProgressBar.Value++;
                                    Trace.WriteLine(String.Format("Progreso: {0}%", (int)((float)this.taskProgressBar.Value / (float)this.taskProgressBar.Maximum * 100)));
                                }));
                                Thread.Sleep(20000);
                                response = entry.Type == Device.DeviceType.KVR ? QueryTran(ip, SQL).Split(',') : QueryVal(ip, SQL).Split(',');
                                String pendingAfter = response[0].Trim();
                                row.Cells[2].Value = pendingAfter;
                                if (Int16.Parse(pendingAfter) > 0) {
                                    _replicaDownRetry.Add(entry);
                                }
                                break;
                            }
                            catch (Exception) { time++; }

                    }).Start();
                    Application.DoEvents();
                }

            }
        }
    }
}
