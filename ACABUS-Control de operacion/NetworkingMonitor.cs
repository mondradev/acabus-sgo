using ACABUS_Control_de_operacion.Acabus;
using ACABUS_Control_de_operacion.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ACABUS_Control_de_operacion
{
    public partial class NetworkingMonitor : Form
    {

        private class DeviceInMonitoring
        {
            public String IPAddress { get; set; }
            public String Name { get; set; }
        }

        private Boolean _inStoping = false;
        private List<DeviceInMonitoring> _devicesInMonitoring = new List<DeviceInMonitoring>();
        private MultiThread _multiThread = new MultiThread() { Capacity = 30 };
        private Int16 TIMEOUT_REFRESH = 600;

        public NetworkingMonitor()
        {
            InitializeComponent();

            this.stationComboBox.SelectedIndexChanged += (sender, args) =>
            {
                _inStoping = true;
                String selectedItem = this.stationComboBox.SelectedItem.ToString();
                _multiThread.KillAllThreads(() =>
                {
                    _inStoping = false;
                    _devicesInMonitoring.Clear();

                    if (selectedItem.Equals("Todas las estaciones"))
                    {
                        foreach (Trunk trunk in AcabusData.Trunks)
                            foreach (Station station in trunk.GetStations())
                            {
                                String stationIP = ((Device)station.GetDevices().GetValue(0))?.IP;
                                if (station.Connected)
                                    _devicesInMonitoring.Add(new DeviceInMonitoring()
                                    {
                                        Name = station.Name,
                                        IPAddress = stationIP
                                    });
                            }
                    }
                    else
                    {
                        foreach (Trunk trunk in AcabusData.Trunks)
                            foreach (Station station in trunk.GetStations())
                            {
                                if (station.Name.Equals(selectedItem))
                                {
                                    foreach (Device device in station.GetDevices())
                                    {
                                        _devicesInMonitoring.Add(new DeviceInMonitoring()
                                        {
                                            Name = device.GetNumeSeri(),
                                            IPAddress = device.IP
                                        });
                                    }
                                }
                            }
                    }

                    RefreshMonitoring();
                    RunMonitoring();
                });
            };

            LoadStations();

            WindowState = FormWindowState.Maximized;
        }

        private void RefreshMonitoring()
        {
            this.BeginInvoke(new Action(() =>
            {
                this.monitorChart.Series.Clear();

                foreach (DeviceInMonitoring device in _devicesInMonitoring)
                {

                    Trace.WriteLine(String.Format("Agregando equipo: {0}", device.Name), "DEBUG");
                    this.monitorChart.Series.Add(new Series(device.Name, 1)
                    {
                        ChartType = SeriesChartType.Column
                    });
                }
            }));
        }


        private void RunMonitoring()
        {
            foreach (DeviceInMonitoring device in _devicesInMonitoring)
            {
                _multiThread.RunTask(String.Format("Networking Monitor: {0}", device.Name), () =>
                 {
                     while (!_inStoping)
                     {
                         int ping = ConnectionTCP.SendToPing(device.IPAddress);
                         ping = ping < 0 ? 0 : ping;
                         this.BeginInvoke(new Action(() =>
                         {
                             int tempPing = ping;
                             int index = GetSerie(device);
                             if (index < 0) return;
                             this.monitorChart.Series[index].Points.Clear();
                             this.monitorChart.Series[index].Points.AddY(tempPing);
                         }));
                         Thread.Sleep(TIMEOUT_REFRESH);
                     }
                 });
            }
        }

        private int GetSerie(DeviceInMonitoring device)
        {
            foreach (Series serie in this.monitorChart.Series)
            {
                if (serie.Name.Equals(device.Name))
                {
                    return this.monitorChart.Series.IndexOf(serie);
                }

            }
            return -1;
        }

        private void LoadStations()
        {
            this.stationComboBox.Items.Clear();
            this.stationComboBox.Items.Add("Todas las estaciones");
            foreach (Trunk trunk in AcabusData.Trunks)
                foreach (Station station in trunk.GetStations())
                    if (station.Connected)
                        this.stationComboBox.Items.Add(station.Name);
            this.stationComboBox.SelectedIndex = 0;
        }



    }
}
