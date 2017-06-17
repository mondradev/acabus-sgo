using Acabus.Modules.CctvReports.Models;
using InnSyTech.Standard.Database;
using System;
using System.Data.SQLite;

namespace InnSyTech.Debug
{
    class Program
    {
        static void Main(string[] args)
        {

            var r = DbManager.CreateSession(typeof(SQLiteConnection), new SQLiteConfiguration()).GetObjects<Incidence>(typeof(Incidence));

        }
    }
}
