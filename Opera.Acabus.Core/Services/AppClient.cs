using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
using Opera.Acabus.Core.DataAccess;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
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
        private readonly AdaptiveMessageRequest _request;

        /// <summary>
        /// Crea una instancia nueva de <see cref="AppClient"/>.
        /// </summary>
        public AppClient()
        {
            AppToken = AcabusDataContext.ConfigContext["App"]?.ToString("Token");
            ServerPort = (Int32)(AcabusDataContext.ConfigContext["Server"]?.ToInteger("Port") ?? 5500);
            ServerIP = IPAddress.Parse(AcabusDataContext.ConfigContext["Server"]?.ToString("IP") ?? "127.0.0.1");
            RulesMsgPath = AcabusDataContext.ConfigContext["Message"]?.ToString("Rules");

            using (SHA256 sha256 = SHA256.Create())
            {
                HashRules = sha256.ComputeHash(File.ReadAllBytes(RulesMsgPath));
            }

            _request = new AdaptiveMessageRequest(RulesMsgPath, ServerIP, ServerPort);
            _token = AcabusDataContext.ConfigContext["App"]?.ToString("DeviceKey");
        }

        /// <summary>
        /// Obtiene el identificador de aplicación especificado en la configuración.
        /// </summary>
        public String AppToken { get; }

        /// <summary>
        /// Versión de las reglas de mensajes.
        /// </summary>
        public Byte[] HashRules { get; }

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
        public IAdaptiveMessage CreateMessage()
        {
            IAdaptiveMessage message = _request.CreateMessage();

            message.SetAPIToken(Encoding.UTF8.GetBytes(AppToken));
            message.SetHashRules(HashRules);
            message.SetDeviceToken(Encoding.UTF8.GetBytes(_token));

            return message;
        }

        /// <summary>
        /// Envía un mensaje nuevo al servidor.
        /// </summary>
        /// <param name="message">Mensaje a envíar.</param>
        /// <returns>Un instancia Task que devuelve la respuesta.</returns>
        public Task<IAdaptiveMessage> SendMessage(IAdaptiveMessage message)
            => _request.Send(message);

        /// <summary>
        /// Envía un mensaje nuevo al servidor con una secuencia como respuesta.
        /// </summary>
        /// <typeparam name="TResult">Tipo de dato de la colección a solicitar.</typeparam>
        /// <param name="message">Mensaje a envíar.</param>
        /// <param name="convertor">Función de conversión del contenido del mensaje a <typeparamref name="TResult"/>.</param>
        /// <returns>Un instancia Task que devuelve la colección.</returns>      
        public Task<AdaptiveMessageCollection<TResult>> SendMessage<TResult>(IAdaptiveMessage message, Func<IAdaptiveMessage, TResult> convertor)
            => _request.Send(message, convertor);


    }
}