using InnSyTech.Standard.Net.Messenger.Iso8583;
using Opera.Acabus.Core.Services;
using System;
using System.IO;
using System.Threading;
using System.Xml;

namespace Opera.Acabus.Server.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            Core.Services.Server.Initialize();
           
            var message = new Message();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Path.Combine(Environment.CurrentDirectory, "template.xml"));
            Message.SetTemplate(xmlDoc);

            message.AddField(1, "HOLA");
            message.AddField(21, 5);
            message.AddField(4, "AYUDA AL MUNDO");
            message.AddField(6, DateTime.Now);
            message.AddField(12, 52205.2);
            message.AddField(60, new byte[] { 255, 25, 1, 140 });

            Console.WriteLine("Mensaje a enviar: " + message.ToString());

            Client.SendMessage(message);

            while (true)
            {
                Thread.Sleep(10);
                if (Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    Core.Services.Server.CloseAllSessions();
                    break;
                }
            }

        }
    }
}
