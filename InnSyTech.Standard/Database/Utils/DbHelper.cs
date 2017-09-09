using InnSyTech.Standard.Database.Linq.DbDefinitions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace InnSyTech.Standard.Database.Utils
{
    /// <summary>
    /// Define funciones auxiliares utilizadas por las clases definidas en <see cref="Database"/>.
    /// </summary>
    internal static class DbHelper
    {
        /// <summary>
        /// Obtiene el nombre de la entidad a manipular.
        /// </summary>
        /// <param name="typeOfInstance">Tipo de la instancia que es una entidad.</param>
        /// <returns>El nombre de la tabla en la base de datos.</returns>
        public static String GetEntityName(Type typeOfInstance)
        {
            if (!IsEntity(typeOfInstance))
                return null;

            String name = null;
            foreach (Attribute attribute in typeOfInstance.GetCustomAttributes())
                if (attribute is EntityAttribute)
                    name = (attribute as EntityAttribute).TableName;

            return String.IsNullOrEmpty(name) ? typeOfInstance.Name : name;
        }

        /// <summary>
        /// Obtiene el nombre que maneja la base de datos para el miembro de la entidad especificada.
        /// </summary>
        /// <param name="member">Miembro a obtener el nombre.</param>
        /// <param name="entity">Entidad a la que se relaciona el miembro.</param>
        /// <returns>El nombre del miembro en la base de datos.</returns>
        public static string GetFieldName(MemberInfo member, Type entity = null)
        {
            bool isNotEntityMember = !IsEntity(member.ReflectedType);
            bool isNotEntity = (entity != null && !DbHelper.IsEntity(entity));
            if (isNotEntityMember && isNotEntity)
                throw new ArgumentException("El miembro especificado no pertenece a una entidad.");

            if (!isNotEntityMember)
                foreach (Attribute attribute in member.GetCustomAttributes())
                    if (attribute is ColumnAttribute)
                    {
                        ColumnAttribute columnAttribute = (attribute as ColumnAttribute);

                        if (columnAttribute.IsIgnored)
                            throw new InvalidOperationException("El atributo está señalado como ignorado.");

                        return String.IsNullOrEmpty(columnAttribute.Name)
                            ? member.Name
                            : columnAttribute.Name;
                    }

            if (entity != null && !isNotEntity)
                return GetFields(entity).SingleOrDefault(f => (f.PropertyInfo as MemberInfo).Name == member.Name)
                    ?.Name;

            return member.Name;
        }

        /// <summary>
        /// Obtiene todo los campos especificados de un tipo de dato.
        /// </summary>
        /// <param name="instanceType">Tipo de dato que contiene los campos.</param>
        /// <returns>Una enumaración de campos.</returns>
        public static IEnumerable<DbFieldInfo> GetFields(Type instanceType)
        {
            foreach (PropertyInfo property in instanceType.GetProperties())
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

                            yield return new DbFieldInfo(
                                name,
                                property,
                                columnAttribute.IsPrimaryKey,
                                columnAttribute.Converter,
                                columnAttribute.IsAutonumerical,
                                columnAttribute.IsForeignKey,
                                columnAttribute.ForeignKeyName);
                        }
                }
                else
                    yield return new DbFieldInfo(property.Name, property);
            }
        }

        /// <summary>
        /// Obtiene el campo de una llave primaria especificada en un tipo de dato.
        /// </summary>
        /// <param name="instanceType">Tipo de dato que contiene una llave primaria.</param>
        /// <returns>El campo con llave primaria.</returns>
        /// <exception cref="InvalidOperationException">El tipo no tiene llave primaria.</exception>
        public static DbFieldInfo GetPrimaryKey(Type instanceType)
        {
            try
            {
                return GetFields(instanceType).First(field => field.IsPrimaryKey);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"El tipo de la instancia {instanceType.FullName} no contiene llave primaria.", ex);
            }
        }

        /// <summary>
        /// Indica si la instancia es una entidad de base de datos.
        /// </summary>
        /// <param name="typeOfInstance">Tipo de la instancia a evaluar.</param>
        /// <returns>Un true si la instancia es una entidad.</returns>
        public static bool IsEntity(Type typeOfInstance)
        {
            foreach (var item in typeOfInstance.GetCustomAttributes())
                if (item is EntityAttribute)
                    return true;

            return false;
        }

        /// <summary>
        /// Indica si el campo es una llave foreanea de una entidad.
        /// </summary>
        /// <param name="member">Miembro a revisar.</param>
        /// <returns>Un true si es llave foranea.</returns>
        public static bool IsForeignKey(MemberInfo member)
        {
            foreach (Attribute attribute in member.GetCustomAttributes())
                if (attribute is ColumnAttribute)
                {
                    ColumnAttribute columnAttribute = (attribute as ColumnAttribute);

                    if (columnAttribute.IsIgnored)
                        throw new InvalidOperationException("El atributo está señalado como ignorado.");

                    return columnAttribute.IsForeignKey;
                }
            return false;
        }

        /// <summary>
        /// Traduce el resultado Sql en instancias que representan esta informacion.
        /// </summary>
        /// <param name="instaceType">Tipo de la instancia.</typeparam>
        /// <param name="reader">Lector de la base de datos.</param>
        /// <returns>Una instancia con la informacion persistida.</returns>
        public static object ToInstance(Type instanceType, DbDataReader reader, DbEntityDefinition definition, int depth)
        {
            if (depth < 0)
                return null;

            var primaryKey = GetPrimaryKey(instanceType);
            var dbFields = GetFields(instanceType).SkipWhile(field => field.IsPrimaryKey);

            var instance = Activator.CreateInstance(instanceType);

            if (!TrySetDbValue(primaryKey, instance, reader, String.Format("{0}_{1}", definition.Alias, primaryKey.Name)))
                return null;

            foreach (var dbField in dbFields.OrderBy(field => field))
            {
                if (dbField.IsPrimaryKey) continue;
                if (dbField.IsForeignKey) continue;

                if (IsEntity(dbField.PropertyType))
                    continue;

                TrySetDbValue(dbField, instance, reader, String.Format("{0}_{1}", definition.Alias, dbField.Name));
            }
            if (depth > 0)
                foreach (var foreignKey in dbFields.Where(f => f.IsForeignKey))
                    TrySetReferenceValue(
                        foreignKey,
                        instance,
                        reader,
                        definition.DependentsEntities
                            .Single(e => e.DependencyField.GetFieldName().Equals(foreignKey.Name) && e.EntityType == foreignKey.PropertyType),
                        depth - 1
                    );

            return instance;
        }

        /// <summary>
        /// Intenta establece el valor obtenido de la base de datos en la propiedad de la instancia.
        /// </summary>
        /// <param name="dbField">Campo a settear desde la base de datos.</param>
        /// <param name="instance">Instancia a setear la propiedad.</param>
        /// <param name="reader">Lector de la base de datos.</param>
        /// <param name="fieldName">Nombre del campo de la base de datos.</param>
        /// <returns>Un valor <see cref="true"/> en caso de establecer el valor correctamente.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// El resultado de la base de datos no tiene el campo especificado.
        /// </exception>
        private static bool TrySetDbValue(DbFieldInfo dbField, Object instance, DbDataReader reader, String fieldName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(fieldName);

                if (reader.IsDBNull(ordinal)) return false;

                object dbValue = reader[ordinal];

                dbField.SetValue(instance, dbValue);

                return true;
            }
            catch (IndexOutOfRangeException @in)
            {
                throw new IndexOutOfRangeException($"El resultado devuelto por la base de datos no cuenta con un campo: {fieldName}", @in);
            }
            catch
            {
                object dbValue = dbField.PropertyType.IsValueType ? Activator.CreateInstance(dbField.PropertyType) : null;
                if (dbField.PropertyInfo.SetMethod != null)
                    dbField.PropertyInfo.SetValue(instance, dbValue);
                return false;
            }
        }

        /// <summary>
        /// Intenta establecer el valor de la propiedad de referencia.
        /// </summary>
        /// <param name="foreignField">Campo de la llave foranea.</param>
        /// <param name="instance">Instancia a setear la propiedad.</param>
        /// <param name="reader">Lector de la base de datos.</param>
        /// <param name="definition">Definición de la entidad.</param>
        /// <param name="depth">Profundidad de las referencias a cargar.</param>
        private static void TrySetReferenceValue(DbFieldInfo foreignField, object instance, DbDataReader reader, DbEntityDefinition definition, int depth)
        {
            object reference = ToInstance(foreignField.PropertyType, reader, definition, depth);
            foreignField.SetValue(instance, reference);
        }
    }
}