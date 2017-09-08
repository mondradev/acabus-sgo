using System;
using System.Data.Common;

namespace InnSyTech.Standard.Database
{
    /// <summary>
    /// Permite la construcción de sesiones de acceso a base de datos especificando el tipo de esta.
    /// </summary>
    public static class DbFactory
    {
        /// <summary>
        /// Crea una sesión a de conexión a una base de datos especificada por el tipo de conexión en el parametro <typeparamref name="ConnectionType"/>.
        /// </summary>
        /// <typeparam name="ConnectionType">Tipo de conexión a la base de datos.</typeparam>
        /// <param name="dialect">Dialecto utilizado para la comunicación correcta de la base de datos.</param>
        /// <returns>Una sesión de conexión a la base de datos.</returns>
        public static IDbSession CreateSession<ConnectionType>(IDbDialect dialect)
            => CreateSession(typeof(ConnectionType), dialect);

        /// <summary>
        /// Crea una sesión a de conexión a una base de datos especificada por el tipo de conexión en
        /// el parametro <paramref name="connectionType"/>.
        /// </summary>
        /// <param name="connectionType">Tipo de conexión a la base de datos.</param>
        /// <param name="dialect">
        /// Dialecto utilizado para la comunicación correcta de la base de datos.
        /// </param>
        /// <returns>Una sesión de conexión a la base de datos.</returns>
        public static IDbSession CreateSession(Type connectionType, IDbDialect dialect)
        {
            if (!typeof(DbConnection).IsAssignableFrom(connectionType))
                throw new ArgumentOutOfRangeException(nameof(connectionType), "El tipo de conexión no hereda de System.Data.Common.DbConnection");

            return new DbSession((DbConnection)Activator.CreateInstance(connectionType, dialect.ConnectionString), dialect);
        }
    }
}