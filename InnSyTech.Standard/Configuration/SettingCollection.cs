using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace InnSyTech.Standard.Configuration
{
    public sealed class SettingCollection
    {
        private XmlDocument _xmlDoc;

        public SettingCollection(XmlDocument doc)
        {
            _xmlDoc = doc;
        }

        public object this[string name] => GetSetting(name);
        public object this[string category, string name] => GetSetting(name, category);

        public Object GetSetting(string name, string category = null)
        {
            var node = null as XmlNode;

            if (!String.IsNullOrEmpty(category))
                node = _xmlDoc.GetElementsByTagName(category).Cast<XmlNode>().SingleOrDefault();

            node = node.SelectSingleNode(name);
            if (node.Attributes.Cast<XmlAttribute>().SingleOrDefault(a => a.Name == "type") != null)
                return Parse(node);
            return node.Value;
        }

        internal XmlDocument GetXmlDocument()
            => _xmlDoc;


        public IEnumerable<Object> GetSettings(string name, string category = null)
        {
            var node = null as XmlNode;

            if (!String.IsNullOrEmpty(category))
                node = _xmlDoc.GetElementsByTagName(category).Cast<XmlNode>().SingleOrDefault();

            foreach (XmlNode nodeSetting in node.SelectNodes(name))
                yield return Parse(node);
        }

        private Type GetInstanceType(XmlNode node)
            => Type.GetType(node.Attributes["type"].Value);

        private Object Parse(XmlNode node)
        {
            var type = GetInstanceType(node);
            var instance = Activator.CreateInstance(type);

            foreach (XmlNode propertyNode in node.ChildNodes)
            {
                var property = type.GetProperty(propertyNode.Name);
                if (!property.CanWrite)
                {
                    var field = (type as TypeInfo).GetDeclaredField("_" + propertyNode.Name.Substring(0, 1).ToLower() + propertyNode.Name.Substring(1));
                    if (field is null)
                        continue;
                    if (field.FieldType.IsEnum)
                        field.SetValue(instance, int.Parse(propertyNode.InnerText));
                    else
                        field.SetValue(instance, Convert.ChangeType(propertyNode.InnerText, field.FieldType));
                }
                else
                    property.SetValue(instance, Convert.ChangeType(propertyNode.InnerText, property.PropertyType));
            }

            return instance;
        }
    }
}