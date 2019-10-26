using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets
{
    /// <summary>
    /// Representa una petición de mensaje adaptativo.
    /// </summary>
    public sealed class AdaptiveMessageRequest
    {

        /// <summary>
        /// Crea una instancia nueva para realizar una petición con el servidor.
        /// </summary>
        /// <param name="rules">Reglas de composición de mensajes.</param>
        public AdaptiveMessageRequest(String rulesPath, IPAddress ipAddress, int port)
        {
            Rules = AdaptiveMessageRules.Load(rulesPath);
            Port = port;
            IPAddress = ipAddress;

            RemoteEndPoint = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
        public AdaptiveMessageRules Rules { get; }

        /// <summary>
        /// Obtiene la instancia que controla la conexión al equipo remoto.
        /// </summary>
        public Socket RemoteEndPoint { get; }

        /// <summary>
        /// Crea un mensaje vacío a partir de las reglas especificadas en la petición.
        /// </summary>
        /// <returns>Un mensaje nuevo.</returns>
        public IAdaptiveMessage CreateMessage() => new AdaptiveMessage(Rules);

        /// <summary>
        /// Realiza una petición asincrónica al servidor.
        /// </summary>
        /// <param name="message">Mensaje que representa la petición.</param>
        /// <returns>La tarea de petición.</returns>
        public Task<IAdaptiveMessage> Send(IAdaptiveMessage message)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (!RemoteEndPoint.Connected)
                        RemoteEndPoint.Connect(IPAddress, Port);

                    int bytesTransferred = RemoteEndPoint.Send(message.Serialize());

                    if (bytesTransferred <= 0)
                        return null;

                    var response = AdaptiveMessageSocketHelper.ReadBuffer(RemoteEndPoint, Rules);

                    return response;
                }
                catch
                {
                    return null;
                }
                finally
                {
                    if (RemoteEndPoint.Connected)
                        RemoteEndPoint.Close();
                }
            });
        }

        /// <summary>
        /// Realiza una petición asincrónica con una secuencia de datos como respuesta.
        /// </summary>
        /// <param name="message">Mensaje que representa la petición.</param>
        public Task<AdaptiveMessageCollection<TResult>> Send<TResult>(IAdaptiveMessage message, Func<IAdaptiveMessage, TResult> converter)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (!RemoteEndPoint.Connected)
                        RemoteEndPoint.Connect(IPAddress, Port);

                    var response = message;

                    int bytesTransferred = RemoteEndPoint.Send(response.Serialize());

                    if (bytesTransferred <= 0)
                        return null;

                    Trace.WriteLine("Bytes enviados: " + bytesTransferred, "DEBUG");

                    response = AdaptiveMessageSocketHelper.ReadBuffer(RemoteEndPoint, Rules);

                    if (!response.IsEnumerable())
                        return null;

                    return new AdaptiveMessageCollection<TResult>(response, this, converter);
                }
                catch
                {
                    if (RemoteEndPoint.Connected)
                        RemoteEndPoint.Close();

                    return null;
                }
            });
        }
    }
}