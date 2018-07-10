using InnSyTech.Standard.Net.Communications.AdaptativeMessages;
using InnSyTech.Standard.Net.Communications.AdaptativeMessages.Sockets;
using Opera.Acabus.Core.DataAccess;
using System;
using System.Threading.Tasks;

namespace Opera.Acabus.Server.Core
{
    public static class ServerController
    {
        private static readonly AdaptativeMsgServer _msgServer;

        static ServerController()
        {

            string path = AcabusDataContext.ConfigContext.Read("Message")?.ToString("Rules")
                ?? throw new InvalidOperationException("No existe una ruta válida para cargar las reglas de mensajes.");

            MessageRules rules = MessageRules.Load(path)
                ?? throw new InvalidOperationException("No se logró cargar las reglas de composición de mensajes.");

            _msgServer = new AdaptativeMsgServer(rules);

            _msgServer.Received += ReceiveHandle;

        }

        public static void Start() => Task.Run(() => _msgServer.Startup(), _msgServer.CancellationTokenSource.Token);

        public static void Stop() => _msgServer.Shutdown();
        
        private static Message ProcessRequest(Message message)
        {
            return null;
        }

        private static void ReceiveHandle(object sender, AdaptativeMsgArgs e)
        {
            switch (e.Request)
            {
                case Message m when !Validate(m):
                    e.Response(CreateError("Mensaje no valido", 403));
                    break;
                case null:
                    e.Response(CreateError("Petición incorrecta", 403));
                    break;
                default:
                    e.Response(ProcessRequest(e.Request));
                    break;
            }
        }

        private static bool Validate(Message message)
        {
            return false;
        }

        private static Message CreateError(string message, int code)
            => new Message(_msgServer.Rules)
            {
                { 20, code },
                { 21, message }
            };
    }
}
