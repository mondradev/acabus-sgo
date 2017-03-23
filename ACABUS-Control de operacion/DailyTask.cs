using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace ACABUS_Control_de_operacion
{
    public partial class DailyTask : Form
    {
        private const String STATIONS_CAPTION = "Todas las estaciones";
        private const String DEVICES_CAPTION = "Todos los equipos";

        private MultiThread _multiThread = new MultiThread();
        private List<String[]> _tempRows = new List<String[]>();
        private Boolean _isReadyTable = false;
        private Boolean _inStoping = false;
        private DateTime _taskTime;
        private bool _hasColumns;
        private int _countcolumns;

        public DailyTask()
        {
            Trunk.LoadConfiguration();
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
            Int16 processCount = (Int16)_multiThread.Threads.Count;
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
            new Thread(() =>
            {
                this.Name = "Deteniendo tarea actual";
                _multiThread.KillAllThreads();
                IncrementProgressBar();
            }).Start();
            stopTaskButton.Enabled = false;
        }

        private void CheckReplicaButtonOnClick(object sender, EventArgs e)
        {
            this.resultTable.Rows.Clear();
            this.resultTable.Columns.Clear();
            this._hasColumns = false;
            this._countcolumns = 0;

            this.GenerateColumns(new String[] {
                "Número de Serie", "Dirección IP", "Última trans. registrada",
                "Primera trans. s/enviar", "Trans. pendientes", "Tiempo sin replicar"
            }, () =>
            {

                this.resultTable.Columns[2].ValueType = typeof(DateTime);
                this.resultTable.Columns[3].ValueType = typeof(DateTime);
                this.resultTable.Columns[4].ValueType = typeof(Int16);
                this.resultTable.Columns[5].ValueType = typeof(TimeSpan);
            });

            this._hasColumns = true;
            this._countcolumns = 6;

            Device[] devices = GetDevice();

            this.InitializeTask("Revisión de réplica", (Int16)(devices.Length * 2));
            new Thread(() =>
            {
                foreach (Device device in devices)
                {
                    if (this._inStoping) break;
                    this._multiThread.RunTask(() =>
                    {
                        String query = device.Type == Device.DeviceType.KVR
                                                        ? PostgreSQL.PENDING_INFO_TO_SEND_DEVICE_R_S
                                                        : PostgreSQL.PENDING_INFO_TO_SEND_DEVICE_I_O;
                        if (this.IsAvaibleIP(device.IP))
                            this.RunQueryInDevice(query, device);
                        else
                            throw new Exception(String.Format("No hay accesos al host {0}", device.IP));
                    }, (ex) =>
                    {
                        if (ex is ThreadAbortException || ex is ThreadInterruptedException) return;

                        this.BeginInvoke(new Action(() =>
                        {
                            String[] row = new String[] {
                                device.GetNumeSeri(),
                                device.IP
                            };
                            if (this._isReadyTable)
                                this.resultTable.Rows.Add(row);
                            else
                                this._tempRows.Add(row);
                        }));

                    });
                }
            }).Start();
        }

        private void RunQueryInDeviceOnClick(object sender, EventArgs e)
        {
            this.resultTable.Rows.Clear();
            this.resultTable.Columns.Clear();

            Device[] devices = GetDevice();
            this._hasColumns = false;
            this._countcolumns = 0;

            this.InitializeTask("Consulta SQL en masivo", (Int16)(devices.Length * 2));

            String query = queryBox.Text.Trim().Replace("\n", " ").Replace("\r", "");

            foreach (Device device in devices)
            {
                if (this._inStoping) break;
                this._multiThread.RunTask(() =>
                {
                    if (this.IsAvaibleIP(device.IP))
                        this.RunQueryInDevice(query, device);

                });
            }
        }

        private void RunQueryInDevice(string query, Device device)
        {

            PostgreSQL psql = PostgreSQL.CreateConnection(device.IP, 5432, "postgres", "4c4t3k", "SITM");
            psql.TimeOut = 1000;
            String[][] response;
            try
            {
                response = psql.ExecuteQuery(query);
            }
            catch (NpgsqlException ex)
            {
                Trace.WriteLine(ex.Message);
                Trace.WriteLine(String.Format("Host {0} falló al realizar consulta PSQL a través del controlador de PostgreSQL\nIntentando por SSH con la credenciales\nUsername: {1}\nPassword: ******", device.IP, "teknei"));
                response = psql.ExcuteQueryBySsh(query, "teknei", "4c4t3k");
            }
            if (response.Length <= 0)
                throw new Exception(String.Format("El host {0} no respondió con un resultado", device.IP));

            if (!this._hasColumns || this._countcolumns < response[0].Length)
            {
                this.GenerateColumns(response[0]);
                this._hasColumns = true;
                this._countcolumns = (Int16)response[0].Length;
            }
            for (int i = 1; i < response.Length; i++)
            {
                String[] row = response[i];
                if (row[0].Contains("Error al obtener información"))
                    throw new Exception(String.Format("Error al obtener la información del host {0}", device.IP));
                this.BeginInvoke(new Action(delegate
                {
                    String[] tmpRow = row;
                    if (this._isReadyTable)
                        this.resultTable.Rows.Add(tmpRow);
                    else
                        this._tempRows.Add(tmpRow);
                }));
            }
        }

        private Device[] GetDevice()
        {
            List<Device> devices = new List<Device>();
            foreach (Trunk trunk in Trunk.Trunks)
                foreach (Station station in trunk.Stations)
                {
                    if (!stationsList.SelectedItem.Equals(STATIONS_CAPTION)
                        && !station.Name.Equals(stationsList.SelectedItem))
                        continue;
                    foreach (Device device in station.Devices)
                    {
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
            foreach (Station station in Trunk.Trunks[0].Stations)
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
                    this.currentTaskLabel.Text = "Tarea actual: Ninguna";
                    ActiveControls();
                    _inStoping = false;
                    taskTimeTimer.Stop();
                }));
            }
        }

        private bool IsAvaibleIP(string strIP)
        {
            ConnectionTCP cnnTCP = new ConnectionTCP();
            if (cnnTCP.SendToPing(strIP, 3))
                return true;
            return false;
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

        private void GenerateColumns(String[] row, Action columnsReady = null)
        {
            _isReadyTable = false;
            Int16 i = 0;
            this.BeginInvoke(new Action(delegate
            {
                String[,] rows = GetRows(this.resultTable.Rows);
                this.resultTable.Rows.Clear();
                this.resultTable.Columns.Clear();
                foreach (var item in row)
                {
                    this.resultTable.Columns.Add("column" + i, item);
                    this.resultTable.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    this.resultTable.Columns[i].ReadOnly = true;
                    i++;
                }
                Application.DoEvents();
                if (rows != null)
                    this.resultTable.Rows.Add(rows);
                foreach (var item in this._tempRows.ToArray())
                    this.resultTable.Rows.Add(item);
                this._tempRows.Clear();
                _isReadyTable = true;
                if (columnsReady != null)
                    columnsReady.Invoke();
            }));
        }

        private string[,] GetRows(DataGridViewRowCollection rows)
        {
            int rowsCount = rows.Count;
            if (rows.Count <= 0)
                return null;
            int cellCount = rows[0].Cells.Count;
            String[,] rowsBackup = new string[rowsCount, cellCount];
            for (int i = 0; i < rowsCount; i++)
                for (int j = 0; j < cellCount; j++)
                {
                    var val = rows[i].Cells[j].Value;
                    if (val != null)
                        rowsBackup[i, j] = val.ToString();
                }
            return rowsBackup;
        }

        private void LoadDevices()
        {
            deviceList.Items.Clear();
            deviceList.Items.Add(DEVICES_CAPTION);
            foreach (Trunk trunk in Trunk.Trunks)
                foreach (Station station in trunk.Stations)
                {
                    if (stationsList.SelectedItem.Equals(station.Name))
                    {
                        foreach (Device device in station.Devices)
                        {
                            deviceList.Items.Add(device.GetNumeSeri());
                        }
                    }

                }
            deviceList.SelectedIndex = 0;
        }

        private void ResultTableOnRowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            Int16 columnsCount = (Int16)this.resultTable.Columns.Count;
            for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
                for (int j = 0; j < columnsCount; j++)
                {
                    Object cell = this.resultTable.Rows[i].Cells[j].Value;
                    String value = cell != null ? cell.ToString() : "";
                    if (Regex.IsMatch(value, "^[0-9]{2}:[0-9]{2}:[0-9]{2}$"))
                    {
                        value = TimeSpan.Parse(value).ToString();
                    }
                    else
                    if (Regex.IsMatch(value, "[0-9]{2,4}-[0-9]{2}-[0-9]{2,4}|[0-9]{2,4}/[0-9]{2}/[0-9]{2,4}"))
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
                    this.resultTable.Rows[i].Cells[j].Value = value;
                }
        }

        private void RefreshOnClick(object sender, EventArgs e)
        {
            this.disconnectVehicleTable.Rows.Clear();
            this.badCountersTable.Rows.Clear();
            const string host = "172.17.0.121";
            String query = "SELECT NO_ECON, RECE.FCH_CREA AS ULTIMA_HORA FROM SITM.SFMO_RECE_NAVE AS RECE JOIN SITM.SFVH_VEHI AS VEHI ON RECE.ID_VEHI = VEHI.ID_VEHI WHERE NOW()::TIME - RECE.FCH_CREA::TIME > '00:10:00'::TIME ORDER BY VEHI.NO_ECON ASC, RECE.FCH_CREA DESC";
            String counterQuery = "WITH TEMP_ACCE_SALI AS ( SELECT ID_EQUI, FCH_ACCE_SALI FROM SITM.SBOP_ACCE_SALI WHERE DATE(FCH_ACCE_SALI)>(DATE(NOW())-3) AND ID_EQUI IN (SELECT ID_EQUI FROM SITM.SBEQ_EQUI WHERE ID_VEHI IS NOT NULL) AND ID_TIPO_ACCE NOT IN (327,868) ), MAX_FCH_VALIDA AS ( SELECT ID_EQUI, MAX(FCH_ACCE_SALI) FCH_MAX FROM TEMP_ACCE_SALI GROUP BY ID_EQUI ORDER BY ID_EQUI ), TEMP_CONT_ACCE AS ( SELECT ID_EQUI, FCH_CONT_ACCE FROM SITM.SBOP_CONT_ACCE WHERE DATE(FCH_CONT_ACCE)>(DATE(NOW())-3) AND ID_EQUI IN (SELECT ID_EQUI FROM SITM.SBEQ_EQUI WHERE ID_VEHI IS NOT NULL) ) SELECT NO_ECON, ESTADO_CONTADOR, MAX(FCH_ACCE_SALI) ULTIMA_VALIDA FROM ( SELECT ID_EQUI, CASE WHEN SUM(VAL)>SUM(CONT) THEN 'FALLA CONTADOR' ELSE 'CONTADOR OPERANDO' END ESTADO_CONTADOR FROM ( SELECT ID_EQUI, DATE(FCH_ACCE_SALI) FECHA, COUNT(*) VAL, 0 CONT FROM TEMP_ACCE_SALI S WHERE DATE(FCH_ACCE_SALI) IN ( SELECT DATE(FCH_MAX) FROM MAX_FCH_VALIDA WHERE ID_EQUI = S.ID_EQUI ) GROUP BY ID_EQUI, FECHA UNION SELECT ID_EQUI, DATE(FCH_CONT_ACCE) FECHA, 0 VAL, COUNT(*) CONT FROM TEMP_CONT_ACCE C WHERE DATE(FCH_CONT_ACCE) IN ( SELECT DATE(FCH_MAX) FROM MAX_FCH_VALIDA WHERE ID_EQUI = C.ID_EQUI ) GROUP BY ID_EQUI, FECHA ) GROUP BY ID_EQUI) VC LEFT JOIN SITM.SBEQ_EQUI E ON E.ID_EQUI=VC.ID_EQUI LEFT JOIN TEMP_ACCE_SALI S ON VC.ID_EQUI=S.ID_EQUI WHERE ESTADO_CONTADOR LIKE '%FALLA%' GROUP BY NO_ECON, ESTADO_CONTADOR ORDER BY NO_ECON";
            this.InitializeTask("Actualización | Monitor", 4);
            this._multiThread.RunTask(() =>
            {
                String[][] response;
                PostgreSQL psql = PostgreSQL.CreateConnection(host, 5432, "postgres", "admin", "SITM");
                psql.SetConnectionBySsh(psql.ConnectionBySsh.Replace("/opt/PostgreSQL/9.3/", "/opt/PostgresPlus/9.3AS/"));
                try
                {
                    response = psql.ExecuteQuery(query);
                }
                catch (NpgsqlException ex)
                {
                    Trace.WriteLine(ex.Message);
                    Trace.WriteLine(string.Format("Host {0} falló al realizar consulta PSQL a través del controlador de PostgreSQL\nIntentando por SSH con la credenciales\nUsername: {1}\nPassword: ******", host, "teknei"));
                    response = psql.ExcuteQueryBySsh(query, "Administrador", "Administrador*2016");
                }
                if (response.Length <= 1)
                    throw new Exception(string.Format("El host {0} no respondió con un resultado", host));

                for (int i = 1; i < response.Length; i++)
                {
                    String[] row = response[i];
                    if (row[0].Contains("Error al obtener información"))
                        throw new Exception(string.Format("Error al obtener la información del host {0}", host));
                    this.BeginInvoke(new Action(delegate
                    {
                        String[] tmpRow = row;
                        this.disconnectVehicleTable.Rows.Add(tmpRow);
                    }));
                }
            });
            this._multiThread.RunTask(() =>
            {
                String[][] response;
                PostgreSQL psql = PostgreSQL.CreateConnection(host, 5432, "postgres", "admin", "SITM");
                psql.SetConnectionBySsh(psql.ConnectionBySsh.Replace("/opt/PostgreSQL/9.3/", "/opt/PostgresPlus/9.3AS/"));
                try
                {
                    response = psql.ExecuteQuery(counterQuery);
                }
                catch (NpgsqlException ex)
                {
                    Trace.WriteLine(ex.Message);
                    Trace.WriteLine(string.Format("Host {0} falló al realizar consulta PSQL a través del controlador de PostgreSQL\nIntentando por SSH con la credenciales\nUsername: {1}\nPassword: ******", host, "teknei"));
                    response = psql.ExcuteQueryBySsh(counterQuery, "Administrador", "Administrador*2016");
                }
                if (response.Length <= 1)
                    throw new Exception(string.Format("El host {0} no respondió con un resultado", host));

                for (int i = 1; i < response.Length; i++)
                {
                    String[] row = response[i];
                    if (row[0].Contains("Error al obtener información"))
                        throw new Exception(string.Format("Error al obtener la información del host {0}", host));
                    this.BeginInvoke(new Action(delegate
                    {
                        String[] tmpRow = row;
                        this.badCountersTable.Rows.Add(tmpRow);
                    }));
                }
            });
        }
    }
}
