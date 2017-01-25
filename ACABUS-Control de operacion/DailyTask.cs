using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
                                    row.Cells[3].Value = response[0];
                                    row.Cells[4].Value = dif.ToString();
                                    if (dif > TimeSpan.Parse("00:10:00.000")) {
                                        this._replicaDown.Add(entry);
                                    }
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
            int maxTime = 5;
            String response = "";
            while (time < maxTime) {
                try {
                    using (Ssh ssh = new Ssh(ip, "teknei", "4c4t3k")) {
                        if (ssh.IsConnected()) {
                            response = ssh.SendCommand(String.Format("PGPASSWORD='4c4t3k' /opt/PostgreSQL/9.3/bin/psql -U postgres -d SITM -c \"{0}\" | grep ','", query));
                            break;

                        }
                    }
                }
                catch (Exception) {
                    time++;
                    response = "Error al obtener información,,,";
                    Console.WriteLine(String.Format("{0}: Intentando por SSH de nuevo", ip));
                }
            }
            return response;
        }

        private void restartReplicaButton_Click(object sender, EventArgs e) {

            // Añadir ejecución en hilos
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
                                ssh.SendCommand("ps -fea | grep SNAP*.jar");
                                ssh.SendCommand("echo \"Replica reiniciada\"");
                            }
                        }
                        Application.DoEvents();
                    }
                    catch (Exception) { }
                    Thread.Sleep(150);
                    response = entry.Type == Device.DeviceType.KVR ? QueryTran(ip, SQL).Split(',') : QueryVal(ip, SQL).Split(',');
                    String pendingAfter = response[0];
                    string[] rowArray = new string[] { entry.GetNumeSeri(), pendingBefore, pendingAfter };
                    this.resultTable.Rows.Add(rowArray);
                    Application.DoEvents();
                }

            }
        }
    }
}
