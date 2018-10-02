using InnSyTech.Standard.Configuration;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;
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

namespace Opera.Acabus.Server.Gui
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
        private static readonly List<IServerModule> _modules;

        /// <summary>
        /// Instancia del servidor de mensajes adaptativos.
        /// </summary>
        private static readonly AdaptiveMsgServer _msgServer;

        /// <summary>
        /// Crea una nueva instancia de controlador.
        /// </summary>
        static ServerController()
        {
            _modules = new List<IServerModule>();

            string path = AcabusDataContext.ConfigContext.Read("Message")?.ToString("Rules")
                ?? throw new InvalidOperationException("No existe una ruta válida para cargar las reglas de mensajes.");

            _msgServer = new AdaptiveMsgServer(path);

            _msgServer.Accepted += AcceptedHandle;
            _msgServer.Received += ReceivedHandle;
            _msgServer.Disconnected += DisconnectedHandle;

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
        /// Captura el evento cada vez que un cliente se conecta al servidor.
        /// </summary>
        /// <param name="sender">Instancia del servidor.</param>
        /// <param name="e">Parametros del evento.</param>
        private static void AcceptedHandle(object sender, IAdaptiveMsgClientArgs e)
        {
            IPEndPoint ipClient = e.Connection.RemoteEndPoint as IPEndPoint;
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
        /// Captura el evento cada vez que un cliente se desconecta del servidor.
        /// </summary>
        /// <param name="sender">Instancia del servidor.</param>
        /// <param name="e">Parametros del evento.</param>
        private static void DisconnectedHandle(object sender, IAdaptiveMsgClientArgs e)
        {
            IPEndPoint ipClient = e.Connection.RemoteEndPoint as IPEndPoint;
        }

        /// <summary>
        /// Funcciones internas del nucleo del servidor.
        /// </summary>
        /// <param name="message">Mensaje con la petición.</param>
        /// <param name="callback">Funcción de llamada de vuelta.</param>
        private static void Functions(IMessage message, Action<IMessage> callback)
        {
            if (Helpers.ValidateRequest(message, typeof(ServerController)))
                Helpers.CallFunc(message, typeof(ServerController));
            else
                CreateError("Error al realizar la petición: opera.acabus.server.core", 403, message);

            callback?.Invoke(message);
        }

        #region ApiFunctions

        /// <summary>
        /// Obtiene la estación con el ID especificado.
        /// </summary>
        /// <param name="IDStation">ID de la estación.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetStation([ParameterField(11)] UInt64 IDStation, IMessage message)
        {
            message[12] = AcabusDataContext.AllStations.FirstOrDefault(x => x.ID == IDStation).Serialize();
        }

        #endregion

        /// <summary>
        /// Carga todos los módulos.
        /// </summary>
        private static void LoadModules()
        {
            IEnumerable<ISetting> modulesToLoad = AcabusDataContext.ConfigContext["Server"]?.GetSettings("Modules");

            if (modulesToLoad is null)
                return;

            foreach (var module in modulesToLoad)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(module.ToString("assembly"));
                    var type = assembly.GetType(module.ToString("fullname"));

                    if (type is null)
                        throw new Exception($"Libería no contiene módulo especificado ---> {module.ToString("fullname")}");

                    IServerModule moduleInfo = (IServerModule)Activator.CreateInstance(type);

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
        private static bool Login(IMessage message)
        {
            /***
             *  0, FieldType.Binary, 32, true, "Token de aplicación"
                1, FieldType.Text, 20, true, "Versión de las reglas"
                10, FIeldType.Binary, 32, true, "Token de equipo"
            */

            if (!message.IsSet(0) || !message.IsSet(1) || !message.IsSet(10))
                return false;

            if (!ValidateToken(message.GetValue(0, x => x as byte[])))
                return false;

            Version rulesVersion = Version.Parse(AcabusDataContext.ConfigContext["App"]?.ToString("Version"));
            Version rulesVersionClient = message.GetValue(1, x => Version.Parse(x.ToString()));

            if (!rulesVersion.Equals(rulesVersionClient))
                return false;

            if (!ValidateTokenDevice(message.GetValue(10, x => x as byte[])))
                return false;

            return true;
        }

        /// <summary>
        /// Procesa o redirecciona a otros módulos las peticiones recibidas por el servidor.
        /// </summary>
        /// <param name="message">Mensaje recibido del cliente.</param>
        private static void ProcessRequest(IMessage message, IAdaptiveMsgArgs e)
        {
            /***
             *  0, FieldType.Binary, 32, true, "Token de aplicación"
                1, FieldType.Text, 20, true, "Versión de las reglas"
                2, FieldType.Numeric, 3, false, "Código de respuesta"
                3, FieldType.Text, 255, true, "Mensaje de respuesta"
                4, FieldType.Text, 50, true, "Nombre del módulo"
                5, FieldType.Text, 20, true, "Nombre de la función"
                6, FieldType.Binary, 1, false, "Es enumerable"
                7, FieldType.Numeric, 100, true, "Registros totales del enumerable"
                8, FieldType.Numeric, 100, true, "Posición del enumerable"
                9, FieldType.Numeric, 1, false, "Operaciones del enumerable (Siguiente|Inicio)"
             */

            if (!message.IsSet(5))
            {
                e.Send(CreateError("Error al realizar la petición, no especificó la función a llamar", 403, message));
                return;
            }

            if (message.IsSet(4))
            {
                String modName = message[4].ToString();
                IServerModule module = _modules.FirstOrDefault(x => x.ServiceName.Equals(modName));

                if (module is null)
                {
                    e.Send(CreateError("Error al realizar la petición, no existe el módulo: " + modName, 403, message));
                    return;
                }

                module.Request(message, e.Send);
                return;
            }

            Functions(message, e.Send);
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
                    ProcessRequest(e.Data, e);
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
            return true;
        }

        /// <summary>
        /// Valida el token de equipo indicando si pertenece al sistema actual.
        /// </summary>
        /// <param name="token">Token del equipo.</param>
        /// <returns>Un valor true si el token es valido.</returns>
        private static bool ValidateTokenDevice(byte[] token)
        {
            return true;
        }
    }
}