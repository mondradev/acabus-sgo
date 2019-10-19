using System;

namespace Acabus.Modules.CctvReports.Models
{
    public sealed class ReportQuery
    {
        public String Description { get; set; }
        public String Query { get; set; }

        public override string ToString()
        {
            return Description;
        }
    }
}