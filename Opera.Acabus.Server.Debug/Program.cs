using InnSyTech.Standard.Net.Messenger.Iso8583;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;
using Opera.Acabus.Core.Services;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml;

namespace Opera.Acabus.Server.Debug
{
    class Program
    {
        static void Main(string[] args)
        {

            Message.SetTemplate(Path.Combine(Environment.CurrentDirectory, "acabus.config"));

            Device d = new Device("KVR022901", DeviceType.KVR);

            var msj = new Message();
            msj.AddField(20, d.GetBytes());

            var d2 = ModelsExtension.GetDevice(msj.GetBytes(20));

        }
    }
}
