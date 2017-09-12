using InnSyTech.Standard.Net.Messenger.Iso8583;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;
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
            msj[10] = "Te envío algo desde el cliente!";

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

                res.AddField(63, new Route(2, 2, RouteType.TRUNK) { Name = "OVIEDO", AssignedSection = null }.GetBytes());
                res = client.SendRequest(res);
                Route route = ModelHelper.GetRoute(res.GetBytes(63));
                Console.WriteLine(res);
                Console.WriteLine(route);

                res[63] = new Station(1, 1) { Name = "OVIEDO COSTERA", AssignedSection = "ZONA SUR" }.GetBytes();
                res = client.SendRequest(res);
                Station station = ModelHelper.GetStation(res.GetBytes(63));
                Console.WriteLine(res);
                Console.WriteLine(station);

                res[63] = new Bus(135, "AA-002") { Status = BusStatus.IN_REPAIR, Type = BusType.ARTICULATED }.GetBytes();
                res = client.SendRequest(res);
                Bus bus = ModelHelper.GetBus(res.GetBytes(63));
                Console.WriteLine(res);
                Console.WriteLine(bus);

                res[63] = new Staff(10) { Name = "JAVIER DE JESÚS FLORES MONDRAGÓN", Area = AssignableArea.DATABASE }.GetBytes();
                res = client.SendRequest(res);
                Staff staff = ModelHelper.GetStaff(res.GetBytes(63));
                Console.WriteLine(res);
                Console.WriteLine(staff);

            });

            Task.WaitAll(task, task1);

        }
    }
}