using InnSyTech.Standard.Database;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace InnSyTech.Debug
{
    public static class DbDaoSqlite
    {
        private static SQLiteConfiguration _configuration;

        private static DbSession _session;

        static DbDaoSqlite()
        {
            _configuration = new SQLiteConfiguration();
            _session = DbManager.CreateSession(typeof(SQLiteConnection), _configuration);
        }

        public static bool Delete<T>(T instance) => _session.Delete(instance);

        public static T GetObject<T>(Object idkey) => (T)_session.GetObject<T>( idkey);

        public static IEnumerable<T> GetObjects<T>() => _session.GetObjects<T>().Select(item => (T)item);

        public static bool Save<T>(T instance) => _session.Save(instance);

        public static bool Update<T>(T instance) => _session.Update(instance);
    }
}