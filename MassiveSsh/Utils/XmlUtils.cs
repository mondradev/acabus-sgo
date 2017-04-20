using System;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace MassiveSsh.Utils
{
    public sealed class XmlUtils
    {

        public static String GetAttribute(XmlNode node, String name)
        {
            return node.Attributes[name]?.Value?.ToString();
        }


        private static Attribute GetCustomAttribute(PropertyInfo property, Type type)
        {
            foreach (Attribute attributes in property.GetCustomAttributes(false))
            {
                if (attributes.GetType() == type) return attributes;
            }
            return null;
        }

        private static Attribute GetCustomAttribute(Type typeInstance, Type attributeType)
        {
            if (typeInstance == null) return null;
            foreach (Attribute attribute in typeInstance?.GetCustomAttributes(false))
            {
                if (attribute.GetType() == attributeType) return attribute;
            }
            return null;
        }

        public static XmlDocument ToXml(Object instance, String name)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);
            doc.AppendChild(ToXmlNodeRecursive(instance, name, doc));
            return doc;
        }

        private static XmlNode ToXmlNodeRecursive(Object instance, String Name, XmlDocument doc)
        {
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

        public static bool GetAttributeBool(XmlNode node, string name)
        {
            Boolean.TryParse(GetAttribute(node, name), out bool attribute);
            return attribute;
        }

        public static Int16 GetAttributeInt(XmlNode node, string name)
        {
            Int16.TryParse(GetAttribute(node, name), out Int16 attribute);
            return attribute;
        }
    }
}
