using System;
using System.Collections.Generic;
using System.Xml;

namespace InnSyTech.Standard.Configuration
{
    /// <summary>
    /// Implementación de <see cref="ISetting"/>.
    /// </summary>
    internal sealed class Setting : ISetting
    {
        /// <summary>
        /// Conjunto de atributos.
        /// </summary>
        private Dictionary<String, Object> _attributes;

        /// <summary>
        /// Crea una instancia nueva de <see cref="Setting"/>.
        /// </summary>
        /// <param name="node">Node Xml de la configuración.</param>
        public Setting(XmlNode node)
        {
            _attributes = new Dictionary<string, object>();

            if (node != null)
                Process(this, node);
        }

        /// <summary>
        /// Obtiene el valor del atributo de la configuración.
        /// </summary>
        /// <param name="name">Nombre del atributo.</param>
        /// <returns>El valor del atributo de configuración.</returns>
        public Object this[String name] => GetValue(name);

        /// <summary>
        /// Obtiene la configuración del nombre especificado.
        /// </summary>
        /// <param name="name">Nombre la configuración a obtener.</param>
        /// <returns>Obtiene el valor de una configuración.</returns>
        public ISetting GetSetting(String name)
        {
            var attr = GetValue(name);

            if (attr == null)
                return null;

            if (attr is ISetting)
                return attr as ISetting;

            return null;
        }

        /// <summary>
        /// Obtiene el valor de un atributo o configuración del nombre especificado.
        /// </summary>
        /// <param name="name">Nombre la configuración o atributo a obtener.</param>
        /// <returns>Obtiene el valor de una configuración o atributo.</returns>
        public Object GetValue(String name)
        {
            var attr = _attributes.ContainsKey(name) ? _attributes[name] : null;

            if (attr == null)
                return null;

            return attr;
        }

        /// <summary>
        /// Obtiene los valores de la configuración.
        /// </summary>
        /// <returns>Un diccionario con el contenido de la configuración.</returns>
        public IReadOnlyDictionary<string, object> GetValues()
            => _attributes;

        /// <summary>
        /// Obtiene los valores de la configuración.
        /// </summary>
        /// <returns>Un diccionario con el contenido de la configuración.</returns>
        internal Dictionary<string, object> GetAttributes()
            => _attributes;

        /// <summary>
        /// Procesa el nodo para extraer todos sus nodos y atributos.
        /// </summary>
        /// <param name="setting">Instancia que representa la configuración.</param>
        /// <param name="node">Nodo Xml de configuración.</param>
        private static void Process(Setting setting, XmlNode node)
        {
            foreach (XmlAttribute attr in node.Attributes)
                setting._attributes.Add(attr.Name, attr.Value);

            foreach (XmlNode child in node.ChildNodes)
                setting._attributes.Add(child.Name, new Setting(child));
        }
    }
}