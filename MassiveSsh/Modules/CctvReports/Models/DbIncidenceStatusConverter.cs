using Acabus.Models;
using InnSyTech.Standard.Database.Utils;

namespace Acabus.Modules.CctvReports.Models
{
    internal class DbIncidenceStatusConverter : DbEnumConverter<IncidenceStatus>
    {
    }

    internal class DbPriorityConverter : DbEnumConverter<Priority>
    {
    }
}