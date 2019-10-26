using System.Net.Sockets;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets
{
    /// <summary>
    /// Defines las propiedades de los argumentos utilizados para la transferencia de mensajes adaptativos.
    /// </summary>
    /// <seealso cref="AdaptiveMessageServer"/>
    /// <seealso cref="IAdaptiveMessage"/>
    /// <seealso cref="AdaptiveMessageRequest"/>
    public interface IAdaptiveMessageReceivedArgs
    {
        /// <summary>
        /// Obtiene la conexión que desencadenó el evento.
        /// </summary>
        Socket Connection { get; }

        /// <summary>
        /// Obtiene la información captura en el evento.
        /// </summary>
        IAdaptiveMessage Data { get; }

        /// <summary>
        /// Crea un mensaje nuevo.
        /// </summary>
        /// <returns>Obtiene un mensaje totalmente nuevo.</returns>
        IAdaptiveMessage CreateMessage();

        /// <summary>
        /// Envía un mensaje al otro extremo de la conexión.
        /// </summary>
        /// <param name="response">Mensaje de respuesta.</param>
        void Response(IAdaptiveMessage response);

        /// <summary>
        /// Envía de vuelta el mensaje de la petición al otro extremo de la conexión.
        /// </summary>
        void Response();
    }
}