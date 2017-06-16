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
        private DbConnection _connection;

        private List<DbTransaction> _transactions;

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
        public bool Save<T>(T instance)
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
        public bool Update<T>(T instance)
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

                if (command.ExecuteNonQuery() == 0)
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
        public TResult GetObject<TResult>(Type typeOfInstance, Object idKey)
        {
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
        public ICollection<TResult> GetObjects<TResult>(Type typeOfInstance)
        {
            ICollection<TResult> objects = new List<TResult>();
            DbTransaction transaction = null;
            ValidateConnection();
            lock (_transactions)
            {
                try
                {
                    transaction = BeginTransaction();

                    ReadList<TResult>(typeOfInstance, objects as IList, transaction);

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

        private bool HasChildren(DbField field, out Type typeChildren)
        {
            if (!String.IsNullOrEmpty(field.ForeignKeyName)
                && (field.PropertyType as TypeInfo).ImplementedInterfaces.Contains(typeof(ICollection)))
            {
                typeChildren = field.PropertyType.GetGenericArguments().FirstOrDefault();
                return true;
            }
            typeChildren = null;
            return false;
        }

        /// <summary>
        /// Lee la información de una instancia que contenga la llave primaria especificada por el parametro.
        /// </summary>
        /// <param name="typeOfInstance">Tipo de la instancia a obtener.</param>
        /// <param name="idKey">Valor de la llave primaria de la instancia.</param>
        /// <param name="transaction">Instancia <see cref="DbTransaction"/> que administra la transacción actual.</param>
        /// <param name="dependenceInstance">Instancia dependiente.</param>
        /// <returns>Una instancia del tipo especificado.</returns>
        private TResult ReadData<TResult>(Type typeOfInstance, object idKey, DbTransaction transaction, Object dependenceInstance = null)
        {
            DbCommand command = _connection.CreateCommand();
            DbField primaryKeyField = DbField.GetPrimaryKey(typeOfInstance);
            command.Transaction = transaction;

            command.CommandText = String.Format("SELECT * FROM {0} WHERE {1}=@{1}",
                                                    GetTableName(typeOfInstance),
                                                    primaryKeyField.Name);

            CreateParameter(command, primaryKeyField.Name, idKey);

            IEnumerable<DbField> fields = DbField.GetFields(typeOfInstance);
            DbDataReader reader = null;
            TResult data = default(TResult);
            try
            {
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    data = (TResult)Activator.CreateInstance(typeOfInstance);
                    primaryKeyField.SetValue(data, reader[primaryKeyField.Name]);
                    foreach (DbField field in fields)
                    {
                        if (field.IsPrimaryKey) continue;

                        if (HasChildren(field, out Type typeChildren))
                            ReadList<Object>(typeChildren, (field.GetValue(data) as IList), transaction, data, field.ForeignKeyName);
                        else
                        {
                            if (reader.IsDBNull(reader.GetOrdinal(field.Name)))
                                continue;

                            if (reader.GetFieldType(reader.GetOrdinal(field.Name)) == typeof(DateTime)
                                || reader.GetFieldType(reader.GetOrdinal(field.Name)) == typeof(TimeSpan))
                                if (String.IsNullOrEmpty(reader.GetString(reader.GetOrdinal(field.Name)).Trim()))
                                    continue;

                            Object dbFieldValue = reader[field.Name];

                            if (!field.IsForeignKey)
                                field.SetValue(data, dbFieldValue);
                            else if (dependenceInstance != null && field.PropertyType == dependenceInstance.GetType())
                            {
                                DbField dependencePrimaryKeyField = DbField.GetPrimaryKey(dependenceInstance.GetType());
                                var valueFromDb = Convert.ChangeType(dbFieldValue, dependencePrimaryKeyField.PropertyType);
                                var key = dependencePrimaryKeyField.GetValue(dependenceInstance);

                                if (valueFromDb.Equals(key))
                                    field.SetValue(data, dependenceInstance);
                                else
                                    field.SetValue(data, ReadData<Object>(field.PropertyType, dbFieldValue, transaction, data));
                            }
                            else
                                field.SetValue(data, ReadData<Object>(field.PropertyType, dbFieldValue, transaction, data));
                        }
                    }
                }

                return (TResult)data;
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
        private void ReadList<T>(Type typeOfInstance, IList objects, DbTransaction transaction, Object dependenceInstance = null, String foreignKeyName = "")
        {
            DbField dependencePrimaryKeyField = dependenceInstance is null ? null : DbField.GetPrimaryKey(dependenceInstance.GetType());
            DbCommand command = _connection.CreateCommand();

            command.Transaction = transaction;
            if (dependenceInstance is null)
                command.CommandText = String.Format("SELECT * FROM {0}", GetTableName(typeOfInstance));
            else
            {
                command.CommandText = String.Format("SELECT * FROM {0} WHERE {1}=@ForeignKey", GetTableName(typeOfInstance), foreignKeyName);
                CreateParameter(command, "ForeignKey", dependencePrimaryKeyField.GetValue(dependenceInstance));
            }

            DbDataReader reader = null;
            try
            {
                reader = command.ExecuteReader();

                T data = default(T);
                IEnumerable<DbField> fields = DbField.GetFields(typeOfInstance);

                while (reader.Read())
                {
                    data = (T)Activator.CreateInstance(typeOfInstance);

                    DbField primaryKeyField = DbField.GetPrimaryKey(typeOfInstance);
                    primaryKeyField.SetValue(data, reader[primaryKeyField.Name]);

                    foreach (DbField field in fields)
                    {
                        if (field.IsPrimaryKey) continue;

                        if (HasChildren(field, out Type typeChildren))
                            ReadList<Object>(typeChildren, (field.GetValue(data) as IList), transaction, data, field.ForeignKeyName);
                        else
                        {
                            if (reader.IsDBNull(reader.GetOrdinal(field.Name)))
                                continue;

                            if (reader.GetFieldType(reader.GetOrdinal(field.Name)) == typeof(DateTime)
                                || reader.GetFieldType(reader.GetOrdinal(field.Name)) == typeof(TimeSpan))
                                if (String.IsNullOrEmpty(reader.GetString(reader.GetOrdinal(field.Name)).Trim()))
                                    continue;

                            Object dbFieldValue = reader[field.Name];

                            if (!field.IsForeignKey)
                                field.SetValue(data, dbFieldValue);
                            else if (dependenceInstance != null && field.PropertyType == dependenceInstance.GetType())
                            {
                                var valueFromDb = Convert.ChangeType(dbFieldValue, dependencePrimaryKeyField.PropertyType);
                                var key = dependencePrimaryKeyField.GetValue(dependenceInstance);

                                if (valueFromDb.Equals(key))
                                    field.SetValue(data, dependenceInstance);
                                else
                                    field.SetValue(data, ReadData<Object>(field.PropertyType, dbFieldValue, transaction, data));
                            }
                            else
                                field.SetValue(data, ReadData<Object>(field.PropertyType, dbFieldValue, transaction, data));
                        }
                    }

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
    }
}