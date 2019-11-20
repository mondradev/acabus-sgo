using InnSyTech.Standard.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets
{
    /// <summary>
    /// Clase auxiliar de sockets.
    /// </summary>
    public static class AdaptiveMessageSocketHelper
    {
        /// <summary>
        /// Obtiene los datos leidos desde el buffer del socket.
        /// </summary>
        public static byte[] ReadBuffer(Socket endPoint)
        {
            int bytesTransferred = 0;
            byte[] buffer = new byte[1024];

            using ManualResetEvent locker = new ManualResetEvent(false);

            AsyncCallback callback = null;

            callback = new AsyncCallback((result) =>
            {
                if (!endPoint.Connected)
                {
                    locker.Set();
                    return;
                }

                bytesTransferred = endPoint.EndReceive(result);

                if (bytesTransferred <= 0)
                {
                    endPoint.BeginReceive(buffer, 0, 1024, SocketFlags.None, callback, endPoint);
                    return;
                }

                Trace.TraceInformation("Bytes recibidos: " + bytesTransferred + " desde " + endPoint.RemoteEndPoint);

                locker.Set();
            });

            locker.Reset();

            endPoint.BeginReceive(buffer, 0, 1024, SocketFlags.None, callback, endPoint);

            locker.WaitOne();

            return bytesTransferred == 0 ? null : buffer.Slice(0, bytesTransferred);
        }

        /// <summary>
        /// Lee el buffer y los convierte en un mensaje compatible.
        /// </summary>
        /// <returns>Mensaje leido del buffer.</returns>
        public static IAdaptiveMessage ReadMessage(Socket endPoint, AdaptiveMessageRules rules)
        {
            byte[] buffer = ReadBuffer(endPoint);

            if (buffer.IsNull())
                throw new SocketException((int)SocketError.Interrupted);

            return AdaptiveMessage.Deserialize(buffer, rules);
        }

        /// <summary>
        /// Envía una colección a través de <see cref="IAdaptiveMessage"/>.
        /// </summary>
        /// <typeparam name="T">Tipo de dato de la colección.</typeparam>
        /// <param name="collection">Colección a enviar.</param>
        /// <param name="args">Controlador de la petición.</param>
        /// <param name="serializer">Función serializadora.</param>
        public static void SendCollection<T>(ICollection<T> collection, IAdaptiveMessageReceivedArgs args, Action<T, IAdaptiveMessage> serializer)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            if (!args.Data.IsEnumerable())
                args.Data.SetAsEnumerable(collection.Count);

            args.Response();

            while (args.Connection.Connected && collection.Count > 0)
            {
                try
                {
                    args.Data.Clear();

                    ReadMessage(args.Connection, args.Data.Rules)?.CopyTo(args.Data);

                    if (args.Data.GetPosition() >= collection.Count)
                        break;

                    if (collection.Count > 0)
                        serializer(collection.ElementAt(args.Data.GetPosition()), args.Data);

                    args.Response();
                }
                catch (SocketException ex)
                {
                    Trace.TraceError(ex.Message);
                    break;
                }
            }
        }
    }
}