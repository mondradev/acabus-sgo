using InnSyTech.Standard.Structures.Trees;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace InnSyTech.Standard.Database
{
    public sealed class DbSession
    {
        private static Dictionary<Type, IList> _cache;
        private DbConnection _connection;

        private List<DbTransaction> _transactions;

        static DbSession()
        {
            _cache = new Dictionary<Type, IList>();
        }

        internal DbSession(DbConnection connection)
        {
            if (connection is null)
                throw new ArgumentNullException("El parametro 'connection' no puede ser un valor nulo.");

            _connection = connection;

            _transactions = new List<DbTransaction>();
        }

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
        /// Crea un parametro la sentencia actual en el comando especificado.
        /// </summary>
        /// <param name="command">Comando que contiene la sentencia a ejecutar.</param>
        /// <param name="parameterName">Nombre del parametro.</param>
        /// <param name="parameterValue">Valor del parametro.</param>
        private void CreateParameter(DbCommand command, string parameterName, object parameterValue)
        {
            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = String.Format("@{0}", parameterName);
            parameter.Value = parameterValue;

            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// Crea los parametros especificados por la instancia pasada como argumento de la función.
        /// </summary>
        /// <param name="command">Comando que contiene la sentencia a ejecutar.</param>
        /// <param name="instance">Instancia de la cual se extraerán los parametros.</param>
        private void CreateParameters(DbCommand command, object instance)
        {
            var matches = Regex.Matches(command.CommandText, "@[0-9a-zA-Z_]*");
            foreach (Match match in matches)
            {
                var dbField = DbField.GetFields(instance.GetType()).FirstOrDefault(field => field.Name == match.Value.Replace("@", ""));
                if (dbField.IsForeignKey) continue;
                var parameter = command.CreateParameter();
                parameter.ParameterName = match.Value;
                parameter.Value = dbField.GetValue(instance);

                command.Parameters.Add(parameter);
            }
        }

        /// <summary>
        /// Indica si una instancia existe en la base de datos.
        /// </summary>
        /// <typeparam name="T">Tipo de la instancia.</typeparam>
        /// <param name="instance">Una instancia a validar.</param>
        /// <returns>Un valor <see cref="true"/> si la instancia existe.</returns>
        private bool ExistsInstance<T>(T instance, DbTransaction transaction)
        {
            DbCommand command = null;
            try
            {
                DbField primaryKey = DbField.GetPrimaryKey(instance.GetType());
                command = _connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = $"SELECT COUNT(*) FROM {GetTableName(instance.GetType())} WHERE {primaryKey.Name}=@{primaryKey.Name}";
                CreateParameter(command, primaryKey.Name, primaryKey.GetValue(instance));

                if (primaryKey.GetValue(instance) is null)
                    return false;

                return (Int64)command.ExecuteScalar() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocurrió un error al consultar si la instancia {instance.GetType()} existe.", ex);
            }
            finally
            {
                if (command != null)
                    command.Dispose();
            }
        }

        /// <summary>
        /// Obtiene el nombre de la entidad a manipular.
        /// </summary>
        /// <param name="typeOfInstance">Tipo de la instancia que es una entidad.</param>
        /// <returns>El nombre de la tabla en la base de datos.</returns>
        private String GetTableName(Type typeOfInstance)
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
        /// Indica si la instancia depende de otra.
        /// </summary>
        /// <param name="type">Tipo de la instancia.</param>
        /// <returns>Un valor <see cref="true"/> si la instancia depende de otra.</returns>
        private bool HasParentEntity(Type type)
        {
            var fields = DbField.GetFields(type).Where(field => field.IsForeignKey);
            return fields.Count() > 0;
        }

        /// <summary>
        /// Indica si la instancia es una entidad de base de datos.
        /// </summary>
        /// <param name="typeOfInstance">Tipo de la instancia a evaluar.</param>
        /// <returns>Un valor <see cref="true"/> si la instancia es una entidad.</returns>
        private bool IsEntity(Type typeOfInstance)
        {
            foreach (var item in typeOfInstance.GetCustomAttributes())
                if (item is EntityAttribute)
                    return true;

            return false;
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

        #region SaveDataUnidirectional

        /// <summary>
        /// Permite escribir los valores de una instancia en una tupla de una tabla en base de datos.
        /// </summary>
        /// <typeparam name="T">Tipo de la instancia a trasladar a una base de datos.</typeparam>
        /// <param name="instance">Instancia que deseamos escribir en la base de datos.</param>
        /// <returns>Un valor <see cref="true"/> si la instancia se guardó correctamente.</returns>
        public bool Save<T>(ref T instance)
        {
            DbTransaction transaction = null;
            ValidateConnection();
            lock (_transactions)
            {
                try
                {
                    transaction = BeginTransaction();

                    SaveInstance(instance, transaction);

                    transaction.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("Error al realizar el guardado de la instancia: {0}; Mensaje: {1}",
                        instance.ToString(), ex.Message), "ERROR");
                    if (transaction != null)
                        transaction.Rollback();

                    return false;
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
        /// Crea una cadena con la sentencia adecuada para guardar la instancia.
        /// </summary>
        /// <param name="instance">Instancia a guardar.</param>
        /// <param name="typeOfInstance">Tipo de la instancia a guardar.</param>
        /// <returns>Una sentencia SQL para realizar la inserción de la instancia en la base de datos.</returns>
        private string CreateSaveStatement(Object instance, Type typeOfInstance)
        {
            if (!IsEntity(typeOfInstance))
                throw new ArgumentException("El tipo de la instancia debe tener el atributo que indica que es una Entidad.");

            StringBuilder statement = new StringBuilder();
            StringBuilder parameters = new StringBuilder();

            foreach (DbField field in DbField.GetFields(typeOfInstance))
            {
                if (!String.IsNullOrEmpty(field.ForeignKeyName))
                    continue;

                object defaultValue = field.PropertyType.IsValueType
                                                        ? Activator.CreateInstance(field.PropertyType)
                                                        : null;
                object propertyValue = field.GetValue(instance);

                if (field.IsPrimaryKey && field.IsAutonumerical
                    && propertyValue.Equals(defaultValue))
                    continue;
                statement.AppendFormat("{0},", field.Name);
                parameters.AppendFormat("@{0},", field.Name);
            }

            statement.Remove(statement.Length - 1, 1);
            parameters.Remove(parameters.Length - 1, 1);

            String saveStatement = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", GetTableName(typeOfInstance), statement, parameters);

            statement.Clear();
            parameters.Clear();

            return saveStatement;
        }

        /// <summary>
        /// Permite guardar la instancia en la base de datos de manera recursiva incluyendo todas las
        /// instancias de las cuales esta es dependiente. Por último devuelve el valor de la llave primaria.
        /// </summary>
        /// <typeparam name="T">Tipo de la instancia a guardar.</typeparam>
        /// <param name="instance">Instancia a guardar.</param>
        /// <param name="transaction">
        /// Instancia de <see cref="DbTransaction"/> que administra la transacción actual.
        /// </param>
        /// <param name="childInstance">Instancia dependiente de la instancia a guardar.</param>
        /// <returns>El valor de la llave primaria de la instancia.</returns>
        private Object SaveInstance<T>(T instance, DbTransaction transaction, object childInstance = null)
        {
            DbCommand command = null;

            if (instance?.GetType() is null)
                return null;

            try
            {
                if (ExistsInstance(instance, transaction))
                    return DbField.GetPrimaryKey(instance.GetType()).GetValue(instance);

                command = _connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = CreateSaveStatement(instance, instance.GetType());
                if (HasParentEntity(instance.GetType()))
                    foreach (var item in DbField.GetFields(instance.GetType()).Where(field => field.IsForeignKey))
                        CreateParameter(command, item.Name, SaveInstance(item.GetValue(instance), transaction, instance));
                CreateParameters(command, instance);
                Trace.WriteLine($"Ejecutando SQL: {command.CommandText}", "DEBUG");

                int rows = command.ExecuteNonQuery();
                if (!DbField.GetPrimaryKey(instance.GetType()).IsAutonumerical)
                    return DbField.GetPrimaryKey(instance.GetType()).GetValue(instance);
                command.Parameters.Clear();
                command.CommandText = String.Format("SELECT {0}()", Configuration.LastInsertFunctionName);
                Trace.WriteLine($"Ejecutando SQL: {command.CommandText}", "DEBUG");

                DbField.GetPrimaryKey(instance.GetType()).SetValue(instance, command.ExecuteScalar());
                return DbField.GetPrimaryKey(instance.GetType()).GetValue(instance);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar la instancia: {instance.GetType()}, Mensaje: {ex.Message}", ex);
            }
            finally
            {
                if (command != null)
                    command.Dispose();
            }
        }

        #endregion SaveDataUnidirectional

        #region UpdateUnidirectional

        /// <summary>
        /// Actualiza los valores de las propiedades de una instancia existente en la base de datos.
        /// </summary>
        /// <param name="instance">Instancia a actualizar.</param>
        /// <returns>Un valor <see cref="true"/> si la instancia fue actualizada correctamente.</returns>
        public bool Update<T>(ref T instance)
        {
            DbTransaction transaction = null;
            DbCommand command = null;
            ValidateConnection();
            lock (_transactions)
            {
                try
                {
                    transaction = BeginTransaction();

                    UpdateInstance(instance, transaction);

                    transaction.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("Error al realizar la actualzación de la instancia: {0}; Mensaje: {1}",
                        instance.ToString(), ex.Message), "ERROR");
                    if (transaction != null)
                        transaction.Rollback();

                    return false;
                }
                finally
                {
                    if (command != null)
                        command.Dispose();

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
        /// Crea la sentencia SQL necesaria para la actualización de la instancia.
        /// </summary>
        /// <param name="typeOfInstance">Tipo de la instancia a actualizar.</param>
        /// <returns>Un sentencia SQL para la actualización.</returns>
        private string CreateUpdateStatement(Type typeOfInstance)
        {
            StringBuilder parameters = new StringBuilder();

            foreach (DbField field in DbField.GetFields(typeOfInstance))
            {
                if (!String.IsNullOrEmpty(field.ForeignKeyName))
                    continue;
                if (field.IsPrimaryKey) continue;
                parameters.AppendFormat("{0}=@{0},", field.Name);
            }

            parameters.Remove(parameters.Length - 1, 1);

            String saveStatement = String.Format("UPDATE {0} SET {1} WHERE {2}=@{2}", GetTableName(typeOfInstance), parameters, DbField.GetPrimaryKey(typeOfInstance).Name);

            parameters.Clear();

            return saveStatement;
        }

        /// <summary>
        /// Actualiza una instancia de manera recursiva a todas las instancias de las cual depende la actual.
        /// </summary>
        /// <typeparam name="T">Tipo de la instancia.</typeparam>
        /// <param name="instance">Instancia a actualizar.</param>
        /// <param name="transaction">
        /// Instancia de <see cref="DbTransaction"/> que administra la transaccióna actual.
        /// </param>
        /// <param name="childInstance">Instancia pendiente de la actual.</param>
        /// <returns>El valor de la llave primaria de la instancia.</returns>
        private Object UpdateInstance<T>(T instance, DbTransaction transaction, Object childInstance = null)
        {
            DbCommand command = null;

            if (instance?.GetType() is null)
                return null;

            try
            {
                command = _connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = CreateUpdateStatement(instance.GetType());
                if (HasParentEntity(instance.GetType()))
                    foreach (var item in DbField.GetFields(instance.GetType()).Where(field => field.IsForeignKey))
                        CreateParameter(command, item.Name, UpdateInstance(item.GetValue(instance), transaction, instance));
                CreateParameters(command, instance);

                Trace.WriteLine($"Ejecutando SQL: {command.CommandText}", "DEBUG");

                int rows = command.ExecuteNonQuery();
                if (rows == 0)
                    return SaveInstance(instance, transaction, childInstance);

                return DbField.GetPrimaryKey(instance.GetType()).GetValue(instance);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar los valores de la instancia {instance.GetType()}, Mensaje: {ex.Message}", ex);
            }
            finally
            {
                if (command != null)
                    command.Dispose();
            }
        }

        #endregion UpdateUnidirectional

        #region DeleteUndirectional

        /// <summary>
        /// Borra una instancia almacenada en la base de datos.
        /// </summary>
        /// <param name="instance">Instancia a borrar.</param>
        /// <returns>Un valor verdadero si se borró la instancia correctamente.</returns>
        public bool Delete<T>(T instance)
        {
            DbTransaction transaction = null;
            DbCommand command = null;
            ValidateConnection();
            lock (_transactions)
            {
                try
                {
                    transaction = BeginTransaction();

                    DeleteInstance(instance, transaction);

                    transaction.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("Error al realizar el borrado de la instancia: {0}; Mensaje: {1}", instance.ToString(), ex.Message), "ERROR");
                    if (transaction != null)
                        transaction.Rollback();

                    return false;
                }
                finally
                {
                    if (command != null)
                        command.Dispose();

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
        /// Borra de manera recursiva una instancia y las instancias a las cuales es dependiente.
        /// </summary>
        /// <typeparam name="T">Tipo de la instancia a borrar.</typeparam>
        /// <param name="instance">Instancia a borrar.</param>
        /// <param name="transaction">Instancia <see cref="DbTransaction"/> que administra la transacción actual.</param>
        /// <param name="childInstance">Instancia dependiente de la actual.</param>
        /// <returns>Un valor <see cref="true"/> si el borrado es completado.</returns>
        private Boolean DeleteInstance<T>(T instance, DbTransaction transaction, Object childInstance = null)
        {
            DbCommand command = null;

            try
            {
                DbField primaryKey = DbField.GetPrimaryKey(instance.GetType());

                if (primaryKey is null)
                    throw new ArgumentException("La estructura de la instancia requiere una campo de llave primaria.");

                String tableName = GetTableName(instance.GetType());

                command = _connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = String.Format("DELETE FROM {0} WHERE {1}=@{1}", tableName, primaryKey.Name);
                CreateParameter(command, primaryKey.Name, primaryKey.GetValue(instance));

                if (HasParentEntity(instance.GetType()))
                    foreach (var item in DbField.GetFields(instance.GetType()).Where(field => field.IsForeignKey))
                        DeleteInstance(item.GetValue(instance), transaction, instance);

                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al intentar borrar la instancia {instance.GetType()}.", ex);
            }
            finally
            {
                if (command != null)
                    command.Dispose();
            }
        }

        #endregion DeleteUndirectional

        #region GetUnidirectional

        /// <summary>
        /// Obtiene la instancia que tenga asignada la llave primaria especificada.
        /// </summary>
        /// <param name="typeOfInstance">Tipo de la instancia.</param>
        /// <param name="idKey">Valor de la llave primaria de la instancia.</param>
        /// <returns>Una instancia del tipo especificada.</returns>
        public TResult GetObject<TResult>(Object idKey)
        {
            if (_cache.ContainsKey(typeof(TResult)))
            {
                var instance = default(TResult);
                var primaryKey = DbField.GetPrimaryKey(typeof(TResult));
                Boolean exists = false;
                foreach (var item in _cache[typeof(TResult)])

                    if (exists = primaryKey.GetValue(item).Equals(idKey))
                    {
                        instance = (TResult)item;
                        break;
                    }
                if (exists)
                    _cache[typeof(TResult)].Remove(instance);
            }

            Type typeOfInstance = typeof(TResult);
            DbTransaction transaction = null;
            ValidateConnection();
            lock (_transactions)
            {
                try
                {
                    transaction = BeginTransaction();
                    TResult data = ReadData<TResult>(typeOfInstance, idKey, transaction);
                    transaction.Commit();

                    return data;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("Error al realizar la extracción de datos: {0}; Mensaje: {1}",
                        typeOfInstance.Name, ex.Message), "ERROR");

                    if (transaction != null)
                        transaction.Rollback();

                    return default(TResult);
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
        /// Obtiene una colección de elementos del tipo especificado.
        /// </summary>
        /// <param name="typeOfInstance">Tipo a devolver de la base de datos.</param>
        /// <returns>Una colección de instancias del tipo especificado.</returns>
        public ICollection<TResult> GetObjects<TResult>(DbFilter filter = null)
        {
            if (_cache.ContainsKey(typeof(TResult))) _cache[typeof(TResult)].Clear();

            Type typeOfInstance = typeof(TResult);
            ICollection<TResult> objects = new List<TResult>();
            DbTransaction transaction = null;
            ValidateConnection();
            lock (_transactions)
            {
                try
                {
                    transaction = BeginTransaction();

                    ReadList<TResult>(typeOfInstance, objects as IList, transaction, filter);

                    transaction.Commit();

                    return objects;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("Error al realizar la extracción multiple de datos: {0}; Mensaje: {1}",
                        typeOfInstance.Name, ex.Message), "ERROR");

                    if (transaction != null)
                        transaction.Rollback();

                    return objects;
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
        /// Convierte el filtro en una cadena valida para una sentencia Sql.
        /// </summary>
        /// <param name="filter">Filtro a aplicar a la sentencia.</param>
        /// <param name="alias">Alias de entidad a aplicar el filtro.</param>
        /// <returns>Una cadena valida para una sentencia Sql.</returns>
        private String FilterToString(DbFilter filter, String alias)
        {
            StringBuilder filtersSql = new StringBuilder();
            var filters = filter.GetFilters();
            foreach (var f in filters)
            {
                object value = f.Expression.Value;
                string parameterName = f.Expression.PropertyName.Replace('.', '_');
                var count = Regex.Matches(filtersSql.ToString(), String.Format("@{0}", parameterName)).Count;
                if (count > 0)
                    parameterName += count;
                filtersSql.AppendFormat("{2} {3}{0} {4} {1} ",
                    f.Expression.PropertyName,
                    f.Expression.Operator == WhereOperator.IN ? f.Expression.Value : "@" + parameterName,
                    f.Type,
                    f.Expression.PropertyName.Contains('.') ? string.Empty : alias + ".",
                    OperatorToString(f.Expression.Operator));
            }

            if (filters.Count < 1)
                return "";

            var lenght = filters[0].Type.ToString().Length;
            return filtersSql.ToString().Substring(lenght);
        }

        /// <summary>
        /// Determina si el campo es una colección de entidades dependientes.
        /// </summary>
        /// <param name="field">Campo a evaluar.</param>
        /// <param name="typeChildren">Tipo de las entidades dependientes.</param>
        /// <returns>Un valor <see cref="true"/> si es una colección de entidades dependientes.</returns>
        private bool HasChildren(DbField field, out Type typeChildren)
        {
            if (!String.IsNullOrEmpty(field.ForeignKeyName)
                && ((field.PropertyType as TypeInfo).ImplementedInterfaces.Contains(typeof(ICollection))
                || (field.PropertyType as TypeInfo).ImplementedInterfaces.Contains(typeof(IEnumerable))))
            {
                typeChildren = field.PropertyType.GetGenericArguments().FirstOrDefault();
                return true;
            }
            typeChildren = null;
            return false;
        }

        /// <summary>
        /// Convierte la instancia <see cref="WhereOperator"/> en el operador lógico Sql.
        /// </summary>
        /// <param name="@operator">Instancia a convertir en cadena.</param>
        /// <returns>La cadena que representa el operador lógico Sql.</returns>
        private String OperatorToString(WhereOperator @operator)
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
        /// Lee la información de una instancia que contenga la llave primaria especificada por el parametro.
        /// </summary>
        /// <param name="typeOfInstance">Tipo de la instancia a obtener.</param>
        /// <param name="idKey">Valor de la llave primaria de la instancia.</param>
        /// <param name="transaction">Instancia <see cref="DbTransaction"/> que administra la transacción actual.</param>
        /// <param name="dependenceInstance">Instancia dependiente.</param>
        /// <returns>Una instancia del tipo especificado.</returns>
        private TResult ReadData<TResult>(Type typeOfInstance, object idKey, DbTransaction transaction, Object dependenceInstance = null, String foreignKeyName = "")
        {
            DbField primaryKeyField = DbField.GetPrimaryKey(typeOfInstance);

            if (_cache.ContainsKey(typeOfInstance) && dependenceInstance is null)
                if ((_cache[typeOfInstance] as List<TResult>).FirstOrDefault(item
                    => primaryKeyField.GetValue(item).Equals(idKey)) != null)
                    return (_cache[typeOfInstance] as List<TResult>).FirstOrDefault(item
                        => primaryKeyField.GetValue(item).Equals(idKey));

            DbField dependencePrimaryKeyField = dependenceInstance is null ? null : DbField.GetPrimaryKey(dependenceInstance.GetType());
            DbCommand command = _connection.CreateCommand();

            command.Transaction = transaction;

            StringBuilder tablesNames = new StringBuilder();
            Tree<Tuple<Type, String, ParsedInstance>> treeControl = null;
            String commandText = CreateStatement(typeOfInstance, ref treeControl, out String alias, tablesNames);
            String dependenceStatement = String.Format("WHERE {0}.{1}=@Key",
                treeControl.Value.Item2,
                String.IsNullOrEmpty(foreignKeyName) ? primaryKeyField.Name : foreignKeyName);

            command.CommandText = String.Format("SELECT {0} FROM {1} {2}", commandText, tablesNames.ToString(),
                dependenceStatement);

            CreateParameter(command, "Key", idKey);

            IEnumerable<DbField> fields = DbField.GetFields(typeOfInstance);
            DbDataReader reader = null;
            try
            {
                reader = command.ExecuteReader();

                while (reader.Read())
                    return ToInstance<TResult>(typeOfInstance, reader, treeControl, transaction);

                return default(TResult);
            }
            finally
            {
                if (reader != null)
                    reader.Close();

                if (command != null)
                    command.Dispose();
            }
        }

        /// <summary>
        /// Obtiene una lista de las instancias especificadas por el parametro.
        /// </summary>
        /// <param name="typeOfInstance"></param>
        /// <param name="objects"></param>
        /// <param name="transaction"></param>
        /// <param name="dependenceInstance"></param>
        private void ReadList<T>(Type typeOfInstance, IList objects, DbTransaction transaction, DbFilter filter, Object dependenceInstance = null, String foreignKeyName = "")
        {
            DbField dependencePrimaryKeyField = dependenceInstance is null ? null : DbField.GetPrimaryKey(dependenceInstance.GetType());

            DbCommand command = _connection.CreateCommand();

            command.Transaction = transaction;

            StringBuilder tablesNames = new StringBuilder();
            Tree<Tuple<Type, String, ParsedInstance>> treeControl = null;
            String commandText = CreateStatement(typeOfInstance, ref treeControl, out String alias, tablesNames);
            String dependenceStatement = !String.IsNullOrEmpty(foreignKeyName)
                ? String.Format("WHERE {0}.{1}=@ForeignKey", treeControl.Value.Item2, foreignKeyName)
                : String.Empty;

            if (filter != null && filter.GetFilters().Count > 0)
                if (String.IsNullOrEmpty(dependenceStatement))
                    dependenceStatement = String.Format("WHERE {0}", FilterToString(filter, treeControl.Value.Item2));
                else
                    dependenceStatement += String.Format(" AND {0}", FilterToString(filter, treeControl.Value.Item2));

            command.CommandText = String.Format("SELECT {0} FROM {1} {2}", commandText, tablesNames.ToString(),
                dependenceStatement);
            if (dependencePrimaryKeyField != null)
                CreateParameter(command, "ForeignKey", dependencePrimaryKeyField.GetValue(dependenceInstance));
            if (filter != null && filter.GetFilters().Count > 0)
                foreach (var item in filter.GetFilters())
                {
                    if (item.Expression.Operator == WhereOperator.IN)
                        continue;

                    var parameterName = item.Expression.PropertyName.Replace('.', '_');
                    int count = (command.Parameters as IEnumerable).Cast<DbParameter>()
                        .Where(paramter => (paramter as DbParameter).ParameterName.Contains(String.Format("@{0}", parameterName))).Count();
                    if (count > 0)
                        parameterName += count;
                    CreateParameter(command, parameterName, item.Expression.Value);
                }

            DbDataReader reader = null;
            try
            {
                reader = command.ExecuteReader();

                T data = default(T);
                IEnumerable<DbField> fields = DbField.GetFields(typeOfInstance);

                while (reader.Read())
                {
                    foreach (var node in treeControl)
                        node.Value.Item3.Parsed = false;

                    data = ToInstance<T>(typeOfInstance, reader, treeControl, transaction);
                    objects.Add(data);
                }
            }
            finally
            {
                if (reader != null)
                    reader.Close();

                if (command != null)
                    command.Dispose();
            }
        }

        #endregion GetUnidirectional

        #region ReadData

        /// <summary>
        /// Traduce el resultado Sql en instancias que representan esta informacion.
        /// </summary>
        /// <typeparam name="T">Tipo de la instancia padre.</typeparam>
        /// <param name="type">Tipo de dato de la instancia padre.</param>
        /// <param name="reader">Lector de la base de datos.</param>
        /// <param name="parent">Arbol de jerarquía que lleva el control de la lectura de los datos.</param>
        /// <param name="transaction">Instancia de la transaccion con la cual se ha hecho la lectura.</param>
        /// <returns>Una instancia con la informacion persistida.</returns>
        internal T ToInstance<T>(Type type, DbDataReader reader, Tree<Tuple<Type, String, ParsedInstance>> parent, DbTransaction transaction = null)
        {
            var primaryKey = DbField.GetPrimaryKey(type);
            var dbFields = DbField.GetFields(type).SkipWhile(field => field.IsPrimaryKey);

            var instance = Activator.CreateInstance(type);
            var alias = parent.Value.Item2;
            parent.Value.Item3.Parsed = true;

            if (!TrySetDbValue(primaryKey, instance, reader, String.Format("{0}_{1}", alias, primaryKey.Name)))
                return default(T);

            if (_cache.ContainsKey(instance.GetType()))
            {
                Boolean exists = false;
                foreach (var item in _cache[instance.GetType()])
                    if (exists = primaryKey.GetValue(instance).Equals(primaryKey.GetValue(item)))
                        return (T)item;
                if (!exists) _cache[instance.GetType()].Add(instance);
            }
            else
                _cache.Add(instance.GetType(), new List<Object>() { instance });

            foreach (var dbField in dbFields.OrderBy(field => field))
            {
                if (dbField.IsPrimaryKey) continue;
                if (dbField.IsForeignKey)
                {
                    dbField.SetValue(instance, ToInstance<Object>(dbField.PropertyType, reader, parent.Children.FirstOrDefault(node => node.Value.Item1 == dbField.PropertyType && !node.Value.Item3.Parsed), transaction));
                }
                else if (String.IsNullOrEmpty(dbField.ForeignKeyName))
                {
                    string fieldName = String.Format("{0}_{1}", alias, dbField.Name);
                    TrySetDbValue(dbField, instance, reader, fieldName);
                }
                else if (HasChildren(dbField, out Type typeChildren) && transaction != null)
                {
                    var initT = DateTime.Now;
                    if (_cache.ContainsKey(typeChildren))
                    {
                        var foreignField = DbField.GetFields(typeChildren).FirstOrDefault(field => field.Name.Equals(dbField.ForeignKeyName));
                        if (foreignField != null)
                        {
                            List<object> source = (_cache[typeChildren] as List<Object>);
                            if (source != null)
                            {
                                var list = source.Where(item =>
                                {
                                    var foreignValue = foreignField.GetValue(item);
                                    if (foreignValue != null)
                                        return primaryKey.GetValue(foreignValue).Equals(primaryKey.GetValue(instance));
                                    return false;
                                });

                                if (list != null && list.Count() > 0)
                                {
                                    if (dbField.GetValue(instance) is ICollection<Object> objects)
                                    {
                                        foreach (var item in list)
                                            objects.Add(item);
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                    ReadList<Object>(typeChildren, (dbField.GetValue(instance) as IList), transaction, null, instance, dbField.ForeignKeyName);
                    Trace.WriteLine($"Read list {DateTime.Now - initT}: {typeChildren.FullName} --> {dbField.ForeignKeyName}:{primaryKey.GetValue(instance)}", "DEBUG");
                }
                else if (!String.IsNullOrEmpty(dbField.ForeignKeyName))
                    dbField.SetValue(instance, ReadData<Object>(dbField.PropertyType, primaryKey.GetValue(instance), transaction, instance, dbField.ForeignKeyName));
            }

            return (T)instance;
        }

        /// <summary>
        /// Crea un enunciado Sql valido para obtener todos los datos que representan al tipo de dato pasador por argumento.
        /// </summary>
        /// <param name="type">Tipo de datos a obtener de la base de datos.</param>
        /// <param name="parent">Arbol de jerarquia que lleva el control de la traduccion de los datos.</param>
        /// <param name="aliasType">Alias de la entidad.</param>
        /// <param name="tables">Sentencia de las entidades.</param>
        /// <returns></returns>
        private String CreateStatement(Type type, ref Tree<Tuple<Type, String, ParsedInstance>> parent, out String aliasType, StringBuilder tables = null)
        {
            if (!IsEntity(type))
            {
                aliasType = "";
                return "";
            }

            Tree<Tuple<Type, string, ParsedInstance>> tree;

            if (parent is null)
            {
                parent = new Tree<Tuple<Type, string, ParsedInstance>>(new Tuple<Type, string, ParsedInstance>(type, "T0", new ParsedInstance(false)));
                tree = parent;
            }
            else
            {
                tree = new Tree<Tuple<Type, string, ParsedInstance>>(new Tuple<Type, string, ParsedInstance>(type, "T" + (parent.Root.Descendants.Count() + 1), new ParsedInstance(false)));
                parent.Add(tree);
            }

            var dbFields = DbField.GetFields(type);
            StringBuilder fields = new StringBuilder();
            tables = tables ?? new StringBuilder();
            string alias = tree.First(child => child.Value.Item1 == type).Value.Item2;
            aliasType = alias;
            if (tree.Root == tree)
                tables.AppendFormat("{0} {1} ", GetTableName(type), alias);

            foreach (var dbField in dbFields.OrderBy(field => field))
            {
                if (dbField.IsForeignKey)
                {
                    StringBuilder childtable = new StringBuilder();
                    fields.AppendFormat("{0}, ", CreateStatement(dbField.PropertyType, ref tree, out String childAlias, childtable));
                    tables.AppendFormat("LEFT OUTER JOIN {0} {2} ON {2}.{1}={3}.{4} ",
                        GetTableName(dbField.PropertyType),
                        DbField.GetPrimaryKey(dbField.PropertyType).Name,
                        childAlias,
                        alias,
                        dbField.Name
                        ).Append(childtable.ToString());
                }
                else if (String.IsNullOrEmpty(dbField.ForeignKeyName))
                {
                    fields.AppendFormat("{0}.{1} {0}_{1}, ", alias, dbField.Name);
                }
            }
            return fields.Remove(fields.Length - 2, 1).ToString();
        }

        /// <summary>
        /// Intenta obtener el valor desde la base de datos.
        /// </summary>
        /// <param name="dbField">Campo a settear desde la base de datos.</param>
        /// <param name="instance">Instancia a settear la propiedad.</param>
        /// <param name="reader">Lector de la base de datos.</param>
        /// <param name="fieldName">Nombre del campo de la base de datos.</param>
        /// <returns>Un valor <see cref="true"/> en caso de establecer el valor correctamente.</returns>
        private bool TrySetDbValue(DbField dbField, Object instance, DbDataReader reader, string fieldName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(fieldName);
                if (reader.IsDBNull(ordinal)) return false;

                object dbValue;
                if (reader.GetFieldType(ordinal) == typeof(DateTime))
                {
                    var datetimeStr = reader.GetString(ordinal);
                    if (DateTime.TryParse(datetimeStr, out DateTime result))
                        dbValue = result;
                    else
                        dbValue = default(DateTime);
                }
                else
                    dbValue = reader[ordinal];

                dbField.SetValue(instance, dbValue);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Estructura auxiliar para la lectura correcta del arbol de jerarquía de control de datos.
        /// </summary>
        internal class ParsedInstance
        {
            /// <summary>
            /// Crea una nueva instancia de <see cref="ParsedInstance"/>.
            /// </summary>
            /// <param name="parsed">Indica si la instancia ya fue parseada.</param>
            public ParsedInstance(Boolean parsed)
                => Parsed = parsed;

            /// <summary>
            /// Obtiene o establece si la instancia ya fue parseada.
            /// </summary>
            public bool Parsed { get; set; }
        }

        #endregion NewImplementation

        #region Batch

        /// <summary>
        /// Ejecuta una consulta de lectura.
        /// </summary>
        /// <param name="query">Consulta a realizar.</param>
        /// <param name="header">Nombre de las columnas.</param>
        /// <returns>Una matriz con todos los datos obtenidos.</returns>
        public Object[][] ExecuteQuery(String query, out String[] header)
        {
            List<Object[]> responseData = new List<Object[]>();

            DbTransaction transaction = null;
            DbCommand command = _connection.CreateCommand();
            ValidateConnection();
            lock (_transactions)
            {
                try
                {
                    transaction = BeginTransaction();

                    command.Transaction = transaction;
                    command.CommandText = query;
                    command.CommandType = System.Data.CommandType.Text;

                    var response = command.ExecuteReader();
                    int i = 0;
                    header = new string[response.FieldCount];
                    for (int j = 0; j < response.FieldCount; j++)
                        header[j] = response.GetName(j);
                    while (response.Read())
                    {
                        responseData.Add(new Object[response.FieldCount]);
                        for (int j = 0; j < responseData[i].Length; j++)
                            try
                            {
                                responseData[i][j] = response[j];
                            }
                            catch (Exception)
                            {
                                responseData[i][j] = response.GetString(j);
                            }
                        i++;
                    }
                    response.Close();

                    transaction.Commit();

                    return responseData.ToArray();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("Error al realizar la consulta: {0}; Mensaje: {1}", query, ex.Message), "ERROR");

                    if (transaction != null)
                        transaction.Rollback();
                    header = null;
                    return responseData.ToArray();
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
        /// Ejecuta una consulta de lectura.
        /// </summary>
        /// <param name="query">Consulta a realizar.</param>
        /// <returns>Una matriz con todos los datos obtenidos.</returns>
        public Object[][] ExecuteQuery(String query)
            => ExecuteQuery(query, out String[] header);

        #endregion Batch
    }
}