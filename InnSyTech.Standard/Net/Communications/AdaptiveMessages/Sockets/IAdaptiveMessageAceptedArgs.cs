using System.Net.Sockets;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets
{
    /// <summary>
    /// Representa la estructura de los argumentos del evento <see cref="AdaptiveMessageServer.Accepted"/>.
    /// </summary>
    public interface IAdaptiveMessageAcceptedArgs
    {
        /// <summary>
        /// Obtiene la conexión que desencadenó el evento.
        /// </summary>
        Socket Connection { get; }
    }
}