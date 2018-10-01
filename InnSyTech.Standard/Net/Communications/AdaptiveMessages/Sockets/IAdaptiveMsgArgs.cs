using System.Net.Sockets;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets
{
    /// <summary>
    /// Defines las propiedades de los argumentos utilizados para la transferencia de mensajes adaptativos.
    /// </summary>
    /// <seealso cref="AdaptiveMsgServer"/>
    /// <seealso cref="IMessage"/>
    /// <seealso cref="AdaptiveMsgRequest"/>
    public interface IAdaptiveMsgArgs
    {
        /// <summary>
        /// Obtiene la conexión que desencadenó el evento.
        /// </summary>
        Socket Connection { get; }

        /// <summary>
        /// Obtiene la información captura en el evento.
        /// </summary>
        IMessage Data { get; }

        /// <summary>
        /// Crea un mensaje nuevo.
        /// </summary>
        /// <returns>Obtiene un mensaje totalmente nuevo.</returns>
        IMessage CreateMessage();

        /// <summary>
        /// Envía un mensaje al otro extremo de la conexión.
        /// </summary>
        /// <param name="response">Mensaje de respuesta.</param>
        void Send(IMessage response);
    }
}