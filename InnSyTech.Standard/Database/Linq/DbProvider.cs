using InnSyTech.Standard.Database.Linq.DbDefinitions;
using InnSyTech.Standard.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace InnSyTech.Standard.Database.Linq
{
    /// <summary>
    /// Define un proveedor de consultas SQL a Objetos a través de la framework que ofrece <see
    /// cref="System.Linq.IQueryable"/>. Permite realizar consultas a la base de datos a travéz de
    /// métodos de <see cref="IQueryable"/> sin necesidad de escribir código SQL.
    /// </summary>
    internal sealed class DbProvider : IQueryProvider
    {
        /// <summary>
        /// Conexión a la base de datos.
        /// </summary>
        private DbConnection _connection;

        /// <summary>
        /// Dialecto que permite la interpretación de la base de datos.
        /// </summary>
        private IDbDialect _dialect;

        /// <summary>
        /// Transacciones actualmente en ejecución.
        /// </summary>
        private List<DbTransaction> _transactions = new List<DbTransaction>();

        /// <summary>
        /// Crea una instancia nueva de <see cref="DbProvider"/>.
        /// </summary>
        /// <param name="connection">Conexión la basde de datos.</param>
        /// <param name="dialect">Dialecto que utilizará el proveedor para comunicarse la base de datos.</param>
        public DbProvider(DbConnection connection, IDbDialect dialect)
        {
            _connection = connection;
            _dialect = dialect;
        }

        /// <summary>
        /// Obtiene el dialecto utilizado para comunicarse con la base de datos.
        /// </summary>
        public IDbDialect Dialect => _dialect;

        /// <summary>
        /// Comienza una consulta transaccional en la base de datos.
        /// </summary>
        /// <returns>Una instancia de <see cref="DbTransaction"/>.</returns>
        public DbTransaction BeginTransaction()
        {
            if (_connection is null)
                throw new InvalidOperationException("La conexión a la base de datos es nula.");

            if (_connection.State == ConnectionState.Closed)
                _connection.Open();

            if (_transactions.Count > _dialect.TransactionPerConnection)
                Monitor.Wait(this);

            var transaction = _connection.BeginTransaction();

            _transactions.Add(transaction);

            return transaction;

        }

        /// <summary>
        /// Cierra la conexión actual de la base de datos, pero no la desecha.
        /// </summary>
        public void CloseConnection()
            => _connection.Close();

        /// <summary>
        /// Crea un comando para ejecutar una sentencia Sql.
        /// </summary>
        /// <param name="transaction">La transacción en la cual se ejecutará el comando.</param>
        /// <returns>Una instancia de <see cref="DbCommand"/>.</returns>
        public DbCommand CreateCommand(DbTransaction transaction)
        {
            if (_connection is null)
                throw new InvalidOperationException("La conexión a la base de datos es nula.");

            if (_connection.State == ConnectionState.Closed)
                _connection.Open();

            var command = _connection.CreateCommand();

            command.Transaction = transaction;

            return command;
        }

        /// <summary>
        /// Crea una consulta que puede ser traducida a SQL.
        /// </summary>
        /// <param name="expression">Expresión raiz de la consulta.</param>
        /// <returns>Una consulta de elementos.</returns>
        public IQueryable CreateQuery(Expression expression)
        {
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(DbQuery<>)
                    .MakeGenericType(TypeHelper.GetElementType(expression.Type)), new object[] { this, expression });
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Crea una consulta genérica que pueder traducida a SQL.
        /// </summary>
        /// <typeparam name="TElement">Tipo de datos de los elementos de la consulta.</typeparam>
        /// <param name="expression">Expresión raíz de la consulta.</param>
        /// <returns>Una consulta de elementos.</returns>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new DbQuery<TElement>(this, expression);

        /// <summary>
        /// Libera los recursos de la transacción.
        /// </summary>
        /// <param name="transaction">La transacción a liberar.</param>
        public void EndTransaction(DbTransaction transaction)
        {
            _transactions.Remove(transaction);

            transaction.Dispose();

            Monitor.Pulse(this);
        }

        /// <summary>
        /// Ejecuta la consulta traduciendo primero a código SQL.
        /// </summary>
        /// <typeparam name="TResult">Tipo de resultadao de la consulta.</typeparam>
        /// <param name="expression">Expresión donde comienza la consulta.</param>
        /// <returns>El resultado de la consulta ejecutada.</returns>
        public TResult Execute<TResult>(Expression expression)
                    => (TResult)Execute(expression);

        /// <summary>
        /// Ejecuta la consulta traduciendo primero a código SQL.
        /// </summary>
        /// <param name="expression">Expresión donde comienza la consulta.</param>
        /// <returns>El resultado de la consulta ejecutada.</returns>
        public object Execute(Expression expression)
        {
            lock (this)
            {
                DbTransaction transaction = BeginTransaction();
                DbCommand command = CreateCommand(transaction);

                try
                {
                    command.CommandText = GetQueryText(expression, out DbStatementDefinition definition);

                    Trace.WriteLine($"Ejecutando: {command.CommandText}", "DEBUG");

                    DbDataReader reader = command.ExecuteReader();

                    Type elementType = TypeHelper.GetElementType(expression.Type);
                    
                    return DbReader.Process(elementType, reader, definition);
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                        transaction.Rollback();

                    Trace.WriteLine((ex.Message + ex.StackTrace).JoinLines(), "ERROR");

                    return null;
                }
                finally
                {
                    if (transaction != null)
                        EndTransaction(transaction);

                    if (command != null)
                        command.Dispose();

                    CloseConnection();
                }
            }
        }

        /// <summary>
        /// Obtiene el texto de la sentencia SQL que se utilizará para obtener los elementos deseados.
        /// </summary>
        /// <param name="expression">Expresión raíz donde comienza la consulta.</param>
        /// <param name="definition">
        /// Definición del enunciado que espefica la construcción de la sentencia SQL.
        /// </param>
        /// <returns>Un enunciado SQL que se utilizará para el SQL.</returns>
        public string GetQueryText(Expression expression, out DbStatementDefinition definition)
            => new DbStatementTraslator()
                .Translate(DbEvaluatorExpression.PartialEval(expression), _dialect, out definition);
    }
}