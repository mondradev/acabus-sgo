using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Opera.Acabus.Configurations
{
    /// <summary>
    /// Define la estructura de un componente configurable de algún módulo.
    /// </summary>
    public interface IConfigurable
    {
        /// <summary>
        /// Obtiene la lista de comandos accionables desde la interfaz de configuración.
        /// </summary>
        List<Tuple<String, ICommand>> Commands { get; }

        /// <summary>
        /// Obtiene la lista de datos para la vista previa de la configuración.
        /// </summary>
        List<Tuple<String, Func<Object>>> PreviewData { get; }

        /// <summary>
        /// Obtiene el título del componente de configuración.
        /// </summary>
        String Title { get; }
    }
}