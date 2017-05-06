using Acabus.DataAccess;
using Acabus.Models;
using Acabus.Services;
using Acabus.Utils.MVVM;
using Acabus.Utils.SecureShell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Acabus.Modules.TrunkMonitor
{
    /// <summary>
    /// Proporciona una estructura para manipular la tareas que se prodrán ejecutar dentro del monitor de vía.
    /// </summary>
    internal class TaskTrunkMonitor
    {
        /// <summary>
        /// Nombre de la tarea como se visualizará en el monitor.
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// Acción que realizará la tarea cuando esta se seleccione.
        /// </summary>
        public Action<Station> Task { get; private set; }

        /// <summary>
        /// Crea una instance de una tarea de monitor de vía inicializando sus propiedades.
        /// </summary>
        /// <param name="name">Nombre de la tarea.</param>
        /// <param name="task">Acción a realizar por la tarea.</param>
        internal TaskTrunkMonitor(String name, Action<Station> task)
        {
            Name = name;
            Task = task;
        }
    }

    /// <summary>
    /// Modelo la vista del monitor de vía, proporciona toda las funciones que realizará el monitor de vía.
    /// </summary>
    public class TrunkMonitorModelView : NotifyPropertyChanged
    {
        /// <summary>
        /// Tareas disponibles para la ejecución en el monitor de vía.
        /// </summary>
        private TaskTrunkMonitor[] _tasks;

        /// <summary>
        /// Número total de tareas disponibles.
        /// </summary>
        private UInt16 _nTasks;

        /// <summary>
        /// Obtiene un valor True en caso que el monitor de vía se encuentre en ejecución.
        /// </summary>
        private Boolean _isRunnig;

        /// <summary>
        /// Temporizador del monitor de enlaces.
        /// </summary>
        private Timer _linkMonitor;

        /// <summary>
        /// Temporizador del monitor de alarmas.
        /// </summary>
        private Timer _alertMonitor;

        /// <summary>
        /// Temporizador del monitor de vehículos sin conexión.
        /// </summary>
        private Timer _busAlertMonitor;

        /// <summary>
        /// Obtiene el tiempo (minutos) de espera del monitor de enlaces.
        /// </summary>
        private const Double TIME_WAIT_LINK = 0.5;

        /// <summary>
        /// Obtiene el tiempo (minutos) de espera del monitor de alarmas.
        /// </summary>
        private const Double TIME_WAIT_ALERT = 0.5;

        /// <summary>
        /// Campo que provee a la propiedad 'Instance'.
        /// </summary>
        private static TrunkMonitorModelView _instance;

        /// <summary>
        /// Obtiene la instancia del modelo de la vista del monitor de vía.
        /// </summary>
        public static TrunkMonitorModelView Instance => _instance;

        /// <summary>
        /// Campo que provee a la propiedad 'Alerts'.
        /// </summary>
        private ObservableCollection<Alert> _alerts;

        /// <summary>
        /// Obtiene una lista de las alarmas de los dispositivos conectados en ruta.
        /// </summary>
        public ObservableCollection<Alert> Alerts {
            get {
                if (_alerts == null)
                    _alerts = new ObservableCollection<Alert>();
                return _alerts;
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'BusDisconnected'.
        /// </summary>
        private ObservableCollection<BusDisconnectedAlert> _busDisconnected;

        /// <summary>
        /// Obtiene una lista de los autobuses sin envíar su ubicación.
        /// </summary>
        public ObservableCollection<BusDisconnectedAlert> BusDisconnected {
            get {
                if (_busDisconnected == null)
                    _busDisconnected = new ObservableCollection<BusDisconnectedAlert>();
                return _busDisconnected;
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedAlert'.
        /// </summary>
        private Alert _selectedAlert;

        /// <summary>
        /// Obtiene o establece la alerta actualmente seleccionada en la tabla.
        /// </summary>
        public Alert SelectedAlert {
            get => _selectedAlert;
            set {
                _selectedAlert = value;
                OnPropertyChanged("SelectedAlert");
            }
        }

        /// <summary>
        /// Crea una instance del modelo de la vista del monitor de vía.
        /// </summary>
        public TrunkMonitorModelView()
        {
            _instance = this;
            LoadedHandlerCommand = new CommandBase((param) =>
              {
                  _isRunnig = true;
                  InitLinkMonitor();
                  InitAlertMonitor();
                  InitBusAlertMonitor();
              });
            UnloadedHandlerCommand = new CommandBase((param) =>
            {
                _linkMonitor.Dispose();
                _alertMonitor.Dispose();
                _isRunnig = false;
            });
            SelectedAlertCommand = new CommandBase((param) =>
            {
                if (SelectedAlert != null)
                    SelectedAlert.State = AlertState.READ;
            });
        }

        /// <summary>
        /// Inicializa el monitor de alarmas de vehículos sin conexión.
        /// </summary>
        private void InitBusAlertMonitor()
        {
            BusDisconnected.Clear();
            _busAlertMonitor = new Timer(delegate
            {
                Trace.WriteLine("Actualizando vehículos sin conexión", "DEBUG");
                var credentialDBServer = AcabusData.GetCredential("Server", "DataBase");
                var credentialSshServer = AcabusData.GetCredential("Server", "Ssh");

                SshPostgreSQL psql = SshPostgreSQL.CreateConnection(
                    AcabusData.PGPathPlus,
                    AcabusData.CC.FindDevice((device) => device.Type == DeviceType.DB).IP,
                    AcabusData.PGPort,
                    credentialDBServer.Username,
                    credentialDBServer.Password,
                    AcabusData.PGDatabaseName,
                    credentialSshServer.Username,
                    credentialSshServer.Password);

                var response = psql.ExecuteQuery(AcabusData.BusDisconnectedQuery);
                if (response == null) return;

                App.Current.Dispatcher.Invoke(()
                            => BusDisconnected.Clear());

                for (UInt16 i = 1; i < response.Length; i++)
                {
                    var row = response[i];
                    var exists = false;
                    foreach (var alert in BusDisconnected)
                        exists = alert.EconomicNumber == row[0];
                    if (exists)
                    {
                        foreach (var alert in BusDisconnected)
                            if (alert.EconomicNumber == row[0])
                                App.Current.Dispatcher.Invoke(()
                                    => alert.LastSentLocation = DateTime.Parse(row[1]));
                    }
                    else
                        App.Current.Dispatcher.Invoke(()
                            => BusDisconnected.Add(BusDisconnectedAlert.CreateBusAlert(row[0], DateTime.Parse(row[1]))));
                }

            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(TIME_WAIT_ALERT));
        }

        /// <summary>
        /// Inicializa el monitor de alarmas de estación.
        /// </summary>
        private void InitAlertMonitor()
        {
            Alerts.Clear();
            _alertMonitor = new Timer(delegate
            {
                Trace.WriteLine("Actualizando alarmas", "DEBUG");
                var credentialDBServer = AcabusData.GetCredential("Server", "DataBase");
                var credentialSshServer = AcabusData.GetCredential("Server", "Ssh");

                SshPostgreSQL psql = SshPostgreSQL.CreateConnection(
                    AcabusData.PGPathPlus,
                    AcabusData.CC.FindDevice((device) => device.Type == DeviceType.DB).IP,
                    AcabusData.PGPort,
                    credentialDBServer.Username,
                    credentialDBServer.Password,
                    AcabusData.PGDatabaseName,
                    credentialSshServer.Username,
                    credentialSshServer.Password);

                var response = psql.ExecuteQuery(AcabusData.TrunkAlertQuery);
                if (response == null) return;
                for (UInt16 i = 1; i < response.Length; i++)
                {
                    var row = response[i];
                    var numeseri = row[1].Replace("TSI", "TOR").Replace("TS", "TOR").Replace("TD", "TOR");
                    var exists = false;
                    foreach (var alert in Alerts)
                        if (alert.ID == UInt32.Parse(row[0]) && alert.Device.NumeSeri == numeseri)
                            exists = true;
                    if (!exists)
                        App.Current.Dispatcher.Invoke(()
                            => Alerts.Add(Alert.CreateAlert(UInt32.Parse(row[0]), numeseri, row[2], DateTime.Parse(row[3]))));
                }
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(TIME_WAIT_ALERT));
        }

        /// <summary>
        /// Permite crear una tarea que se ejecutará desde el monitor de vía.
        /// </summary>
        /// <param name="name">Nombre de la tarea a crear.</param>
        /// <param name="task">Acción que realizará la tarea al ejecutarse.</param>
        public static void CreateTask(String name, Action<Station> task)
        {
            if (Instance._tasks == null)
            {
                Instance._tasks = new TaskTrunkMonitor[5];
                Instance._nTasks = 0;
            }

            if (Instance._nTasks == Instance._tasks.Length)
                Array.Resize(ref Instance._tasks, Instance._tasks.Length + 5);

            Instance._tasks[Instance._nTasks] = new TaskTrunkMonitor(name, task);
            Instance._nTasks++;
            if (Instance._isRunnig)
                Instance.UpdateTaskList();
        }

        /// <summary>
        /// Actualiza el menu contextual del monitor de vía, utilizando la lista de tareas (_task).
        /// </summary>
        private void UpdateTaskList()
        {

        }

        /// <summary>
        /// Inicializa el monitor de enlace de estaciones.
        /// </summary>
        private void InitLinkMonitor()
        {
            _linkMonitor = new Timer(delegate
            {
                UpdateLinkStatus(Links);
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(TIME_WAIT_LINK));
        }

        /// <summary>
        /// Actualiza el estado de los enlaces.
        /// </summary>
        private void UpdateLinkStatus(ObservableCollection<Link> links, StateValue state = StateValue.GOOD)
        {
            if (links == null) return;

            CancellationTokenSource _token = new CancellationTokenSource();
            foreach (var link in links)
            {
                new Task(() =>
                {
                    if (state != StateValue.DISCONNECTED)
                        LinkService.DoPing(link);
                    else
                        link.State = StateValue.DISCONNECTED;
                    if (link.State > state)
                        link.State = state;
                    UpdateLinkStatus(link.StationB?.Links, link.State);

                }, _token.Token).Start();
            }
        }

        /// <summary>
        /// Obtiene o establece la lista de enlaces de estaciones.
        /// </summary>
        public ObservableCollection<Link> Links {
            get => AcabusData.CC.Links;
        }

        /// <summary>
        /// Obtiene el comando cuando se desencadena el evento Loaded de la vista.
        /// </summary>
        public ICommand LoadedHandlerCommand { get; }

        /// <summary>
        /// Obtiene el comando cuando se desencadena el evento Unloaded de la vista.
        /// </summary>
        public ICommand UnloadedHandlerCommand { get; }

        /// <summary>
        /// Obtiene el comando cuando se desencadena el evento SelectedChange de la tabla.
        /// </summary>
        public ICommand SelectedAlertCommand { get; }
    }
}
