using InnSyTech.Standard.Net.Communications.AdaptativeMessages;
using InnSyTech.Standard.Net.Communications.AdaptativeMessages.Sockets;
using Newtonsoft.Json;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LibraryTest
{
    public class Program
    {

        public static void Main(string[] args)
        {
            string version = "0.1.0";
            AdaptativeMsgServer server = new AdaptativeMsgServer(MessageRules.Load(AcabusDataContext.ConfigContext.Read("Message")?.ToString("Rules")));

            server.Received += (sender, e) =>
            {
                var msg = e.Request ?? e.CreateMessage();

                if (!msg.ContainsID(1))
                {
                    msg[20] = "No es posible identificar el mensaje, reglas de composición de mensajes incorrectas.";
                    e.Response(msg);

                    return;
                }

                if (!version.Equals(msg[1]))
                {
                    msg[20] = String.Format("La versión de las reglas de mensajes no coinciden con la del servidor.\nServidor: v{0}\nCliente: v{1}", version, msg[1]);
                    e.Response(msg);

                    return;
                }

                List<Station> response = AcabusDataContext.AllStations.Where(x => !x.IsExternal).ToList();

                msg[3] = response.FirstOrDefault()?.Serialize();

                e.Response(msg);

            };

            Task.Run(() => server.Startup());

            AdaptativeMsgRequest request = new AdaptativeMsgRequest(new MessageRules() {
                { new FieldDefinition (1, FieldDefinition.FieldType.Text, 10, true) },
                { new FieldDefinition (10, FieldDefinition.FieldType.Text, 10) },
                { new FieldDefinition (11, FieldDefinition.FieldType.Text, 10) },
                { new FieldDefinition(3, FieldDefinition.FieldType.Binary, 1024) },
                { new FieldDefinition (20, FieldDefinition.FieldType.Text, 255) }
            }, IPAddress.Loopback, server.Port);

            var res = request.DoRequest(new Message(request.Rules) {
                { 1, "0.1.0" }
            });

            var station = res.GetValue(3, x => ModelHelper.GetStation(x as byte[]));

            Console.WriteLine(res[20]);
            Console.WriteLine(JsonConvert.SerializeObject(station));

            Console.ReadLine();

            server.Shutdown();

        }
    }
}