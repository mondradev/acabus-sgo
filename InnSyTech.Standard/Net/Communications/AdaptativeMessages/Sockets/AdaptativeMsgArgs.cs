using System;
using System.Net.Sockets;

namespace InnSyTech.Standard.Net.Communications.AdaptativeMessages.Sockets
{
    /// <summary>
    /// Contiene los datos del evento ocurrido dentro de la comunicación de sockets con mensajes adaptativos.
    /// </summary>
    /// <seealso cref="AdaptativeMsgServer"/>
    /// <seealso cref="Message"/>
    /// <seealso cref="AdaptativeMsgRequest"/>
    public sealed class AdaptativeMsgArgs
    {
        /// <summary>
        /// Reglas de composición de mensajes.
        /// </summary>
        private readonly MessageRules _rules;

        /// <summary>
        /// Crea una nueva instancia y establece los datos del evento ocurrido.
        /// </summary>
        /// <param name="connection">Conexión que generó el evento.</param>
        /// <param name="rules">Reglas que establece como se componen los mensajes.</param>
        /// <param name="buffer">Buffer de datos de la respuesta.</param>
        public AdaptativeMsgArgs(Socket connection, MessageRules rules, Byte[] buffer = null)
        {
            _rules = rules;
            Connection = connection;

            try
            {
                Request = buffer != null ? Message.Deserialize(buffer, rules) : null;
            }
            catch
            {
                Request = null;
            }
        }

        /// <summary>
        /// Obtiene la conexión que desencadenó el evento.
        /// </summary>
        public Socket Connection { get; }

        /// <summary>
        /// Obtiene la petición realizada del evento.
        /// </summary>
        public Message Request { get; }

        public Message CreateMessage()
                            => new Message(_rules);

        /// <summary>
        /// Envía un mensaje de respuesta al otro extremo de la conexión.
        /// </summary>
        /// <param name="response">Mensaje de respuesta.</param>
        public void Response(Message response)
        {
            byte[] buffer = response.Serialize();
            Connection?.Send(buffer);
        }
    }
}