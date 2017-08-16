using InnSyTech.Standard.Database;
using InnSyTech.Standard.Structures.Trees;
using Opera.Acabus.Core.Models;
using Opera.Acabus.Mantto.Models;
using System;
using System.Data.SQLite;
using System.Linq;

namespace InnSyTech.Debug
{
    class Program
    {

        private class SQLiteConfiguration : IDbConfiguration
        {
            public string ConnectionString => "Data Source=Resources/acabus_data.dat;Password=acabus*data*dat";

            public string LastInsertFunctionName => "last_insert_rowid";

            public int TransactionPerConnection => 1;
        }

        static void Main(string[] args)
        {


            var conf = new SQLiteConfiguration();
            var con = new SQLiteConnection(conf.ConnectionString);
            var crud = new DbCrud(con) { Configuration = conf };
            var init = DateTime.Now;
            var results = crud.Read<Station>(null, true);
            foreach (var incidence in results)
            {
            }
            Console.WriteLine("Total: " + results.Count());
            Console.WriteLine("Tiempo total: " + (DateTime.Now - init));
            Console.ReadKey();
        }
    }
}
