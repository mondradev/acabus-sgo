using InnSyTech.Standard.Database.Utils;
using System;
using System.Data;
using System.Data.Common;
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
                    .MakeGenericType(TypeSystem.GetElementType(expression.Type)), new object[] { this, expression });
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new DbSqlQuery<TElement>(this, expression);

        public string GetQueryText(Expression expression)
            => new SqlExpressionVisitor().Translate(expression, _dialect);

        public TResult Execute<TResult>(Expression expression)
                    => (TResult)Execute(expression);

        public object Execute(Expression expression)
        {
            DbCommand command = null;
            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                command = _connection.CreateCommand();
                command.CommandText = GetQueryText(expression);

                DbDataReader reader = command.ExecuteReader();

                Type elementType = TypeSystem.GetElementType(expression.Type);

                return Activator.CreateInstance(
                    typeof(DbSqlReader<>).MakeGenericType(elementType),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new object[] { reader },
                    null
                );
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                if (command != null)
                    command.Dispose();

                if (_connection != null && _connection.State != ConnectionState.Closed)
                    _connection.Close();
            }
        }
    }
}