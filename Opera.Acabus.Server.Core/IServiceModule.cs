using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
using System;

namespace Opera.Acabus.Server.Core.Models
{
    /// <summary>
    /// Representa la estructura básica de un módulo del servidor.
    /// </summary>
    public interface IServiceModule
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
        /// <param name="e">Parametros utilizados para el envío de respuestas al cliente.</param>
        void Request(IAdaptiveMessageReceivedArgs e);
    }
}