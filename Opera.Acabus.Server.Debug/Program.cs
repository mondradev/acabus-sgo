using Acabus.Modules.CctvReports.Models;
using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Linq;
using InnSyTech.Standard.Database.Sqlite;
using Opera.Acabus.Core.Models;
using Opera.Acabus.TrunkMonitor.Models;
using System;
using System.Data.SQLite;
using System.Linq;

namespace Opera.Acabus.Server.Debug
{
    internal class DbDialect : IDbDialect
    {
        public string ConnectionString => @"Data Source=C:\Users\javi_\Documents\projects\ACABUS-Control de operacion\Opera.Acabus.Sgo\bin\Release\Resources\acabus_data.dat ; Password=acabus*data*dat";

        public IDbConverter DateTimeConverter => new DbDateTimeConverter();

        public string LastInsertFunctionName => "";

        public int TransactionPerConnection => 1;
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            //Message.SetTemplate(Path.Combine(Environment.CurrentDirectory, "acabus.config"));

            //Device d = new Device("KVR022901", DeviceType.KVR);

            //var msj = new Message();
            //msj.AddField(20, d.GetBytes());

            //var d2 = ModelsExtension.GetDevice(msj.GetBytes(20));

            var db = DbFactory.CreateSession<SQLiteConnection>(new DbDialect());

            var query = db.Read<Incidence>()
                .LoadReference(3)
                //.Execute()
                .Where(i => i.Description.Description == "AYUDA" 
                    && i.Description.ID == 1 
                    && i.Device.Station.Name == "M" 
                    && i.Device.Vehicle.ID == 2 
                    && i.Device.Station.StationNumber == 5 
                    && i.AssignedAttendance.Technician.Name == "MM")
              ;

            foreach (var l in query)
            {
                Console.WriteLine(l);
            }
        }
    }
}