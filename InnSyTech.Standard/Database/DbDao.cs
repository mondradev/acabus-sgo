using System.Collections.Generic;

namespace InnSyTech.Standard.Database
{
    public static class DbDao
    {
        public static bool Delete<T>(T instance) => DbManager.Session.Delete(instance);

        public static T GetObject<T>(object key) => (T)DbManager.Session.GetObject(typeof(T), key);

        public static IEnumerable<T> GetObjects<T>() => (IEnumerable<T>)DbManager.Session.GetObjects(typeof(T));

        public static bool Save<T>(T instance) => DbManager.Session.Save(instance);

        public static bool Update<T>(T instance) => DbManager.Session.Update(instance);
    }
}