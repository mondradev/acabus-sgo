using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Linq;
using InnSyTech.Standard.Net.Messenger.Iso8583;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;
using Opera.Acabus.Core.Services;
using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Xml;

namespace Opera.Acabus.Server.Debug
{
    class DbDialect : IDbDialect
    {
        public string ConnectionString => @"Data Source=C:\Users\javi_\Documents\projects\ACABUS-Control de operacion\Opera.Acabus.Sgo\bin\Release\Resources\acabus_data.dat ; Password=acabus*data*dat";

        public string LastInsertFunctionName => "";

        public int TransactionPerConnection => 1;
    }

    class Program
    {
        static void Main(string[] args)
        {

            //Message.SetTemplate(Path.Combine(Environment.CurrentDirectory, "acabus.config"));

            //Device d = new Device("KVR022901", DeviceType.KVR);

            //var msj = new Message();
            //msj.AddField(20, d.GetBytes());

            //var d2 = ModelsExtension.GetDevice(msj.GetBytes(20));

            var db = DbSqlFactory.CreateSession<SQLiteConnection>(new DbDialect());

            var query = from s in db.Read<Station>()
                        where s.Name == "RENACIMIENTO"
                        select s;

            foreach (var s in query)
            {
                Console.WriteLine(s);
            }
        }
    }
}
