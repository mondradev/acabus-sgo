﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace InnSyTech.Standard.Database.Linq
{
    internal class SqlExpressionVisitor : ExpressionVisitor
    {
        private StringBuilder _statement;

        internal string Translate(Expression expression, IDbDialect dialect)
        {
            _statement = new StringBuilder();

            Visit(expression);

            return _statement.ToString();
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            _statement.Append("(");

            this.Visit(b.Left);

            switch (b.NodeType)
            {
                case ExpressionType.And:

                    _statement.Append(" AND ");

                    break;

                case ExpressionType.Or:

                    _statement.Append(" OR");

                    break;

                case ExpressionType.Equal:

                    _statement.Append(" = ");

                    break;

                case ExpressionType.NotEqual:

                    _statement.Append(" <> ");

                    break;

                case ExpressionType.LessThan:

                    _statement.Append(" < ");

                    break;

                case ExpressionType.LessThanOrEqual:

                    _statement.Append(" <= ");

                    break;

                case ExpressionType.GreaterThan:

                    _statement.Append(" > ");

                    break;

                case ExpressionType.GreaterThanOrEqual:

                    _statement.Append(" >= ");

                    break;

                default:

                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }

            this.Visit(b.Right);

            _statement.Append(")");

            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c.Value is IQueryable q)
            {
                // assume constant nodes w/ IQueryables are table references

                _statement.Append("SELECT * FROM ");

                _statement.Append(q.ElementType.Name);
            }
            else if (c.Value == null)
            {
                _statement.Append("NULL");
            }
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:

                        _statement.Append(((bool)c.Value) ? 1 : 0);

                        break;

                    case TypeCode.String:

                        _statement.Append("'");

                        _statement.Append(c.Value);

                        _statement.Append("'");

                        break;

                    case TypeCode.Object:

                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));

                    default:

                        _statement.Append(c.Value);

                        break;
                }
            }

            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                _statement.Append(m.Member.Name);

                return m;
            }

            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                _statement.Append("SELECT * FROM (");

                this.Visit(m.Arguments[0]);

                _statement.Append(") AS T WHERE ");

                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                this.Visit(lambda.Body);

                return m;
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:

                    _statement.Append(" NOT ");

                    this.Visit(u.Operand);

                    break;

                default:

                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }

            return u;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }

            return e;
        }
    }
}