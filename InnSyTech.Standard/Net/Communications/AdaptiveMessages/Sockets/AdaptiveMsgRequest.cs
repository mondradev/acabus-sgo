﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets
{
    /// <summary>
    /// Representa una petición de mensaje adaptativo.
    /// </summary>
    public sealed class AdaptiveMsgRequest
    {
        /// <summary>
        /// Gestiona la conexión de la petición.
        /// </summary>
        private readonly Socket _socket;

        /// <summary>
        /// Crea una instancia nueva para realizar una petición con el servidor.
        /// </summary>
        /// <param name="rules">Reglas de composición de mensajes.</param>
        public AdaptiveMsgRequest(String rulesPath, IPAddress ipAddress, int port)
        {
            Rules = MessageRules.Load(rulesPath);
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
        internal MessageRules Rules { get; }

        /// <summary>
        /// Crea un mensaje vacío a partir de las reglas especificadas en la petición.
        /// </summary>
        /// <returns>Un mensaje nuevo.</returns>
        public IMessage CreateMessage() => new Message(this.Rules);

        /// <summary>
        /// Realiza una petición asincrónica al servidor.
        /// </summary>
        /// <param name="message">Mensaje que representa la petición.</param>
        /// <param name="callback">Función que se ejecuta al recibir la respuesta.</param>
        public Task DoRequest(IMessage message, Action<IMessage> callback)
        {
            return Task.Run(() =>
            {
                int bytesTransferred = _socket.Send(message.Serialize());

                if (bytesTransferred <= 0)
                    callback?.Invoke(null);

                callback?.Invoke(ReadBuffer());
            });
        }

        /// <summary>
        /// Lee el buffer y los convierte en un mensaje compatible.
        /// </summary>
        /// <returns>Mensaje leido del buffer.</returns>
        private IMessage ReadBuffer()
        {
            int bytesTransferred = 0;

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