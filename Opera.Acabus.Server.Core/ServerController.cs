using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Server.Core.Utils;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Opera.Acabus.Server.Core
{
    /// <summary>
    /// Controlador de peticiones del servidor de SGO. Provee de la funcionalidad básica para aceptar
    /// y rechazar peticiones, así como delegar las peticiones a los diferentes módulos del servidor.
    /// </summary>
    public static class ServerController
    {
        /// <summary>
        /// Instancia del servidor de mensajes adaptativos.
        /// </summary>
        private static readonly AdaptiveMsgServer _msgServer;

        /// <summary>
        /// Crea una nueva instancia de controlador.
        /// </summary>
        static ServerController()
        {
            string path = AcabusDataContext.ConfigContext.Read("Message")?.ToString("Rules")
                ?? throw new InvalidOperationException("No existe una ruta válida para cargar las reglas de mensajes.");

            _msgServer = new AdaptiveMsgServer(path);

            _msgServer.Accepted += AcceptedHandle;
            _msgServer.Received += ReceivedHandle;
        }

        /// <summary>
        /// Evento que se desencadena cuando el estado del servidor es alterado.
        /// </summary>
        public static event EventHandler<ServiceStatus> StatusChanged;

        /// <summary>
        /// Obtiene si el servidor está actualmente en ejecución.
        /// </summary>
        public static bool Running => _msgServer.Started;

        /// <summary>
        /// Inicia el proceso del servidor.
        /// </summary>
        public static void Start() => Task.Run(() =>
        {
            _msgServer.Startup();
            StatusChanged?.Invoke(_msgServer, _msgServer.Started ? ServiceStatus.ON : ServiceStatus.OFF);
        }, _msgServer.CancellationTokenSource.Token);

        /// <summary>
        /// Detiene el proceso del servidor.
        /// </summary>
        public static void Stop()
        {
            _msgServer.Shutdown();
            StatusChanged?.Invoke(_msgServer, ServiceStatus.OFF);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void AcceptedHandle(object sender, IAdaptiveMsgClientArgs e)
        {
            Trace.WriteLine("Cliente aceptado: " + (e.Connection.RemoteEndPoint as IPEndPoint).Address);
        }

        /// <summary>
        /// Crea un mensaje básico de error.
        /// </summary>
        /// <param name="text">Mensaje a indicar al cliente.</param>
        /// <param name="code">Código de respuesta al cliente.</param>
        /// <param name="message">Instancia IMessage a devolver al cliente.</param>
        /// <returns>Una instancia de mensaje.</returns>
        private static IMessage CreateError(string text, int code, IMessage message)
        {
            message[3] = text;
            message[2] = code;

            return message;
        }

        /// <summary>
        /// Evalua si la sesión es valida.
        /// </summary>
        /// <param name="message">Mensaje recibido del cliente.</param>
        /// <returns>Un true si es un cliente valido.</returns>
        private static bool Login(IMessage message)
        {
            return false;
        }

        /// <summary>
        /// Procesa o redirecciona a otros módulos las peticiones recibidas por el servidor.
        /// </summary>
        /// <param name="message">Mensaje recibido del cliente.</param>
        /// <returns>Un mensaje de respuesta.</returns>
        private static IMessage ProcessRequest(IMessage message)
        {
            return null;
        }

        /// <summary>
        /// Captura la llegada de nuevas peticiones.
        /// </summary>
        /// <param name="sender">Instancia del servidor.</param>
        /// <param name="e">Parametros del evento.</param>
        private static void ReceivedHandle(object sender, IAdaptiveMsgArgs e)
        {
            switch (e.Data)
            {
                case IMessage m when !Login(m):
                    e.Send(CreateError("Mensaje no valido", 403, m));
                    break;

                case null:
                    e.Send(CreateError("Petición incorrecta", 403, e.CreateMessage()));
                    break;

                default:
                    e.Send(ProcessRequest(e.Data));
                    break;
            }
        }
    }
}