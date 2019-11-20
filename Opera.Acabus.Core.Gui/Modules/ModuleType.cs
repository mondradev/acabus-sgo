using Opera.Acabus.Core.Modules;

namespace Opera.Acabus.Core.Gui.Modules
{
    /// <summary>
    /// Enumera los tipos de módulos disponibles.
    /// </summary>
    public enum ModuleType
    {
        /// <summary>
        /// Tipo visor. Este tipo requiere un valor válido en la propiedad <see
        /// cref="IModuleInfo.ViewType"/> y <see cref="IModuleInfo.Icon"/>.
        /// </summary>
        VIEWER,

        /// <summary>
        /// Tipo configuración. Este tipo requiere un valor válido en <see cref="IModuleInfo.ViewType"/>.
        /// </summary>
        CONFIGURATION,

        /// <summary>
        /// Tipo de servicio. Este tipo se ejecuta en un hilo independiente.
        /// </summary>
        SERVICE
    }
}