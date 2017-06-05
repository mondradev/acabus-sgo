using InnSyTech.Standard.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InnSyTech.Debug
{
    public class SQLiteDateTimeConverter : IDbConverter
    {
        public object ConverterFromDb(object data)
        {
            return DateTime.Parse(data.ToString());
        }

        public object ConverterToDbData(object property)
        {
            return ((DateTime)property).ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }
}
