using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.TrunkMonitor.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Opera.Acabus.TrunkMonitor.DataAccess
{
    /// <summary>
    /// Gestiona toda la configuración del modulo <see cref="TrunkMonitor"/>.
    /// </summary>
    public static class AcabusDataExtensions
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="AllLinks" />.
        /// </summary>
        private static ICollection<Link> _allLinks;

        /// <summary>
        /// Obtiene una lista de todos los enlaces de comunicación.
        /// </summary>
        public static ICollection<Link> AllLinks
            => _allLinks ?? (_allLinks = new ObservableCollection<Link>());

        /// <summary>
        /// Permite la carga de los datos utilizados por el módulo <see cref="TrunkMonitor"/>
        /// </summary>
        public static void LoadTrunkMonitor()
        {
            _allLinks = AcabusData.Session.GetObjects<Link>();
        }
    }
}