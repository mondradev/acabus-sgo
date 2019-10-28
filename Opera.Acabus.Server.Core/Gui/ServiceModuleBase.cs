using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
using Opera.Acabus.Server.Core.Models;
using Opera.Acabus.Server.Core.Utils;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

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
        /// <param name="e">Parametros utilizados para el envío de respuestas al cliente.</param>
        public void Request(IAdaptiveMessageReceivedArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    if (ServerHelper.ValidateRequest(e.Data, GetType()))
                        ServerHelper.CallFunc(e, GetType());
                    else
                        throw new ServiceException("No se encontró la función solicitada o no coindicieron los parametros especificados.", AdaptiveMessageResponseCode.BAD_REQUEST, e.Data.GetFunctionName(), ServiceName);

                    e.Response();
                }
                catch (ServiceException ex)
                {
                    e.SendException(ex);
                }
                catch (Exception ex)
                {
                    e.SendException(new ServiceException(e.Data.GetFunctionName(), ServiceName, ex));
                }
            });
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