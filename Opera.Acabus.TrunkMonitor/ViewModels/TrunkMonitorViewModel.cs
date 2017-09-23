using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui.Modules;
using Opera.Acabus.Core.Models;
using Opera.Acabus.TrunkMonitor.Helpers;
using Opera.Acabus.TrunkMonitor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
        /// Campo que provee a la propiedad '<see cref="Links"/>'.
        /// </summary>
        private ObservableCollection<Link> _links;

        /// <summary>
        /// Temporizador del monitor de estaciones y externos.
        /// </summary>
        private Timer _stationMonitor;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="TasksAvailable" />.
        /// </summary>
        private ICollection<TaskTrunkMonitor> _taskAvailable;

        /// <summary>
        /// Crea una instance del modelo de la vista del monitor de vía.
        /// </summary>
        public TrunkMonitorViewModel()
        {
            ViewModelService.Register(this);

            var torniquiteType = new[]
            {
                DeviceType.TD,
                DeviceType.TOR,
                DeviceType.TSI,
                DeviceType.TS
            };

            CreateTask("Alertas", (station) =>
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var message in station.GetStateInfo().Messages)
                    stringBuilder.AppendLine(message);

                if (stringBuilder.Length == 0)
                    stringBuilder.AppendLine("Sin alertas.");

                ShowMessage(stringBuilder.Insert(0, $"Estación: {station}\n\n").ToString());
            });
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
            List<Link> linksList = TrunkMonitorModule.AllLinks.ToList();

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
            _linkMonitor.Dispose();
        }

        /// <summary>
        /// Inicializa el monitor de enlace de estaciones.
        /// </summary>
        private void InitMonitor()
        {
            _linkMonitor = new Timer(delegate
            {
                UpdateLinkStatus(Links);
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(TIME_WAIT_LINK));

            _stationMonitor = new Timer(delegate
            {
                UpdateStationStatus(ControlCenter);
            }, null, TimeSpan.FromSeconds(0.5), TimeSpan.FromMinutes(TIME_WAIT_STATION));
        }

        /// <summary>
        /// Actualiza el estado de los enlaces.
        /// </summary>
        private void UpdateLinkStatus(ObservableCollection<Link> links, LinkState state = LinkState.GOOD)
        {
            if (links == null) return;

            foreach (var link in links)
            {
                Task.Run(() =>
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
                });
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

            stationA.CheckDevice();

            foreach (var link in stationA.GetLinks())
                UpdateStationStatus(link.StationB);
        }
    }
}