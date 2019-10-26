using System.Net.Sockets;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets
{
    /// <summary>
    /// Implementa los argumentos del evento <see cref="AdaptiveMessageServer.Accepted"/>.
    /// </summary>
    internal sealed class AdaptiveMessageAcceptedArgs : IAdaptiveMessageAcceptedArgs
    {
        /// <summary>
        /// Crea una nueva instancia de <see cref="AdaptiveMessageServer"/>.
        /// </summary>
        /// <param name="connection">Cliente que realizó la conexión.</param>
        public AdaptiveMessageAcceptedArgs(Socket connection)
        {
            Connection = connection;
        }

        /// <summary>
        /// Obtiene la conexión que desencadenó el evento.
        /// </summary>
        public Socket Connection { get; }
    }
}