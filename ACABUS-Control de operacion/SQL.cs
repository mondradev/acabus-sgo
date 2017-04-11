using ACABUS_Control_de_operacion.Acabus;
using ACABUS_Control_de_operacion.Utils;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace ACABUS_Control_de_operacion
{
    public partial class SQLModule : Form
    {
        private const String STATIONS_CAPTION = "Todas las estaciones";
        private const String DEVICES_CAPTION = "Todos los equipos";

        private MultiThread _multiThread = new MultiThread();
        private Boolean _inStoping = false;
        private DateTime _taskTime;

        public SQLModule()
        {
            this._multiThread.ThreadsChanged += ThreadsChanged;
            this._multiThread.Capacity = 20;
            this.InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.LoadStations();
            this.taskTimeTimer.Tick += TaskTimeTimerOnTick;
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
            Int16 processCount = (Int16)_multiThread.Count;
            this.BeginInvoke(new Action(() =>
            {
                this.threadsStatusLabel.Text = String.Format("{0} Subprocesos", processCount);
            }));
        }

        private void DeviceListOnSelectedChanged(object sender, EventArgs e)
        {
            LoadDevices();
        }

        private void StopTaskButtonOnClick(object sender, EventArgs e)
        {
            _inStoping = true;
            InitializeTask("Deteniendo tarea", 2);
            IncrementProgressBar();
            _multiThread.KillAllThreads(() =>
            {
                IncrementProgressBar();
                this.BeginInvoke(new Action(() =>
                {
                    stopTaskButton.Enabled = false;
                }));
            });
        }

        private void CheckReplicaButtonOnClick(object sender, EventArgs e)
        {
            DataGridSql dataGrdSql = new DataGridSql()
            {
                DataGrid = this.resultTable,
                MultiThreadManager = _multiThread
            };
            dataGrdSql.ClearDataGrid();
            dataGrdSql.PgPath = AcabusData.PG_PATH;
            dataGrdSql.PortDb = 5432;
            dataGrdSql.DataBase = "SITM";
            dataGrdSql.UsernameDb = "postgres";
            dataGrdSql.PasswordDb = "4c4t3k";
            dataGrdSql.UsernameSsh = "teknei";
            dataGrdSql.PasswordSsh = "4c4t3k";

            Device[] devices = GetDevice();

            this.InitializeTask("Revisión de réplica", (Int16)(devices.Length * 2));
            new Thread(() =>
            {
                foreach (Device device in devices)
                {
                    if (this._inStoping) break;

                    String query = device.Type == Device.DeviceType.KVR
                                                        ? PostgreSQL.PENDING_INFO_TO_SEND_DEVICE_R_S
                                                        : PostgreSQL.PENDING_INFO_TO_SEND_DEVICE_I_O;
                    if (device.Type == Device.DeviceType.KVR && ((Kvr)device).IsExtern && !device.Station.Connected)
                        return;
                    dataGrdSql.Host = device.IP;
                    dataGrdSql.ExecuteAndFill(query, (ex) =>
                    {
                        dataGrdSql.AddRow(new String[] {
                        device.GetNumeSeri(),
                                device.IP
                        });
                    });
                }
            }).Start();
        }

        private void RunQueryInDeviceOnClick(object sender, EventArgs e)
        {
            DataGridSql dataGrdSql = new DataGridSql()
            {
                DataGrid = this.resultTable,
                MultiThreadManager = _multiThread
            };
            dataGrdSql.ClearDataGrid();
            dataGrdSql.UsernameDb = "postgres";
            dataGrdSql.PortDb = 5432;
            dataGrdSql.PgPath = AcabusData.PG_PATH;

            Device[] devices = GetDevice();

            String query = queryBox.Text.Trim().Replace("\n", " ").Replace("\r", "");
            if (String.IsNullOrEmpty(query)) return;

            this.InitializeTask("Consulta SQL en masivo", (Int16)(devices.Length * 2));

            foreach (Device device in devices)
            {
                if (this._inStoping) break;
                dataGrdSql.Host = device.IP;
                dataGrdSql.DataBase = "SITM";
                dataGrdSql.PasswordDb = "4c4t3k";
                dataGrdSql.UsernameSsh = "teknei";
                dataGrdSql.PasswordSsh = "4c4t3k";
                if (device.Type == Device.DeviceType.KVR && ((Kvr)device).IsExtern && !device.Station.Connected)
                {
                    Kvr kvrExtern = (Kvr)device;
                    dataGrdSql.DataBase = kvrExtern.DataBaseName;
                    dataGrdSql.PasswordDb = "admin";
                    dataGrdSql.UsernameSsh = "Administrador";
                    dataGrdSql.PasswordSsh = "Administrador*2016";
                }
                dataGrdSql.ExecuteAndFill(query);
            }
        }


        private Device[] GetDevice()
        {
            List<Device> devices = new List<Device>();
            foreach (Trunk trunk in AcabusData.Trunks)
                foreach (Station station in trunk.GetStations())
                {
                    if ((!stationsList.SelectedItem.Equals(STATIONS_CAPTION)
                        && !station.Name.Equals(stationsList.SelectedItem)) || !station.Connected)
                        continue;
                    foreach (Device device in station.GetDevices())
                    {
                        if (!device.Status || !device.HasDataBase) continue;
                        if (deviceList.SelectedItem.Equals(DEVICES_CAPTION))
                        {
                            if (device.Type == Device.DeviceType.KVR && kvrCheck.Checked)
                                devices.Add(device);
                            if (device.Type == Device.DeviceType.PMR && pmrCheck.Checked)
                                devices.Add(device);
                            if (device.Type == Device.DeviceType.TOR && toresCheck.Checked)
                                devices.Add(device);
                        }
                        else
                        {
                            if (device.GetNumeSeri().Equals(deviceList.SelectedItem))
                                devices.Add(device);
                        }
                    }
                }
            return devices.ToArray();
        }

        private void LoadStations()
        {
            stationsList.Items.Clear();
            stationsList.Items.Add(STATIONS_CAPTION);
            foreach (Trunk trunk in AcabusData.Trunks)
                foreach (Station station in trunk.GetStations())
                {
                    stationsList.Items.Add(station.Name);
                }
            stationsList.SelectedIndex = 0;
            LoadDevices();
        }

        private void InitializeTask(String taskName, Int16 totalSteps)
        {
            if (totalSteps <= 0 || String.IsNullOrEmpty(taskName))
                return;
            DesactiveControls();
            this.taskProgressBar.Maximum = totalSteps;
            this.taskProgressBar.Value = 0;
            this.currentTaskLabel.Text = String.Format("Tarea actual: {0}", taskName);
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
                    this.currentTaskLabel.Text = "Tarea actual: Ninguna";
                    this.progressLabel.Text = "0 %";
                    ActiveControls();
                    _inStoping = false;
                    taskTimeTimer.Stop();
                }));
            }
            if (this.taskProgressBar.Value == this.taskProgressBar.Maximum)
                this._multiThread.KillAllThreads();
        }

        private void DesactiveControls()
        {
            checkReplicaButton.Enabled = false;
            runQueryButton.Enabled = false;
            queryBox.Enabled = false;
            kvrCheck.Enabled = false;
            pmrCheck.Enabled = false;
            toresCheck.Enabled = false;
            stationsList.Enabled = false;
            stopTaskButton.Enabled = true;
            deviceList.Enabled = false;
        }

        private void ActiveControls()
        {
            checkReplicaButton.Enabled = true;
            runQueryButton.Enabled = true;
            queryBox.Enabled = true;
            kvrCheck.Enabled = true;
            pmrCheck.Enabled = true;
            toresCheck.Enabled = true;
            stationsList.Enabled = true;
            deviceList.Enabled = true;
            stopTaskButton.Enabled = false;
        }


        private void LoadDevices()
        {
            deviceList.Items.Clear();
            deviceList.Items.Add(DEVICES_CAPTION);
            foreach (Trunk trunk in AcabusData.Trunks)
                foreach (Station station in trunk.GetStations())
                {
                    if (stationsList.SelectedItem.Equals(station.Name))
                    {
                        foreach (Device device in station.GetDevices())
                        {
                            deviceList.Items.Add(device.GetNumeSeri());
                        }
                    }

                }
            deviceList.SelectedIndex = 0;
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
                        value = tempDateTime.ToString();
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
    }
}
