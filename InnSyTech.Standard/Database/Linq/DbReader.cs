using InnSyTech.Standard.Database.Linq.DbDefinitions;
using InnSyTech.Standard.Database.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace InnSyTech.Standard.Database.Linq
{
    /// <summary>
    /// Proporciona métodos que permite interpretar la respuesta desde la base de datos y obtener la secuencia resultante.
    /// </summary>
    internal static class DbReader
    {
        /// <summary>
        /// Procesa el lector de secuencia de datos y devuelve una secuencia según el resultado de la consulta.
        /// </summary>
        /// <param name="elementType">Tipo de dato de la secuencia.</param>
        /// <param name="reader">Lector de secuencia de la base de datos.</param>
        /// <param name="definition">Definición de la consulta Sql utilizada para la lectura.</param>
        /// <returns>Una secuencia de datos.</returns>
        public static object Process(Type elementType, DbDataReader reader, DbStatementDefinition definition)
        {
            var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IList;
            var readItem = 0;
            while (reader.Read())
            {
                object instance = DbHelper.ToInstance(definition.Entities.First().EntityType, reader, definition.Entities.First(), definition.ReferenceDepth);

                if (definition.Select != null)
                    instance = definition.Select.DynamicInvoke(instance);

                list.Add(instance);
                readItem++;

                if (definition.CountToTake != 0 && readItem == definition.CountToTake)
                    break;
            }

            reader?.Close();

            if (definition.CountToTake == 1 && !definition.IsEnumerable)
                if (list.Count >= definition.CountToTake)
                    return list[0];
                else return null;
            return list;
        }
    }
}