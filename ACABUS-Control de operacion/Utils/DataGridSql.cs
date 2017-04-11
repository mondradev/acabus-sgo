using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace ACABUS_Control_de_operacion.Utils
{
    public class DataGridSql
    {
        private bool _hasColumns;
        private int _countcolumns;
        private bool _isReadyTable;
        private List<String[]> _tempRows;

        public MultiThread MultiThreadManager { get; set; }
        public DataGridView DataGrid { get; set; }

        public String UsernameDb { get; set; }
        public String PasswordDb { get; set; }
        public String Host { get; set; }
        public String DataBase { get; set; }
        public Int16 PortDb { get; set; }
        public String PgPath { get; set; }
        public String UsernameSsh { get; set; }
        public String PasswordSsh { get; set; }

        public DataGridSql()
        {
            this._tempRows = new List<string[]>();
        }

        public void ExecuteAndFill(String query, Action<Exception> onError = null)
        {
            if (MultiThreadManager == null)
                throw new NullReferenceException("No se ha asiganado un administrador de hilos");

            var pgpath = PgPath;
            var host = Host;
            var portdb = PortDb;
            var usernamedb = UsernameDb;
            var passworddb = PasswordDb;
            var database = DataBase;
            var usernamessh = UsernameSsh;
            var passwordssh = PasswordSsh;

            this.MultiThreadManager.RunTask(String.Format("Run SQL Thread: {0}", Host), () =>
            {
                if (!ConnectionTCP.IsAvaibleIP(Host))
                {
                    Trace.WriteLine(String.Format("No hay accesos al host {0}", Host), "ERROR");
                    return;
                }

                SshPostgreSQL psql = SshPostgreSQL.CreateConnection(pgpath, host, portdb, usernamedb, passworddb, database, usernamessh, passwordssh);
                String[][] response;

                response = psql.ExecuteQuery(query);

                try
                {
                    Fill(response);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("El host {0} no respondió correctamente", Host), "ERROR");
                    if (onError != null)
                        onError.Invoke(ex);
                }

            });
        }

        public void Fill(String[][] data)
        {
            if (data == null || data.Length <= 0)
                throw new Exception("La matriz no contiene datos");

            if (!this._hasColumns || this._countcolumns < data[0].Length)
            {
                this.GenerateColumns(data[0]);
                this._hasColumns = true;
                this._countcolumns = (Int16)data[0].Length;
            }
            for (int i = 1; i < data.Length; i++)
            {
                String[] row = data[i];
                DataGrid.BeginInvoke(new Action(delegate
                {
                    String[] tmpRow = row;
                    if (this._isReadyTable)
                        this.DataGrid.Rows.Add(tmpRow);
                    else
                        this._tempRows.Add(tmpRow);
                }));
            }
        }

        private void GenerateColumns(string[] header)
        {
            this._isReadyTable = false;
            Int16 i = 0;
            DataGrid.BeginInvoke(new Action(delegate
            {
                String[,] rows = GetRows(this.DataGrid.Rows);
                this.DataGrid.Rows.Clear();
                this.DataGrid.Columns.Clear();
                foreach (var item in header)
                {
                    this.DataGrid.Columns.Add("column" + i, item);
                    this.DataGrid.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    this.DataGrid.Columns[i].ReadOnly = true;
                    i++;
                }
                Application.DoEvents();
                if (rows != null)
                    this.DataGrid.Rows.Add(rows);
                foreach (var item in this._tempRows.ToArray())
                    this.DataGrid.Rows.Add(item);
                this._tempRows.Clear();
                this._isReadyTable = true;
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

        public void ClearDataGrid()
        {
            if (DataGrid != null)
            {
                DataGrid.Rows.Clear();
                DataGrid.Columns.Clear();
            }
        }

        public void AddRow(String[] rowData)
        {
            DataGrid.BeginInvoke(new Action(() =>
            {
                String[] row = rowData;
                if (this._isReadyTable)
                    this.DataGrid.Rows.Add(row);
                else
                    this._tempRows.Add(row);
            }));
        }
    }
}
