using InnSyTech.Standard.Net.Messenger.Iso8583;
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
            //Core.Services.Server.Initialize();
            //while (true)
            //{
            //    Thread.Sleep(10);
            //    if (Console.ReadKey().Key == ConsoleKey.Escape)
            //    {
            //        Core.Services.Server.CloseAllSessions();
            //        break;
            //    }
            //}

            var message = new Message();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Path.Combine(Environment.CurrentDirectory, "template.xml"));
            Message.SetTemplate(xmlDoc);

            message.AddField(1, "HOLA");
            message.AddField(4, "AYUDA AL MUNDO");
            message.AddField(6, DateTime.Now);

            var encode = message.ToString();

            Console.WriteLine(String.Format("Campo {0}: {1}", 1, message.GetField(1)));
            Console.WriteLine(String.Format("Campo {0}: {1}", 4, message.GetField(4)));
            Console.WriteLine(String.Format("Campo {0}: {1}", 6, message.GetField(6)));

            Console.WriteLine(encode);
        }
    }
}
