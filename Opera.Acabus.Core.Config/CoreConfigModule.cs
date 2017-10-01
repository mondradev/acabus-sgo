using Opera.Acabus.Core.Config.Views;
using Opera.Acabus.Core.Gui.Modules;
using System;
using System.Windows;

namespace Opera.Acabus.Core.Config
{
    /// <summary>
    /// Define la configuración del nucleo del SGO.
    /// </summary>
    public class CoreConfigModule : ModuleInfoBase
    {
        /// <summary>
        /// Crea una instancia nueva de <see cref="CoreConfigModule"/>.
        /// </summary>
        public CoreConfigModule() : base(null) { }

        /// <summary>
        /// Obtiene el autor del módulo.
        /// </summary>
        public override string Author => "Javier de J. Flores Mondragón";

        /// <summary>
        /// Obtiene el nombre código del módulo.
        /// </summary>
        public override string CodeName => "Sgo_Core_Config";

        /// <summary>
        /// Obtiene el icono que representa al módulo.
        /// </summary>
        public override FrameworkElement Icon => null;

        /// <summary>
        /// Obtiene el tipo de módulo.
        /// </summary>
        public override ModuleType ModuleType => ModuleType.CONFIGURATION;

        /// <summary>
        /// Obtiene el nombre del módulo.
        /// </summary>
        public override string Name => "Catálogos generales";

        /// <summary>
        /// Obtiene el lado de la barra en la cual aparece el botón que da acceso al módulo.
        /// </summary>
        public override Side Side => Side.LEFT;

        /// <summary>
        /// Obtiene el tipo de la bista principal del módulo.
        /// </summary>
        public override Type ViewType => typeof(CoreConfigView);

        /// <summary>
        /// Permite la carga de los datos utilizados por el módulo <see cref="Config"/>.
        /// </summary>
        /// <returns>Un valor true cuando el módulo ha cargado correctamente.</returns>
        public override bool LoadModule() => true;
    }
}