using InnSyTech.Standard.Database.Linq;
using System;
using System.Data.Common;
using System.Linq;

namespace InnSyTech.Standard.Database
{
    internal sealed class DbSqlSession : IDbSqlSession
    {
        private DbSqlProvider provider;

        public DbSqlSession(DbConnection dbConnection, IDbDialect dialect)
        {
            this.provider = new DbSqlProvider(dbConnection, dialect);
        }

        public bool Create(Type instanceType)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Type instanceType, bool cascade = false)
        {
            throw new NotImplementedException();
        }

        public IOrderedQueryable<TResult> Read<TResult>()
         => new DbSqlQuery<TResult>(provider);

        public bool Update(Type instanceType, bool cascade = false)
        {
            throw new NotImplementedException();
        }
    }
}