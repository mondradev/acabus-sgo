using Acabus.Modules.CctvReports.Models;
using InnSyTech.Standard.Database;
using InnSyTech.Standard.Structures.Trees;
using System;
using System.Text;

namespace InnSyTech.Debug
{
    class Program
    {
        private static Tree<Tuple<Type, string>> tree;

        static void Main(string[] args)
        {

            // var r = DbManager.CreateSession(typeof(SQLiteConnection), new SQLiteConfiguration()).GetObjects<Incidence>();
            StringBuilder tables = new StringBuilder();
            String statement = DbReadData.CreateStatement(typeof(Incidence), ref tree, tables);

            foreach (var item in tree)
                Console.WriteLine(item.Value.Item1 + ":" + item.Value.Item2);
            Console.WriteLine();
            foreach (var item in tree)
                Console.WriteLine(item);

            Console.WriteLine(String.Format("\nSELECT {0} FROM {1}", statement, tables));
        }
    }
}
