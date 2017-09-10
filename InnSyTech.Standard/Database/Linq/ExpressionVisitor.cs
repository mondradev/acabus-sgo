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
        private List<ChainHelper> _chains = new List<ChainHelper>();
        private DbStatementDefinition _statementDefinition = new DbStatementDefinition();
        private IDbDialect _dialect;
        private StringBuilder _statement;

        internal string Translate(Expression expression, IDbDialect dialect)
        {
            _dialect = dialect;
            _statement = new StringBuilder();

            Visit(expression);

            throw new NotImplementedException();

            TranslateChains();
            TranslateFields();

            return _statement.ToString();
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
                _statement.Append("SELECT {fields} FROM ")
                    .Append("{{entities}}");
                _statementDefinition.AddEntity(new DbEntityDefinition() { Entity = q.ElementType });
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

                ///_statement.AppendFormat("{{{1}}}.{0}", DbHelper.GetFieldName(m.Member, m.Expression.Type), _chains.Count);
                _selectedList?.Add(entityReference.CreateMember(m.Member));

                while (expMember != null)
                {
                    if (DbHelper.IsEntity(expMember.Expression.Type))
                    {
                        entity.Entity = expMember.Expression.Type;
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
                    _selectedList = _statementDefinition.Filters;
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    Visit(m.Arguments[0]);
                    // _statement.Append(" WHERE ");
                    Visit(lambda.Body);
                    break;

                case "OrderBy":
                    _selectedList = _statementDefinition.Orders;
                    Visit(m.Arguments[0]);
                    _statement.Append(" ORDER BY ");
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

            _selectedList = null;

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

        //private void LoadReference(MemberInfo instance, int depth, ChainHelper chain = null)
        //{
        //    if (depth < 0)
        //        return;

        //    if (chain == null)
        //        chain = new ChainHelper();

        //    chain.AddLast(instance);

        //    var references = DbHelper.GetFields(TypeHelper.GetMemberType(instance)).Where(f => f.IsForeignKey);

        //    if (depth == 0 || references.Count() == 0)
        //    {
        //        _chains.Add(chain);
        //        return;
        //    }

        //    foreach (var item in references)
        //        LoadReference(item.PropertyInfo as MemberInfo, depth - 1, new ChainHelper() { chain });
        //}

        private void LoadReference(MemberInfo instance, int depth, DbEntityDefinition entity = null)
        {
            if (depth < 0)
                return;

            if (entity == null)
                entity = new DbEntityDefinition();
            else
                entity = entity.CreateDependentsEntity(instance);

            entity.Entity = TypeHelper.GetMemberType(instance);

            var references = DbHelper.GetFields(TypeHelper.GetMemberType(instance)).Where(f => f.IsForeignKey);

            if (depth == 0 || references.Count() == 0)
            {
                _statementDefinition.AddEntity(entity);
                return;
            }

            foreach (var reference in references)
                LoadReference(reference.PropertyInfo as MemberInfo, depth - 1, entity);

        }

        private void TranslateChains()
        {
            StringBuilder builder = new StringBuilder();

            if (_chains.Count > 0)
            {
                var currentChain = 0;

                while (currentChain < _chains.Count)
                {
                    var lastChain = _chains[currentChain];

                    foreach (var chain in _chains.SkipWhile(ch => ch != lastChain))
                        lastChain.GetEqualsNodes(chain);

                    currentChain++;
                }

                int typesCount = 0;

                foreach (var chain in _chains)
                    foreach (var type in chain)
                    {
                        if (!String.IsNullOrEmpty(type.Alias)) continue;
                        if (!DbHelper.IsEntity(TypeHelper.GetMemberType(type.Member))) continue;

                        type.Alias = String.Format("T{0}", typesCount);
                        typesCount++;
                    }

                builder.AppendFormat(" {0} ", _chains.Merge().ElementAt(0).Alias);

                foreach (var chain in _chains)
                {
                    var lastNode = chain[0];
                    foreach (var entity in chain.Skip(1))
                    {
                        if (!builder.ToString().Contains(String.Format(" {0} ", entity.Alias)))
                        {
                            var entityType = TypeHelper.GetMemberType(entity.Member);

                            if (!DbHelper.IsEntity(entityType)) continue;

                            var lastEntityType = TypeHelper.GetMemberType(lastNode.Member);
                            var foreignKey = DbHelper.GetFieldName(entity.Member);
                            var primaryKey = DbHelper.GetPrimaryKey(entityType);

                            builder.AppendFormat("LEFT OUTER JOIN {4} {5} ON {0}.{1} = {2}.{3} ",
                                lastNode.Alias,
                                foreignKey,
                                entity.Alias,
                                primaryKey.Name,
                                DbHelper.GetEntityName(entityType),
                                entity.Alias);
                        }
                        lastNode = entity;
                    }
                }
            }

            _statement.Replace("{joins}", builder.ToString());

            for (int i = 0; i < _chains.Count; i++)
                _statement.Replace($"{{{i}}}", _chains[i].Last(e => DbHelper.IsEntity(TypeHelper.GetMemberType(e.Member))).Alias);
        }

        private void TranslateFields()
        {
            var entities = _chains
                .Merge()
                .Distinct()
                .Where(e => DbHelper.IsEntity(TypeHelper.GetMemberType(e.Member)));

            StringBuilder fields = new StringBuilder();

            foreach (var entity in entities)
                foreach (var field in DbHelper.GetFields(TypeHelper.GetMemberType(entity.Member)))
                    fields.AppendFormat("{0}.{1} {0}_{1}, ", entity.Alias, field.Name);

            _statement.Replace("{fields}", fields.Remove(fields.Length - 2, 2).ToString());
        }
    }
}