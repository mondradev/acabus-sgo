using System;
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

        public bool Delete(object instance)
        {
            DbTransaction transaction = null;
            DbCommand command = null;
            ValidateConnection();
            lock (_transactions)
            {
                try
                {
                    transaction = BeginTransaction();

                    DbField primaryKey = DbField.GetPrimaryKey(instance.GetType());

                    if (primaryKey is null)
                        throw new ArgumentException("La estructura de la instancia requiere una campo de llave primaria.");

                    String tableName = GetTableName(instance.GetType());

                    if (tableName is null)
                        throw new ArgumentException("La estructura requiere un nombre de la tabla en la base de datos.");

                    command = _connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandText = String.Format("DELETE FROM {0} WHERE {1}=@{1}", tableName, primaryKey.Name);
                    CreateParameter(command, primaryKey.Name, primaryKey.GetValue(instance));

                    int rows = command.ExecuteNonQuery();

                    transaction.Commit();

                    return rows > 0;
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

        public object GetObject(Type typeOfInstance, Object idKey)
        {
            DbTransaction transaction = null;
            DbCommand command = null;
            DbDataReader reader = null;
            ValidateConnection();
            lock (_transactions)
            {
                try
                {
                    transaction = BeginTransaction();

                    command = _connection.CreateCommand();
                    command.Transaction = transaction;

                    command.CommandText = String.Format("SELECT * FROM {0} T1 WHERE {1}=@{1}", GetTableName(typeOfInstance), DbField.GetPrimaryKey(typeOfInstance).Name);
                    CreateParameter(command, DbField.GetPrimaryKey(typeOfInstance).Name, idKey);

                    reader = command.ExecuteReader();

                    Object data = null;
                    IEnumerable<DbField> fields = DbField.GetFields(typeOfInstance);

                    while (reader.Read())
                    {
                        if (data != null)
                            throw new InvalidOperationException("La consulta devolvió más de un elemento");

                        data = Activator.CreateInstance(typeOfInstance);
                        foreach (DbField field in fields)
                            try { field.SetValue(data, reader[field.Name]); } catch { }
                    }

                    reader.Close();

                    transaction.Commit();

                    return data;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("Error al realizar la extracción de datos: {0}; Mensaje: {1}", typeOfInstance.Name, ex.Message), "ERROR");

                    if (reader != null)
                        reader.Close();

                    if (transaction != null)
                        transaction.Rollback();

                    return null;
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

        public IEnumerable<object> GetObjects(Type typeOfInstance)
        {
            ICollection<object> objects = new List<object>();
            DbTransaction transaction = null;
            DbCommand command = null;
            DbDataReader reader = null;
            ValidateConnection();
            lock (_transactions)
            {
                try
                {
                    transaction = BeginTransaction();

                    command = _connection.CreateCommand();
                    command.Transaction = transaction;

                    command.CommandText = String.Format("SELECT * FROM {0}", GetTableName(typeOfInstance));

                    reader = command.ExecuteReader();

                    Object data = null;
                    IEnumerable<DbField> fields = DbField.GetFields(typeOfInstance);

                    while (reader.Read())
                    {
                        data = Activator.CreateInstance(typeOfInstance);
                        foreach (DbField field in fields)
                            try { field.SetValue(data, reader[field.Name]); } catch { }
                        objects.Add(data);
                    }

                    reader.Close();

                    transaction.Commit();

                    return objects;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("Error al realizar la extracción multiple de datos: {0}; Mensaje: {1}", typeOfInstance.Name, ex.Message), "ERROR");

                    if (reader != null)
                        reader.Close();

                    if (transaction != null)
                        transaction.Rollback();

                    return objects;
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

        public bool Save(object instance)
        {
            DbTransaction transaction = null;
            DbCommand command = null;
            ValidateConnection();
            lock (_transactions)
            {
                try
                {
                    transaction = BeginTransaction();

                    command = _connection.CreateCommand();
                    command.Transaction = transaction;

                    command.CommandText = CreateSaveStatement(instance.GetType());
                    CreateParameters(command, instance);

                    int rows = command.ExecuteNonQuery();

                    command.Parameters.Clear();
                    command.CommandText = String.Format("SELECT {0}()", Configuration.LastInsertFunctionName);

                    DbField.GetPrimaryKey(instance.GetType()).SetValue(instance, command.ExecuteScalar());

                    transaction.Commit();

                    return rows > 0;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("Error al realizar el salvado de la instancia: {0}; Mensaje: {1}", instance.ToString(), ex.Message), "ERROR");
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

        public bool Update(object instance)
        {
            DbTransaction transaction = null;
            DbCommand command = null;
            ValidateConnection();
            lock (_transactions)
            {
                try
                {
                    transaction = BeginTransaction();

                    command = _connection.CreateCommand();
                    command.Transaction = transaction;

                    command.CommandText = CreateUpdateStatement(instance.GetType());

                    CreateParameters(command, instance);

                    int rows = command.ExecuteNonQuery();

                    transaction.Commit();

                    return rows > 0;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("Error al realizar la actualzación de la instancia: {0}; Mensaje: {1}", instance.ToString(), ex.Message), "ERROR");
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

        private void CreateParameter(DbCommand command, string name, object idKey)
        {
            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = String.Format("@{0}", name);
            parameter.Value = idKey;

            command.Parameters.Add(parameter);
        }

        private void CreateParameters(DbCommand command, object instance)
        {
            var matches = Regex.Matches(command.CommandText, "@[0-9a-zA-Z]*");
            foreach (Match match in matches)
            {
                var dbField = DbField.GetFields(instance.GetType()).First(field => field.Name == match.Value.Replace("@", ""));
                var parameter = command.CreateParameter();
                parameter.ParameterName = match.Value;
                parameter.Value = dbField.GetValue(instance);

                command.Parameters.Add(parameter);
            }
        }

        private string CreateSaveStatement(Type typeOfInstance)
        {
            StringBuilder statement = new StringBuilder();
            StringBuilder parameters = new StringBuilder();

            foreach (DbField field in DbField.GetFields(typeOfInstance))
            {
                if (field.IsPrimaryKey ) continue;
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

        private String GetTableName(Type typeOfInstance)
        {
            foreach (Attribute attribute in typeOfInstance.GetCustomAttributes())
                if (attribute is TableAttribute)
                    return (attribute as TableAttribute).Name;

            return typeOfInstance.Name;
        }

        private bool OpenConnection()
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                _connection.Open();

            return _connection.State == System.Data.ConnectionState.Open;
        }

        private void ValidateConnection()
        {
            if (_connection is null)
                throw new InvalidOperationException(@"Requiere inicializarse el controlador de la base de datos,
                                        use InnSyTech.Standard.Database.DbManager.Initialize(...).");
        }
    }
}