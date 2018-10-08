using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Services;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
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

            using (SHA256 sha256 = SHA256.Create())
            {
                data[AdaptiveMessageFieldID.APIToken.ToInt32()] = Encoding.UTF8.GetBytes("n3crg3q6grmq3grcqbq73bbcqqmymgc7");
                data[AdaptiveMessageFieldID.HashRules.ToInt32()] = sha256.ComputeHash(File.ReadAllBytes(path));
                data[AdaptiveMessageFieldID.FunctionName.ToInt32()] = "CreateRoute";
                data[AdaptiveMessageFieldID.DeviceToken.ToInt32()] = Encoding.UTF8.GetBytes("8c3nqrg73q6rq36qgctggeqg7mhqyh8q");
            }

            data[12] = 13;
            data[17] = "CAYACO";
            data[13] = 1;
            data[18] = "SIN ASIGNACIÓN";

            request.DoRequest(data, x =>
            {
                Console.WriteLine("Simple Request");
                Console.WriteLine(String.Format("Cod: {0}\nMessage: {1}\nResponse: {2}", x[3], x[4], BitConverter.ToBoolean(x[22] as byte[], 0)));
                Console.WriteLine(String.Format("API Key: " + Encoding.UTF8.GetString(x[1] as byte[])));
                Console.WriteLine(String.Format("Device Key: " + Encoding.UTF8.GetString(x[11] as byte[])));
                Console.WriteLine();

            }).Wait();

            data[12] = 14;
            data[17] = "LAS ANCLAS";
            data[13] = 1;
            data[18] = "SIN ASIGNACIÓN";

            request.DoRequest(data, x =>
            {
                Console.WriteLine("Simple Request");
                Console.WriteLine(String.Format("Cod: {0}\nMessage: {1}\nResponse: {2}", x[3], x[4], BitConverter.ToBoolean(x[22] as byte[], 0)));
                Console.WriteLine(String.Format("API Key: " + Encoding.UTF8.GetString(x[1] as byte[])));
                Console.WriteLine(String.Format("Device Key: " + Encoding.UTF8.GetString(x[11] as byte[])));
                Console.WriteLine();

            }).Wait();

            data[AdaptiveMessageFieldID.FunctionName.ToInt32()] = "CreateStation";
            data[12] = 36;
            data[17] = "EL COLOSO";
            data[13] = 2;
            data[18] = "SIN ASIGNACIÓN";
            data[23] = BitConverter.GetBytes(true);

            request.DoRequest(data, x =>
            {
                Console.WriteLine("Simple Request");
                Console.WriteLine(String.Format("Cod: {0}\nMessage: {1}\nResponse: {2}", x[3], x[4], BitConverter.ToBoolean(x[22] as byte[], 0)));
                Console.WriteLine(String.Format("API Key: " + Encoding.UTF8.GetString(x[1] as byte[])));
                Console.WriteLine(String.Format("Device Key: " + Encoding.UTF8.GetString(x[11] as byte[])));
                Console.WriteLine();

            }).Wait();

            data[AdaptiveMessageFieldID.FunctionName.ToInt32()] = "GetRoutes";
            data[12] = 2;

            Console.WriteLine("Enumerable Request");

            request.DoRequestToList(data, e =>
            {
                IMessage x = e.Current;
                Console.WriteLine(String.Format("Pos: {0}\nCod: {1}\nMessage: {2}\nResponse: {3}", x[9], x[3], x[4], ModelHelper.GetRoute(x[60] as byte[])));
            }).Wait();

            data[AdaptiveMessageFieldID.FunctionName.ToInt32()] = "GetStations";
            data[12] = 2;

            Console.WriteLine("Enumerable Request");

            request.DoRequestToList(data, e =>
            {
                IMessage x = e.Current;
                Console.WriteLine(String.Format("Pos: {0}\nCod: {1}\nMessage: {2}\nResponse: {3}", x[9], x[3], x[4], ModelHelper.GetStation(x[60] as byte[])));
            });

            Console.ReadLine();

        }
    }
}