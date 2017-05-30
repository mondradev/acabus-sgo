using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InnSyTech.Standard.Database
{
    internal class DbField
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Converter'.
        /// </summary>
        private Func<object, object> _converter;

        /// <summary>
        /// Campo que provee a la propiedad 'IsPrimaryKey'.
        /// </summary>
        private Boolean _isPrimaryKey;

        /// <summary>
        /// Campo que provee a la propiedad 'Name'.
        /// </summary>
        private String _name;

        public DbField(String name, Boolean isPrimaryKey = false, Func<object, object> converter = null)
        {
            _name = name;
            _isPrimaryKey = isPrimaryKey;
            _converter = converter;
        }

        /// <summary>
        /// Obtiene el convertidor de datos para el campo en la base de datos.
        /// </summary>
        public Func<object, object> Converter => _converter;

        /// <summary>
        /// Obtiene si el campo es llave primaria en la base de datos.
        /// </summary>
        public Boolean IsPrimaryKey => _isPrimaryKey;

        /// <summary>
        /// Obtiene el nombre del campo en la base de datos.
        /// </summary>
        public String Name => _name;

        /// <summary>
        /// Obtiene todo los campos especificados de un tipo de dato.
        /// </summary>
        /// <param name="typeOfInstance">Tipo de dato que contiene los campos.</param>
        /// <returns>Una enumaración de campos.</returns>
        public static IEnumerable<DbField> GetFields(Type typeOfInstance)
        {
            foreach (PropertyInfo property in typeOfInstance.GetProperties())
            {
                if (property.GetCustomAttributes().Count() > 0)
                {
                    foreach (Attribute attribute in property.GetCustomAttributes())
                        if (attribute is ColumnAttribute)
                        {
                            ColumnAttribute columnAttribute = (attribute as ColumnAttribute);

                            if (columnAttribute.IsIgnored) continue;

                            String name = columnAttribute.Name;
                            name = String.IsNullOrEmpty(name) ? property.Name : name;

                            yield return new DbField(name, columnAttribute.IsPrimaryKey, columnAttribute.Converter);
                        }
                }
                else
                    yield return new DbField(property.Name);
            }
        }

        /// <summary>
        /// Obtiene el campo de una llave primaria especificada en un tipo de dato.
        /// </summary>
        /// <param name="typeOfInstance">Tipo de dato que contiene una llave primaria.</param>
        /// <returns>El campo con llave primaria.</returns>
        public static DbField GetPrimaryKey(Type typeOfInstance)
        {
            return GetFields(typeOfInstance).First(field => field.IsPrimaryKey);
        }

        /// <summary>
        /// Obtiene el valor de la propiedad de la instancia especificada por el campo actual.
        /// </summary>
        /// <param name="instance">Instancia que contiene la propiedad.</param>
        /// <returns>El valor de la propiedad de la instancia.</returns>
        public Object GetValue(Object instance)
        {
            if (instance is null)
                throw new ArgumentNullException("La instancia no puede ser nula");

            Type type = instance.GetType();

            Boolean isTable = false;
            foreach (Attribute attribute in type.GetCustomAttributes())
                if (isTable = attribute is TableAttribute)
                    break;

            if (!isTable)
                throw new ArgumentException("La instancia debe tener el atributo 'TableAttribute'.");

            foreach (PropertyInfo property in type.GetProperties())
                if (property.GetCustomAttributes().Count() > 0)
                {
                    foreach (Attribute attribute in property.GetCustomAttributes())
                        if (attribute is ColumnAttribute)
                            if ((attribute as ColumnAttribute).Name == Name)
                                return property.GetValue(instance);
                }
                else if (property.Name == Name)
                    return property.GetValue(instance);

            throw new ArgumentException("El tipo de la instancia no tiene definido el campo actual.");
        }

        /// <summary>
        /// Establece el valor de la propiedad de la instancia especificada por el campo actual.
        /// </summary>
        /// <param name="instance">Instancia que contiene la propiedad.</param>
        /// <param name="value">Valor nuevo de la propiedad.</param>
        public void SetValue(Object instance, Object value)
        {
            if (instance is null)
                throw new ArgumentNullException("La instancia no puede ser nula");

            Type type = instance.GetType();

            Boolean isTable = false;
            foreach (Attribute attribute in type.GetCustomAttributes())
                if (isTable = attribute is TableAttribute)
                    break;

            if (!isTable)
                throw new ArgumentException("La instancia debe tener el atributo 'TableAttribute'.");

            foreach (PropertyInfo property in type.GetProperties())
                if (property.GetCustomAttributes().Count() > 0)
                {
                    foreach (Attribute attribute in property.GetCustomAttributes())
                        if (attribute is ColumnAttribute)
                            if ((attribute as ColumnAttribute).Name == Name)
                            {
                                property.SetValue(instance, value);
                                return;
                            }
                }
                else if (property.Name == Name)
                {
                    property.SetValue(instance, value);
                    return;
                }

            throw new ArgumentException("El tipo de la instancia no tiene definido el campo actual.");
        }
    }
}