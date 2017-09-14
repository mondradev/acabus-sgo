using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Opera.Acabus.Core.Gui.Modules
{
    /// <summary>
    /// Clase abstracta que implementa la interfaz <see cref="IModuleInfo"/> y sirve como base para
    /// la creación de caractaristicas de un módulo.
    /// </summary>
    public abstract class ModuleInfoBase : IModuleInfo
    {
        /// <summary>
        /// Dependencias del módulo.
        /// </summary>
        private HashSet<Assembly> _dependencies;

        /// <summary>
        /// Crea una instancia nueva de <see cref="ModuleInfoBase"/> especificando sus dependencias.
        /// </summary>
        /// <param name="dependencies">Dependencias del módulo.</param>
        public ModuleInfoBase(Assembly[] dependencies)
        {
            _dependencies = dependencies != null ? new HashSet<Assembly>(dependencies) : new HashSet<Assembly>();
            _dependencies.Add(typeof(IModuleInfo).Assembly);
            _dependencies.Add(typeof(DataAccess.AcabusDataContext).Assembly);
        }

        /// <summary>
        /// Crea una instancia de <see cref="ModuleInfoBase"/>.
        /// </summary>
        public ModuleInfoBase() : this(null)
        {
        }

        /// <summary>
        /// Obtiene el autor del módulo.
        /// </summary>
        public abstract string Author { get; }

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