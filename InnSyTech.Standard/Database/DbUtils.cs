using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace InnSyTech.Standard.Database
{
    /// <summary>
    /// Define funciones utilizadas por <see cref="DbCrud"/>.
    /// </summary>
    internal static class DbUtils
    {
        /// <summary>
        /// Obtiene el nombre de la entidad a manipular.
        /// </summary>
        /// <param name="typeOfInstance">Tipo de la instancia que es una entidad.</param>
        /// <returns>El nombre de la tabla en la base de datos.</returns>
        public static String GetEntityName(Type typeOfInstance)
        {
            if (!IsEntity(typeOfInstance))
                throw new ArgumentException("El tipo de la instancia debe tener especificado el atributo Entity");
            String name = null;
            foreach (Attribute attribute in typeOfInstance.GetCustomAttributes())
                if (attribute is EntityAttribute)
                    name = (attribute as EntityAttribute).TableName;

            return String.IsNullOrEmpty(name) ? typeOfInstance.Name : name;
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
        /// Traduce el resultado Sql en instancias que representan esta informacion.
        /// </summary>
        /// <param name="instaceType">Tipo de la instancia.</typeparam>
        /// <param name="reader">Lector de la base de datos.</param>
        /// <returns>Una instancia con la informacion persistida.</returns>
        public static Object ToInstance(Type instanceType, DbDataReader reader)
        {
            var primaryKey = DbField.GetPrimaryKey(instanceType);
            var dbFields = DbField.GetFields(instanceType).SkipWhile(field => field.IsPrimaryKey);

            var instance = Activator.CreateInstance(instanceType);

            if (!TrySetDbValue(primaryKey, instance, reader, primaryKey.Name))
                return null;

            foreach (var dbField in dbFields.OrderBy(field => field))
            {
                if (dbField.IsPrimaryKey) continue;
                if (dbField.IsForeignKey) continue;

                if (IsEntity(dbField.PropertyType))
                    continue;

                TrySetDbValue(dbField, instance, reader, dbField.Name);
            }

            return instance;
        }

        /// <summary>
        /// Convierte una instancia <see cref="WhereOperator"/> en una cadena Sql valida.
        /// </summary>
        /// <param name="@operator">Operador a convertir.</param>
        /// <returns>La cadena Sql que representa el operador.</returns>
        public static String ToSqlString(this WhereOperator @operator)
        {
            switch (@operator)
            {
                case WhereOperator.LESS_THAT:
                    return "<";

                case WhereOperator.GREAT_THAT:
                    return ">";

                case WhereOperator.EQUALS:
                    return "=";

                case WhereOperator.NO_EQUALS:
                    return "<>";

                case WhereOperator.LESS_AND_EQUALS:
                    return "<=";

                case WhereOperator.GREAT_AND_EQUALS:
                    return ">=";

                case WhereOperator.LIKE:
                    return "LIKE";

                case WhereOperator.IS:
                    return "IS";

                case WhereOperator.IN:
                    return "IN";

                default:
                    return "=";
            }
        }

        /// <summary>
        /// Intenta establece el valor obtenido de la base de datos en la propiedad de la instancia.
        /// </summary>
        /// <param name="dbField">Campo a settear desde la base de datos.</param>
        /// <param name="instance">Instancia a settear la propiedad.</param>
        /// <param name="reader">Lector de la base de datos.</param>
        /// <param name="fieldName">Nombre del campo de la base de datos.</param>
        /// <returns>Un valor <see cref="true"/> en caso de establecer el valor correctamente.</returns>
        private static bool TrySetDbValue(DbField dbField, Object instance, DbDataReader reader, String fieldName)
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
                dbField.SetValue(instance, dbValue);
                return false;
            }
        }
    }
}