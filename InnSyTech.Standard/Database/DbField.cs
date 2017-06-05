using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InnSyTech.Standard.Database
{
    /// <summary>
    /// Define una estructura que permite establecer como se manejan los datos correspondientes a un
    /// campo dentro de la base de datos.
    /// </summary>
    internal class DbField
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Converter'.
        /// </summary>
        private IDbConverter _converter;

        /// <summary>
        /// Campo que provee a la propiedad 'IsAutonumerical'.
        /// </summary>
        private Boolean _isAutonumerical;

        /// <summary>
        /// Campo que provee a la propiedad 'IsForeignKey'.
        /// </summary>
        private Boolean _isForeignKey;

        /// <summary>
        /// Campo que provee a la propiedad 'IsPrimaryKey'.
        /// </summary>
        private Boolean _isPrimaryKey;

        /// <summary>
        /// Campo que provee a la propiedad 'Name'.
        /// </summary>
        private String _name;

        /// <summary>
        /// Obtiene toda la información que representa la propiedad enlazada al campo de la base de datos.
        /// </summary>
        private PropertyInfo _propertyInfo;

        /// <summary>
        /// Crea una nueva instancia de un campo enlazado a una propiedad.
        /// </summary>
        /// <param name="name">Nombre del campo.</param>
        /// <param name="isPrimaryKey">Si es llave primaria.</param>
        /// <param name="converter">Convertidor de datos para el enlace.</param>
        public DbField(String name, PropertyInfo propertyInfo, Boolean isPrimaryKey = false, Type converter = null)
        {
            _name = name;
            _isPrimaryKey = isPrimaryKey;
            _propertyInfo = propertyInfo;
            _isAutonumerical = false;
            _isForeignKey = false;

            if (converter != null)
                _converter = (IDbConverter)Activator.CreateInstance(converter);
        }

        /// <summary>
        /// Obtiene el convertidor de datos para el campo en la base de datos.
        /// </summary>
        public IDbConverter Converter => _converter;

        /// <summary>
        /// Obtiene si el campo es autonumerico.
        /// </summary>
        public Boolean IsAutonumerical => _isAutonumerical;

        /// <summary>
        /// Obtiene si el campo es llave foranea en la base de datos.
        /// </summary>
        public Boolean IsForeignKey => _isForeignKey;

        /// <summary>
        /// Obtiene si el campo es llave primaria en la base de datos.
        /// </summary>
        public Boolean IsPrimaryKey => _isPrimaryKey;

        /// <summary>
        /// Obtiene el nombre del campo en la base de datos.
        /// </summary>
        public String Name => _name;

        /// <summary>
        /// Obtiene el tipo de la propiedad enlazada al campo.
        /// </summary>
        public Type PropertyType => _propertyInfo.PropertyType;

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

                            yield return new DbField(name, property, columnAttribute.IsPrimaryKey, columnAttribute.Converter)
                            {
                                _isForeignKey = columnAttribute.IsForeignKey,
                                _isAutonumerical = columnAttribute.IsAutonumerical
                            };
                        }
                }
                else
                    yield return new DbField(property.Name, property);
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
            if (Converter is null)
                return _propertyInfo.GetValue(instance);

            return Converter.ConverterToDbData(_propertyInfo.GetValue(instance));
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
            try
            {
                if (Converter is null)
                    _propertyInfo.SetValue(instance, value);
                else
                    _propertyInfo.SetValue(instance, Converter.ConverterFromDb(value));
            }
            catch { }
        }
    }
}