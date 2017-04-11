using ACABUS_Control_de_operacion.Acabus;
using ACABUS_Control_de_operacion.Utils;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace ACABUS_Control_de_operacion
{
    public partial class DBKVRsExternos : Form
    {
        private string USERNAME = "Administrador";
        private string PASSWORD = "Administrador*2016";
        private static String FILENAME = "kvrExternData.list";

        private static List<KVRextern> _kvrExterns = new List<KVRextern>();

        private MultiThread _multiTask = new MultiThread()
        {
            Capacity = 12
        };

        private class KVRextern
        {
            public String NumeSeri { get; set; }
            public String DataBase { get; set; }
        }

        static DBKVRsExternos()
        {
            LoadData();
        }

        public DBKVRsExternos()
        {
            InitializeComponent();
            LoadDataBases();
            LoadTable();
        }

        private static void LoadData()
        {
            if (!File.Exists(FILENAME)) return;

            String[] kvrs = File.ReadAllLines(FILENAME);
            Kvr[] kvrExterns = GetKVRExterns();

            _kvrExterns.Clear();

            foreach (String kvr in kvrs)
            {
                String numeSeri = kvr.Split(',')[0];
                String database = kvr.Split(',')[1];
                if (String.IsNullOrEmpty(numeSeri) || String.IsNullOrEmpty(database))
                    continue;
                _kvrExterns.Add(new KVRextern()
                {
                    NumeSeri = numeSeri,
                    DataBase = database
                });
                foreach (Kvr kvre in kvrExterns)
                    if (kvre.GetNumeSeri().Equals(numeSeri))
                    {
                        kvre.DataBaseName = database.ToString();
                        break;
                    }
            }

        }

        private void LoadTable()
        {
            LoadData();
            foreach (KVRextern kvr in _kvrExterns)
                foreach (DataGridViewRow item in this.kvrExternTable.Rows)
                    if (item.Cells[0].Value != null && item.Cells[0].Value.Equals(kvr.NumeSeri))
                        if (this.dataBaseColumn.Items.Contains(kvr.DataBase))
                            item.Cells[1].Value = kvr.DataBase;

        }

        private void SaveDataOnClick(object sender, EventArgs e)
        {
            SaveData();
            LoadDataBases();
            LoadTable();
            MessageBox.Show("Los datos se guardaron correctamente.");
            Hide();
            Dispose();
        }

        private void SaveData()
        {
            File.Delete(FILENAME);
            for (Int16 i = 0; i < this.kvrExternTable.Rows.Count; i++)
            {
                Object numeseri = this.kvrExternTable.Rows[i].Cells[0].Value;
                Object database = this.kvrExternTable.Rows[i].Cells[1].Value;
                if (numeseri == null || database == null) continue;
                File.AppendAllText(FILENAME, String.Format("{0},{1}\n", numeseri, database));
            }
        }

        private void RefreshDataBaseOnClick(object sender, EventArgs e)
        {
            LoadDataBases();
        }

        private void LoadDataBases()
        {
            this.kvrExternTable.Rows.Clear();

            Kvr[] kvrExterns = GetKVRExterns();

            foreach (Kvr kvr in kvrExterns)
                this.kvrExternTable.Rows.Add(new String[] { kvr.GetNumeSeri() });

            String databasequery = String.Format("SELECT datname FROM pg_database WHERE datname like 'SITM_KVR%' ORDER BY datname");
            String[][] response;
            const string host = "172.17.0.125";
            this.dataBaseColumn.Items.Clear();
            SshPostgreSQL psql = SshPostgreSQL.CreateConnection(AcabusData.PG_PATH, host, 5432, "postgres", "admin", "SITM_REPLICA", USERNAME, PASSWORD);
            try
            {
                response = psql.ExecuteQuery(databasequery);
            }
            catch (NpgsqlException ex)
            {
                Trace.WriteLine(ex.Message, "ERROR");
                response = null;
            }
            if (response.Length <= 1)
                throw new Exception(string.Format("El host {0} no respondió correctamente", host));

            for (int i = 1; i < response.Length; i++)
            {
                String[] row = response[i];
                dataBaseColumn.Items.Add(row[0]);
            }

        }

        private static Kvr[] GetKVRExterns()
        {
            List<Kvr> kvrs = new List<Kvr>();
            foreach (Trunk trunk in AcabusData.Trunks)
                foreach (Station station in trunk.GetStations())
                    foreach (Device device in station.GetDevices())
                    {
                        if (device.Type == Device.DeviceType.KVR)
                            if (((Kvr)device).IsExtern && device.Status && device.HasDataBase && !station.Connected)
                                kvrs.Add((Kvr)device);
                    }
            return kvrs.ToArray();
        }
    }
}
