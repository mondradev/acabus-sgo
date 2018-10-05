using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
using Opera.Acabus.Core.DataAccess;
using System;
using System.Net;
using System.Text;
using System.Threading;

namespace LibraryTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            String path = AcabusDataContext.ConfigContext["Message"]["Rules"].ToString();
            AdaptiveMsgRequest request = new AdaptiveMsgRequest(path, IPAddress.Loopback, 5500);

            IMessage data = request.CreateMessage();
            data[1] = Encoding.UTF8.GetBytes("n3crg3q6grmq3grcqbq73bbcqqmymgc7");
            data[2] = "0.0.0.00000";
            data[6] = "GetStation";
            data[11] = Encoding.UTF8.GetBytes("8c3nqrg73q6rq36qgctggeqg7mhqyh8q");
            data[12] = 1;

            request.DoRequest(data, x =>
            {
                Console.WriteLine("Simple Request");
                Console.WriteLine(String.Format("Cod: {0}\nMessage: {1}\nResponse: {2}", x[3], x[4], ModelHelper.GetStation(x[13] as byte[])));
                Console.WriteLine(String.Format("API Key: " + Encoding.UTF8.GetString(x[1] as byte[])));
                Console.WriteLine(String.Format("Device Key: " + Encoding.UTF8.GetString(x[11] as byte[])));
                Console.WriteLine();

            }).Wait();

            data[6] = "GetStations";
            data[12] = 2;

            Console.WriteLine("Enumerable Request");

            request.DoRequestToList(data, e =>
            {
                IMessage x = e.Current;
                Console.WriteLine(String.Format("Pos: {0}, Cod: {1}\nMessage: {2}\nResponse: {3}", x[9], x[3], x[4], ModelHelper.GetStation(x[13] as byte[])));
            });

            Console.ReadLine();

        }
    }
}