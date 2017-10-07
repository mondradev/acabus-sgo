using Opera.Acabus.Core.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Tipo configuración. Este tipo requiere un valor válido en <see cref="IModuleInfo.ViewType"/>.
        /// </summary>
        CONFIGURATION,

        /// <summary>
        /// Tipo de servicio. Este tipo se ejecuta en un hilo independiente.
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
    /// Clase abstracta que implementa la interfaz <see cref="IModuleInfo"/> y sirve como base para
    /// la creación de caractaristicas de un módulo que involucra la interfaz gráfica.
    /// </summary>
    public abstract class ModuleInfoGui : IModuleInfo
    {
        /// <summary>
        /// Dependencias del módulo.
        /// </summary>
        private HashSet<Assembly> _dependencies;

        /// <summary>
        /// Crea una instancia nueva de <see cref="ModuleInfoGui"/> especificando sus dependencias.
        /// </summary>
        /// <param name="dependencies">Dependencias del módulo.</param>
        public ModuleInfoGui(Assembly[] dependencies)
        {
            _dependencies = dependencies != null ? new HashSet<Assembly>(dependencies) : new HashSet<Assembly>();
            _dependencies.Add(typeof(ModuleInfoGui).Assembly);
            _dependencies.Add(typeof(DataAccess.AcabusDataContext).Assembly);
        }

        /// <summary>
        /// Crea una instancia nueva de <see cref="ModuleInfoGui"/>.
        /// </summary>
        public ModuleInfoGui() : this(null) { }

        /// <summary>
        /// Obtiene el autor del módulo.
        /// </summary>
        public abstract string Author { get; }

        /// <summary>
        /// Obtiene el nombre código del módulo
        /// </summary>
        public abstract string CodeName { get; }

        /// <summary>
        /// Obtiene la lista de dependencias del módulo.
        /// </summary>
        public IReadOnlyList<Assembly> Dependencies => _dependencies.ToList();

        /// <summary>
        /// Obtiene el nombre del archivo Dll del módulo.
        /// </summary>
        public string DllFilename => GetType().Assembly?.GetName().Name;

        /// <summary>
        /// Obtiene el icono que representa al módulo.
        /// </summary>
        public abstract FrameworkElement Icon { get; }

        /// <summary>
        /// Obtiene el tipo de módulo.
        /// </summary>
        public abstract ModuleType ModuleType { get; }

        /// <summary>
        /// Obtiene el nombre del módulo.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Obtiene el lado de la barra en el cual aparece el botón que da acceso al módulo.
        /// </summary>
        public abstract Side Side { get; }

        /// <summary>
        /// Obtiene la versión del módulo.
        /// </summary>
        public string Version => GetType().Assembly?.GetName().Version.ToString();

        /// <summary>
        /// Obtiene el tipo de la interfaz gráfica del módulo.
        /// </summary>
        public abstract Type ViewType { get; }

        /// <summary>
        /// Carga la configuración inicial del módulo. Este método es llamadó cuando se carga el
        /// módulo en memoria.
        /// </summary>
        /// <returns>Un valor true si el módulo se cargó correctamente.</returns>
        public abstract bool LoadModule();
    }
}