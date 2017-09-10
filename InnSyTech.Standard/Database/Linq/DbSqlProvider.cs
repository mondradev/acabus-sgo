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
    internal sealed class DbSqlProvider : IQueryProvider
    {
        private DbConnection _connection;
        private IDbDialect _dialect;

        public DbSqlProvider(DbConnection connection, IDbDialect dialect)
        {
            _connection = connection;
            _dialect = dialect;
        }

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

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new DbSqlQuery<TElement>(this, expression);

        public TResult Execute<TResult>(Expression expression)
                    => (TResult)Execute(expression);

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

        public string GetQueryText(Expression expression)
            => new ExpressionVisitor()
            .Translate(EvaluatorExpression.PartialEval(expression), _dialect);
    }
}