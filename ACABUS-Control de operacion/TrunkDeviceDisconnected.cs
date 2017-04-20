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
    public partial class TrunkDeviceDisconnected : Form
    {

        private static String FILENAME = "deviceData.list";

        private static List<DeviceDisconnected> _deviceDisconnected = new List<DeviceDisconnected>();

        private MultiThread _multiTask = new MultiThread()
        {
            Capacity = 12
        };

        private class DeviceDisconnected
        {
            public String NumeSeri { get; set; }
            public String DataBase { get; set; }
        }

        static TrunkDeviceDisconnected()
        {
            LoadData();
        }

        public TrunkDeviceDisconnected()
        {
            InitializeComponent();
            LoadDataBases();
            LoadTable();
        }

        private static void LoadData()
        {
            if (!File.Exists(FILENAME)) return;

            String[] deviceDisconnectedFile = File.ReadAllLines(FILENAME);
            Device[] deviceDisconnected = GetTrunkDeviceDisconnected();

            _deviceDisconnected.Clear();

            foreach (String deviceFile in deviceDisconnectedFile)
            {
                String numeSeri = deviceFile.Split(',')[0];
                String database = deviceFile.Split(',')[1];
                if (String.IsNullOrEmpty(numeSeri) || String.IsNullOrEmpty(database))
                    continue;
                _deviceDisconnected.Add(new DeviceDisconnected()
                {
                    NumeSeri = numeSeri,
                    DataBase = database
                });
                foreach (Device device in deviceDisconnected)
                    if (device.GetNumeSeri().Equals(numeSeri))
                    {
                        device.DataBaseName = database.ToString();
                        break;
                    }
            }

        }

        private void LoadTable()
        {
            LoadData();
            foreach (DeviceDisconnected device in _deviceDisconnected)
                foreach (DataGridViewRow item in this.deviceDisconnectedTable.Rows)
                    if (item.Cells[0].Value != null && item.Cells[0].Value.Equals(device.NumeSeri))
                        if (this.dataBaseColumn.Items.Contains(device.DataBase))
                            item.Cells[1].Value = device.DataBase;

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
            for (Int16 i = 0; i < this.deviceDisconnectedTable.Rows.Count; i++)
            {
                Object numeseri = this.deviceDisconnectedTable.Rows[i].Cells[0].Value;
                Object database = this.deviceDisconnectedTable.Rows[i].Cells[1].Value;
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
            this.deviceDisconnectedTable.Rows.Clear();

            Device[] deviceDisconnected = GetTrunkDeviceDisconnected();

            foreach (Device device in deviceDisconnected)
                this.deviceDisconnectedTable.Rows.Add(new String[] { device.GetNumeSeri() });

            String databasequery = String.Format(AcabusData.QUERY_DB_BACKUP);
            String[][] response;
            const string host = AcabusData.REPLICA_SERVER_IP;
            this.dataBaseColumn.Items.Clear();
            SshPostgreSQL psql = SshPostgreSQL.CreateConnection(
                AcabusData.PG_PATH,
                host,
                AcabusData.PG_PORT,
                AcabusData.PG_USERNAME,
                AcabusData.PG_PASSWORD_SERVER,
                AcabusData.DATABASE_REPLICA_NAME,
                AcabusData.SSH_USERNAME_SERVER,
                AcabusData.SSH_PASSWORD_SERVER
            );
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

        private static Device[] GetTrunkDeviceDisconnected()
        {
            List<Device> devicesDisconnected = new List<Device>();
            foreach (Trunk trunk in AcabusData.Trunks)
                foreach (Station station in trunk.GetStations())
                    foreach (Device device in station.GetDevices())
                    {
                        if (device.Status && device.HasDataBase && !station.Connected)
                            devicesDisconnected.Add(device);
                    }
            return devicesDisconnected.ToArray();
        }
    }
}
