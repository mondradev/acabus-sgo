using InnSyTech.Standard.Database.Linq;
using InnSyTech.Standard.Database.Utils;
using InnSyTech.Standard.Utils;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace InnSyTech.Standard.Database
{
    /// <summary>
    /// Implementa <see cref="IDbSession"/> para proporcionar el acceso a la base de datos a través
    /// de las funciones de crear, leer, actualizar y eliminar.
    /// </summary>
    internal sealed class DbSession : IDbSession
    {
        /// <summary>
        /// Crea una nueva instancia de <see cref="DbSession"/>.
        /// </summary>
        /// <param name="dbConnection">La conexión a la base de datos.</param>
        /// <param name="dialect">El dialecto de comunicación de la base de datos.</param>
        public DbSession(DbConnection dbConnection, DbDialectBase dialect)
            => Provider = new DbProvider(dbConnection, dialect);

        /// <summary>
        /// Obtiene el proveedor de consultas para la base de datos.
        /// </summary>
        public DbProvider Provider { get; }

        /// <summary>
        /// Crea una instancia persistente a partir de un tipo definido que corresponde a una tabla
        /// en la base de datos. Esto equivale a un INSERT INTO de Sql.
        /// </summary>
        /// <typeparam name="TData">Tipo de dato de la instancia a persistir.</typeparam>
        /// <param name="instance">Instancia que se desea persistir.</param>
        /// <param name="referenceDepth">Especifica la profundidad de referencias a crear en caso de no existir.</param>
        /// <returns>Un true en caso que la instancia sea persistida correctamente.</returns>
        public bool Create<TData>(TData instance, int referenceDepth = 0)
        {
            lock (Provider)
            {
                DbTransaction transaction = Provider.BeginTransaction();
                DbCommand command = Provider.CreateCommand(transaction);

                try
                {
                    CreateInternal(instance, referenceDepth, command);

                    transaction.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                        transaction.Rollback();

                    Trace.WriteLine((ex.Message + ex.StackTrace).JoinLines(), "ERROR");

                    return false;
                }
                finally
                {
                    if (transaction != null)
                        Provider.EndTransaction(transaction);

                    if (command != null)
                        command.Dispose();

                    Provider.CloseConnection();
                }
            }
        }

        /// <summary>
        /// Elimina una instancia persistida en la base de datos, si esta cuenta con campo de estado
        /// para visibilidad, solo se cambia a su valor oculto para ser ignorada en las lecturas.
        /// Esto equivale a una DELETE FROM de Sql.
        /// </summary>
        /// <typeparam name="TData">Tipo de dato de la instancia a eliminar.</typeparam>
        /// <param name="instance">Instancia a eliminar.</param>
        /// <returns>Un true si la instancia fue borrada así como sus referencias de ser necesario.</returns>
        public bool Delete<TData>(TData instance)
        {
            lock (Provider)
            {
                DbTransaction transaction = Provider.BeginTransaction();
                DbCommand command = Provider.CreateCommand(transaction);

                try
                {
                    var tablename = DbHelper.GetEntityName(instance.GetType());
                    var primaryKey = DbHelper.GetPrimaryKey(instance.GetType());

                    var parameter = command.CreateParameter();

                    command.CommandText = String.Format("DELETE FROM {0} WHERE {1} = @key", tablename, primaryKey.Name);

                    parameter.ParameterName = "@key";
                    parameter.Value = primaryKey.GetValue(instance);

                    command.Parameters.Add(parameter);

                    Trace.WriteLine($"Ejecutando: {command.CommandText}", "DEBUG");

                    command.ExecuteNonQuery();

                    transaction.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                        transaction.Rollback();

                    Trace.WriteLine((ex.Message + ex.StackTrace).JoinLines(), "ERROR");

                    return false;
                }
                finally
                {
                    if (transaction != null)
                        Provider.EndTransaction(transaction);

                    if (command != null)
                        command.Dispose();

                    Provider.CloseConnection();
                }
            }
        }

        /// <summary>
        /// Carga los valores para una propiedad de tipo colección que representan una referencia externa a la entidad especificada.
        /// </summary>
        /// <typeparam name="TData">Tipo de la instancia a cargar su referencia.</typeparam>
        /// <param name="instance">Instancia persistida que tiene la referencia.</param>
        /// <param name="propertyName">Nombre de la propiedad.</param>
        /// <returns>Un true en caso de cargar correctamente la propiedad.</returns>
        public bool LoadRefences<TData>(TData instance, string propertyName)
        {
            try
            {
                var propertyCollection = instance.GetType().GetProperty(propertyName);
                Type typeInstance = TypeHelper.GetElementType(propertyCollection.PropertyType);

                if (!typeof(ICollection<>).MakeGenericType(typeInstance).IsAssignableFrom(propertyCollection.PropertyType))
                    throw new ArgumentException($"La propiedad especificada no es una colección: '{propertyName}'.");

                var field = DbHelper.GetField(propertyCollection);
                var primaryKey = DbHelper.GetPrimaryKey(instance.GetType());

                if (String.IsNullOrEmpty(field.ForeignKeyName))
                    throw new ArgumentException($"La propiedad no incluye el campo de referencia utilizado para la relación.");

                var collection = field.GetValue(instance);

                var foreignKey = DbHelper.GetFields(typeInstance)
                    .Where(f => f.IsForeignKey && f.Name == field.ForeignKeyName)
                    .FirstOrDefault();

                var collectionRead = GetType().GetMethod("Read")
                    .MakeGenericMethod(typeInstance)
                    .Invoke(this, null) as IQueryable;

                var parameter = Expression.Parameter(typeInstance, "d");
                var foreignProperty = Expression.MakeMemberAccess(parameter, foreignKey.PropertyInfo);
                var foreignID = Expression.MakeMemberAccess(foreignProperty, primaryKey.PropertyInfo);
                var primaryValue = Expression.Constant(primaryKey.GetValue(instance));
                var equals = Expression.Equal(foreignID, primaryValue);
                var lambda = Expression.Lambda(equals, parameter);

                var whereMethod = typeof(Queryable)
                     .GetMethods().First(m => m.Name.Equals(nameof(Queryable.Where)) && m.IsGenericMethod)
                     .MakeGenericMethod(typeInstance);

                collectionRead = whereMethod.Invoke(null, new object[] { collectionRead, lambda }) as IQueryable;

                var addMethod = propertyCollection.PropertyType.GetMethod("Add");

                foreach (var item in collectionRead)
                {
                    foreignKey.SetValue(item, instance);
                    addMethod.Invoke(collection, new[] { item });
                }

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.PrintMessage().JoinLines(), "ERROR");
                return false;
            }
        }

        /// <summary>
        /// Realiza una lectura de elementos del tipo especificado que corresponde a una tabla en la
        /// base de datos, gracias a la estructura <see cref="IQueryable{T}"/> se puede aplicar
        /// filtros y ordernamiento como otros métodos que se encuentren disponibles. Esto equivale a
        /// un SELECT FROM.
        /// </summary>
        /// <typeparam name="TResult">El tipo de dato de la lectura.</typeparam>
        /// <returns>Una consulta que extrae datos de la base de datos relacional.</returns>
        public IQueryable<TResult> Read<TResult>()
            => new DbQuery<TResult>(Provider);

        /// <summary>
        /// Actualiza los atributosde la instancia persistida en la base de datos. Esto equivale a un
        /// UPDATE de Sql. Si se requiere actualizar todas las referencias se necesita colocar el
        /// parametro <paramref name="cascade"/> en true.
        /// </summary>
        /// <typeparam name="TData">Tipo de datos de la instancia persistida.</typeparam>
        /// <param name="instance">Instancia persistida en la base de datos.</param>
        /// <param name="referenceDepth">
        /// Especifica la profundidad de referencias a actualizar en caso de no existir, se crearán.
        /// </param>
        /// <returns>
        /// Un true si la instancia fue correctamente actualizada así como sus referencias de ser necesario.
        /// </returns>
        public bool Update<TData>(TData instance, int referenceDepth = 0)
        {
            lock (Provider)
            {
                DbTransaction transaction = Provider.BeginTransaction();
                DbCommand command = Provider.CreateCommand(transaction);

                try
                {
                    UpdateInternal(instance, referenceDepth, command);

                    transaction.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                        transaction.Rollback();

                    Trace.WriteLine((ex.Message + ex.StackTrace).JoinLines(), "ERROR");

                    return false;
                }
                finally
                {
                    if (transaction != null)
                        Provider.EndTransaction(transaction);

                    if (command != null)
                        command.Dispose();

                    Provider.CloseConnection();
                }
            }
        }

        /// <summary>
        /// Permite persistir una instancia y sus referencias de manera recursiva.
        /// </summary>
        /// <typeparam name="TData">Tipo de la instancia a persistir</typeparam>
        /// <param name="instance">Instancia a persistir.</param>
        /// <param name="referenceDepth">profundidad de las referencias.</param>
        /// <param name="command">Comando utilizado para ejecución de consultas.</param>
        private void CreateInternal<TData>(TData instance, int referenceDepth, DbCommand command)
        {
            IEnumerable<DbFieldInfo> fields = DbHelper.GetFields(instance.GetType()).Where(f => String.IsNullOrEmpty(f.ForeignKeyName));
            var primaryKey = fields.FirstOrDefault(f => f.IsPrimaryKey);

            if (referenceDepth > 0)
                foreach (var foreignKey in fields.Where(f => f.IsForeignKey))
                    CreateInternal(foreignKey.GetValue(instance), referenceDepth - 1, command);

            if (primaryKey.IsAutonumerical && primaryKey.GetValue(instance)?.ToString() != "0")
                return;

            StringBuilder statement = new StringBuilder();

            if (!primaryKey.IsAutonumerical)
                statement.AppendFormat("INSERT INTO {0} ({1}) SELECT * FROM (SELECT {2}) WHERE @{3} NOT IN (SELECT {3} FROM {0})",
                    DbHelper.GetEntityName(instance.GetType()),
                    "{{fields}}",
                    "{{parametersAndFields}}",
                    primaryKey.Name);
            else
                statement.Append("INSERT INTO ")
                .Append(DbHelper.GetEntityName(instance.GetType()))
                .Append(" ({{fields}}) VALUES ({{parameters}})");

            command.Parameters.Clear();

            foreach (var field in fields)
            {
                if (field.IsAutonumerical && field.IsPrimaryKey)
                    continue;

                var parameter = command.CreateParameter();

                parameter.ParameterName = $"@{field.Name}";
                parameter.Value = field.IsForeignKey ? field.GetForeignValue(instance) : field.GetValue(instance);

                command.Parameters.Add(parameter);

                statement.Replace("{{parameters}}", String.Format("{0}, {1}", parameter.ParameterName, "{{parameters}}"));
                statement.Replace("{{fields}}", String.Format("{0}, {1}", field.Name, "{{fields}}"));
                statement.Replace("{{parametersAndFields}}", String.Format("@{0} AS {0}, {1}", field.Name, "{{parametersAndFields}}"));
            }

            statement.Replace(", {{fields}}", String.Empty);
            statement.Replace(", {{parameters}}", String.Empty);
            statement.Replace(", {{parametersAndFields}}", String.Empty);

            command.CommandText = statement.ToString();

            Trace.WriteLine($"Ejecutando: {command.CommandText}", "DEBUG");

            command.ExecuteNonQuery();

            if (!primaryKey.IsAutonumerical)
                return;

            command.CommandText = String.Format("SELECT {0}()", Provider.Dialect.LastInsertFunctionName);

            var idGenereted = command.ExecuteScalar();
            primaryKey.SetValue(instance, idGenereted);
        }

        /// <summary>
        /// Permite actualizar los valores de una instancia persistida, en caso de que alguna referencia no exista, esta será creada.
        /// </summary>
        /// <typeparam name="TData">Tipo de la instancia a actualizar.</typeparam>
        /// <param name="instance">Instancia a actualizar.</param>
        /// <param name="referenceDepth">Nivel de profundidad de las referencias a actualizar.</param>
        /// <param name="command">Comando utilizado para la ejecución de consultas.</param>
        private void UpdateInternal<TData>(TData instance, int referenceDepth, DbCommand command)
        {
            IEnumerable<DbFieldInfo> fields = DbHelper.GetFields(instance.GetType()).Where(f => String.IsNullOrEmpty(f.ForeignKeyName));
            var primaryKey = fields.FirstOrDefault(f => f.IsPrimaryKey);

            if (referenceDepth > 0)
                foreach (var foreignKey in fields.Where(f => f.IsForeignKey))
                    UpdateInternal(foreignKey.GetValue(instance), referenceDepth - 1, command);

            if (primaryKey.IsAutonumerical && primaryKey.GetValue(instance)?.ToString() == "0")
                CreateInternal(instance, 0, command);

            if (primaryKey.GetValue(instance) == null)
                throw new ArgumentNullException(nameof(instance), "La instancia actual no tiene llave primaria establecida con un valor, no puede ser actualizada.");

            StringBuilder statement = new StringBuilder();

            statement.AppendFormat("UPDATE {0} SET {1} WHERE {2}=@{2}",
                DbHelper.GetEntityName(instance.GetType()),
                "{{parametersAndFields}}",
                primaryKey.Name);

            command.Parameters.Clear();

            foreach (var field in fields)
            {
                var parameter = command.CreateParameter();

                parameter.ParameterName = $"@{field.Name}";
                parameter.Value = field.IsForeignKey ? field.GetForeignValue(instance) : field.GetValue(instance);

                command.Parameters.Add(parameter);
                statement.Replace("{{parametersAndFields}}", String.Format("{0} = @{0}, {1}", field.Name, "{{parametersAndFields}}"));
            }

            statement.Replace(", {{parametersAndFields}}", String.Empty);

            command.CommandText = statement.ToString();

            Trace.WriteLine($"Ejecutando: {command.CommandText}", "DEBUG");

            command.ExecuteNonQuery();
        }
    }
}