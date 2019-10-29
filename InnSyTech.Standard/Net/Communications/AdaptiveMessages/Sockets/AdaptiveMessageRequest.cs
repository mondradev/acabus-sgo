using System;
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
        /// Obtiene la instancia que controla la conexión al equipo remoto.
        /// </summary>
        public Socket RemoteEndPoint { get; }

        /// <summary>
        /// Obtiene las reglas que permiten serializar y deserializar los mensajes.
        /// </summary>
        public AdaptiveMessageRules Rules { get; }

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
                        throw new SocketException((int)SocketError.Interrupted);

                    var response = AdaptiveMessageSocketHelper.ReadBuffer(RemoteEndPoint, Rules);

                    return response;
                }
                catch (SocketException ex)
                {
                    message.SetResponse($"No se logró enviar la petición [Razón={ex.Message}]", AdaptiveMessageResponseCode.SERVICE_UNAVAILABLE);

                    return null;
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

                   int bytesTransferred = RemoteEndPoint.Send(message.Serialize());

                   if (bytesTransferred <= 0)
                       throw new SocketException((int)SocketError.Interrupted);
                   else
                       AdaptiveMessageSocketHelper.ReadBuffer(RemoteEndPoint, Rules).CopyTo(message);

                   if (!message.IsEnumerable() || message.GetResponseCode() != AdaptiveMessageResponseCode.PARTIAL_CONTENT)
                       message.SetAsEnumerable(0);
               }
               catch (SocketException ex)
               {
                   message.SetResponse($"No se logró enviar la petición [Razón={ex.Message}]", AdaptiveMessageResponseCode.SERVICE_UNAVAILABLE);
                   message.SetAsEnumerable(0);
               }
               catch
               {
                   if (RemoteEndPoint.Connected)
                       RemoteEndPoint.Close();

                   message.SetAsEnumerable(0);
               }

               return new AdaptiveMessageCollection<TResult>(message, this, converter);
           });
        }
    }
}