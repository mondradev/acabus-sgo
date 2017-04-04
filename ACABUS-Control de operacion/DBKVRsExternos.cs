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
            KVR[] kvrExterns = GetKVRExterns();

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
                foreach (KVR kvre in kvrExterns)
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
                foreach (DataGridViewRow item in this.dataGridView1.Rows)
                    if (item.Cells[0].Value != null && item.Cells[0].Value.Equals(kvr.NumeSeri))
                        item.Cells[1].Value = kvr.DataBase;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            SaveData();
            LoadDataBases();
            LoadTable();
        }

        private void SaveData()
        {
            File.Delete(FILENAME);
            KVR[] kvrExterns = GetKVRExterns();
            for (Int16 i = 0; i < this.dataGridView1.Rows.Count; i++)
            {
                Object numeseri = this.dataGridView1.Rows[i].Cells[0].Value;
                Object database = this.dataGridView1.Rows[i].Cells[1].Value;
                if (numeseri == null || database == null) continue;
                File.AppendAllText(FILENAME, String.Format("{0},{1}\n", numeseri, database));
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            LoadDataBases();
        }

        private void LoadDataBases()
        {
            this.dataGridView1.Rows.Clear();

            KVR[] kvrExterns = GetKVRExterns();

            foreach (KVR kvr in kvrExterns)
                this.dataGridView1.Rows.Add(new String[] { kvr.GetNumeSeri() });

            String databasequery = String.Format("SELECT datname FROM pg_database WHERE datname like 'SITM_KVR%' ORDER BY datname");
            String[][] response;
            const string host = "172.17.0.125";
            this.dataBaseColumn.Items.Clear();
            PostgreSQL psql = PostgreSQL.CreateConnection(host, 5432, "postgres", "admin", "SITM_REPLICA");
            try
            {
                response = psql.ExecuteQuery(databasequery);
            }
            catch (NpgsqlException ex)
            {
                Trace.WriteLine(ex.Message, "ERROR");
                Trace.WriteLine(string.Format("Host {0} falló al realizar consulta PSQL a través del controlador de PostgreSQL\nIntentando por SSH con la credenciales\nUsername: {1}\nPassword: ******", host, USERNAME), "DEBUG");
                response = psql.ExcuteQueryBySsh(databasequery, USERNAME, PASSWORD);
            }
            if (response.Length <= 1)
                throw new Exception(string.Format("El host {0} no respondió con un resultado", host));
            for (int i = 1; i < response.Length; i++)
            {
                String[] row = response[i];
                if (row[0].Contains("Error al obtener información"))
                    throw new Exception(string.Format("Error al obtener la información del host {0}", host));
                dataBaseColumn.Items.Add(row[0]);
            }

        }

        private static KVR[] GetKVRExterns()
        {
            List<KVR> kvrs = new List<KVR>();
            foreach (Trunk trunk in Trunk.Trunks)
                foreach (Station station in trunk.Stations)
                    foreach (Device device in station.Devices)
                    {
                        if (device.Type == Device.DeviceType.KVR)
                            if (((KVR)device).IsExtern)
                                kvrs.Add((KVR)device);
                    }
            return kvrs.ToArray();
        }
    }
}
