using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Opera.Acabus.Core.DataAccess
{
    /// <summary>
    /// Define la estructura de una configuración.
    /// </summary>
    public interface ISetting
    {
        /// <summary>
        /// Nombre de la configuración.
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Obtiene el valor del atributo especificado.
        /// </summary>
        /// <param name="attributeName">Nombre del atributo.</param>
        /// <returns>Valor del atributo.</returns>
        Object this[String attributeName] { get; }
    }

    /// <summary>
    /// Controla toda la configuración de la aplicación a través de un documento XML.
    /// </summary>
    public static class XmlConfiguration
    {
        /// <summary>
        /// Archivo de configuración de las rutas.
        /// </summary>
        private static readonly String CONFIG_FILENAME = Path.Combine(Environment.CurrentDirectory, "Resources\\acabus_sgo.cnf");

        /// <summary>
        /// Instancia del documento de configuración de la aplicación.
        /// </summary>
        private static XmlDocument _xmlDocument;

        /// <summary>
        /// Constructor estático de la clase <see cref="XmlConfiguration"/>.
        /// </summary>
        static XmlConfiguration()
        {
            _xmlDocument = new XmlDocument();
            if (!File.Exists(CONFIG_FILENAME))
            {
                XmlElement element = _xmlDocument.CreateElement("SGO");
                _xmlDocument.AppendChild(element);
                _xmlDocument.Save(CONFIG_FILENAME);
            }
            _xmlDocument.Load(CONFIG_FILENAME);
        }

        /// <summary>
        /// Crea una sección de configuración y la guarda en el archivo xml.
        /// </summary>
        /// <param name="sectionName">Nombre de la nueva sección.</param>
        public static void CreateSection(String sectionName)
        {
            XmlElement sectionNode = _xmlDocument.CreateElement(sectionName);
            XmlNode rootNode = _xmlDocument.SelectSingleNode("SGO");
            rootNode.AppendChild(sectionNode);
            _xmlDocument.Save(CONFIG_FILENAME);
        }

        /// <summary>
        /// Crea una configuración especificando sus attributos y a la sección que pertenece.
        /// </summary>
        public static void CreateSetting(String settingName, Dictionary<String, Object> attributes, String sectionName)
        {
            XmlElement settingNode = _xmlDocument.CreateElement(settingName);
            XmlNode sectionNode = _xmlDocument.SelectSingleNode(sectionName);

            foreach (var key in attributes.Keys)
            {
                XmlAttribute attribute = _xmlDocument.CreateAttribute(key);
                attribute.Value = attribute[key].Value;
                settingNode.Attributes.Append(attribute);
            }

            sectionNode.AppendChild(sectionNode);
            _xmlDocument.Save(CONFIG_FILENAME);
        }

        /// <summary>
        /// Obtiene todas las configuraciones que cumplen con los parametros especificados.
        /// </summary>
        /// <param name="settingName">Nombre de los nodos de configuración.</param>
        /// <param name="sectionName">Nombre de la sección de configuración.</param>
        /// <returns>Una lista de configuraciones.</returns>
        public static ICollection<ISetting> GetSettings(String settingName, String sectionName)
        {
            XmlNode sectionNode = _xmlDocument.SelectSingleNode(sectionName);
            XmlNodeList settingNodeList = sectionNode.SelectNodes(settingName);

            ICollection<ISetting> settingList = new List<ISetting>();

            foreach (XmlNode settingNode in settingNodeList)
            {
                Setting setting = new Setting(settingNode.Name);
                foreach (XmlAttribute attribute in sectionNode.Attributes)
                    setting.Add(attribute.Name, attribute.Value);

                settingList.Add(setting);
            }

            return settingList;
        }

        /// <summary>
        /// Reemplaza la configuración de la sección especificada por la pasada por argumento.
        /// </summary>
        /// <param name="settings">Configuración a guardar.</param>
        /// <param name="sectionName">Nombre de la sección.</param>
        public static void SaveSettings<T>(ICollection<T> settings, String sectionName)
        {
            var xmlNode = ToXml(settings, sectionName);
            var settingNode = _xmlDocument.SelectSingleNode(sectionName);

            if (settingNode is null) return;
            settingNode.InnerXml = xmlNode.InnerXml;
            _xmlDocument.Save(CONFIG_FILENAME);
        }

        /// <summary>
        /// Convierte una instancia de cualquier tipo en un elemento Xml.
        /// </summary>
        /// <param name="instance">Instancia a convertir.</param>
        /// <param name="name">Nombre del elemento.</param>
        /// <returns>Un elemento Xml que representa la instancia pasada por argumento.</returns>
        public static XmlNode ToXml(Object instance, String name)
            => ToXmlNodeRecursive(instance, name, _xmlDocument);

        /// <summary>
        /// Obtiene el primer atribute de la propiedad que cumple con el tipo especificado.
        /// </summary>
        /// <param name="property">Propiedad a extraer el atributo.</param>
        /// <param name="type">Tipo de los atributos deseados.</param>
        /// <returns>El primer atributo del tipo especificado.</returns>
        private static Attribute GetCustomAttribute(PropertyInfo property, Type type)
        {
            foreach (Attribute attributes in property.GetCustomAttributes(false))
                if (attributes.GetType() == type) return attributes;
            return null;
        }

        /// <summary>
        /// Obtiene el primero atributo del tipo que cumple con el tipo especificado.
        /// </summary>
        /// <param name="typeInstance">Tipo a extraer el atributo.</param>
        /// <param name="type">Tipo de los atributos deseados.</param>
        /// <returns>El primer atributo del tipo especificado.</returns>
        private static Attribute GetCustomAttribute(Type typeInstance, Type attributeType)
        {
            if (typeInstance == null) return null;
            foreach (Attribute attribute in typeInstance?.GetCustomAttributes(false))
                if (attribute.GetType() == attributeType) return attribute;
            return null;
        }

        /// <summary>
        /// Convierte una instancia de manera recursiva en un elemento Xml.
        /// </summary>
        /// <param name="instance">Instancia a convetir en Xml.</param>
        /// <param name="Name">Nombre del nuevo elemento Xml.</param>
        /// <param name="doc">Documento Xml padre.</param>
        /// <returns>Un nuevo elemento Xml a partir de la instancia pasada por argumento.</returns>
        private static XmlNode ToXmlNodeRecursive(Object instance, String Name, XmlDocument doc)
        {
            if (instance == null) return null;

            Type type = instance.GetType();
            var attribute = GetCustomAttribute(type, typeof(XmlAnnotationAttribute));
            XmlNode node = doc.CreateElement(String.IsNullOrEmpty(Name)
                ? attribute != null
                    ? ((XmlAnnotationAttribute)attribute).Name
                    : type.Name
                : Name);
            if (type.GetInterface("IList") != null && type != typeof(String))
            {
                IList list = instance as IList;
                foreach (var item in list)
                    node.AppendChild(ToXmlNodeRecursive(item, String.Empty, doc));
            }
            else
                foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    attribute = GetCustomAttribute(property, typeof(XmlAnnotationAttribute));
                    if (attribute != null && ((XmlAnnotationAttribute)attribute).Ignore) continue;
                    XmlAttribute attr = doc.CreateAttribute(property.Name);
                    if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(String))
                    {
                        var valueObj = property.GetValue(instance);
                        attr.Value = valueObj != null ? valueObj.ToString() : String.Empty;
                        node.Attributes.Append(attr);
                    }
                    else if (property.GetValue(instance) != null && property.GetValue(instance).GetType().IsEnum)
                    {
                        var valueObj = property.GetValue(instance);
                        attr.Value = valueObj != null ? valueObj.ToString() : String.Empty;
                        node.Attributes.Append(attr);
                    }
                    else
                    {
                        var value = property.GetValue(instance);
                        var typeProperty = property.PropertyType;
                        node.AppendChild(ToXmlNodeRecursive(value, typeProperty.GetInterface("IList") != null
                                                    && typeProperty != typeof(String) ? property.Name : "", doc));
                    }
                }
            return node;
        }

        /// <summary>
        /// Implementa a <see cref="ISetting"/> y define el comportamiento de una configuración de aplicación.
        /// </summary>
        private sealed class Setting : ISetting
        {
            /// <summary>
            /// Diccionario de atributos de la configuración.
            /// </summary>
            private Dictionary<String, Object> attributes;

            /// <summary>
            /// Crea una nueva instancia de <see cref="Setting"/>.
            /// </summary>
            /// <param name="name">Nombre de la configuración.</param>
            public Setting(String name)
            {
                Name = name;
                attributes = new Dictionary<String, Object>();
            }

            /// <summary>
            /// Obtiene el nombre la configuración.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Obtiene el valor del atributo especificado.
            /// </summary>
            /// <param name="attributeName">Nombre del atributo a obtener su valor.</param>
            /// <returns>El valor del atributo especificado.</returns>
            public object this[string attributeName] => attributes[attributeName];

            /// <summary>
            /// Agrega un nuevo attributo a la configuración.
            /// </summary>
            public void Add(String name, Object value)
                => attributes.Add(name, value);
        }
    }

    /// <summary>
    /// Representa un atributo personalizado que define algunas caracteristicas de las propiedades de
    /// las instancias que se desean convertir a elementos Xml.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class XmlAnnotationAttribute : Attribute
    {
        /// <summary>
        /// Obtiene o establece si la propiedad es ignorada.
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// Obtiene o estable el nombre de la propiedad en su versión Xml.
        /// </summary>
        public string Name { get; set; }
    }
}