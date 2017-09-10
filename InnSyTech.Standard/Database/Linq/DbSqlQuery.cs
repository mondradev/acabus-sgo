using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace InnSyTech.Standard.Database.Linq
{
    internal class DbSqlQuery<TData> : IOrderedQueryable<TData>
    {
        public DbSqlQuery(DbSqlProvider provider)
        {
            if (provider is null)
                throw new ArgumentNullException("El proveedor de datos no puede ser nulo.");

            Provider = provider;
            Expression = Expression.Constant(this);
        }

        public DbSqlQuery(DbSqlProvider provider, Expression expression)
        {
            if (provider is null)
                throw new ArgumentNullException("El proveedor de datos no puede ser nulo.");

            if (expression is null)
                throw new ArgumentNullException("La expresión no puede ser nula.");

            if (!typeof(IQueryable<TData>).IsAssignableFrom(expression.Type))
                throw new ArgumentOutOfRangeException(nameof(expression), "El tipo de expresión no es valido.");

            Provider = provider;
            Expression = expression;
        }

        public Type ElementType => typeof(TData);
        public Expression Expression { get; }
        public IQueryProvider Provider { get; }

        public IEnumerator<TData> GetEnumerator()
            => Provider.Execute<IEnumerable<TData>>(Expression).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => Provider.Execute<IEnumerable>(Expression).GetEnumerator();
       
    }
}