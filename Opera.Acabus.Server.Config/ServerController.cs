using InnSyTech.Standard.Configuration;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
using InnSyTech.Standard.Net.Notifications.Push;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Services;
using Opera.Acabus.Server.Core;
using Opera.Acabus.Server.Core.Models;
using Opera.Acabus.Server.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Opera.Acabus.Server.Config
{
    /// <summary>
    /// Controlador de peticiones del servidor de SGO. Provee de la funcionalidad básica para aceptar
    /// y rechazar peticiones, así como delegar las peticiones a los diferentes módulos del servidor.
    /// </summary>
    public static class ServerController
    {
        /// <summary>
        /// Listado de todos los módulos registrados.
        /// </summary>
        private static readonly List<IServiceModule> _modules;

        /// <summary>
        /// Instancia del servidor de mensajes adaptativos.
        /// </summary>
        private static readonly AdaptiveMessageServer _msgServer;

        /// <summary>
        /// Notificador de actualizaciones.
        /// </summary>
        private static readonly PushNotifier<PushAcabus> _notifier;

        /// <summary>
        /// Crea una nueva instancia de controlador.
        /// </summary>
        static ServerController()
        {
            _modules = new List<IServiceModule>();

            string path = AcabusDataContext.ConfigContext.Read("Message")?.ToString("Rules")
                ?? throw new InvalidOperationException("No existe una ruta válida para cargar las reglas de mensajes.");

            _msgServer = new AdaptiveMessageServer(path);

            _msgServer.Accepted += AcceptedHandle;
            _msgServer.Received += ReceivedHandle;
            _msgServer.Disconnected += DisconnectedHandle;

            _notifier = new PushNotifier<PushAcabus>();
            _notifier.StartedChanged += (sender, started) =>
            {
                if (!started)
                    Stop();
            };

            ServerNotify.Notified += (sender, data) => _notifier.Notify(data);

            _notifier.Start();

            LoadModules();
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
        /// Obtiene los módulos de servicios registrados actualmente.
        /// </summary>
        public static List<IServiceModule> GetServerModules()
            => _modules;

        /// <summary>
        /// Inicia el proceso del servidor.
        /// </summary>
        public static void Start() => Task.Run(() =>
        {
            _msgServer.Startup();
            _notifier.Start();
            StatusChanged?.Invoke(_msgServer, _msgServer.Started ? ServiceStatus.ON : ServiceStatus.OFF);

            Trace.TraceInformation("Server is started");
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
        /// Captura el evento cada vez que un cliente se conecta al servidor.
        /// </summary>
        /// <param name="sender">Instancia del servidor.</param>
        /// <param name="e">Parametros del evento.</param>
        private static void AcceptedHandle(object sender, IAdaptiveMessageAcceptedArgs e)
        {
            IPEndPoint ipClient = e.Connection.RemoteEndPoint as IPEndPoint;

            Trace.TraceInformation(String.Format("Cliente aceptado [IP={0}]", ipClient));
        }

        /// <summary>
        /// Captura el evento cada vez que un cliente se desconecta del servidor.
        /// </summary>
        /// <param name="sender">Instancia del servidor.</param>
        /// <param name="e">Parametros del evento.</param>
        private static void DisconnectedHandle(object sender, IAdaptiveMessageAcceptedArgs e)
        {
            IPEndPoint ipClient = e.Connection.RemoteEndPoint as IPEndPoint;

            Trace.TraceInformation(String.Format("Cliente desconectado [IP={0}]\n\n", ipClient));
        }

        /// <summary>
        /// Carga todos los módulos.
        /// </summary>
        private static void LoadModules()
        {
            IEnumerable<ISetting> modulesToLoad = AcabusDataContext.ConfigContext["Server"]?.GetSettings("Modules");

            if (modulesToLoad is null)
                return;

            _modules.Add(new ServerCoreFunctions());

            foreach (var module in modulesToLoad)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(module.ToString("assembly"));
                    var type = assembly.GetType(module.ToString("fullname"));

                    if (type is null)
                        throw new Exception($"Libería no contiene módulo especificado ---> {module.ToString("fullname")}");

                    IServiceModule moduleInfo = (IServiceModule)Activator.CreateInstance(type);

                    _modules.Add(moduleInfo);
                }
                catch (FileNotFoundException)
                {
                    Trace.WriteLine($"No se encontró el módulo '{module.ToString("fullname")}'", "NOTIFY");
                }
                catch (Exception)
                {
                    Trace.WriteLine($"No se encontró módulo '{module.ToString("fullname")}' en libería '{module.ToString("assembly")}'", "NOTIFY");
                }
            }
        }

        /// <summary>
        /// Evalua si la sesión es valida.
        /// </summary>
        /// <param name="message">Mensaje recibido del cliente.</param>
        /// <returns>Un true si es un cliente valido.</returns>
        private static bool Login(IAdaptiveMessage message)
        {
            /***
             *  1, FieldType.Binary, 32, true, "Token de aplicación"
                11, FIeldType.Binary, 32, true, "Token de equipo"
            */

            if (!message.HasAPIToken() || !message.HasDeviceToken())
                return false;

            if (!ValidateToken(message.GetAPIToken()))
                return false;

            if (!ValidateTokenDevice(message.GetDeviceToken()))
                return false;

            return true;
        }

        /// <summary>
        /// Procesa o redirecciona a otros módulos las peticiones recibidas por el servidor.
        /// </summary>
        /// <param name="message">Mensaje recibido del cliente.</param>
        private static void ProcessRequest(IAdaptiveMessageReceivedArgs e)
        {
            IAdaptiveMessage message = e.Data;

            try
            {
                if (!message.HasFunctionName())
                    throw new ServiceException("No se especificó la función a llamar", AdaptiveMessageResponseCode.BAD_REQUEST, "Processor", "Server");

                String modName = message.GetModuleName() ?? "Server Core";
                IServiceModule module = _modules.FirstOrDefault(x => x.ServiceName.Equals(modName));

                if (module is null)
                    throw new ServiceException(String.Format("El módulo especificado no existe [Módulo={0}, Función={1}]",
                        AdaptiveMessageResponseCode.BAD_REQUEST, modName, message.GetFunctionName()), "Processor", "Server");

                module.Request(e);
            }
            catch (ServiceException ex)
            {
                e.SendException(ex);
            }
            catch (Exception ex)
            {
                e.SendException(new ServiceException("Processor", "Server", ex));
            }
        }

        /// <summary>
        /// Captura la llegada de nuevas peticiones.
        /// </summary>
        /// <param name="sender">Instancia del servidor.</param>
        /// <param name="e">Parametros del evento.</param>
        private static void ReceivedHandle(object sender, IAdaptiveMessageReceivedArgs e)
        {
            switch (e.Data)
            {
                case IAdaptiveMessage m when !Login(m):
                    e.SendException(new ServiceException("No se logró autenticar", AdaptiveMessageResponseCode.UNAUTHORIZED, "Authenticator", "Server"));
                    break;

                case null:
                    e.SendException(new ServiceException("Sin datos para procesar la petición", AdaptiveMessageResponseCode.BAD_REQUEST, "Processor", "Server"));
                    break;

                default:
                    ProcessRequest(e);
                    break;
            }
        }

        /// <summary>
        /// Valida el token de aplicación.
        /// </summary>
        /// <param name="token">Token de aplicación.</param>
        /// <returns>Un valor true si el token es valido.</returns>
        private static bool ValidateToken(byte[] token)
        {
            return token != null;
        }

        /// <summary>
        /// Valida el token de equipo indicando si pertenece al sistema actual.
        /// </summary>
        /// <param name="token">Token del equipo.</param>
        /// <returns>Un valor true si el token es valido.</returns>
        private static bool ValidateTokenDevice(byte[] token)
        {
            return token != null;
        }
    }
}