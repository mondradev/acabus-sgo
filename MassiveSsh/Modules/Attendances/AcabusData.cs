using Acabus.Utils;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Acabus.DataAccess
{
    internal static partial class AcabusData
    {


        /// <summary>
        /// Campo que provee a la propiedad 'Sections'.
        /// </summary>
        private static ICollection<String> _sections;

        /// <summary>
        /// Obtiene un listado de los tramos disponibles.
        /// </summary>
        public static ICollection<String> Sections => _sections;

        public static void LoadAttendanceModule()
        {
            _sections = new List<String>();
            FillList(ref _sections, XmlToSections, "Sections", "Section");
        }

        private static String XmlToSections(XmlNode xmlNode)
        {
            return XmlUtils.GetAttribute(xmlNode, "Name")?.ToUpper();
        }
    }
}
