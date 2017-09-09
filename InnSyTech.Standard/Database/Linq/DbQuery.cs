using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace InnSyTech.Standard.Database.Linq
{
    /// <summary>
    /// Consulta SQL utilizada para obtener información desde una base de datos relacional.
    /// </summary>
    /// <typeparam name="TData">Tipo de dato que manejará la consulta.</typeparam>
    internal class DbQuery<TData> : IOrderedQueryable<TData>
    {
        /// <summary>
        /// Crea una instancia nueva de <see cref="DbQuery{TData}"/>.
        /// </summary>
        /// <param name="provider">Proveedor que se utiliza para acceder a los datos.</param>
        public DbQuery(DbProvider provider)
        {
            if (provider is null)
                throw new ArgumentNullException("El proveedor de datos no puede ser nulo.");

            Provider = provider;
            Expression = Expression.Constant(this);
        }

        /// <summary>
        /// Crea una nueva instancia de <see cref="DbQuery{TData}"/>.
        /// </summary>
        /// <param name="provider">Proveedor que se utiliza para acceder a los datos.</param>
        /// <param name="expression">Expresión de la consulta.</param>
        public DbQuery(DbProvider provider, Expression expression)
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

        /// <summary>
        /// Obtiene el tipo de elemento de la consulta.
        /// </summary>
        public Type ElementType => typeof(TData);

        /// <summary>
        /// Obtiene la expresión de la consulta.
        /// </summary>
        public Expression Expression { get; }

        /// <summary>
        /// Obtiene el proveedor de datos de la consulta.
        /// </summary>
        public IQueryProvider Provider { get; }

        /// <summary>
        /// Obtiene el enumerador genérico de la consulta actual.
        /// </summary>
        /// <returns>El enumerador genérico de la consulta.</returns>
        public IEnumerator<TData> GetEnumerator()
            => Provider.Execute<IEnumerable<TData>>(Expression).GetEnumerator();

        /// <summary>
        /// Obtiene el enumerador de la consulta actual.
        /// </summary>
        /// <returns>El enumerador de la consulta.</returns>
        IEnumerator IEnumerable.GetEnumerator()
            => Provider.Execute<IEnumerable>(Expression).GetEnumerator();
    }
}