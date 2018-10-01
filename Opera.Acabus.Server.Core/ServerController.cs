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
    public static class ServerController
    {
        private static readonly AdaptiveMsgServer _msgServer;

        static ServerController()
        {
            string path = AcabusDataContext.ConfigContext.Read("Message")?.ToString("Rules")
                ?? throw new InvalidOperationException("No existe una ruta válida para cargar las reglas de mensajes.");

            _msgServer = new AdaptiveMsgServer(path);

            _msgServer.Accepted += AcceptedHandle;
            _msgServer.Received += ReceivedHandle;
        }

        public static event EventHandler<ServiceStatus> StatusChanged;

        public static bool Running => _msgServer.Started;

        public static void Start() => Task.Run(() =>
        {
            _msgServer.Startup();
            StatusChanged?.Invoke(_msgServer, _msgServer.Started ? ServiceStatus.ON : ServiceStatus.OFF);
        }, _msgServer.CancellationTokenSource.Token);

        public static void Stop()
        {
            _msgServer.Shutdown();
            StatusChanged?.Invoke(_msgServer, ServiceStatus.OFF);
        }

        private static void AcceptedHandle(object sender, IAdaptiveMsgClientArgs e)
        {
            Trace.WriteLine("Cliente aceptado: " + (e.Connection.RemoteEndPoint as IPEndPoint).Address);
        }

        private static IMessage CreateError(string text, int code, IMessage message)
        {
            message[3] = text;
            message[2] = code;

            return message;
        }

        private static bool Login(IMessage message)
        {

            return false;
        }

        private static IMessage ProcessRequest(IMessage message)
        {
            return null;
        }

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