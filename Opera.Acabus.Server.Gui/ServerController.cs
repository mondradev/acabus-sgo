﻿using InnSyTech.Standard.Configuration;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
using InnSyTech.Standard.Net.Notifications.Push;
using InnSyTech.Standard.Utils;
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
using System.Security.Cryptography;
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
        private static readonly List<IServiceModule> _modules;

        /// <summary>
        /// Instancia del servidor de mensajes adaptativos.
        /// </summary>
        private static readonly AdaptiveMsgServer _msgServer;

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

            _msgServer = new AdaptiveMsgServer(path);

            _msgServer.Accepted += AcceptedHandle;
            _msgServer.Received += ReceivedHandle;
            _msgServer.Disconnected += DisconnectedHandle;

            _notifier = new PushNotifier<PushAcabus>();

            ServerNotify.Notified += (sender, data) => { _notifier.Notify(data); };

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
        /// Inicia el proceso del servidor.
        /// </summary>
        public static void Start() => Task.Run(() =>
        {
            _msgServer.Startup();
            _notifier.Start();
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

            Trace.WriteLine("Petición desde: " + ipClient);
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
        /// <param name="e">Instancia que controla el evento de la petición.</param>
        private static void Functions(IMessage message, Action<IMessage> callback, IAdaptiveMsgArgs e)
        {
            if (ServerHelper.ValidateRequest(message, typeof(ServerCoreFunctions)))
                ServerHelper.CallFunc(message, typeof(ServerCoreFunctions));
            else
               ServerHelper.CreateError("Error al realizar la petición: opera.acabus.server.core."
                    + message.GetString(AcabusAdaptiveMessageFieldID.FunctionName.ToInt32()), 403, e);

            callback?.Invoke(message);
        }

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
        /// Obtiene los módulos de servicios registrados actualmente. 
        /// </summary>
        public static List<IServiceModule> GetServerModules()
            => _modules;

        /// <summary>
        /// Evalua si la sesión es valida.
        /// </summary>
        /// <param name="message">Mensaje recibido del cliente.</param>
        /// <returns>Un true si es un cliente valido.</returns>
        private static bool Login(IMessage message)
        {
            /***
             *  1, FieldType.Binary, 32, true, "Token de aplicación"
                2, FieldType.Text, 20, true, "Versión de las reglas"
                11, FIeldType.Binary, 32, true, "Token de equipo"
            */

            if (!message.IsSet(AcabusAdaptiveMessageFieldID.APIToken.ToInt32())
                || !message.IsSet(AcabusAdaptiveMessageFieldID.HashRules.ToInt32())
                || !message.IsSet(AcabusAdaptiveMessageFieldID.DeviceToken.ToInt32()))
                return false;

            if (!ValidateToken(message.GetValue(AcabusAdaptiveMessageFieldID.APIToken.ToInt32(), x => x as byte[])))
                return false;

            using (SHA256 sha256 = SHA256.Create())
            {
                String hashRules = sha256.ComputeHash(File.ReadAllBytes(AcabusDataContext.ConfigContext.Read("Message")?.ToString("Rules"))).ToTextPlain();
                String HashRulesClients = message.GetValue(AcabusAdaptiveMessageFieldID.HashRules.ToInt32(), x => (x as byte[]).ToTextPlain());

                if (!hashRules.Equals(HashRulesClients))
                    return false;
            }

            if (!ValidateTokenDevice(message.GetValue(AcabusAdaptiveMessageFieldID.DeviceToken.ToInt32(), x => x as byte[])))
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
             *  1, FieldType.Binary, 32, true, "Token de aplicación"
                2, FieldType.Text, 20, true, "Versión de las reglas"
                3, FieldType.Numeric, 3, false, "Código de respuesta"
                4, FieldType.Text, 255, true, "Mensaje de respuesta"
                5, FieldType.Text, 50, true, "Nombre del módulo"
                6, FieldType.Text, 20, true, "Nombre de la función"
                7, FieldType.Binary, 1, false, "Es enumerable"
                8, FieldType.Numeric, 100, true, "Registros totales del enumerable"
                9, FieldType.Numeric, 100, true, "Posición del enumerable"
                10, FieldType.Numeric, 1, false, "Operaciones del enumerable (Siguiente|Inicio)"
             */
            try
            {
                if (!message.IsSet(6))
                {
                    e.Send(ServerHelper.CreateError("Error al realizar la petición, no especificó la función a llamar", 403, e));
                    return;
                }

                message[AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()] = 200;
                message[AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32()] = "OK";

                if (message.IsSet(AcabusAdaptiveMessageFieldID.ModuleName.ToInt32()))
                {
                    String modName = message[AcabusAdaptiveMessageFieldID.ModuleName.ToInt32()].ToString();
                    IServiceModule module = _modules.FirstOrDefault(x => x.ServiceName.Equals(modName));

                    if (module is null)
                    {
                        e.Send(ServerHelper.CreateError("Error al realizar la petición, no existe el módulo: " + modName, 403, e));
                        return;
                    }

                    module.Request(message, e.Send, e );
                    return;
                }

                Functions(message, e.Send, e);
            }
            catch (Exception ex)
            {
                e.Send(ServerHelper.CreateError(ex.Message, 500, e));
            }
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
                    e.Send(ServerHelper.CreateError("Mensaje no valido", 403, e));
                    break;

                case null:
                    e.Send(ServerHelper.CreateError("Petición incorrecta", 403, e));
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