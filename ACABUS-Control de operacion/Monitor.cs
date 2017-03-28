﻿using Npgsql;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ACABUS_Control_de_operacion
{
    public partial class Monitor : Form
    {
        private const String USERNAME = "Administrador";
        private const string PASSWORD = "Administrador*2016";
        private MultiThread _multiThread = new MultiThread();
        private UncirculatedVehicles _uncirculatedVehiclesDialog;
        private String[] _uncirculatedVehicles;

        public Monitor()
        {
            this._multiThread.Capacity = 20;
            this.InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            LoadUncirculatedVehicles();
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
            _multiThread.KillAllThreads(() =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    startButton.Enabled = true;
                }));
            });
            timer1.Stop();
            StopButton.Enabled = false;
        }


        private void RefreshMonitor()
        {
            const string host = "172.17.0.121";
            String counterQuery = "WITH TEMP_ACCE_SALI AS ( SELECT ID_EQUI, FCH_ACCE_SALI FROM SITM.SBOP_ACCE_SALI WHERE DATE(FCH_ACCE_SALI)>(DATE(NOW())-3) AND ID_EQUI IN (SELECT ID_EQUI FROM SITM.SBEQ_EQUI WHERE ID_VEHI IS NOT NULL) AND ID_TIPO_ACCE NOT IN (327,868) ), MAX_FCH_VALIDA AS ( SELECT ID_EQUI, MAX(FCH_ACCE_SALI) FCH_MAX FROM TEMP_ACCE_SALI GROUP BY ID_EQUI ORDER BY ID_EQUI ), TEMP_CONT_ACCE AS ( SELECT ID_EQUI, FCH_CONT_ACCE FROM SITM.SBOP_CONT_ACCE WHERE DATE(FCH_CONT_ACCE)>(DATE(NOW())-3) AND ID_EQUI IN (SELECT ID_EQUI FROM SITM.SBEQ_EQUI WHERE ID_VEHI IS NOT NULL) AND ID_TIPO_FLUJ=325 ) SELECT NO_ECON, ESTADO_CONTADOR, MAX(FCH_ACCE_SALI) ULTIMA_VALIDA FROM ( SELECT ID_EQUI,SUM(VAL) VAL, SUM(CONT) CONT, CASE WHEN SUM(VAL)>SUM(CONT) THEN CASE WHEN SUM(CONT)=0 THEN 'FALLA CONTADOR EN CERO' ELSE 'FALLA CONTADOR' END ELSE 'CONTADOR OPERANDO' END ESTADO_CONTADOR FROM ( SELECT ID_EQUI, DATE(FCH_ACCE_SALI) FECHA, COUNT(*) VAL, 0 CONT FROM TEMP_ACCE_SALI S WHERE DATE(FCH_ACCE_SALI) IN ( SELECT DATE(FCH_MAX) FROM MAX_FCH_VALIDA WHERE ID_EQUI = S.ID_EQUI ) GROUP BY ID_EQUI, FECHA UNION SELECT ID_EQUI, DATE(FCH_CONT_ACCE) FECHA, 0 VAL, COUNT(*) CONT FROM TEMP_CONT_ACCE C WHERE DATE(FCH_CONT_ACCE) IN ( SELECT DATE(FCH_MAX) FROM MAX_FCH_VALIDA WHERE ID_EQUI = C.ID_EQUI ) GROUP BY ID_EQUI, FECHA ) GROUP BY ID_EQUI) VC LEFT JOIN SITM.SBEQ_EQUI E ON E.ID_EQUI=VC.ID_EQUI LEFT JOIN TEMP_ACCE_SALI S ON VC.ID_EQUI=S.ID_EQUI WHERE ESTADO_CONTADOR LIKE '%FALLA%' GROUP BY NO_ECON, ESTADO_CONTADOR ORDER BY ESTADO_CONTADOR DESC, NO_ECON";
            String alarmQuery = "SELECT NUME_SERI, DES_ALAR, FCH_INI::TIME FROM SITM.SBOP_EQUI_ALAR EA LEFT JOIN SITM.SBCT_ALAR A ON EA.ID_ALAR=A.ID_ALAR LEFT JOIN SITM.SBEQ_EQUI E ON E.ID_EQUI=EA.ID_EQUI WHERE DATE(FCH_INI)=DATE(NOW()) AND DES_ALAR NOT LIKE '%NO HAY CONEXION%' ORDER BY FCH_INI DESC";

            this._multiThread.RunTask(() =>
            {
                String receNaveQuery = String.Format("SELECT NO_ECON, RECE.FCH_CREA AS ULTIMA_HORA FROM SITM.SFMO_RECE_NAVE AS RECE JOIN SITM.SFVH_VEHI AS VEHI ON RECE.ID_VEHI = VEHI.ID_VEHI WHERE (DATE(NOW())>DATE(RECE.FCH_CREA) OR NOW()::TIME - RECE.FCH_CREA::TIME > '00:10:00'::TIME) AND VEHI.NO_ECON NOT IN ({0}) ORDER BY VEHI.NO_ECON ASC, RECE.FCH_CREA DESC", ProcessArray(this._uncirculatedVehicles));
                String[][] response;
                PostgreSQL psql = PostgreSQL.CreateConnection(host, 5432, "postgres", "admin", "SITM");
                psql.SetConnectionBySsh(psql.ConnectionBySsh.Replace("/opt/PostgreSQL/9.3/", "/opt/PostgresPlus/9.3AS/"));
                try
                {
                    response = psql.ExecuteQuery(receNaveQuery);
                }
                catch (NpgsqlException ex)
                {
                    Trace.WriteLine(ex.Message, "ERROR");
                    Trace.WriteLine(string.Format("Host {0} falló al realizar consulta PSQL a través del controlador de PostgreSQL\nIntentando por SSH con la credenciales\nUsername: {1}\nPassword: ******", host, USERNAME), "DEBUG");
                    response = psql.ExcuteQueryBySsh(receNaveQuery, USERNAME, PASSWORD);
                }
                if (response.Length <= 1)
                    throw new Exception(string.Format("El host {0} no respondió con un resultado", host));
                bool readyDisconnections = false;
                for (int i = 1; i < response.Length; i++)
                {
                    String[] row = response[i];
                    if (row[0].Contains("Error al obtener información"))
                        throw new Exception(string.Format("Error al obtener la información del host {0}", host));
                    this.BeginInvoke(new Action(delegate
                    {
                        String[] tmpRow = row;
                        if (!readyDisconnections)
                        {
                            this.disconnectVehicleTable.Rows.Clear();
                            readyDisconnections = true;
                        }
                        this.disconnectVehicleTable.Rows.Add(tmpRow);
                    }));
                }
            }, (ex) =>
            {
                Trace.WriteLine(ex.Message, "ERROR");
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
                    Trace.WriteLine(ex.Message, "ERROR");
                    Trace.WriteLine(string.Format("Host {0} falló al realizar consulta PSQL a través del controlador de PostgreSQL\nIntentando por SSH con la credenciales\nUsername: {1}\nPassword: ******", host, USERNAME), "DEBUG");
                    response = psql.ExcuteQueryBySsh(counterQuery, USERNAME, PASSWORD);
                }
                if (response.Length <= 1)
                    throw new Exception(string.Format("El host {0} no respondió con un resultado", host));
                bool readyCounters = false;
                for (int i = 1; i < response.Length; i++)
                {
                    String[] row = response[i];
                    if (row[0].Contains("Error al obtener información"))
                        throw new Exception(string.Format("Error al obtener la información del host {0}", host));
                    this.BeginInvoke(new Action(delegate
                    {
                        String[] tmpRow = row;
                        if (!readyCounters)
                        {
                            this.badCountersTable.Rows.Clear();
                            readyCounters = true;
                        }
                        this.badCountersTable.Rows.Add(tmpRow);
                    }));
                }
            }, (ex) =>
            {
                Trace.WriteLine(ex.Message, "ERROR");
            });

            this._multiThread.RunTask(() =>
            {
                String[][] response;
                PostgreSQL psql = PostgreSQL.CreateConnection(host, 5432, "postgres", "admin", "SITM");
                psql.SetConnectionBySsh(psql.ConnectionBySsh.Replace("/opt/PostgreSQL/9.3/", "/opt/PostgresPlus/9.3AS/"));
                try
                {
                    response = psql.ExecuteQuery(alarmQuery);
                }
                catch (NpgsqlException ex)
                {
                    Trace.WriteLine(ex.Message, "ERROR");
                    Trace.WriteLine(string.Format("Host {0} falló al realizar consulta PSQL a través del controlador de PostgreSQL\nIntentando por SSH con la credenciales\nUsername: {1}\nPassword: ******", host, USERNAME), "DEBUG");
                    response = psql.ExcuteQueryBySsh(alarmQuery, USERNAME, PASSWORD);
                }
                if (response.Length <= 1)
                    throw new Exception(string.Format("El host {0} no respondió con un resultado", host));
                bool readyAlarms = false;
                for (int i = 1; i < response.Length; i++)
                {
                    String[] row = response[i];
                    if (row[0].Contains("Error al obtener información"))
                        throw new Exception(string.Format("Error al obtener la información del host {0}", host));
                    this.BeginInvoke(new Action(delegate
                    {
                        String[] tmpRow = row;
                        if (!readyAlarms)
                        {
                            this.alarmTrunkTable.Rows.Clear();
                            readyAlarms = true;
                        }
                        this.alarmTrunkTable.Rows.Add(tmpRow);
                    }));
                }
            }, (ex) =>
            {
                Trace.WriteLine(ex.Message, "ERROR");
            });
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
                    Trace.WriteLine(String.Format("Valor recibido en la tabla: {0}", value), "DEBUG");
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
                this._uncirculatedVehiclesDialog = new UncirculatedVehicles();
                this._uncirculatedVehiclesDialog.FormClosing += (senderClosing, args) =>
                {
                    LoadUncirculatedVehicles();
                };
            }
            this._uncirculatedVehiclesDialog.ShowDialog();
        }
    }
}
