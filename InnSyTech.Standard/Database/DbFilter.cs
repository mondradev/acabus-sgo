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
        GREAT_AND_EQUALS,
        LIKE,
        IS,
        IN
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
        public DbFilter AddWhere(DbFilterExpression field, WhereType type = WhereType.AND)
        {
            fields.Add(new DbFilterValue(field, type));
            return this;
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

    public sealed class DbFilterExpression
    {
        private string _propertyName;
        private object _value;
        private WhereOperator _operator;

        public String PropertyName => _propertyName;
        public Object Value => _value;
        public WhereOperator Operator => _operator;

        public DbFilterExpression(string propertyName, object value, WhereOperator @operator)
        {
            _propertyName = propertyName;
            _value = value;
            _operator = @operator;
        }
    }

    public sealed class DbFilterValue
    {
        private DbFilterExpression _expression;

        public DbFilterExpression Expression => _expression;

        private WhereType _type;

        public WhereType Type => _type;


        public DbFilterValue(DbFilterExpression expression, WhereType type)
        {
            _expression = expression;
            _type = type;
        }
    }
}