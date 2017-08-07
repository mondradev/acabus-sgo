using System;
using System.Windows;

namespace Opera.Acabus.Core.Gui.Modules
{
    /// <summary>
    /// Define la estructura básica de un módulo para el Sistema Gestor de Operaciones.
    /// </summary>
    public interface ISgoModule
    {
        /// <summary>
        /// Obtiene el icono del módulo.
        /// </summary>
        FrameworkElement Icon { get; }

        /// <summary>
        /// Indica si es un módulo secundario o de configuración.
        /// </summary>
        Boolean IsSecundary { get; }

        /// <summary>
        /// Indica si es un módulo de solo servicio
        /// </summary>
        Boolean IsService { get; }

        /// <summary>
        /// Obtiene el nombre del módulo.
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Obtiene el tipo de dato de la vista del módulo.
        /// </summary>
        Type View { get; }

        /// <summary>
        /// Carga el módulo en el sistema y devuelve un valor que indica si lo hizo correctamente.
        /// </summary>
        /// <returns>Un valor <see cref="true"/> si cargó correctamente.</returns>
        bool LoadModule();
    }
}