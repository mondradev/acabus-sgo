using InnSyTech.Standard.Database.Linq;
using MaterialDesignThemes.Wpf;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui.Modules;
using Opera.Acabus.TrunkMonitor.Models;
using Opera.Acabus.TrunkMonitor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Opera.Acabus.TrunkMonitor
{
    /// <summary>
    /// Define la información del módulo de monitor de vía y equipos externos.
    /// </summary>
    public sealed class TrunkMonitorModule : ModuleInfoBase
    {
        /// <summary>
        /// Crea una instancia nueva de <see cref="TrunkMonitorModule"/>.
        /// </summary>
        public TrunkMonitorModule() : base(null) { }

        /// <summary>
        /// Obtiene una lista de todos los enlaces de comunicación.
        /// </summary>
        public static IQueryable<Link> AllLinks => AcabusDataContext.DbContext?
            .Read<Link>()
            .LoadReference(1);

        /// <summary>
        /// Obtiene el autor del módulo.
        /// </summary>
        public override string Author => "Javier de J. Flores Mondragón";

        /// <summary>
        /// Obtiene el icono del módulo.
        /// </summary>
        public override FrameworkElement Icon => new PackIcon() { Kind = PackIconKind.SourceMerge };

        /// <summary>
        /// Obtiene el tipo de módulo.
        /// </summary>
        public override ModuleType ModuleType => ModuleType.VIEWER;

        /// <summary>
        /// Obtiene el nombre del módulo.
        /// </summary>
        public override string Name => "Monitor de equipos";

        /// <summary>
        /// Obtiene el lado de la barra en el cual aparece el botón que da acceso al módulo.
        /// </summary>
        public override Side Side => Side.LEFT;

        /// <summary>
        /// Obtiene el tipo de la vista principal del módulo.
        /// </summary>
        public override Type ViewType => typeof(TrunkMonitorView);

        /// <summary>
        /// Permite la carga de los datos utilizados por el módulo <see cref="TrunkMonitor"/>
        /// </summary>
        public override bool LoadModule() => true;
    }
}