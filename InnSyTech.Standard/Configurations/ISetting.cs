using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Obtiene todos los atributos de la configuración.
        /// </summary>
        /// <returns>Un diccionario con el contenido de la configuración.</returns>
        IReadOnlyDictionary<String, Object> GetValues();

        /// <summary>
        /// Obtiene el entero de un atributo compatible.
        /// </summary>
        /// <param name="name">Nombre del atributo.</param>
        /// <returns>El valor del entero.</returns>
        long ToInteger(string name);

        /// <summary>
        /// Obtiene la cadena que representa el valor del atributo.
        /// </summary>
        /// <param name="name">Nombre del atributo.</param>
        /// <returns>Cadena del valor del atributo.</returns>
        string ToString(string name);

        /// <summary>
        /// Obtiene todas las configuraciones que incluye en el interior del nodo especificado.
        /// </summary>
        /// <param name="name">Nombre del nodo de configuración.</param>
        /// <returns>Una secuencias de configuraciones.</returns>
        IReadOnlyList<ISetting> GetSettings(string name);
    }
}