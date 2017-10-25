using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui.Modules;
using Opera.Acabus.Core.Models;
using Opera.Acabus.TrunkMonitor.Helpers;
using Opera.Acabus.TrunkMonitor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Opera.Acabus.TrunkMonitor.ViewModels
{
    /// <summary>
    /// Define las diferentes tareas que se ejecutan.
    /// </summary>
    internal enum TaskDescriptor
    {
        /// <summary>
        /// Del tipo replica.
        /// </summary>
        REPLY,

        /// <summary>
        /// Del tipo de enlace.
        /// </summary>
        LINK,

        /// <summary>
        /// Del tipo de estación.
        /// </summary>
        STATION_LINK
    }

    /// <summary>
    /// Modelo la vista del monitor de vía, proporciona toda las funciones que realizará el monitor
    /// de vía.
    /// </summary>
    public class TrunkMonitorViewModel : ModuleViewerBase
    {
        /// <summary>
        /// Obtiene el tiempo (minutos) de espera del monitor de enlaces.
        /// </summary>
        private const Double TIME_WAIT_LINK = 0.5;

        /// <summary>
        /// Obtiene el tiempo (minutos) de espera para verificar la replica.
        /// </summary>
        private const int TIME_WAIT_REPLICA = 10;

        /// <summary>
        /// Obtiene el tiempo (minutos) de espera del monitor de estaciones.
        /// </summary>
        private const Double TIME_WAIT_STATION = 0.5;

        /// <summary>
        /// Campo que provee a la propiedad ' <see cref="ControlCenter"/>'.
        /// </summary>
        private Station _controlCenter;

        /// <summary>
        /// Temporizador del monitor de enlaces.
        /// </summary>
        private Timer _linkMonitor;

        /// <summary>
        /// Campo que provee a la propiedad ' <see cref="Links"/>'.
        /// </summary>
        private ObservableCollection<Link> _links;

        /// <summary>
        /// Temporizador del monitor de replica de estaciones y externos.
        /// </summary>
        private Timer _replyMonitor;

        /// <summary>
        /// Indica si el monitor de enlaces está en ejecución.
        /// </summary>
        private bool _runningLinkMonitor = false;

        /// <summary>
        /// Indica si el monitor de replica está en ejecución.
        /// </summary>
        private bool _runningReplyMonitor = false;

        /// <summary>
        /// Indica si el monitor de estaciones está en ejecución.
        /// </summary>
        private bool _runningStationMonitor = false;

        /// <summary>
        /// Una lista de todas las tareas actualmente en ejecución.
        /// </summary>
        private ObservableCollection<TaskService> _runningTasks;

        /// <summary>
        /// Temporizador del monitor de estaciones y externos.
        /// </summary>
        private Timer _stationMonitor;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="TasksAvailable"/>.
        /// </summary>
        private ICollection<TaskTrunkMonitor> _taskAvailable;

        /// <summary>
        /// Es la fuente del token de cancelación de todos los procesos del módulo.
        /// </summary>
        private CancellationTokenSource _tokenSource;

        /// <summary>
        /// Crea una instance del modelo de la vista del monitor de vía.
        /// </summary>
        public TrunkMonitorViewModel()
        {
            ViewModelService.Register(this);

            CreateTask("Alertas", (station) =>
            {
                StringBuilder stringBuilder = new StringBuilder();
                StationStateInfo stateInfo = station.GetStateInfo();

                foreach (var message in stateInfo.Messages)
                    stringBuilder.AppendLine(message.Message);

                if (stringBuilder.Length == 0)
                    stringBuilder.AppendLine("Sin alertas.");

                ShowMessage(stringBuilder.Insert(0, $"Estación: {station}\n\n").ToString());
            });

            _tokenSource = new CancellationTokenSource();
            _runningTasks = new ObservableCollection<TaskService>();

            _runningTasks.CollectionChanged += (sender, arguments) =>
            {
                if (arguments.Action != NotifyCollectionChangedAction.Remove
                    && arguments.Action != NotifyCollectionChangedAction.Reset)
                    return;

                var runningTasks = _runningTasks.ToArray();

                if (!runningTasks.Any(t => t?.Descriptor == TaskDescriptor.LINK))
                    _runningLinkMonitor = false;

                if (!runningTasks.Any(t => t?.Descriptor == TaskDescriptor.REPLY))
                    _runningReplyMonitor = false;

                if (!runningTasks.Any(t => t?.Descriptor == TaskDescriptor.STATION_LINK))
                    _runningStationMonitor = false;

                if (!runningTasks.Any())
                    _tokenSource = new CancellationTokenSource();

                foreach (var taskService in runningTasks.Where(t => t == null || t.Task.IsCanceled))
                    if (taskService != null)
                        _runningTasks.Remove(taskService);
            };
        }

        /// <summary>
        /// Obtiene la estación que representa a Centro de Control.
        /// </summary>
        public Station ControlCenter => _controlCenter;

        /// <summary>
        /// Obtiene o establece la lista de enlaces de estaciones.
        /// </summary>
        public ObservableCollection<Link> Links => _links;

        /// <summary>
        /// Obtiene una lista de las tareas disponibles en el monitor de vía.
        /// </summary>
        public ICollection<TaskTrunkMonitor> TasksAvailable
            => _taskAvailable ?? (_taskAvailable = new ObservableCollection<TaskTrunkMonitor>());

        /// <summary>
        /// Permite crear una tarea que se ejecutará desde el monitor de vía.
        /// </summary>
        /// <param name="name">Nombre de la tarea a crear.</param>
        /// <param name="task">Acción que realizará la tarea al ejecutarse.</param>
        public void CreateTask(String name, Action<Station> task)
            => TasksAvailable.Add(new TaskTrunkMonitor(name, task));

        /// <summary>
        /// Permite crear una tarea que se ejecutará desde el monitor de vía.
        /// </summary>
        /// <param name="task">Nueva tarea.</param>
        public void CreateTask(TaskTrunkMonitor task)
            => TasksAvailable.Add(task);

        /// <summary>
        /// Función llamada por el comando <see cref="LoadCommand"/>.
        /// </summary>
        /// <param name="parameter">Parametro del comando.</param>
        protected override void OnLoad(object parameter)
        {
            _links = null;
            OnPropertyChanged(nameof(Links));

            List<Link> linksList = TrunkMonitorModule.AllLinks?.ToList();

            if (linksList == null)
                return;

            _controlCenter = linksList.Where(l => l.StationA.Name.Contains("CENTRO DE CONTROL"))
                .Select(l => l.StationA)
                .FirstOrDefault();

            _links = ControlCenter?.GetLinks();

            foreach (var link in linksList)
            {
                AcabusDataContext.DbContext.LoadRefences(link.StationA, nameof(Station.Devices));
                AcabusDataContext.DbContext.LoadRefences(link.StationB, nameof(Station.Devices));
            }

            OnPropertyChanged(nameof(Links));

            InitMonitor();
        }

        /// <summary>
        /// Función llamada por el comando <see cref="UnloadCommand"/>.
        /// </summary>
        /// <param name="parameter">Parametro del comando.</param>
        protected override void OnUnload(object parameter)
        {
            _tokenSource?.Cancel();

            _linkMonitor?.Dispose();
            _stationMonitor?.Dispose();
            _replyMonitor?.Dispose();

            _runningLinkMonitor = false;
            _runningReplyMonitor = false;
            _runningStationMonitor = false;

            _runningTasks.Clear();
        }

        /// <summary>
        /// Verifica la replica de los equipos de las estaciones.
        /// </summary>
        /// <param name="initialStation">Estación inicial.</param>
        private void CheckReply(Station initialStation)
        {
            if (initialStation == null)
                return;

            if (_tokenSource.IsCancellationRequested)
                return;

            foreach (var link in initialStation.GetLinks())
            {
                if (_tokenSource.IsCancellationRequested)
                    break;

                var currentTask = null as Task;

                currentTask = Task.Run(() =>
                {
                    try
                    {
                        CheckReply(link.StationB);
                    }
                    finally
                    {
                        _runningTasks.Remove(new TaskService(currentTask, TaskDescriptor.REPLY));
                    }
                }, _tokenSource.Token);

                _runningTasks.Add(new TaskService(currentTask, TaskDescriptor.REPLY));
            }

            if (_tokenSource.IsCancellationRequested)
                return;

            initialStation.VerifyReply(_tokenSource.Token);
        }

        /// <summary>
        /// Inicializa el monitor de enlace de estaciones.
        /// </summary>
        private void InitMonitor()
        {
            _linkMonitor = new Timer(delegate
            {
                if (_runningLinkMonitor && _runningTasks.Count > 0)
                {
                    foreach (var t in _runningTasks.Where(rt => rt.Task.IsCompleted).ToArray())
                        if (t != null)
                            _runningTasks.Remove(t);

                    return;
                }

                _runningLinkMonitor = true;

                UpdateLinkStatus(Links);
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(TIME_WAIT_LINK));

            _stationMonitor = new Timer(delegate
            {
                if (_runningStationMonitor && _runningTasks.Count > 0)
                    return;

                _runningStationMonitor = true;

                UpdateStationStatus(ControlCenter);
            }, null, TimeSpan.FromSeconds(0.5), TimeSpan.FromMinutes(TIME_WAIT_STATION));

            _replyMonitor = new Timer(delegate
            {
                if (_runningReplyMonitor && _runningTasks.Count > 0)
                    return;

                _runningReplyMonitor = true;

                CheckReply(ControlCenter);
            }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(TIME_WAIT_REPLICA));
        }

        /// <summary>
        /// Actualiza el estado de los enlaces.
        /// </summary>
        private void UpdateLinkStatus(ObservableCollection<Link> links, LinkState state = LinkState.GOOD)
        {
            if (links == null) return;

            foreach (var link in links)
            {
                if (_tokenSource.IsCancellationRequested)
                    break;

                var currentTask = null as Task;

                currentTask = Task.Run(() =>
                 {
                     try
                     {
                         if (_tokenSource.IsCancellationRequested)
                             return;

                         if (state != LinkState.DISCONNECTED)
                         {
                             var ping = link.DoPing();

                             if (link.State == LinkState.DISCONNECTED || ping < 0)
                             {
                                 link.State = LinkState.DISCONNECTED;
                                 SendNotify(String.Format("Enlace a {0} sin conexión", link.StationB));
                             }
                         }
                         else
                         {
                             link.State = LinkState.DISCONNECTED;
                             link.Ping = -1;
                         }

                         if (_tokenSource.IsCancellationRequested)
                             return;

                         UpdateLinkStatus(link.StationB?.GetLinks(), link.State);
                     }
                     finally
                     {
                         var task = new TaskService(currentTask, TaskDescriptor.LINK);

                         lock (_runningTasks)
                         {
                             if (_runningTasks.Contains(task))
                                 _runningTasks.Remove(task);
                         }
                     }
                 }, _tokenSource.Token);

                _runningTasks.Add(new TaskService(currentTask, TaskDescriptor.LINK));
            }
        }

        /// <summary>
        /// Actualiza el estado de las estaciones.
        /// </summary>
        /// <param name="stationA">Estación de partida.</param>
        private void UpdateStationStatus(Station stationA)
        {
            if (stationA == null)
                return;

            if (_tokenSource.IsCancellationRequested)
                return;

            var currentTask = null as Task;

            currentTask = Task.Run(() =>
            {
                try
                {
                    stationA.CheckDevice();

                    foreach (var link in stationA.GetLinks())
                    {
                        if (_tokenSource.IsCancellationRequested)
                            break;

                        UpdateStationStatus(link.StationB);
                    }
                }
                finally
                {
                    _runningTasks.Remove(new TaskService(currentTask, TaskDescriptor.STATION_LINK));
                }
            });

            _runningTasks.Add(new TaskService(currentTask, TaskDescriptor.STATION_LINK));
        }
    }

    /// <summary>
    /// Representa una tarea de servicio proporcionado por el módulo.
    /// </summary>
    internal class TaskService
    {
        /// <summary>
        /// Crea una instancia nueva.
        /// </summary>
        /// <param name="task">La tarea a ser contenida.</param>
        /// <param name="descriptor">Descriptor de la tarea.</param>
        public TaskService(Task task, TaskDescriptor descriptor)
        {
            Task = task;
            Descriptor = descriptor;
        }

        /// <summary>
        /// Obtiene el descriptor de la tarea.
        /// </summary>
        public TaskDescriptor Descriptor { get; }

        /// <summary>
        /// Obtiene la tarea.
        /// </summary>
        public Task Task { get; }

        /// <summary>
        /// Determina si dos instancias son igual.
        /// </summary>
        /// <param name="obj">Otra instancia a comparar.</param>
        /// <returns>Un valor de true si las instancias son igual.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(TaskService)) return false;

            return (obj as TaskService).GetHashCode() == GetHashCode();
        }

        /// <summary>
        /// Obtiene el código hash de la instancia actual.
        /// </summary>
        /// <returns>Código hash del objeto.</returns>
        public override int GetHashCode()
            => Tuple.Create(Task, Descriptor).GetHashCode();
    }
}