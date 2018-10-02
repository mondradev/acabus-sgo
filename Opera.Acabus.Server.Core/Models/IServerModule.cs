using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using System;
using System.Threading.Tasks;

namespace Opera.Acabus.Server.Core.Models
{
    /// <summary>
    /// Representa la estructura básica de un módulo del servidor.
    /// </summary>
    public interface IServerModule
    {
        /// <summary>
        /// Obtiene el nombre del servicio.
        /// </summary>
        String ServiceName { get; }

        /// <summary>
        /// Obtiene el estatus del servicio.
        /// </summary>
        ServiceStatus Status { get; }

        /// <summary>
        /// Realiza una petición asíncrona al módulo.
        /// </summary>
        /// <param name="message">Mensaje con la petición.</param>
        /// <param name="callback">Función de llamada de vuelta.</param>
        /// <returns>Una instancia Task.</returns>
        Task Request(IMessage message, Action<IMessage> callback);
    }
}