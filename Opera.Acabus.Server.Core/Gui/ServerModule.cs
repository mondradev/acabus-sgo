using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using Opera.Acabus.Server.Core.Models;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Opera.Acabus.Server.Core.Gui
{
    /// <summary>
    /// Implementación parcial de <see cref="IServerModule"/> para notificar a la vista de los cambios ocurridos en el servicio.
    /// </summary>
    public abstract class ServerModule : IServerModule, INotifyPropertyChanged
    {
        /// <summary>
        /// Estado actual del servicio.
        /// </summary>
        private ServiceStatus _status = ServiceStatus.OFF;

        /// <summary>
        /// Evento que surge cuando se cambia el valor de una propiedad.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Obtiene el nombre del servicio.
        /// </summary>
        public abstract string ServiceName { get; }

        /// <summary>
        /// Obtiene el estado del servicio.
        /// </summary>
        public ServiceStatus Status {
            get => _status;
            private set {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        /// Realiza una petición asíncrona al módulo.
        /// </summary>
        /// <param name="message">Mensaje con la petición.</param>
        /// <param name="callback">Función de llamada de vuelta.</param>
        /// <returns>Una instancia Task.</returns>
        public abstract Task Request(IMessage message, Action<IMessage> callback);

        /// <summary>
        /// Captura el evento <see cref="PropertyChanged"/> y notifica a la interfaz que ha cambiado
        /// la propiedad.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad cambiada.</param>
        protected void OnPropertyChanged(String propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}