using System;
using System.Diagnostics;
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
        }

        /// <summary>
        /// Obtiene la dirección IP del servidor a conectar.
        /// </summary>
        public IPAddress IPAddress { get; }

        /// <summary>
        /// Realiza una petición asincrónica con una secuencia de datos como respuesta.
        /// </summary>
        /// <param name="message">Mensaje que representa la petición.</param>
        /// <param name="onSuccess">Controlador del recorrido de la secuencia.</param>
        public Task DoRequestToList(IMessage message, Action<IAdaptiveMsgEnumerator> onSuccess, Action<Int32, String> onFail = null)
        {
            if (!_socket.Connected)
                _socket.Connect(IPAddress, Port);

            return Task.Run(() =>
            {
                var response = message;

                while (true)
                {
                    int bytesTransferred = _socket.Send(response.Serialize());

                    if (bytesTransferred <= 0)
                        break;

                    Trace.WriteLine("Bytes enviados: " + bytesTransferred, "DEBUG");

                    response = ReadBuffer();

                    if (!response.IsSet(3))
                    {
                        onFail?.Invoke(0, "No se logró recibir correctamente el mensaje");
                        break;
                    }

                    if (response.GetInt32(3) != 200)
                    {
                        onFail?.Invoke(response.GetInt32(3), response.GetString(4));
                        break;
                    }

                    if (!response.IsSet(7))
                    {
                        onFail?.Invoke(response.GetInt32(3), "El mensaje no es una enumeración, utilice DoRequest()");
                        break;
                    }

                    int count = response.GetInt32(8);

                    if (count == 0)
                        break;

                    AdaptiveMsgEnumerator msgEnumerator = new AdaptiveMsgEnumerator(response);
                    onSuccess?.Invoke(msgEnumerator);

                    if (msgEnumerator.Breaking)
                        break;

                    if (!response.IsSet(10) || response.GetValue(10, x => Convert.ToInt32(x)) != 1)
                        response[10] = 0;

                    if (response.GetValue(9, x => Convert.ToInt32(x)) >= response.GetValue(8, x => Convert.ToInt32(x)) - 1)
                        break;
                }
            });
        }

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
        public IMessage CreateMessage() => new Message(Rules);

        /// <summary>
        /// Realiza una petición asincrónica al servidor.
        /// </summary>
        /// <param name="message">Mensaje que representa la petición.</param>
        /// <param name="onSuccess">Función que se ejecuta al recibir la respuesta.</param>
        public Task DoRequest(IMessage message, Action<IMessage> onSuccess, Action<int, string> onFail = null)
        {
            if (!_socket.Connected)
                _socket.Connect(IPAddress, Port);

            return Task.Run(() =>
            {
                int bytesTransferred = _socket.Send(message.Serialize());

                if (bytesTransferred <= 0)
                    onSuccess?.Invoke(null);

                var response = ReadBuffer();

                if (!response.IsSet(3))
                {
                    onFail?.Invoke(0, "No se logró recibir correctamente el mensaje");
                    return;
                }

                if (response.GetInt32(3) != 200)
                    onFail?.Invoke(response.GetInt32(3), response.GetString(4));

                onSuccess?.Invoke(response);
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

                if (bytesTransferred <= 0)
                    continue;

                Trace.WriteLine("Bytes recibidos: " + bytesTransferred, "DEBUG");

                return Message.Deserialize(buffer, Rules);
            }
        }
    }
}