using System;

namespace Acabus.Modules.CctvReports.Models
{
    public struct CashDestiny
    {
        public String Description { get; set; }
        public CashType Type { get; set; }

        public override string ToString()
        {
            return Description;
        }
    }

    public enum CashType
    {
        MONEY,
        BILL,
        GENERAL
    }
}