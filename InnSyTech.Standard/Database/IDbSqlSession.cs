using System;
using System.Linq;

namespace InnSyTech.Standard.Database
{
    public interface IDbSqlSession
    {
        bool Create(Type instanceType);

        bool Delete(Type instanceType, bool cascade = false);
        
        IOrderedQueryable<TResult> Read<TResult>();

        bool Update(Type instanceType, bool cascade = false);
    }
}