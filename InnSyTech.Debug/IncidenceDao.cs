using Acabus.Modules.CctvReports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InnSyTech.Debug
{
    public static class IncidenceDao
    {
        public static Incidence GetObject(String folio) => DbDaoSqlite.GetObject<Incidence>(folio);

        public static IEnumerable<Incidence> GetObjects() => DbDaoSqlite.GetObjects<Incidence>();

        public static bool Save(this Incidence incidence) => DbDaoSqlite.Save(incidence);

        public static bool Update(this Incidence incidence) => DbDaoSqlite.Update(incidence);

        public static RefundOfMoney GetRefund(long id) => DbDaoSqlite.GetObject<RefundOfMoney>(id);
    }
}
