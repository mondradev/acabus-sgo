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
        /// Obtiene un listado de los destinos del dinero.
        /// </summary>
        public static ICollection<CashDestiny> CashDestiny => _cashDestiny;

        public static void LoadCCTVModule()
        {
            _cashDestiny = new List<CashDestiny>();
            FillList(ref _cashDestiny, XmlToCashDestiny, "CashDestiny", "Destiny");
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
    }
}
