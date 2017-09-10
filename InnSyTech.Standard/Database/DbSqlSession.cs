using InnSyTech.Standard.Database.Linq;
using System;
using System.Data.Common;
using System.Linq;

namespace InnSyTech.Standard.Database
{
    /// <summary>
    /// Implementa <see cref="IDbSession"/> para proporcionar el acceso a la base de datos a través de las funciones de crear, leer, actualizar y eliminar.
    /// </summary>
    internal sealed class DbSqlSession : IDbSession
    {
        /// <summary>
        /// Crea una nueva instancia de <see cref="DbSqlSession"/>.
        /// </summary>
        /// <param name="dbConnection">La conexión a la base de datos.</param>
        /// <param name="dialect">El dialecto de comunicación de la base de datos.</param>
        public DbSqlSession(DbConnection dbConnection, IDbDialect dialect)
            => Provider = new DbProvider(dbConnection, dialect);

        /// <summary>
        /// Obtiene el proveedor de consultas para la base de datos.
        /// </summary>
        public DbProvider Provider { get; }

        /// <summary>
        /// Crea una instancia persistente a partir de un tipo definido que corresponde a una tabla
        /// en la base de datos. Esto equivale a un INSERT INTO de Sql.
        /// </summary>
        /// <typeparam name="TData">Tipo de dato de la instancia a persistir.</typeparam>
        /// <param name="instance">Instancia que se desea persistir.</param>
        /// <returns>Un true en caso que la instancia sea persistida correctamente.</returns>
        public bool Create<TData>(TData instance)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Elimina una instancia persistida en la base de datos, si esta cuenta con campo de estado
        /// para visibilidad, solo se cambia a su valor oculto para ser ignorada en las lecturas.
        /// Esto equivale a una DELETE FROM de Sql. Si hay referencias a esta instancia hay que
        /// colocar parametro <paramref name="cascade"/> en true para realizar un borrado en cascada.
        /// </summary>
        /// <typeparam name="TData">Tipo de dato de la instancia a eliminar.</typeparam>
        /// <param name="instance">Instancia a eliminar.</param>
        /// <param name="cascade">Indica si se elimina en cascada todas sus referencias.</param>
        /// <returns>Un true si la instancia fue borrada así como sus referencias de ser necesario.</returns>
        public bool Delete<TData>(TData instance, bool cascade = false)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Realiza una lectura de elementos del tipo especificado que corresponde a una tabla en la base de datos, gracias a la estructura <see cref="IQueryable{T}"/> se puede aplicar filtros y ordernamiento como otros métodos que se encuentren disponibles. Esto equivale a un SELECT FROM.
        /// </summary>
        /// <typeparam name="TResult">El tipo de dato de la lectura.</typeparam>
        /// <returns>Una consulta que extrae datos de la base de datos relacional.</returns>
        public IQueryable<TResult> Read<TResult>()
            => new DbSqlQuery<TResult>(Provider);

        /// <summary>
        /// Actualiza los atributosde la instancia persistida en la base de datos. Esto equivale a un
        /// UPDATE de Sql. Si se requiere actualizar todas las referencias se necesita colocar el
        /// parametro <paramref name="cascade"/> en true.
        /// </summary>
        /// <typeparam name="TData">Tipo de datos de la instancia persistida.</typeparam>
        /// <param name="instance">Instancia persistida en la base de datos.</param>
        /// <param name="cascade">
        /// Indica si se actualiza en cascada la instancia. Esto significa que las referencias a este
        /// dato se actualizarán.
        /// </param>
        /// <returns>
        /// Un true si la instancia fue correctamente actualizada así como sus referencias de ser necesario.
        /// </returns>
        public bool Update<TData>(TData instance, bool cascade = false)
        {
            throw new NotImplementedException();
        }
    }
}