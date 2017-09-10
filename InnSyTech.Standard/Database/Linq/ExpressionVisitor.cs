using InnSyTech.Standard.Database.Utils;
using InnSyTech.Standard.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace InnSyTech.Standard.Database.Linq
{
    internal class ExpressionVisitor : System.Linq.Expressions.ExpressionVisitor
    {

        private DbStatementDefinition _statementDefinition = new DbStatementDefinition();
        private List<Tuple<String, DbFieldDefinition>> _fieldLabels = new List<Tuple<string, DbFieldDefinition>>();
        private IDbDialect _dialect;
        private StringBuilder _statement;

        internal string Translate(Expression expression, IDbDialect dialect)
        {
            _dialect = dialect;
            _statement = new StringBuilder();

            Visit(expression);

            ProcessDefinition(_statementDefinition);

            return _statement.ToString();
        }

        private void ProcessDefinition(DbStatementDefinition statementDefinition)
        {
            int count = 0;
            foreach (var statement in _statementDefinition)
                foreach (var entity in statement.Entities)
                    count = ProcessEntity(count, entity);

            foreach (var label in _fieldLabels)
                _statement.Replace(label.Item1, label.Item2.Entity.Alias);

            _statement
                .Replace(", {{fields}}", "")
                .Replace(" {{entities}}", "");
        }

        private int ProcessEntity(int count, DbEntityDefinition entity)
        {
            StringBuilder fieldsString = new StringBuilder();
            String entityName = DbHelper.GetEntityName(entity.EntityType);

            entity.Alias = String.Format("T{0}", count++);

            foreach (var dbFields in DbHelper.GetFields(entity.EntityType))
                fieldsString.AppendFormat("{1}.{0} {1}_{0}, ", dbFields.Name, entity.Alias);

            _statement.Replace("{{fields}}", String.Format("{0}{1}", fieldsString.ToString(), "{{fields}}"));

            if (entity.Alias.Equals("T0"))
                _statement.Replace("{{entities}}", String.Format("{0} T0 {1}", entityName, "{{entities}}"));
            else
                _statement.Replace("{{entities}}", string.Format(
                    "LEFT JOIN {0} {1} ON {1}.{2} = {3}.{4} {5}",
                    entityName,
                    entity.Alias,
                    DbHelper.GetPrimaryKey(entity.EntityType).Name,
                    entity.DependencyEntity.Alias,
                    entity.DependencyMember.GetFieldName(),
                    "{{entities}}"
                ));

            foreach (var subEntity in entity.DependentsEntities)
                count = ProcessEntity(count, subEntity);

            return count;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            _statement.Append("(");

            Visit(b.Left);

            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    _statement.Append(" AND ");
                    break;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    _statement.Append(" OR ");
                    break;

                case ExpressionType.Equal:
                    if ((b.Right as ConstantExpression).Value == null)
                        _statement.Append(" IS ");
                    else
                        _statement.Append(" = ");
                    break;

                case ExpressionType.NotEqual:
                    if ((b.Right as ConstantExpression).Value == null)
                        _statement.Append(" IS NOT ");
                    else
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

            Visit(b.Right);

            _statement.Append(")");

            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c.Value is IQueryable q)
            {
                _statement.Append("SELECT {{fields}} FROM {{entities}} ");

                _statementDefinition.AddEntity(new DbEntityDefinition() { EntityType = q.ElementType });
            }
            else if (c.Value == null)
                _statement.Append("NULL");
            else
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

                    case TypeCode.DateTime:
                        _statement.Append("'");
                        _statement.Append(_dialect.DateTimeConverter?.ConverterToDbData(c.Value) ?? c.Value);
                        _statement.Append("'");
                        break;

                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                    default:
                        _statement.Append(c.Value);
                        break;
                }

            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m != null)
            {
                var expMember = m;
                var entityReference = new DbEntityDefinition();
                var entity = entityReference;

                DbFieldDefinition fieldDef = entityReference.CreateMember(m.Member);
                String label = String.Format("{{FL{0}}}", _fieldLabels.Count);

                _selectedList?.Add(fieldDef);
                _fieldLabels.Add(Tuple.Create(label, fieldDef));
                _statement.AppendFormat("{0}.{1}", label, fieldDef.GetFieldName());

                while (expMember != null)
                {
                    if (DbHelper.IsEntity(expMember.Expression.Type))
                    {
                        entity.EntityType = expMember.Expression.Type;
                        if (entity.DependentsEntities.Count > 0)
                            entity.DependentsEntities.First().DependencyMember = entity.CreateMember(expMember.Member);
                    }
                    else
                        throw new InvalidOperationException("Una de los tipos de los miembros involucrado no corresponde a una entidad.");

                    if (expMember.Expression.NodeType == ExpressionType.Parameter)
                        break;

                    expMember = expMember.Expression as MemberExpression;

                    entity = entity.CreateDependencyEntity(expMember?.Member);
                }

                _statementDefinition.AddEntity(entityReference);
                return m;
            }
            throw new ArgumentNullException(nameof(m));
        }

        List<DbFieldDefinition> _selectedList;

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType != typeof(Queryable) && m.Method.DeclaringType != typeof(DbQueryable))
                throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));

            switch (m.Method.Name)
            {
                case "Where":
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    Visit(m.Arguments[0]);
                    _statement.Append(" WHERE ");
                    _selectedList = _statementDefinition.Filters;
                    Visit(lambda.Body);
                    break;

                case "OrderBy":
                    Visit(m.Arguments[0]);
                    _statement.Append(" ORDER BY ");
                    _selectedList = _statementDefinition.Orders;
                    Visit(m.Arguments[1]);
                    break;

                case "LoadReference":
                    var depth = (int)(m.Arguments[1] as ConstantExpression).Value;
                    LoadReference(TypeHelper.GetElementType((m.Arguments[0] as ConstantExpression).Type), depth);
                    Visit(m.Arguments[0]);
                    break;

                default:
                    break;
            }
            return m;
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    _statement.Append(" NOT ");
                    this.Visit(u.Operand);
                    break;

                case ExpressionType.Quote:
                    Visit(u.Operand);
                    break;

                case ExpressionType.Convert:
                    Visit(u.Operand);
                    break;

                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }

            return u;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
                e = ((UnaryExpression)e).Operand;

            return e;
        }

        private void LoadReference(MemberInfo instance, int depth, DbEntityDefinition entity = null)
        {
            if (depth < 0)
                return;

            if (entity == null)
                entity = new DbEntityDefinition();
            else
                entity = entity.CreateDependentsEntity(instance);

            entity.EntityType = TypeHelper.GetMemberType(instance);

            var references = DbHelper.GetFields(TypeHelper.GetMemberType(instance)).Where(f => f.IsForeignKey);

            if (depth == 0 || references.Count() == 0)
            {
                _statementDefinition.AddEntity(entity);
                return;
            }

            foreach (var reference in references)
                LoadReference(reference.PropertyInfo as MemberInfo, depth - 1, entity);

        }
    }
}