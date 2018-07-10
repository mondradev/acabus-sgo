using InnSyTech.Standard.Net.Communications.AdaptativeMessages;
using InnSyTech.Standard.Net.Communications.AdaptativeMessages.Sockets;
using Opera.Acabus.Core.DataAccess;
using System;
using System.Net;
using System.Threading;

namespace Opera.Acabus.Core.Services
{
    /// <summary>
    /// Gestiona las conexiones al servidor de aplicación y provee de toda la funcionalidad que su
    /// identificador de aplicación le permite.
    /// </summary>
    public sealed class AppClient
    {

        /// <summary>
        /// Señala el token de cancelación.
        /// </summary>
        private CancellationTokenSource _tokenSource;

        /// <summary>
        /// Crea una instancia nueva de <see cref="AppClient"/>.
        /// </summary>
        public AppClient()
        {
            AppID = AcabusDataContext.ConfigContext.Read("App")?.ToInteger("ID") ?? 0;
            ServerID = AcabusDataContext.ConfigContext.Read("Server")?.ToInteger("ID") ?? 0;
            ServerPort = (Int32)(AcabusDataContext.ConfigContext.Read("Server")?.ToInteger("Port") ?? 9000);
            ServerIP = IPAddress.Parse(AcabusDataContext.ConfigContext.Read("Server")?.ToString("IP") ?? "127.0.0.1");
            Rules = MessageRules.Load(AcabusDataContext.ConfigContext.Read("Message")?.ToString("Rules"));
        }

        /// <summary>
        /// Obtiene las reglas de composición de los mensajes.
        /// </summary>
        public MessageRules Rules { get; }

        /// <summary>
        /// Obtiene el identificador de aplicación especificado en la configuración.
        /// </summary>
        public Int64 AppID { get; }

        /// <summary>
        /// Obtiene el identificador del servidor a conectarse.
        /// </summary>
        public Int64 ServerID { get; }

        /// <summary>
        /// Obtiene la dirección IP del servidor.
        /// </summary>
        public IPAddress ServerIP { get; }

        /// <summary>
        /// Obtiene el puerto TCP con el cual se realiza la conexión al servidor.
        /// </summary>
        public Int32 ServerPort { get; }

        /// <summary>
        /// Obiente o establece el token de cancelación de la instancia.
        /// </summary>
        public CancellationTokenSource TokenSource {
            get => _tokenSource ?? (_tokenSource = new CancellationTokenSource());
            set => _tokenSource = value;
        }

        /// <summary>
        /// Envía un mensaje al servidor y espera una respuesta.
        /// </summary>
        /// <param name="message">Mensaje a envíar.</param>
        /// <returns>Mensaje con el cual ha respondido el servidor.</returns>
        public Message SendRequest(Message message)
        {
            AdaptativeMsgRequest request = new AdaptativeMsgRequest(Rules, ServerIP, ServerPort);
            return request.DoRequest(message);
        }
    }
}