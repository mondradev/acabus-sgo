using System.Net.Sockets;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets
{
    /// <summary>
    /// Implementa los argumentos del evento <see cref="AdaptiveMsgServer.Accepted"/>.
    /// </summary>
    internal sealed class AdaptiveMsgClientArgs : IAdaptiveMsgClientArgs
    {
        /// <summary>
        /// Crea una nueva instancia de <see cref="AdaptiveMsgServer"/>.
        /// </summary>
        /// <param name="connection">Cliente que realizó la conexión.</param>
        public AdaptiveMsgClientArgs(Socket connection)
        {
            Connection = connection;
        }

        /// <summary>
        /// Obtiene la conexión que desencadenó el evento.
        /// </summary>
        public Socket Connection { get; }
    }
}