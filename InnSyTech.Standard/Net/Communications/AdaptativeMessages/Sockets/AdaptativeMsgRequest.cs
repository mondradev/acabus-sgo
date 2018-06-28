using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace InnSyTech.Standard.Net.Communications.AdaptativeMessages.Sockets
{
    /// <summary>
    /// Representa una petición de mensaje adaptativo.
    /// </summary>
    public sealed class AdaptativeMsgRequest
    {
        /// <summary>
        /// Gestiona la conexión de la petición.
        /// </summary>
        private readonly Socket _socket;

        /// <summary>
        /// Crea una instancia nueva para realizar una petición con el servidor.
        /// </summary>
        /// <param name="rules">Reglas de composición de mensajes.</param>
        public AdaptativeMsgRequest(MessageRules rules, IPAddress ipAddress, int port)
        {
            Rules = rules;
            Port = port;
            IPAddress = ipAddress;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(IPAddress, Port);
        }

        /// <summary>
        /// Obtiene la dirección IP del servidor a conectar.
        /// </summary>
        public IPAddress IPAddress { get; }

        /// <summary>
        /// Obtiene o establece el puerto TCP por el cual escucha el servidor.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Obtiene las reglas que permiten serializar y deserializar los mensajes.
        /// </summary>
        public MessageRules Rules { get; }

        /// <summary>
        /// Realiza una petición sincrónica al servidor.
        /// </summary>
        /// <param name="message">Mensaje que representa la petición.</param>
        /// <returns>Mensaje que representa la respuesta del servidor.</returns>
        public Message DoRequest(Message message)
        {
            int bytesTransferred = _socket.Send(message.Serialize());

            if (bytesTransferred <= 0)
                return null;

            while (true)
            {
                Thread.Sleep(10);

                if (_socket.Available <= 0)
                    continue;

                Byte[] buffer = new Byte[_socket.Available];

                bytesTransferred = _socket.Receive(buffer);

                return Message.Deserialize(buffer, Rules);
            }
        }
    }
}