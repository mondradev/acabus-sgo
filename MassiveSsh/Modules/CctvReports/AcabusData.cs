using Acabus.Models;
using Acabus.Modules.CctvReports.Models;
using Acabus.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Acabus.DataAccess
{
    internal static partial class AcabusData
    {

        /// <summary>
        /// Campo que provee a la propiedad 'ReportQueries'.
        /// </summary>
        private static ICollection<ReportQuery> _reportQueries;


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

        /// <summary>
        /// Busca un dispositivo en todos los autobuses.
        /// </summary>
        public static Device FindDeviceBus(String economicNumber, Predicate<Device> predicate)
        {
            foreach (var device in Acabus.Modules.Core.DataAccess.AcabusData.AllVehicles
                                        .FirstOrDefault(vehicle =>
                                            vehicle.EconomicNumber == economicNumber).Devices)
                if (predicate.Invoke(device))
                    return device;
            return null;
        }

        public static void LoadCCTVModule()
        {
            ReportQueries.Clear();
            FillList(ref _reportQueries, XmlToReportQuery, "Reports", "Report");
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