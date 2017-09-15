﻿using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui.Modules;
using Opera.Acabus.Core.Models;
using Opera.Acabus.TrunkMonitor.Helpers;
using Opera.Acabus.TrunkMonitor.Models;
using Opera.Acabus.TrunkMonitor.Service;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Opera.Acabus.TrunkMonitor.ViewModels
{
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
        /// Campo que provee a la propiedad 'Instance'.
        /// </summary>
        private static TrunkMonitorViewModel _instance;

        /// <summary>
        /// Obtiene un valor True en caso que el monitor de vía se encuentre en ejecución.
        /// </summary>
        private Boolean _isRunnig;

        /// <summary>
        /// Temporizador del monitor de enlaces.
        /// </summary>
        private Timer _linkMonitor;

        /// <summary>
        /// Número total de tareas disponibles.
        /// </summary>
        private UInt16 _nTasks;

        /// <summary>
        /// Tareas disponibles para la ejecución en el monitor de vía.
        /// </summary>
        private TaskTrunkMonitor[] _tasks;

        /// <summary>
        /// Crea una instance del modelo de la vista del monitor de vía.
        /// </summary>
        public TrunkMonitorViewModel() { _instance = this; }

        /// <summary>
        /// Obtiene la instancia del modelo de la vista del monitor de vía.
        /// </summary>
        public static TrunkMonitorViewModel Instance => _instance;

        /// <summary>
        /// Obtiene o establece la lista de enlaces de estaciones.
        /// </summary>
        public ObservableCollection<Link> Links => AcabusDataContext.DbContext?.Read<Station>()
            .SingleOrDefault(s => s.Name.Contains("CENTRO DE CONTROL"))?.GetLinks();

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
        /// Función llamada por el comando <see cref="LoadCommand"/>.
        /// </summary>
        /// <param name="parameter">Parametro del comando.</param>
        protected override void OnLoad(object parameter)
        {
            _isRunnig = true;
            InitLinkMonitor();
        }

        /// <summary>
        /// Función llamada por el comando <see cref="UnloadCommand"/>.
        /// </summary>
        /// <param name="parameter">Parametro del comando.</param>
        protected override void OnUnload(object parameter)
        {
            _linkMonitor.Dispose();
            _isRunnig = false;
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
        private void UpdateLinkStatus(ObservableCollection<Link> links, LinkState state = LinkState.GOOD)
        {
            if (links == null) return;

            foreach (var link in links)
            {
                new Task(() =>
                {
                    if (state != LinkState.DISCONNECTED)
                    {
                        link.DoPing();
                        if (link.State == LinkState.DISCONNECTED)
                            SendNotify(String.Format("Enlace {0} sin conexión", link));
                    }
                    else
                        link.State = LinkState.DISCONNECTED;
                    if (link.State > state)
                    {
                        link.State = state;
                        link.Ping = -1;
                    }
                    UpdateLinkStatus(link.StationB?.GetLinks(), link.State);
                }).Start();
            }
        }

        /// <summary>
        /// Actualiza el menu contextual del monitor de vía, utilizando la lista de tareas (_task).
        /// </summary>
        private void UpdateTaskList()
        {
            /// TODO: Implementar actualizar lista de tareas.
        }
    }

    /// <summary>
    /// Proporciona una estructura para manipular la tareas que se prodrán ejecutar dentro del
    /// monitor de vía.
    /// </summary>
    internal class TaskTrunkMonitor
    {
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

        /// <summary>
        /// Nombre de la tarea como se visualizará en el monitor.
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// Acción que realizará la tarea cuando esta se seleccione.
        /// </summary>
        public Action<Station> Task { get; private set; }
    }
}