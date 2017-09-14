using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;

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
        /// Tipo configuración. Este tipo requiere un valor válido en <see
        /// cref="IModuleInfo.ViewType"/> y <see cref="IModuleInfo.Icon"/>.
        /// </summary>
        CONFIGURATION,

        /// <summary>
        /// Tipo de servicio. Este tipo ignora el valor de <see cref="IModuleInfo.ViewType"/>, se
        /// ejecuta en un hilo independiente.
        /// </summary>
        SERVICE
    }

    /// <summary>
    /// Contiene los valores para determinar en que lado de la barra principal de una aplicación se
    /// coloca el elemento visual.
    /// </summary>
    public enum Side
    {
        /// <summary>
        /// Lado izquierdo.
        /// </summary>
        LEFT,

        /// <summary>
        /// Lado derecho (Comúnmente utilizado para botones de configuración o búsqueda).
        /// </summary>
        RIGHT
    }

    /// <summary>
    /// Define la estructura básica de un módulo para el Sistema Gestor de Operaciones.
    /// </summary>
    public interface IModuleInfo
    {
        /// <summary>
        /// Obtiene el autor del módulo.
        /// </summary>
        String Author { get; }

        /// <summary>
        /// Obtiene una lista de dependencias que requiere para funcionar.
        /// </summary>
        IReadOnlyList<Assembly> Dependencies { get; }

        /// <summary>
        /// Obtiene el nombre de la DLL cargada actualmente.
        /// </summary>
        String DllFilename { get; }

        /// <summary>
        /// Obtiene el icono del módulo.
        /// </summary>
        FrameworkElement Icon { get; }

        /// <summary>
        /// Obtiene el tipo de módulo.
        /// </summary>
        ModuleType ModuleType { get; }

        /// <summary>
        /// Obtiene el nombre del módulo.
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Obtiene el lado de la barra al cual se agrega.
        /// </summary>
        Side Side { get; }

        /// <summary>
        /// Obtiene la versión actual del módulo.
        /// </summary>
        String Version { get; }

        /// <summary>
        /// Obtiene el tipo de dato de la vista del módulo.
        /// </summary>
        Type ViewType { get; }

        /// <summary>
        /// Carga el módulo en el sistema y devuelve un valor que indica si lo hizo correctamente.
        /// </summary>
        /// <returns>Un valor true si cargó correctamente.</returns>
        bool LoadModule();
    }
}