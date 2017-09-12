using InnSyTech.Standard.Net.Messenger.Iso8583;
using Opera.Acabus.Core.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Opera.Acabus.Server.Debug
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            AppMessage.SetTemplate(Path.Combine(Environment.CurrentDirectory, "acabus.config"));

            var msj = new AppMessage();
            msj.AddField(10, "Cliente: Hola");

            var task = Task.Run(() =>
            {
                AppServer.Initialize();
            });

            var task1 = Task.Run(() =>
            {
                Thread.Sleep(2000);
                AppClient client = new AppClient();
                var res = client.SendRequest(msj);
                Console.WriteLine(res);
                res.AddField(11, "Cliente: Hola de nuevo");
                res = client.SendRequest(res);
                Console.WriteLine(res);
                res.AddField(12, "Cliente: Ok, Adios!");
                res = client.SendRequest(res);
                Console.WriteLine(res);

                res.AddField(13, "Cliente: Adios :D");
                res = client.SendRequest(res);
                Console.WriteLine(res);
            });

            Task.WaitAll(task, task1);

        }
    }
}