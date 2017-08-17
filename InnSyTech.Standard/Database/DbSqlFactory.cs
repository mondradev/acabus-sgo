using System;
using System.Data.Common;

namespace InnSyTech.Standard.Database
{
    public static class DbSqlFactory
    {
        public static IDbSqlSession CreateSession<ConnectionType>(IDbDialect sqlDialect)
            => CreateSession(typeof(ConnectionType), sqlDialect);

        public static IDbSqlSession CreateSession(Type connectionType, IDbDialect sqlDialect)
        {
            if (!typeof(DbConnection).IsAssignableFrom(connectionType))
                throw new ArgumentOutOfRangeException(nameof(connectionType), "El tipo de conexión no hereda de System.Data.Common.DbConnection");

            return new DbSqlSession((DbConnection)Activator.CreateInstance(connectionType, sqlDialect.ConnectionString), sqlDialect);
        }
    }
}
