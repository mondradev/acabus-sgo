using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace InnSyTech.Standard.Database
{
    /// <summary>
    /// Permite realizar las diversas operaciones a la basde de datos, como crear, leer, actualizar y
    /// borrar datos.
    /// </summary>
    public sealed class DbCrud
    {
        /// <summary>
        /// Controla la conexión a la base de datos.
        /// </summary>
        private DbConnection _connection;

        /// <summary>
        /// Enlista todas las transacciones en ejecución del <see cref="DbCrud"/>.
        /// </summary>
        private List<DbTransaction> _transactions;

        /// <summary>
        /// Crea una nueva instancia de <see cref="DbCrud"/>.
        /// </summary>
        /// <param name="connection">Instancia de conexión a la base de datos.</param>
        public DbCrud(DbConnection connection)
        {
            if (connection is null)
                throw new ArgumentNullException("El parametro 'connection' no puede ser un valor nulo.");

            _connection = connection;

            _transactions = new List<DbTransaction>();
        }

        /// <summary>
        /// Obtiene o establece la configuración de comunicación a la base de datos.
        /// </summary>
        public IDbConfiguration Configuration { get; set; }

        #region CommonFunctions

        /// <summary>
        /// Crea una transacción si el limite de transacciones en la configuración lo permite.
        /// </summary>
        /// <returns>Una instancia <see cref="DbTransaction"/> que administra la transacción actual.</returns>
        private DbTransaction BeginTransaction()
        {
            if (_transactions.Count >
                        Configuration.TransactionPerConnection)
                Monitor.Wait(_transactions);

            if (!OpenConnection())
                throw new Exception("No se abrió la conexión a la base de datos.");

            DbTransaction transaction = _connection.BeginTransaction();
            _transactions.Add(transaction);

            return transaction;
        }

        /// <summary>
        /// Abre una conexión a la base de datos actual.
        /// </summary>
        /// <returns>Un valor <see cref="true"/> si la conexión se abrió correctamente.</returns>
        private bool OpenConnection()
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                _connection.Open();

            return _connection.State == System.Data.ConnectionState.Open;
        }

        /// <summary>
        /// Valida la conexión de la base de datos.
        /// </summary>
        private void ValidateConnection()
        {
            if (_connection is null)
                throw new InvalidOperationException(@"Requiere inicializarse el controlador de la base de datos,
                                        use InnSyTech.Standard.Database.DbManager.Initialize(...).");
        }

        #endregion CommonFunctions

        #region Operations

        /// <summary>
        /// Persiste una nueva instancia en la base de datos.
        /// </summary>
        /// <typeparam name="T">Tipo de la instancia a persistir.</typeparam>
        /// <param name="instanceToCreate">Instancia a persistir.</param>
        /// <returns>Un true en caso de que la operación se realice con exito.</returns>
        public bool Create<T>(T instanceToCreate)
        {
            try
            {
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Elimina una instancia persistida de la base de datos, en caso de tener un campo de
        /// existencia este es puesto en falso para ser ignorado por la lectura.
        /// </summary>
        /// <typeparam name="T">Tipo de la instancia a eliminar.</typeparam>
        /// <param name="instanceToDelete">Instancia a eliminar.</param>
        /// <returns>Un true en caso que la operación se realice correctamente.</returns>
        public bool Delete<T>(T instanceToDelete)
        {
            try
            {
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Realiza la lectura y obtiene una colección de datos del tipo especificado.
        /// </summary>
        /// <typeparam name="TResult">Tipo de dato de la colección.</typeparam>
        /// <param name="filter">Filtro a aplicar a la consulta.</param>
        /// <param name="loadReference">Carga las referencias cuando se realiza la consulta.</param>
        /// <returns>Una colección del tipo especificado.</returns>
        public List<TResult> Read<TResult>(DbFilter filter = null, bool loadReference = false)
        {
            lock (_transactions)
            {
                ValidateConnection();
                DbTransaction transaction = BeginTransaction();
                try
                {
                    var list = ReadInternal(typeof(TResult), filter, loadReference, transaction);

                    transaction.Commit();

                    return list.Cast<TResult>().ToList();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
                finally
                {
                    if (transaction != null)
                    {
                        _transactions.Remove(transaction);
                        transaction.Dispose();
                    }

                    if (_connection != null)
                        _connection.Close();

                    Monitor.Pulse(_transactions);
                }
            }
        }

        /// <summary>
        /// Obtiene una instancia persistida del tipo especificado que coincide con el valor de la llave primaría determinada por el argumento de la función.
        /// </summary>
        /// <typeparam name="TResult">Tipo de dato de la instancia a obtener.</typeparam>
        /// <param name="primaryKeyValue">Valor de la llave primaría.</param>
        /// <param name="loadReference">Carga las referencias cuando se realiza la consulta.</param>
        /// <returns>Una instancia del tipo especificado.</returns>
        public TResult Read<TResult>(object primaryKeyValue, bool loadReference = false)
        {
            DbField primaryKey = DbField.GetPrimaryKey(typeof(TResult));
            return Read<TResult>(new DbFilter()
                .AddWhere(new DbFilterExpression(primaryKey.Name, primaryKeyValue, WhereOperator.EQUALS)), loadReference)
                .FirstOrDefault();
        }

        /// <summary>
        /// Actualiza la información de las propiedades de la instancia especificada.
        /// </summary>
        /// <typeparam name="T">Tipo de la instancia a actualizar.</typeparam>
        /// <param name="instanceToUpdate">Instancia a actualizar.</param>
        /// <returns>Un true en caso de que la operación se realice correctamente.</returns>
        public bool Update<T>(T instanceToUpdate)
        {
            try
            {
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Realiza la lectura y obtiene una colección de datos del tipo especificado.
        /// </summary>
        /// <param name="instanceType"></param>
        /// <param name="filter">Filtro a aplicar a la consulta.</param>
        /// <param name="loadReference">Carga las referencias cuando se realiza la consulta.</param>
        /// <param name="transaction"></param>
        /// <returns>Una colección del tipo especificado.</returns>
        internal List<Object> ReadInternal(Type instanceType, DbFilter filter, bool loadReference, DbTransaction transaction)
        {
            DbCommand command = null;
            try
            {
                String statement = CreateReadStatemente(instanceType);
                String filterStatement = CreateFilterStatement(instanceType, filter);

                command = _connection.CreateCommand();
                command.CommandText = String.Format(@"{0}
                                                        {1}",
                                                    statement, filterStatement);

                if (filter != null)
                    foreach (var f in filter.GetFilters())
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = "@" + f.Expression.PropertyName;
                        parameter.Value = f.Expression.Value;
                        command.Parameters.Add(parameter);
                    }

                var reader = command.ExecuteReader();
                var list = new List<Object>();

                while (reader.Read())
                {
                    var instance = DbUtils.ToInstance(instanceType, reader);

                    if (loadReference)
                        foreach (var referenceField in DbField.GetFields(instanceType)
                            .Where(f => f.IsForeignKey || !String.IsNullOrEmpty(f.ForeignKeyName)))
                            LoadReference(instance, referenceField.Name, transaction);

                    list.Add(instance);
                }

                return list;
            }
            finally
            {
                if (command != null)
                    command.Dispose();
            }
        }

        /// <summary>
        /// Obtiene una instancia persistida del tipo especificado que coincide con el valor de la llave primaría determinada por el argumento de la función.
        /// </summary>
        /// <param name="instanceType">Tipo de dato de la instancia a obtener.</param>
        /// <param name="primaryKeyValue">Valor de la llave primaría.</param>
        /// <param name="loadReference">Carga las referencias cuando se realiza la consulta.</param>
        /// <param name="transaction">Transaction utilizada para la operación.</param>
        /// <returns>Una instancia del tipo especificado.</returns>
        internal Object ReadInternal(Type instanceType, object primaryKeyValue, Boolean loadReference, DbTransaction transaction)
        {
            DbField primaryKey = DbField.GetPrimaryKey(instanceType);
            return ReadInternal(instanceType, new DbFilter()
                .AddWhere(new DbFilterExpression(primaryKey.Name, primaryKeyValue, WhereOperator.EQUALS)), loadReference, transaction)
                .FirstOrDefault();
        }

        /// <summary>
        /// Crea una sentencia de filtros para aplicar al tipo especificado.
        /// </summary>
        /// <param name="type">Tipo a la instancia a filtrar.</param>
        /// <param name="filter">Filtro a aplicar.</param>
        /// <returns>Fragmento de la sentencia SQL que corresponde al filtro.</returns>
        private string CreateFilterStatement(Type type, DbFilter filter)
        {
            if (filter is null)
                return String.Empty;

            StringBuilder filtersSql = new StringBuilder();
            var filters = filter.GetFilters();
            foreach (var f in filters)
            {
                object value = f.Expression.Value;
                string parameterName = f.Expression.PropertyName.Replace('.', '_');
                var count = Regex.Matches(filtersSql.ToString(), String.Format("@{0}", parameterName)).Count;
                if (count > 0)
                    parameterName += count;
                filtersSql.AppendFormat("{2} {0} {3} {1} ",
                    f.Expression.PropertyName,
                    f.Expression.Operator == WhereOperator.IN ? f.Expression.Value : "@" + parameterName,
                    f.Type,
                    f.Expression.Operator.ToSqlString());
            }

            if (filters.Count < 1)
                return "";

            var lenght = filters[0].Type.ToString().Length;
            return "WHERE" + filtersSql.ToString().Substring(lenght);
        }

        /// <summary>
        /// Crea la sentencia de lectura de una instancia persistida.
        /// </summary>
        /// <param name="type">Tipo de la instancia persistida.</param>
        /// <returns>Una sentencia SQL que se utilizará para lectura de la instancia.</returns>
        private string CreateReadStatemente(Type type)
        {
            StringBuilder fields = new StringBuilder();
            IEnumerable<DbField> fieldsData = DbField.GetFields(type);

            if (!DbUtils.IsEntity(type))
                throw new ArgumentException("La instancia no es tipo entidad.");

            foreach (var field in fieldsData)
            {
                if (field.IsForeignKey) continue;
                if (!String.IsNullOrEmpty(field.ForeignKeyName)) continue;

                fields.AppendFormat("{0}, ", field.Name);
            }

            fields.Remove(fields.Length - 2, 2);

            return String.Format(@"SELECT {0}
                                    FROM {1}",
                                    fields.ToString(), DbUtils.GetEntityName(type));
        }

        #endregion Operations

        #region ExtraFunctions

        /// <summary>
        /// Carga las referencias de un campo de una instancia persistida.
        /// </summary>
        /// <typeparam name="T">Tipo de la instancia persistida.</typeparam>
        /// <param name="referenceInstance">Instancia persistida.</param>
        /// <param name="propertyName">Nombre de la propiedad con la referencia.</param>
        public void LoadReference<T>(T referenceInstance, String propertyName)
        {
            lock (_transactions)
            {
                ValidateConnection();
                DbTransaction transaction = BeginTransaction();
                try
                {
                    LoadReference(referenceInstance, propertyName, transaction);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
                finally
                {
                    if (transaction != null)
                    {
                        _transactions.Remove(transaction);
                        transaction.Dispose();
                    }

                    if (_connection != null)
                        _connection.Close();

                    Monitor.Pulse(_transactions);
                }
            }
        }

        /// <summary>
        /// Carga las referencias de un campo de una instancia persistida.
        /// </summary>
        /// <typeparam name="T">Tipo de la instancia persistida.</typeparam>
        /// <param name="referenceInstance">Instancia persistida.</param>
        /// <param name="propertyName">Nombre de la propiedad con la referencia.</param>
        /// <param name="transaction">Transaction utilizada para la operación.</param>
        internal void LoadReference<T>(T referenceInstance, String propertyName, DbTransaction transaction)
        {
            if (String.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException($"El nombre de la propiedad no puede ser nulo o una cadena vacía.");

            IEnumerable<DbField> fields = DbField.GetFields(referenceInstance.GetType());
            DbField propertyField = fields.FirstOrDefault(f => f.Name == propertyName || f.PropertyInfo.Name == propertyName);
            var type = propertyField.PropertyType;

            if (type is null && !IsCollection(type as TypeInfo))
                throw new ArgumentException($"La entidad '{DbUtils.GetEntityName(referenceInstance.GetType())}' no tiene la propiedad especificada: {propertyName}");

            /// Si es una referencia de uno a uno, cuando la instancia persistida está referenciada a otra.
            if (propertyField.IsForeignKey)
            {
                var command = _connection.CreateCommand();
                var primaryKey = fields.First(f => f.IsPrimaryKey);

                command.CommandText = String.Format("SELECT {0} FROM  {1} WHERE {2}=@Fk_Value",
                    propertyField.Name,
                    DbUtils.GetEntityName(referenceInstance.GetType()),
                    primaryKey.Name);

                var parameter = command.CreateParameter();

                parameter.ParameterName = "@Fk_Value";
                parameter.Value = primaryKey.GetValue(referenceInstance);
                command.Parameters.Add(parameter);

                var foreignValue = command.ExecuteScalar();
                object foreignInstance = ReadInternal(propertyField.PropertyType, foreignValue, false, transaction);

                propertyField.SetValue(referenceInstance, foreignInstance);
            }
            /// Si la instancia no está referenciada dentro de la base de datos.
            else if (!String.IsNullOrEmpty(propertyField.ForeignKeyName))
            {
                var primaryKeyValue = fields.FirstOrDefault(f => f.IsPrimaryKey)?.GetValue(referenceInstance);

                var propertyValue = propertyField.GetValue(referenceInstance);
                var propertyType = propertyField.PropertyType.IsGenericType
                    ? propertyField.PropertyType.GenericTypeArguments.FirstOrDefault()
                    : propertyField.PropertyType;

                object result = ReadInternal(propertyType, new DbFilter()
                            .AddWhere(new DbFilterExpression(propertyField.ForeignKeyName, primaryKeyValue, WhereOperator.EQUALS)), false, transaction);

                /// Si es una referencia de uno a varios.
                if (IsCollection(propertyField.PropertyType as TypeInfo))
                {
                    var collection = propertyField.GetValue(referenceInstance);

                    if (collection is null)
                        return;

                    MethodInfo addMethod = collection.GetType().GetMethod("Add");

                    if (addMethod is null)
                        throw new InvalidOperationException("La colección no cuenta con un método Add.");

                    foreach (var item in result as IEnumerable)
                        addMethod.Invoke(collection, new[] { item });
                }
                /// Si es una referencia de uno a uno.
                else
                {
                    foreach (var item in result as IEnumerable)
                        propertyField.SetValue(referenceInstance, item);
                }
            }
        }

        /// <summary>
        /// Versión no genérica de <see cref="LoadReference{T}(T, string)"/>.
        /// </summary>
        /// <param name="type">Tipo de la instancia persistida.</param>
        /// <param name="referenceInstance">Instancia persistida.</param>
        /// <param name="propertyName">Nombre de la propiedad con la referencia.</param>
        internal void LoadReferenceNoGeneric(Type type, object referenceInstance, String propertyName)
        {
            GetType()
                .GetMethod("LoadReference")
                .MakeGenericMethod(type)
                .Invoke(this, new object[] { referenceInstance, propertyName });
        }
        /// <summary>
        /// Indica si el tipo de dato es una instancia o implementa <see cref="ICollection"/> o <see cref="ICollection{T}"/>.
        /// </summary>
        /// <param name="type">Tipo de dato a evaluar.</param>
        /// <returns>Un true si es una colección.</returns>
        private static bool IsCollection(TypeInfo type)
        {
            Type iCollectionGenericType = typeof(ICollection<>);
            Type iCollectionType = typeof(ICollection);

            if (type.ImplementedInterfaces.Contains(iCollectionType))
                return true;

            foreach (var implementedInterface in type.ImplementedInterfaces)
                if (implementedInterface.IsGenericType && implementedInterface.GetGenericTypeDefinition() == iCollectionGenericType)
                    return true;

            if (type == iCollectionType)
                return true;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == iCollectionGenericType)
                return true;

            return false;
        }

        #endregion ExtraFunctions
    }
}