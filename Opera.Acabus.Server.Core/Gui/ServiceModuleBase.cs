using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
using Opera.Acabus.Core.Services;
using Opera.Acabus.Server.Core.Models;
using Opera.Acabus.Server.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Opera.Acabus.Server.Core.Gui
{
    /// <summary>
    /// Implementación parcial de <see cref="IServiceModule"/> para notificar a la vista de los
    /// cambios ocurridos en el servicio. También es una clase base para la creación de servicios
    /// para los distintos módulos que requieran propagar la información a través de los
    /// nodos conectados.
    /// </summary>
    public abstract class ServiceModuleBase : IServiceModule, INotifyPropertyChanged
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
            protected set {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        /// Realiza una petición asíncrona al módulo.
        /// </summary>
        /// <param name="message">Mensaje con la petición.</param>
        /// <param name="callback">Función de llamada de vuelta.</param>
        public void Request(IMessage message, Action<IMessage> callback, IAdaptiveMsgArgs e)
        {
            if (ServerHelper.ValidateRequest(message, GetType()))
                ServerHelper.CallFunc(message, GetType(), this);
            else
                ServerHelper.CreateError("Error al realizar la petición: " + GetType().FullName + " "
                     + message.GetString(AcabusAdaptiveMessageFieldID.FunctionName.ToInt32()), 403, e);

            callback?.Invoke(message);
        }

        /// <summary>
        /// Captura el evento <see cref="PropertyChanged"/> y notifica a la interfaz que ha cambiado
        /// la propiedad.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad cambiada.</param>
        protected void OnPropertyChanged(String propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}