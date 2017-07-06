using Opera.Acabus.Core.Modules.Configurations;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Opera.Acabus.Core.DataAccess
{
    /// <summary>
    /// Esta clase proporciona la comunicación con el módulo de <see cref="Opera.Acabus.Core.Modules.Configurations"/>.
    /// </summary>
    /// <seealso cref="Opera.Acabus.Core"/>
    public static partial class AcabusData
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="Configurables"/>.
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
    }
}