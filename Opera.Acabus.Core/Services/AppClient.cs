using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
using Opera.Acabus.Core.DataAccess;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Opera.Acabus.Core.Services
{
    /// <summary>
    /// Gestiona las conexiones al servidor de aplicación y provee de toda la funcionalidad que su
    /// identificador de aplicación le permite.
    /// </summary>
    public sealed class AppClient
    {
        /// <summary>
        /// Identificador único de equipo.
        /// </summary>
        private readonly String _token;

        /// <summary>
        /// Controlador de peticiones al servidor.
        /// </summary>
        private AdaptiveMsgRequest _request;

        /// <summary>
        /// Crea una instancia nueva de <see cref="AppClient"/>.
        /// </summary>
        public AppClient()
        {
            AppID = AcabusDataContext.ConfigContext["App"]?.ToInteger("Token") ?? 0;
            MsgRulesVersion = Version.Parse(AcabusDataContext.ConfigContext["App"]?.ToString("Version"));
            ServerPort = (Int32)(AcabusDataContext.ConfigContext["Server"]?.ToInteger("Port") ?? 9000);
            ServerIP = IPAddress.Parse(AcabusDataContext.ConfigContext["Server"]?.ToString("IP") ?? "127.0.0.1");
            RulesMsgPath = AcabusDataContext.ConfigContext["Message"]?.ToString("Rules");

            _request = new AdaptiveMsgRequest(RulesMsgPath, ServerIP, ServerPort);
            _token = AcabusDataContext.ConfigContext["App"]?.ToString("DeviceKey");
        }

        /// <summary>
        /// Obtiene el identificador de aplicación especificado en la configuración.
        /// </summary>
        public Int64 AppID { get; }

        /// <summary>
        /// Versión de las reglas de mensajes.
        /// </summary>
        public Version MsgRulesVersion { get; }

        /// <summary>
        /// Obtiene la ubicación de las reglas utilizada para generar los mensajes.
        /// </summary>
        public String RulesMsgPath { get; }

        /// <summary>
        /// Obtiene la dirección IP del servidor.
        /// </summary>
        public IPAddress ServerIP { get; }

        /// <summary>
        /// Obtiene el puerto TCP con el cual se realiza la conexión al servidor.
        /// </summary>
        public Int32 ServerPort { get; }

        /// <summary>
        /// Crea un nuevo mensaje con los campos predeterminados.
        /// </summary>
        /// <returns>Un mensaje vacío.</returns>
        public IMessage CreateMessage()
        {
            IMessage message = _request.CreateMessage();

            message[0] = AppID;
            message[1] = MsgRulesVersion.ToString();
            message[10] = _token;

            return message;
        }

        /// <summary>
        /// Envía un mensaje nuevo al servidor.
        /// </summary>
        /// <param name="message">Mensaje a envíar.</param>
        /// <param name="callback">Función a realizar al recibir la respuesta.</param>
        /// <returns>Un instancia Task.</returns>
        public Task SendMessage(IMessage message, Action<IMessage> callback)
            => _request.DoRequest(message, callback);
    }
}