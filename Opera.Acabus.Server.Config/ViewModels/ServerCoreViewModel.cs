using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Server.Core.Models;
using System;
using System.Collections.ObjectModel;

namespace Opera.Acabus.Server.Config.ViewModels
{
    /// <summary>
    /// Modelo de la vista <see cref="Views.ServerCoreView"/> el cual controla todas las
    /// interacciones con el resto del sistema.
    /// </summary>
    public sealed class ServerCoreViewModel : ViewModelBase
    {
        /// <summary>
        /// Indica el estado del servicio principal del servidor.
        /// </summary>
        private ServiceStatus _serviceStatus;

        /// <summary>
        /// Crea una nueva instancia del modelo.
        /// </summary>
        public ServerCoreViewModel()
        {
            ServiceModule = new ObservableCollection<IServiceModule>();

            ServerController.StatusChanged += (sender, status)
                => Status = status;

            Status = ServerController.Running ? ServiceStatus.ON : ServiceStatus.OFF;
        }

        /// <summary>
        /// Obtiene la lista de los módulos de servicios cargados.
        /// </summary>
        public ObservableCollection<IServiceModule> ServiceModule { get; }

        /// <summary>
        /// Obtiene el nombre del servicio principal del servidor.
        /// </summary>
        public String ServiceName => String.Format("SGO Server {0}", typeof(ServerCoreViewModel).Assembly.GetName().Version);

        /// <summary>
        /// Obtiene el estado actual del servicio principal del servidor.
        /// </summary>
        public ServiceStatus Status {
            get => _serviceStatus;
            private set {
                _serviceStatus = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        /// Este método es llamado durante la carga de la vista.
        /// </summary>
        /// <param name="parameter"></param>
        protected override void OnLoad(object parameter)
        {
            ServiceModule.Clear();

            foreach (IServiceModule module in ServerController.GetServerModules())
                ServiceModule.Add(module);
        }

        /// <summary>
        /// Este método es llamado durante la descarga de la vista.
        /// </summary>
        /// <param name="parameter"></param>
        protected override void OnUnload(object parameter)
        {
            ServiceModule.Clear();
        }
    }
}