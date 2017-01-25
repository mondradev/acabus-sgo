using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            foreach (Trunk trunk in Trunk.Trunks)
                foreach (Station station in trunk.Stations)
                    foreach (Device entry in station.Devices) {
                        string[] rowArray = new string[] { entry.GetNumeSeri() };
                        this.resultTable.Rows.Add(rowArray);
                        Thread task = new Thread(() => {
                        retry:
                            if (isAvaibleIP(entry.IP)) {
                                String[] response;
                                try {
                                    response = entry.Type == Device.DeviceType.KVR ? QueryTran(entry.IP, SQL).Split(',') : QueryVal(entry.IP, SQL).Split(',');
                                    if (response[1] == "")
                                        goto retry;
                                    var row = GetRow(this.resultTable, entry.GetNumeSeri()) as DataGridViewRow;

                                    DateTime lastTran = DateTime.Parse(response[1]);
                                    DateTime lastTranSend = DateTime.Parse(response[2]);
                                    TimeSpan dif = lastTran - lastTranSend;
                                    row.Cells[1].Value = lastTran.ToString();
                                    row.Cells[2].Value = lastTranSend.ToString();
                                    row.Cells[3].Value = response[0];
                                    row.Cells[4].Value = dif.ToString();
                                    if (dif > TimeSpan.Parse("00:05:00.000")) {
                                        this._replicaDown.Add(entry);
                                    }
                                }
                                catch (Exception) { }
                            }
                            Application.DoEvents();
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
                try {
                    return QueryBySSH(iP, SQL.selectQuery(PostgreSQL.queryConsult.queryVal));
                }
                catch (Exception ex2) {
                    Trace.WriteLine(String.Format("{0}: {1}", iP, ex2.Message));
                    return "Error al obtener información,,,";
                }
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
                try {
                    return QueryBySSH(strEquip, SQL.selectQuery(PostgreSQL.queryConsult.queryTran));
                }
                catch (Exception ex2) {
                    Trace.WriteLine(String.Format("{0}: {1}", strEquip, ex2.Message));
                    return "Error al obtener información,,,";
                }
            }
        }

        private String QueryBySSH(String ip, String query) {
            String response = String.Empty;
            using (Ssh ssh = new Ssh(ip, "teknei", "4c4t3k")) {
                if (ssh.IsConnected()) {
                    response = ssh.SendCommand(String.Format("PGPASSWORD='4c4t3k' /opt/PostgreSQL/9.3/bin/psql -U postgres -d SITM -c \"{0}\" | grep ','", query));
                }
            }
            return response;
        }

        private void restartReplicaButton_Click(object sender, EventArgs e) {
            using (Ssh exec = new Ssh("172.17.16.22", "teknei", "4c4t3k")) {
                exec.SendCommand("echo $HOSTNAME");
            }
            if (this._replicaDown.Count > 0) {

                this.resultTable.Rows.Clear();
                this.resultTable.Columns.Clear();

                Dictionary<string, string> SQL = loadConfigurationPostgreSQL();


                this.resultTable.Columns.Add("numeSeri", "Número Serie");
                this.resultTable.Columns.Add("countTranB", "Trans. a/reiniciar");
                this.resultTable.Columns.Add("countTranA", "Trans. d/reiniciar");

                foreach (var entry in this._replicaDown) {
                    var ip = entry.IP;
                    String[] response = entry.Type == Device.DeviceType.KVR ? QueryTran(ip, SQL).Split(',') : QueryVal(ip, SQL).Split(',');
                    String pendingBefore = response[0];
                    using (Ssh ssh = new Ssh(ip, "teknei", "4c4t3k")) {
                        if (ssh.IsConnected()) {
                            String responseRestart = ssh.SendCommand("killall java");
                            Trace.WriteLine(String.Format("Respuesta {0}: {1}", ip, responseRestart));
                            responseRestart = ssh.SendCommand("sh /home/teknei/SITM/SHELL/JAR_CONFIG.sh start");
                            Trace.WriteLine(String.Format("Respuesta {0}: {1}", ip, responseRestart));
                            responseRestart = ssh.SendCommand("ps -fea | grep SNAP*.jar");
                            Trace.WriteLine(String.Format("Respuesta {0}: {1}", ip, responseRestart));
                            responseRestart = ssh.SendCommand("echo \"Listo $HOSTNAME\"");
                            Trace.WriteLine(String.Format("Respuesta {0}: {1}", ip, responseRestart));
                        }
                    }
                    Thread.Sleep(150);
                    response = entry.Type == Device.DeviceType.KVR ? QueryTran(ip, SQL).Split(',') : QueryVal(ip, SQL).Split(',');
                    String pendingAfter = response[0];
                    string[] rowArray = new string[] { entry.GetNumeSeri(), pendingBefore, pendingAfter };
                    this.resultTable.Rows.Add(rowArray);
                }

            }
        }
    }
}
