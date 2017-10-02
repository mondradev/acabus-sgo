using InnSyTech.Standard.Database.Linq.DbDefinitions;
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
    /// <summary>
    ///
    /// </summary>
    internal class DbStatementTraslator : ExpressionVisitor
    {
        private DbDialectBase _dialect;
        private List<object[]> _fieldLabels = new List<object[]>();
        private List<DbFieldDefinition> _selectedList;
        private StringBuilder _statement;

        private DbStatementDefinition _statementDefinition
            = new DbStatementDefinition();

        public string Translate(Expression expression, DbDialectBase dialect, out DbStatementDefinition definition)
        {
            _dialect = dialect;
            _statement = new StringBuilder();

            Visit(expression);

            ProcessDefinition();

            definition = _statementDefinition;

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
                    throw new NotSupportedException(string.Format("El operador binario '{0}' no es soportado", b.NodeType));
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
            var expMember = m;
            var entityReference = null as DbEntityDefinition;
            var entity = entityReference;

            while (expMember != null)
            {
                if (DbHelper.IsEntity(expMember.Expression.Type))
                {
                    if (entityReference == null)
                        entity = (entityReference = new DbEntityDefinition());
                    else
                        entity = entity.CreateDependencyEntity(expMember?.Member);

                    entity.EntityType = expMember.Expression.Type;
                }
                else
                    throw new InvalidOperationException("Un de los tipos de los miembros involucrado no corresponde a una entidad.");

                if (expMember.Expression.NodeType == ExpressionType.Parameter)
                    break;

                expMember = expMember.Expression as MemberExpression;
            }

            DbFieldDefinition fieldDef = entityReference.CreateMember(m.Member);
            String label = String.Format("{{FL{0}}}", _fieldLabels.Count);

            _selectedList?.Add(fieldDef);
            _fieldLabels.Add(new object[] { label, fieldDef });
            if (_statement.Length > 0)
                _statement.AppendFormat("{0}.{1}", label, fieldDef.GetFieldName());
            _statementDefinition.AddEntity(entityReference);

            return m;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType != typeof(Queryable) && m.Method.DeclaringType != typeof(DbQueryable))
                throw new NotSupportedException(String.Format("El método '{0}' no es soportado por la API", m.Method.Name));

            switch (m.Method.Name)
            {
                case "Where":
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    Visit(m.Arguments[0]);
                    if (_statementDefinition.Filters.Count == 0)
                        _statement.Append(" WHERE ");
                    else
                        _statement.Append(" AND ");
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
                    _statementDefinition.ReferenceDepth = depth;
                    LoadReference(TypeHelper.GetElementType(m.Arguments[0].Type), depth);
                    Visit(m.Arguments[0]);
                    break;

                case "Take":
                    var count = (int)(m.Arguments.Last() as ConstantExpression).Value;
                    Visit(m.Arguments.First());
                    _statementDefinition.CountToTake = _statementDefinition.CountToTake > count && _statementDefinition.CountToTake != 0 
                        ? count : _statementDefinition.CountToTake;
                    break;

                case "Single":
                case "SingleOrDefault":
                case "FirstOrDefault":
                case "First":
                    _statementDefinition.IsEnumerable = false;
                    _statementDefinition.CountToTake = 1;
                    Visit(m.Arguments.First());
                    if (m.Arguments.Count > 1)
                    {
                        if (_statementDefinition.Filters.Count == 0)
                            _statement.Append(" WHERE ");
                        else
                            _statement.Append(" AND ");

                        _selectedList = _statementDefinition.Filters;

                        Visit((StripQuotes(m.Arguments.Last()) as LambdaExpression).Body);
                    }
                    break;

                case "Select":
                    LambdaExpression lambdaSelect = (LambdaExpression)StripQuotes(m.Arguments.Last());
                    Visit(m.Arguments.Last());
                    _statementDefinition.Select = lambdaSelect.Compile();
                    Visit(m.Arguments.First());
                    break;

                case "Contains":
                    break;

                case "GroupBy":
                    break;

                case "Any":
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
                    Visit(u.Operand);
                    break;

                case ExpressionType.Quote:
                    Visit(u.Operand);
                    break;

                case ExpressionType.Convert:
                    Visit(u.Operand);
                    break;

                default:
                    throw new NotSupportedException(string.Format("El operador unario '{0}' no es soportado", u.NodeType));
            }

            return u;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
                e = ((UnaryExpression)e).Operand;

            return e;
        }

        /// <summary>
        /// Obtiene el alias de la definición de la entidad. Si la entidad no tiene alias, se busca
        /// entre las entidades que presente el mismo árbol de entidad.
        /// </summary>
        /// <param name="ownerEntity">Entidad a obtener el alias</param>
        /// <returns>El alias de la entidad.</returns>
        private string GetAlias(DbEntityDefinition ownerEntity)
        {
            if (ownerEntity == null)
                throw new ArgumentNullException(nameof(ownerEntity), "La entidad no puede ser nula -->");

            if (!String.IsNullOrEmpty(ownerEntity.Alias))
                return ownerEntity.Alias;

            foreach (var entity in _statementDefinition.Entities)
            {
                var currentEntity = entity;
                var anotherEntity = ownerEntity.GetRoot();
                var isSame = false;

                while (currentEntity != null && anotherEntity != null)
                {
                    if (!(isSame = anotherEntity.EntityType == currentEntity.EntityType))
                        break;

                    if (currentEntity.EntityType == ownerEntity.EntityType)
                        break;

                    anotherEntity = anotherEntity.DependentsEntities.FirstOrDefault();
                    currentEntity = currentEntity.DependentsEntities.FirstOrDefault(f => f.EntityType == anotherEntity.EntityType);
                }

                if (isSame)
                    return currentEntity.Alias;
            }

            throw new InvalidOperationException("No se encontró una entidad similar");
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

        private void ProcessDefinition()
        {
            int count = 0;

            foreach (var statement in _statementDefinition)
                foreach (var entity in statement.Entities)
                    count = ProcessEntity(count, entity);

            foreach (var label in _fieldLabels)
                _statement.Replace(label.First().ToString(), GetAlias((label.Last() as DbFieldDefinition).OwnerEntity));

            _statement
                .Replace(", {{fields}}", "")
                .Replace(" {{entities}}", "");
        }

        private int ProcessEntity(int count, DbEntityDefinition entity)
        {
            StringBuilder fieldsString = new StringBuilder();
            String entityName = DbHelper.GetEntityName(entity.EntityType);

            entity.Alias = String.Format("T{0}", count++);

            foreach (var dbFields in DbHelper.GetFields(entity.EntityType).Where(f => String.IsNullOrEmpty(f.ForeignKeyName)))
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
                    entity.DependencyField.GetFieldName(),
                    "{{entities}}"
                ));

            foreach (var subEntity in entity.DependentsEntities)
                count = ProcessEntity(count, subEntity);

            return count;
        }

        public abstract class ProjectionRow
        {
            public abstract object GetValue(int index);
        }

        internal class ColumnProjector : ExpressionVisitor
        {
            private static MethodInfo GetValue;
            private int iColumn;
            private ParameterExpression row;

            internal ColumnProjector()
            {
                if (GetValue == null)
                {
                    GetValue = typeof(ProjectionRow).GetMethod("GetValue");
                }
            }

            internal Expression ProjectColumns(Expression expression, ParameterExpression row)
            {
                this.row = row;

                Expression selector = this.Visit(expression);

                return selector;
            }

            protected override Expression VisitMember(MemberExpression m)
            {
                if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
                {
                    return Expression.Convert(Expression.Call(this.row, GetValue, Expression.Constant(iColumn++)), m.Type);
                }
                else
                {
                    return base.VisitMember(m);
                }
            }
        }
    }
}