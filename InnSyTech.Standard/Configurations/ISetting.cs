using System;

namespace InnSyTech.Standard.Configuration
{
    /// <summary>
    /// Representa una configuración y sus atributos.
    /// </summary>
    public interface ISetting
    {
        /// <summary>
        /// Obtiene el valor del atributo de la configuración.
        /// </summary>
        /// <param name="name">Nombre del atributo.</param>
        /// <returns>El valor del atributo de configuración.</returns>
        Object this[String name] { get; }

        /// <summary>
        /// Obtiene la configuración del nombre especificado.
        /// </summary>
        /// <param name="name">Nombre la configuración a obtener.</param>
        /// <returns>Obtiene el valor de una configuración.</returns>
        ISetting GetSetting(String name);

        /// <summary>
        /// Obtiene el valor de un atributo o configuración del nombre especificado.
        /// </summary>
        /// <param name="name">Nombre la configuración o atributo a obtener.</param>
        /// <returns>Obtiene el valor de una configuración o atributo.</returns>
        Object GetValue(String name);
    }
}