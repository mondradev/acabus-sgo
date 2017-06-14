using Acabus.Modules.Configurations;
using Acabus.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Acabus.DataAccess
{
    internal partial class AcabusData
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Configurables'.
        /// </summary>
        private static ICollection<IConfigurable> _configurables;

        /// <summary>
        /// Obtiene una lista de el valor de esta propiedad.
        /// </summary>
        public static ICollection<IConfigurable> Configurables {
            get {
                if (_configurables == null)
                    _configurables = new ObservableCollection<IConfigurable>();
                return _configurables;
            }
        }

        /// <summary>
        /// Carga las vistas de configuración de los modulos.
        /// </summary>
        public static void LoadConfigModules()
        {
            Configurables.Clear();
            FillList(ref _configurables, ToConfigurable, "Configurables", "Configurable");
        }

        private static IConfigurable ToConfigurable(XmlNode arg)
            => (IConfigurable)Activator.CreateInstance(Type.GetType(XmlUtils.GetAttribute(arg, "ClassName")));

    }
}
