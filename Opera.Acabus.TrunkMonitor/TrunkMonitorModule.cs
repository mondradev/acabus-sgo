using MaterialDesignThemes.Wpf;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui.Modules;
using Opera.Acabus.TrunkMonitor.Models;
using Opera.Acabus.TrunkMonitor.Views;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Opera.Acabus.TrunkMonitor
{
    /// <summary>
    /// Define la información del módulo de monitor de vía y equipos externos.
    /// </summary>
    public class TrunkMonitorModule : IModuleInfo
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="AllLinks" />.
        /// </summary>
        private static IEnumerable<Link> _allLinks;

        /// <summary>
        /// Obtiene una lista de todos los enlaces de comunicación.
        /// </summary>
        public static IEnumerable<Link> AllLinks => _allLinks;

        /// <summary>
        /// Obtiene el icono del módulo.
        /// </summary>
        public FrameworkElement Icon => new PackIcon() { Kind = PackIconKind.SourceMerge };

        /// <summary>
        /// Indica si el módulo es secundario o de configuración.
        /// </summary>
        public bool IsConfiguration => false;

        /// <summary>
        /// Indica si el módulo es solo de servicio.
        /// </summary>
        public bool IsService => false;

        /// <summary>
        /// Obtiene el nombre del módulo.
        /// </summary>
        public string Name => "Monitor de vía y equipos externos";

        /// <summary>
        /// Obtiene el tipo de la vista principal del módulo.
        /// </summary>
        public Type ViewType => typeof(TrunkMonitorView);

        /// <summary>
        /// Permite la carga de los datos utilizados por el módulo <see cref="TrunkMonitor"/>
        /// </summary>
        public bool LoadModule()
        {
            try
            {
                _allLinks = AcabusDataContext.DbContext?.Read<Link>();
                return true;
            }
            catch { return false; }
        }
    }
}