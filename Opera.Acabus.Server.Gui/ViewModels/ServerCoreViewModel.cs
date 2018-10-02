using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Server.Core.Models;
using System;

namespace Opera.Acabus.Server.Gui.ViewModels
{
    /// <summary>
    /// Modelo de la vista <see cref="Views.ServerCoreView"/> el cual controla todas las interacciones con el resto del sistema.
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
            ServerController.StatusChanged += (sender, status)
                => Status = status;

            Status = ServerController.Running ? ServiceStatus.ON : ServiceStatus.OFF;
        }

        /// <summary>
        /// Obtiene el nombre del servicio principal del servidor.
        /// </summary>
        public String ServiceName { get; } = "SGO Server v0.1";

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
    }
}