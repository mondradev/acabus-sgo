using ACABUS_Control_de_operacion.Acabus;
using ACABUS_Control_de_operacion.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace ACABUS_Control_de_operacion
{
    public partial class StockCard : Form
    {
        private MultiThread _multiThread;
        private DateTime _taskTime;

        static String _database = "SITM";
        static String _username = "postgres";
        static String _password = "4c4t3k";
        static String usernameSsh = "teknei";
        static String passwordSsh = "4c4t3k";

        public StockCard()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this._multiThread = new MultiThread()
            {
                Capacity = 20
            };
            this.taskTimeTimer.Tick += TaskTimeTimerOnTick;
            this._multiThread.ThreadsChanged += ThreadsChanged;
        }

        #region BARRA DE HERRAMIENTAS

        private void TsbExecute_Click(object sender, EventArgs e)
        {
            ProccessRequest();
        }

        #endregion

        #region FUNCIONES

        private void ProccessRequest()
        {
            Kvr[] kvrs = GetKVRs();
            this.dgvResult.Columns.Clear();
            this.dgvResult.Rows.Clear();
            InitializeTask((Int16)(kvrs.Length * 2));
            this.dgvResult.Columns.Add("KVR", "Id KVR");
            this.dgvResult.Columns.Add("Vta", "Total de Ventas");
            this.dgvResult.Columns.Add("Sto", "Total de Stock");
            this.dgvResult.Columns.Add("Sum", "Total de Suministro");

            foreach (Kvr kvr in kvrs)
            {
                _multiThread.RunTask(String.Format("Check Stock Thread: {0}", kvr.GetNumeSeri()), () =>
                 {
                     if (ConnectionTCP.IsAvaibleIP(kvr.IP))
                     {
                         Int16 sales = Int16.Parse(QuerySales(kvr));
                         Int16 stock = Int16.Parse(QueryStock(kvr));
                         Int16 sum = 0;
                         if (kvr.MaxCard > stock && stock > kvr.MinCard)
                             sum = (Int16)(sales * 1.5);
                         else if (kvr.MinCard > stock && stock != 0)
                             sum = (Int16)(sales * 2);
                         else if (stock == 0)
                             sum = (Int16)kvr.MaxCard;

                         String[] row = new String[] {
                            kvr.GetNumeSeri(),
                            sales.ToString(),
                            stock.ToString(),
                            sum.ToString()
                         };
                         this.BeginInvoke(new Action(() =>
                         {
                             String[] tempRow = row;
                             dgvResult.Rows.Add(tempRow);
                         }));
                     }
                 }, (ex) =>
                 {
                     Trace.WriteLine(String.Format("Ocurrió un error al consultar el host: {0}", kvr.IP), "ERROR");
                 });
            }

        }

        private static string QueryStock(Kvr kvr)
        {
            PostgreSQL sql = PostgreSQL.CreateConnection(
                kvr.IP,
                5432,
                _username,
                kvr.IsExtern ? "admin" : _password,
                kvr.IsExtern ? kvr.DataBaseName : _database);
            String[][] response = sql.ExecuteQuery(
                PostgreSQL.CARD_STOCK);
            //true,
            //kvr.IsExtern ? "Administrador" : usernameSsh,
            //kvr.IsExtern ? "Administrador*2016" : passwordSsh);
            return response.Length > 1 ? response[1][0] : "0";
        }

        private static string QuerySales(Kvr kvr)
        {
            PostgreSQL sql = PostgreSQL.CreateConnection(
                kvr.IP,
                5432,
                _username,
                kvr.IsExtern ? "admin" : _password,
                kvr.IsExtern ? kvr.DataBaseName : _database);
            String[][] response = sql.ExecuteQuery(
                PostgreSQL.TOTAL_SALE);
            //true,
            //kvr.IsExtern ? "Administrador" : usernameSsh,
            //kvr.IsExtern ? "Administrador*2016" : passwordSsh);
            return response.Length > 1 ? response[1][0] : "0";
        }



        #endregion

        private Kvr[] GetKVRs()
        {
            List<Kvr> devices = new List<Kvr>();
            foreach (Trunk trunk in AcabusData.Trunks)
                foreach (Station station in trunk.GetStations())
                {
                    foreach (Device device in station.GetDevices())
                    {
                        if (device.Type == Device.DeviceType.KVR)
                            devices.Add((Kvr)device);
                    }
                }
            return devices.ToArray();
        }

        private void TaskTimeTimerOnTick(object sender, EventArgs e)
        {
            Int64 time = (Int64)DateTime.Now.Subtract(_taskTime).TotalMilliseconds;
            this.execTimeLabel.Text = String.Format("{0} ms", time);
        }

        private void ThreadsChanged(object sender, EventArgs e)
        {
            IncrementProgressBar();
            Int16 processCount = (Int16)_multiThread.Count;
            this.BeginInvoke(new Action(() =>
            {
                this.threadsStatusLabel.Text = String.Format("{0} Subprocesos", processCount);
            }));
        }

        private void StopTaskButtonOnClick(object sender, EventArgs e)
        {
            InitializeTask((Int16)(_multiThread.Count * 2));
            IncrementProgressBar();
            new Thread(() =>
            {
                this.Name = "Deteniendo tarea actual";
                _multiThread.KillAllThreads();
            }).Start();
            stopTaskButton.Enabled = false;
        }

        private void InitializeTask(Int16 totalSteps)
        {
            if (totalSteps <= 0)
                return;
            DesactiveControls();
            this.taskProgressBar.Maximum = totalSteps;
            this.taskProgressBar.Value = 0;
            this.execTimeLabel.Text = "0 ms";
            this._taskTime = DateTime.Now;
            this.taskTimeTimer.Start();
            Application.DoEvents();
        }

        private void IncrementProgressBar()
        {
            this.BeginInvoke(new Action(delegate
            {
                if (this.taskProgressBar.Value + 1 <= this.taskProgressBar.Maximum)
                    this.taskProgressBar.Value++;
                ValidateEndTask();
                string progress = String.Format("Progreso: {0}%", (int)((float)this.taskProgressBar.Value / (float)this.taskProgressBar.Maximum * 100));
                this.progressLabel.Text = progress;
            }));
        }

        private void ValidateEndTask()
        {
            if (!_multiThread.IsRunning())
            {
                this.BeginInvoke(new Action(delegate
                {
                    this.taskProgressBar.Value = 0;
                    this.progressLabel.Text = "0 %";
                    ActiveControls();
                    taskTimeTimer.Stop();
                }));
            }
        }

        private void DesactiveControls()
        {
            tsbExecute.Enabled = false;
            stopTaskButton.Enabled = true;
        }

        private void ActiveControls()
        {
            tsbExecute.Enabled = true;
            stopTaskButton.Enabled = false;
        }
    }
}
