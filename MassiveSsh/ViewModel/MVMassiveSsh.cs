using MassiveSsh.Acabus;
using MassiveSsh.Utils;
using MassiveSsh.Utils.MVVM;
using MassiveSsh.Utils.SecureShell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace MassiveSsh.ViewModel
{
    internal class MVMassiveSsh : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private String _command;
        private String _response;
        private MultiThread _multiThread;
        private Station _selectedStation;
        private Device _selectedDevice;
        private bool _allDevices = true;
        private bool _allStations = true;
        private double _maxProgressTask = 100.0;
        private double _progressTask = 0.0;
        private string _localPath;
        private int _lineHistory = -1;

        public Double MaxProgressTask {
            get {
                return _maxProgressTask;
            }
            set {
                _maxProgressTask = value;
                OnPropertyChanged("MaxProgressTask");
            }
        }

        public Double ProgressTask {
            get {
                return _progressTask;
            }
            set {
                _progressTask = value;
                OnPropertyChanged("ProgressTask");
                OnPropertyChanged("LockCommandInput");
            }
        }

        public Boolean LockCommandInput => ProgressTask == 0.0 || ProgressTask == MaxProgressTask;

        public String Response {
            get { return _response; }
            set {
                _response = value;
                OnPropertyChanged("Response");
            }
        }

        public String SshCommand {
            get { return _command; }
            set {
                _command = value;
                OnPropertyChanged("SshCommand");
            }
        }

        public CommandBase SendCommand { get; set; }
        public CommandBase SearchCommand { get; set; }

        public ObservableCollection<Station> Stations {
            get {
                List<Station> stations = new List<Station>();
                foreach (Trunk trunk in AcabusData.Trunks)
                    foreach (Station station in trunk.GetStations())
                        if (station.Connected)
                            stations.Add(station);
                return new ObservableCollection<Station>(stations);
            }
        }

        public ObservableCollection<Device> Devices {
            get {
                if (SelectedStation != null)
                {
                    return new ObservableCollection<Device>(SelectedStation.Devices.FindAll((device) =>
                    {
                        return device.SshEnabled;
                    }));
                }
                return null;
            }
        }

        public Station SelectedStation {
            get {
                return _selectedStation;
            }
            set {
                _selectedStation = value;
                OnPropertyChanged("SelectedStation");
                OnPropertyChanged("Devices");
            }
        }

        public Device SelectedDevice {
            get {
                return _selectedDevice;
            }
            set {
                _selectedDevice = value;
                OnPropertyChanged("SelectedDevice");
            }
        }

        public Boolean AllDevices {
            get {
                return _allDevices;
            }
            set {
                _allDevices = value;
                OnPropertyChanged("AllDevices");
            }
        }

        public Boolean AllStations {
            get {
                return _allStations;
            }
            set {
                _allStations = value;
                OnPropertyChanged("AllStations");
            }
        }

        protected virtual void OnPropertyChanged(String name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public MVMassiveSsh()
        {
            this.SendCommand = new CommandBase(param =>
            {
                if ((String.IsNullOrEmpty(SshCommand) && param != null)
                    || (param != null && !param.ToString().Equals(SshCommand)))
                    SshCommand = param.ToString();
                SendCommandToRemote();
            });
            this.SearchCommand = new CommandBase(param =>
            {
                if (!File.Exists(AcabusData.SSH_HISTORY_FILENAME)) return;
                String[] commands = File.ReadAllLines(AcabusData.SSH_HISTORY_FILENAME);
                if (commands.Length < 1) return;
                if (param == null && !(param is bool)) return;
                int move = param.ToString().Equals("Down") ? +1 : param.ToString().Equals("Up") ? -1 : 0;
                if (_lineHistory == -1) _lineHistory = commands.Length;
                if ((_lineHistory + move) < 0) return;
                if (commands.Length <= (_lineHistory + move))
                    SshCommand = String.Empty;
                else
                    SshCommand = commands[_lineHistory + move];
                _lineHistory += move;
            });
            this._multiThread = new MultiThread() { Capacity = 20 };
            this._multiThread.ThreadsChanged += (sender, args) =>
            {
                ProgressTask++;
                if (!this._multiThread.IsRunning())
                {
                    SshCommand = String.Empty;
                    _lineHistory = -1;
                    if (!String.IsNullOrEmpty(_localPath))
                    {
                        DownloadBackup();
                    }
                }
            };
            AcabusData.LoadConfiguration();
        }

        private void DownloadBackup()
        {
            var localPath = _localPath;
            _localPath = "";
            ProgressTask = 0;
            List<Device> devices = GetDevice();
            MaxProgressTask = devices.Count * 2;
            foreach (var item in devices)
            {
                _multiThread.RunTask(String.Format("Descargando respaldo de {0}", item.IP), () =>
                {
                    String[] filenameBackup = null;
                    using (Ssh connection = new Ssh(item.IP, AcabusData.UsernameSsh, AcabusData.PasswordSsh))
                    {
                        if (connection.IsConnected())
                        {
                            Trace.WriteLine(String.Format("Conectado a: {0}", item.IP), "INFO");

                            String response = connection.SendCommand("ls *.backup");

                            if (!String.IsNullOrEmpty(response))
                            {
                                filenameBackup = response.Split('\n');
                            }
                        }
                        else
                        {
                            Trace.WriteLine(String.Format("Falla al conectar a: {0}", item.IP), "ERROR");
                        }
                    }
                    ShowResponse(item.IP, "Descargando archivos");
                    if (filenameBackup?.Length > 0)
                        using (Scp connection = new Scp(item.IP, AcabusData.UsernameSsh, AcabusData.PasswordSsh))
                        {
                            connection.TransferEvent += Download_TransferEvent;
                            if (connection.IsConnected())
                            {
                                Trace.WriteLine(String.Format("Conectado a: {0}", item.IP), "INFO");

                                ShowResponse(item.IP, "Conectado, descargando...");

                                foreach (String file in filenameBackup)
                                    connection.Download(file.Replace("\r", ""), localPath);
                            }
                            else
                            {
                                Trace.WriteLine(String.Format("Falla al conectar a: {0}", item.IP), "ERROR");
                            }
                        }
                }, (ex) =>
                {
                    Trace.WriteLine(String.Format("Ocurrió un problema al descargar el respaldo", item.IP), "ERROR");
                    Trace.WriteLine(ex.Message, "ERROR");
                    ShowResponse(item.IP, "Ocurrió un error al descargar");
                });
            }
        }

        private void Download_TransferEvent(object sender, Scp.ScpEventArgs e)
        {
            if (e.Status == Scp.ScpStatus.START)
            {
                double fileSize = e.TotalBytes;
                int i = 0;
                String[] unit = new string[] { "B", "KB", "MB", "GB", "TB" };
                while (fileSize > 1023 && i < unit.Length)
                {
                    fileSize /= 1024;
                    i++;
                }
                ShowResponse((sender as Scp).Host, String.Format("Descargando {0} {1:0.00} {2}", e.SourceData, fileSize, unit[i]));
            }
            else if (e.Status == Scp.ScpStatus.END)
            {
                ShowResponse((sender as Scp).Host, String.Format("Descarga completa {0}", e.SourceData));

            }
        }

        private List<Device> GetDevice()
        {
            if (AllDevices || AllStations)
                if (SelectedStation != null && !AllStations)
                    return SelectedStation.Devices.FindAll((device) =>
                    {
                        return device.SshEnabled;
                    });
                else
                    return GetAllDevices();
            else if (SelectedDevice != null)
                return new List<Device>(new Device[] { SelectedDevice });
            else return new List<Device>();
        }

        private List<Device> GetAllDevices()
        {
            List<Device> devices = new List<Device>();
            foreach (Trunk trunk in AcabusData.Trunks)
                foreach (Station station in trunk.Stations)
                    devices.AddRange(station.Devices.FindAll((device) =>
                    {
                        return device.SshEnabled;
                    }));
            return devices;
        }

        private void SendCommandToRemote()
        {
            if (String.IsNullOrEmpty(SshCommand)) return;

            ProgressTask = 0;
            Response = String.Empty;

            File.AppendAllText(AcabusData.SSH_HISTORY_FILENAME, String.Format("{0}\n", SshCommand));

            Boolean inBackup = false;

            if (SshCommand.Equals("backup db"))
            {
                inBackup = true;
                SshCommand = AcabusData.CmdCreateBackup;

                FolderBrowserDialog directory = new FolderBrowserDialog();
                if (directory.ShowDialog() == DialogResult.OK)
                    _localPath = directory.SelectedPath;
                else
                    return;
            }

            if (SshCommand.Equals("upload"))
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    Upload(fileDialog.FileNames);
                }
                return;
            }

            if (SshCommand.Equals("clear"))
            {
                SshCommand = String.Empty;
                return;
            }


            Trace.WriteLine("Comando por envíar: " + SshCommand, "DEBUG");

            if (inBackup)
                ShowResponse("MSsh", "Genera y descarga de backups DB de remotos\n");
            else
                ShowResponse("MSsh", "Enviando comando a remotos\n");

            List<Device> devices = GetDevice();
            MaxProgressTask = devices.Count * 2;
            foreach (var item in devices)
            {
                _multiThread.RunTask(String.Format("Enviando comando a {0}", item.IP), () =>
                {
                    using (Ssh connection = new Ssh(item.IP, AcabusData.UsernameSsh, AcabusData.PasswordSsh))
                    {
                        if (connection.IsConnected())
                        {
                            Trace.WriteLine(String.Format("Conectado a: {0}", item.IP), "INFO");

                            if (inBackup)
                                ShowResponse(item.IP, "Generando backup");
                            else
                                ShowResponse(item.IP, "Conectado, enviando comando...");

                            String response = connection.SendCommand(SshCommand);

                            if (String.IsNullOrEmpty(response))
                                response = "Comando terminado";
                            ShowResponse(item.IP, response);
                        }
                        else
                        {
                            Trace.WriteLine(String.Format("Falla al conectar a: {0}", item.IP), "ERROR");
                        }
                    }
                }, (ex) =>
                {
                    Trace.WriteLine(String.Format("Ocurrió un problema al envíar el comando", item.IP), "ERROR");
                    Trace.WriteLine(ex.Message, "ERROR");
                    ShowResponse(item.IP, "Ocurrió un error al envíar el comando");
                });
            }
        }

        private void Upload(String[] files)
        {
            if (files?.Length < 1) return;
            ProgressTask = 0;
            List<Device> devices = GetDevice();
            MaxProgressTask = devices.Count * 2;
            ShowResponse("MSsh", "Subiendo los siguientes archivos:");
            foreach (var item in files)
            {
                ShowResponse("MSsh", item + "\n");
            }
            foreach (var item in devices)
            {
                _multiThread.RunTask(String.Format("Subiendo archivos a {0}", item.IP), () =>
                {
                    using (Scp connection = new Scp(item.IP, AcabusData.UsernameSsh, AcabusData.PasswordSsh))
                    {
                        connection.TransferEvent += Upload_TransferEvent;
                        if (connection.IsConnected())
                        {
                            Trace.WriteLine(String.Format("Conectado a: {0}", item.IP), "INFO");

                            ShowResponse(item.IP, "Conectado, cargando...");

                            foreach (String file in files)
                                connection.Upload(file, file.Substring(file.LastIndexOf("\\") + 1));
                        }
                        else
                        {
                            Trace.WriteLine(String.Format("Falla al conectar a: {0}", item.IP), "ERROR");
                        }
                    }
                }, (ex) =>
                {
                    Trace.WriteLine(String.Format("Ocurrió un problema al subir el/los archivo(s)", item.IP), "ERROR");
                    Trace.WriteLine(ex.Message, "ERROR");
                    ShowResponse(item.IP, "Ocurrió un error al cargar");
                });
            }
        }

        private void Upload_TransferEvent(object sender, Scp.ScpEventArgs e)
        {
            if (e.Status == Scp.ScpStatus.START)
            {
                double fileSize = e.TotalBytes;
                int i = 0;
                String[] unit = new string[] { "B", "KB", "MB", "GB", "TB" };
                while (fileSize > 1023 && i < unit.Length)
                {
                    fileSize /= 1024;
                    i++;
                }
                ShowResponse((sender as Scp).Host, String.Format("Cargando {0} {1:0.00} {2}", e.SourceData, fileSize, unit[i]));
            }
            else if (e.Status == Scp.ScpStatus.END)
            {
                ShowResponse((sender as Scp).Host, String.Format("Carga completa {0}", e.SourceData));

            }
        }

        public void KillAll()
        {
            _multiThread.KillAllThreads();
        }

        private void ShowResponse(String host, String response)
        {
            Response += String.Format("{0} > {1}\n", host, response);
        }
    }
}
