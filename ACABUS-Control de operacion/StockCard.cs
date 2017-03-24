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

namespace ACABUS_Control_de_operacion
{
    public partial class StockCard : Form
    {
        private MultiThread _multiThread;
        private Boolean _inStoping = false;
        private DateTime _taskTime;

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
            KVR[] kvrs = GetKVRs();
            InitializeTask((Int16)(kvrs.Length * 2));
            this.dgvResult.Columns.Add("KVR", "Id KVR");
            this.dgvResult.Columns.Add("Vta", "Total de Ventas");
            this.dgvResult.Columns.Add("Sto", "Total de Stock");
            this.dgvResult.Columns.Add("Sum", "Total de Suministro");

            foreach (KVR kvr in kvrs)
            {
                _multiThread.RunTask(() =>
                {
                    if (IsAvaibleIP(kvr.IP))
                    {
                        Int16 sales = Int16.Parse(QuerySales(kvr.IP));
                        Int16 stock = Int16.Parse(QueryStock(kvr.IP));
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
                    Trace.WriteLine(String.Format("Ocurrió un error al consultar el host: {0}", kvr.IP));
                });
            }

        }


        private bool IsAvaibleIP(string strIP)
        {
            ConnectionTCP cnnTCP = new ConnectionTCP();
            if (cnnTCP.SendToPing(strIP, 3))
                return true;
            return false;
        }

        private string QueryStock(string strEquip)
        {
            PostgreSQL sql = PostgreSQL.CreateConnection(strEquip, 5432, "postgres", "4c4t3k", "SITM");
            sql.TimeOut = 1000;
            String[][] response = sql.ExecuteQuery(PostgreSQL.CARD_STOCK);
            return response.Length > 1 ? response[1][0] : "0";
        }

        private static string QuerySales(string strEquip)
        {
            PostgreSQL sql = PostgreSQL.CreateConnection(strEquip, 5432, "postgres", "4c4t3k", "SITM");
            sql.TimeOut = 1000;
            String[][] response = sql.ExecuteQuery(PostgreSQL.TOTAL_SALE);
            return response.Length > 1 ? response[1][0] : "0";
        }



        #endregion

        private KVR[] GetKVRs()
        {
            List<KVR> devices = new List<KVR>();
            foreach (Trunk trunk in Trunk.Trunks)
                foreach (Station station in trunk.Stations)
                {
                    foreach (Device device in station.Devices)
                    {
                        if (device.Type == Device.DeviceType.KVR)
                            devices.Add((KVR)device);
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
            if (!_inStoping)
                IncrementProgressBar();
            Int16 processCount = (Int16)_multiThread.Threads.Count;
            this.BeginInvoke(new Action(() =>
            {
                this.threadsStatusLabel.Text = String.Format("{0} Subprocesos", processCount);
            }));
        }

        private void StopTaskButtonOnClick(object sender, EventArgs e)
        {
            _inStoping = true;
            InitializeTask(2);
            IncrementProgressBar();
            new Thread(() =>
            {
                this.Name = "Deteniendo tarea actual";
                _multiThread.KillAllThreads();
                IncrementProgressBar();
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
                Trace.WriteLine(progress);
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
                    _inStoping = false;
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
