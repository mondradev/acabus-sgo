using System;
using System.Collections.Generic;
using System.Reflection;

namespace Opera.Acabus.Core.Modules
{
    /// <summary>
    /// Define la estructura básica de un módulo que proporciona servicios para el Sistema Gestor de Operaciones.
    /// </summary>
    public interface IModuleInfo
    {
        /// <summary>
        /// Obtiene el autor del módulo.
        /// </summary>
        String Author { get; }

        /// <summary>
        /// Obtiene el nombre código del módulo.
        /// </summary>
        string CodeName { get; }

        /// <summary>
        /// Obtiene una lista de dependencias que requiere para funcionar.
        /// </summary>
        IReadOnlyList<Assembly> Dependencies { get; }

        /// <summary>
        /// Obtiene el nombre de la DLL cargada actualmente.
        /// </summary>
        String DllFilename { get; }

        /// <summary>
        /// Obtiene el nombre del módulo.
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Obtiene la versión actual del módulo.
        /// </summary>
        String Version { get; }

        /// <summary>
        /// Carga el módulo en el sistema y devuelve un valor que indica si lo hizo correctamente.
        /// </summary>
        /// <returns>Un valor true si cargó correctamente.</returns>
        bool LoadModule();
    }
}