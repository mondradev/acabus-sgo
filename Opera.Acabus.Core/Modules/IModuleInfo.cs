using System;
using System.Collections.Generic;
using System.Reflection;

namespace Opera.Acabus.Core.Modules
{
    /// <summary>
    /// Define la estructura b�sica de un m�dulo que proporciona servicios para el Sistema Gestor de Operaciones.
    /// </summary>
    public interface IModuleInfo
    {
        /// <summary>
        /// Obtiene el autor del m�dulo.
        /// </summary>
        String Author { get; }

        /// <summary>
        /// Obtiene el nombre c�digo del m�dulo.
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
        /// Obtiene el nombre del m�dulo.
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Obtiene la versi�n actual del m�dulo.
        /// </summary>
        String Version { get; }

        /// <summary>
        /// Carga el m�dulo en el sistema y devuelve un valor que indica si lo hizo correctamente.
        /// </summary>
        /// <returns>Un valor true si carg� correctamente.</returns>
        bool LoadModule();
    }
}