using Acabus.Modules.CctvReports.Models;
using Acabus.Utils;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Acabus.DataAccess
{
    internal static partial class AcabusData
    {
        /// <summary>
        /// Campo que provee a la propiedad 'CashDestiny'.
        /// </summary>
        private static ICollection<CashDestiny> _cashDestiny;

        /// <summary>
        /// Campo que provee a la propiedad 'ReportQueries'.
        /// </summary>
        private static ICollection<ReportQuery> _reportQueries;

        /// <summary>
        /// Obtiene un listado de los destinos del dinero.
        /// </summary>
        public static ICollection<CashDestiny> CashDestiny => _cashDestiny;

        /// <summary>
        /// Obtiene una lista de las consultas de los reportes.
        /// </summary>
        public static ICollection<ReportQuery> ReportQueries {
            get {
                if (_reportQueries == null)
                    _reportQueries = new List<ReportQuery>();
                return _reportQueries;
            }
        }

        public static void LoadCCTVModule()
        {
            _cashDestiny = new List<CashDestiny>();
            ReportQueries.Clear();
            FillList(ref _cashDestiny, XmlToCashDestiny, "CashDestiny", "Destiny");
            FillList(ref _reportQueries, XmlToReportQuery, "Reports", "Report");
        }

        private static CashDestiny XmlToCashDestiny(XmlNode xmlNode)
        {
            var description = XmlUtils.GetAttribute(xmlNode, "Description");
            var type = XmlUtils.GetAttribute(xmlNode, "Type");
            return new CashDestiny()
            {
                Description = description,
                Type = (CashType)Enum.Parse(typeof(CashType), type)
            };
        }

        /// <summary>
        /// Permite obtener las consultas de los reportes.
        /// </summary>
        private static ReportQuery XmlToReportQuery(XmlNode xmlNode)
        {
            return new ReportQuery()
            {
                Query = XmlUtils.GetAttribute(xmlNode, "Query"),
                Description = XmlUtils.GetAttribute(xmlNode, "Description")
            };
        }
    }
}