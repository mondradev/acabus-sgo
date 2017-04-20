using ACABUS_Control_de_operacion.Acabus;
using ACABUS_Control_de_operacion.Utils;
using Npgsql;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ACABUS_Control_de_operacion
{
    public partial class Monitor : Form
    {
        private MultiThread _multiThread = new MultiThread();
        private UncirculatedVehicles _uncirculatedVehiclesDialog;
        private String[] _uncirculatedVehicles;
        private DataGridSql _dgDisconnectVeh;
        private DataGridSql _dgBadCounters;
        private DataGridSql _dgAlarmTrunk;

        public Monitor()
        {
            this._multiThread.Capacity = 20;
            this.InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            LoadUncirculatedVehicles();

            _dgDisconnectVeh = new DataGridSql()
            {
                Host = AcabusData.BD_SERVER_IP,
                UsernameDb = AcabusData.PG_USERNAME,
                PasswordDb = AcabusData.PG_PASSWORD_SERVER,
                UsernameSsh = AcabusData.SSH_USERNAME_SERVER,
                PasswordSsh = AcabusData.SSH_PASSWORD_SERVER,
                DataBase = AcabusData.DATABASE_NAME,
                PortDb = AcabusData.PG_PORT,
                PgPath = AcabusData.PG_PATH_SERVER,
                DataGrid = disconnectVehicleTable,
                MultiThreadManager = _multiThread,
                AutoClearRows = true
            };

            _dgBadCounters = new DataGridSql()
            {
                Host = AcabusData.BD_SERVER_IP,
                UsernameDb = AcabusData.PG_USERNAME,
                PasswordDb = AcabusData.PG_PASSWORD_SERVER,
                UsernameSsh = AcabusData.SSH_USERNAME_SERVER,
                PasswordSsh = AcabusData.SSH_PASSWORD_SERVER,
                DataBase = AcabusData.DATABASE_NAME,
                PortDb = AcabusData.PG_PORT,
                PgPath = AcabusData.PG_PATH_SERVER,
                DataGrid = badCountersTable,
                MultiThreadManager = _multiThread,
                AutoClearRows = true
            };


            _dgAlarmTrunk = new DataGridSql()
            {
                Host = AcabusData.BD_SERVER_IP,
                UsernameDb = AcabusData.PG_USERNAME,
                PasswordDb = AcabusData.PG_PASSWORD_SERVER,
                UsernameSsh = AcabusData.SSH_USERNAME_SERVER,
                PasswordSsh = AcabusData.SSH_PASSWORD_SERVER,
                DataBase = AcabusData.DATABASE_NAME,
                PortDb = AcabusData.PG_PORT,
                PgPath = AcabusData.PG_PATH_SERVER,
                DataGrid = alarmTrunkTable,
                MultiThreadManager = _multiThread,
                AutoClearRows = true
            };
        }

        private void LoadUncirculatedVehicles()
        {
            String uncirculatedVehicles = UncirculatedVehicles.FILENAME_UNCIRCULATED_VEHICLES;
            if (File.Exists(uncirculatedVehicles))
                this._uncirculatedVehicles = File.ReadAllLines(uncirculatedVehicles);
            else
                this._uncirculatedVehicles = null;
        }

        private void StartOnClick(object sender, EventArgs e)
        {
            Trace.WriteLine("Estado: Iniciando monitor", "INFO");
            RefreshMonitor();
            timer1.Start();
            startButton.Enabled = false;
            StopButton.Enabled = true;
        }
        private void StopOnClick(object sender, EventArgs e)
        {
            Trace.WriteLine("Estado: Deteniendo monitor", "INFO");
            timer1.Stop();
            StopButton.Enabled = false;
            _multiThread.KillAllThreads(() =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    startButton.Enabled = true;
                }));
            });
        }


        private void RefreshMonitor()
        {
            if (!ConnectionTCP.IsAvaibleIP(AcabusData.BD_SERVER_IP))
            {
                Trace.WriteLine(String.Format("El servidor {0} no se encuentra disponible", AcabusData.BD_SERVER_IP), "ERROR");
                return;
            }

            String queryLastConnectionVehi = String.Format(AcabusData.QUERY_LAST_CONNECTION_VEHI, ProcessArray(this._uncirculatedVehicles));
            this._dgDisconnectVeh.ExecuteAndFill(queryLastConnectionVehi);

            this._dgBadCounters.ExecuteAndFill(AcabusData.QUERY_STATUS_COUNTER);

            this._dgAlarmTrunk.ExecuteAndFill(AcabusData.QUERY_ALAR_TRUNK);
        }

        private object ProcessArray(string[] array)
        {
            String arrayInLine = "''";
            if (array != null)
                foreach (String item in array)
                    arrayInLine = String.Format("{0},'{1}'", arrayInLine, item.Split(',')[0]);
            return arrayInLine;
        }

        private void ResultTableOnRowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            DataGridView table = (DataGridView)sender;
            Int16 columnsCount = (Int16)table.Columns.Count;
            for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
                for (int j = 0; j < columnsCount; j++)
                {
                    Object cell = table.Rows[i].Cells[j].Value;
                    String value = cell != null ? cell.ToString() : "";
                    if (Regex.IsMatch(value, "^[0-9]{2}:[0-9]{2}:[0-9]{2}$"))
                    {
                        value = TimeSpan.Parse(value).ToString();
                    }
                    else
                    if (Regex.IsMatch(value, "[0-9]{2,4}-[0-9]{2}-[0-9]{2,4}|[0-9]{2,4}/[0-9]{2}/[0-9]{2,4}|[0-9]{2}-[A-Za-z]{3}-[0-9]{2}"))
                    {
                        DateTime tempDateTime = DateTime.Parse(value);
                        if (Regex.IsMatch(value, "[0-9]{2}:[0-9]{2}:[0-9]{2}"))
                            value = tempDateTime.ToString();
                        else
                            value = tempDateTime.ToShortDateString();
                    }
                    else
                    if (Regex.IsMatch(value, "^[0-9]{1,}.[0-9]{1,}$"))
                    {
                        value = Double.Parse(value).ToString();
                    }
                    else
                    if (Regex.IsMatch(value, "^[0-9]{1,}$"))
                    {
                        value = Int32.Parse(value).ToString();
                    }
                    table.Rows[i].Cells[j].Value = value;
                }
        }

        private void RefreshOnTick(object sender, EventArgs e)
        {
            Trace.WriteLine("Estado: Actualizando monitor", "INFO");
            RefreshMonitor();
        }

        private void UncirculatedVehiclesButton_Click(object sender, EventArgs e)
        {
            if (this._uncirculatedVehiclesDialog == null || this._uncirculatedVehiclesDialog.IsDisposed)
            {
                this._uncirculatedVehiclesDialog = new UncirculatedVehicles()
                {

                };
                this._uncirculatedVehiclesDialog.FormClosing += (senderClosing, args) =>
                {
                    LoadUncirculatedVehicles();
                };
            }
            this._uncirculatedVehiclesDialog.ShowDialog(MainForm.Instance);
        }
    }
}
