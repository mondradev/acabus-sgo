using System;
using System.Net.Sockets;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets
{
    /// <summary>
    /// Implementa las funciones para capturar los argumentos de los eventos <seealso cref="AdaptiveMessageServer.Received"/>
    /// </summary>
    internal sealed class AdaptiveMessageReceivedArgs : IAdaptiveMessageReceivedArgs
    {
        /// <summary>
        /// Reglas de composición de mensajes.
        /// </summary>
        private readonly AdaptiveMessageRules _rules;

        /// <summary>
        /// Crea una nueva instancia y establece los datos del evento ocurrido.
        /// </summary>
        /// <param name="connection">Conexión que generó el evento.</param>
        /// <param name="rules">Reglas que establece como se componen los mensajes.</param>
        /// <param name="buffer">Buffer de datos de la respuesta.</param>
        public AdaptiveMessageReceivedArgs(Socket connection, AdaptiveMessageRules rules, Byte[] buffer = null)
        {
            _rules = rules;
            Connection = connection;

            try
            {
                Data = buffer != null ? AdaptiveMessage.Deserialize(buffer, rules) : null;
            }
            catch
            {
                Data = null;
            }
        }

        /// <summary>
        /// Obtiene la conexión que desencadenó el evento.
        /// </summary>
        public Socket Connection { get; }

        /// <summary>
        /// Obtiene la información captura en el evento.
        /// </summary>
        public IAdaptiveMessage Data { get; }

        /// <summary>
        /// Crea un mensaje nuevo.
        /// </summary>
        /// <returns>Obtiene un mensaje totalmente nuevo.</returns>
        public IAdaptiveMessage CreateMessage()
                            => new AdaptiveMessage(_rules);

        /// <summary>
        /// Envía un mensaje al otro extremo de la conexión.
        /// </summary>
        /// <param name="response">Mensaje de respuesta.</param>
        public void Response(IAdaptiveMessage response)
        {
            byte[] buffer = response.Serialize();
            Connection?.Send(buffer);
        }

        /// <summary>
        /// Envía de vuelta el mensaje de la petición al otro extremo de la conexión.
        /// </summary>
        public void Response()
            => Response(Data);
    }
}