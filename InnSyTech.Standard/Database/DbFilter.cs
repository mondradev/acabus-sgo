using System;
using System.Collections.Generic;

namespace InnSyTech.Standard.Database
{
    public enum WhereOperator
    {
        LESS_THAT,
        GREAT_THAT,
        EQUALS,
        NO_EQUALS,
        LESS_AND_EQUALS,
        GREAT_AND_EQUALS
    }

    public enum WhereType
    {
        AND,
        OR
    }

    public sealed class DbFilter
    {
        private List<DbFilterValue> fields;

        /// <summary>
        /// Agrega filtros de consulta.
        /// </summary>
        public void AddWhere(DbFilterExpression field, WhereType type)
        {
            fields.Add(new DbFilterValue(field, type));
        }

        internal List<DbFilterValue> GetFilters()
            => fields;

        public DbFilter(List<DbFilterValue> filters)
        {
            fields = filters;
        }

        public DbFilter() : this(new List<DbFilterValue>())
        {

        }
    }

    public sealed class DbFilterExpression : Tuple<String, Object, WhereOperator>
    {

        public DbFilterExpression(string item1, object item2, WhereOperator item3) : base(item1, item2, item3)
        {
        }
    }

    public sealed class DbFilterValue : Tuple<DbFilterExpression, WhereType>
    {
        public DbFilterValue(DbFilterExpression item1, WhereType item2) : base(item1, item2)
        {
        }
    }
}