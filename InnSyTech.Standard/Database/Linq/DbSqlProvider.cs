using InnSyTech.Standard.Utils;
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace InnSyTech.Standard.Database.Linq
{
    /// <summary>
    /// Define un proveedor de consultas SQL a Objetos a través de la framework que ofrece <see cref="System.Linq.IQueryable"/>. Permite realizar consultas a la base de datos a travéz de métodos de <see cref="IQueryable"/> sin necesidad de escribir código SQL.
    /// </summary>
    internal sealed class DbSqlProvider : IQueryProvider
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
        /// Crea una instancia nueva de <see cref="DbSqlProvider"/>.
        /// </summary>
        /// <param name="connection">Conexión la basde de datos.</param>
        /// <param name="dialect">Dialecto que utilizará el proveedor para comunicarse la base de datos.</param>
        public DbSqlProvider(DbConnection connection, IDbDialect dialect)
        {
            _connection = connection;
            _dialect = dialect;
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
                return (IQueryable)Activator.CreateInstance(typeof(DbSqlQuery<>)
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
            => new DbSqlQuery<TElement>(this, expression);

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
            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            DbCommand command = _connection.CreateCommand();
            command.CommandText = GetQueryText(expression);

            Trace.WriteLine($"Execute: {command.CommandText}");

            DbDataReader reader = command.ExecuteReader();

            Type elementType = TypeHelper.GetElementType(expression.Type);

            return Activator.CreateInstance(
                typeof(DbSqlReader<>).MakeGenericType(elementType),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[] { reader, command, _connection },
                null
            );
        }

        /// <summary>
        /// Obtiene el texto de la sentencia SQL que se utilizará para obtener los elementos deseados.
        /// </summary>
        /// <param name="expression">Expresión raíz donde comienza la consulta.</param>
        /// <returns>Un enunciado SQL que se utilizará para el SQL.</returns>
        public string GetQueryText(Expression expression)
            => new ExpressionVisitor()
            .Translate(EvaluatorExpression.PartialEval(expression), _dialect);
    }
}