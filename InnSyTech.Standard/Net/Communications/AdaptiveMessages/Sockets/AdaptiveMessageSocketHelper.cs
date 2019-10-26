using System;
using System.Diagnostics;
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
        /// Lee el buffer y los convierte en un mensaje compatible.
        /// </summary>
        /// <returns>Mensaje leido del buffer.</returns>
        public static IAdaptiveMessage ReadBuffer(Socket endPoint, AdaptiveMessageRules rules)
        {
            while (true)
            {
                Thread.Sleep(10);

                if (endPoint.Available <= 0)
                    continue;

                Byte[] buffer = new Byte[endPoint.Available];

                int bytesTransferred = endPoint.Receive(buffer);

                if (bytesTransferred <= 0)
                    continue;

                Trace.WriteLine("Bytes recibidos: " + bytesTransferred, "DEBUG");

                return AdaptiveMessage.Deserialize(buffer, rules);
            }
        }
    }
}