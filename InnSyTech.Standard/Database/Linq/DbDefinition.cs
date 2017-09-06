using InnSyTech.Standard.Database.Utils;
using InnSyTech.Standard.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InnSyTech.Standard.Database.Linq
{
    internal sealed class DbEntityDefinition
    {
        public DbEntityDefinition()
        {
            DependentsEntities = new List<DbEntityDefinition>();
            Members = new List<DbFieldDefinition>();
        }

        public String Alias { get; set; }

        public DbEntityDefinition DependencyEntity { get; set; }

        public DbFieldDefinition DependencyMember { get; set; }

        public List<DbEntityDefinition> DependentsEntities { get; }

        public Type EntityType { get; set; }

        public List<DbFieldDefinition> Members { get; }

        public DbEntityDefinition CreateDependencyEntity(MemberInfo member)
        {
            if (member == null)
                return null;

            var entity = new DbEntityDefinition();

            entity.DependentsEntities.Add(this);
            DependencyEntity = entity;
            DependencyMember = entity.CreateMember(member);

            return entity;
        }

        public DbEntityDefinition CreateDependentsEntity(MemberInfo member)
        {
            if (member == null)
                return null;

            var entity = new DbEntityDefinition();

            DependentsEntities.Add(entity);

            entity.DependencyEntity = this;
            entity.DependencyMember = CreateMember(member);

            return entity;
        }

        public DbFieldDefinition CreateMember(MemberInfo member)
            => new DbFieldDefinition(member, this);

        public DbEntityDefinition GetRoot()
        {
            var entity = DependencyEntity ?? this;

            while (entity.DependencyEntity != null)
                entity = entity.DependencyEntity;

            return entity;
        }

        public override string ToString()
        {
            StringBuilder format = new StringBuilder();

            format.AppendFormat("Type: {0}, ", EntityType?.Name ?? "null");

            if (!String.IsNullOrEmpty(Alias))
                format.AppendFormat("Alias: {0}, ", Alias);

            if (DependencyEntity != null)
                format.AppendFormat("Dependency: {{ Entity: {{{0}}}, Member: {1}}}, ", DependencyEntity, DependencyMember.ToString() ?? "null");

            format.AppendFormat("Dependents: {0}, Member: {1}", DependentsEntities.Count, Members.Count);

            return format.ToString();
        }
    }

    internal sealed class DbFieldDefinition
    {
        public DbFieldDefinition(MemberInfo member, DbEntityDefinition entity)
        {
            Entity = entity;
            Member = member;
            Entity.Members.Add(this);
        }

        public DbEntityDefinition Entity { get; set; }
        public MemberInfo Member { get; set; }

        public override string ToString()
            => String.Format("{0}", DbHelper.GetFieldName(Member, Entity?.EntityType));

        public String GetFieldName()
            => DbHelper.GetFieldName(Member, Entity.EntityType);
    }

    internal sealed class DbStatementDefinition : IEnumerable<DbStatementDefinition>
    {
        public DbStatementDefinitionEnumerator _enumerator;

        public DbStatementDefinition()
        {
            Entities = new List<DbEntityDefinition>();
            Filters = new List<DbFieldDefinition>();
            Orders = new List<DbFieldDefinition>();
        }

        public List<DbEntityDefinition> Entities { get; }
        public List<DbFieldDefinition> Filters { get; }
        public List<DbFieldDefinition> Orders { get; }
        public DbStatementDefinition SubStatementDefinition { get; set; }

        public void AddEntity(DbEntityDefinition entity)
        {
            DbEntityDefinition rootEntity = entity.GetRoot();

            for (int i = 0; i < Entities.Count; i++)
            {
                if (TryMerge(rootEntity, Entities[i], out DbEntityDefinition result))
                    Entities[i] = result;
            }

            foreach (var entityAdded in Entities)
                if (entityAdded.EntityType == rootEntity.EntityType)
                    return;

            Entities.Add(rootEntity);
        }

        public DbStatementDefinition ConvertToSubStatement()
                    => new DbStatementDefinition() { SubStatementDefinition = this };

        public IEnumerator<DbStatementDefinition> GetEnumerator()
            => _enumerator ?? (_enumerator = new DbStatementDefinitionEnumerator(this));

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public override string ToString()
        {
            return base.ToString();
        }

        public bool TryMerge(DbEntityDefinition entity, DbEntityDefinition anotherEntity, out DbEntityDefinition result)
        {
            result = null;

            if (entity.EntityType != anotherEntity.EntityType)
                return false;

            result = MergeDependents(entity, anotherEntity);

            return true;
        }

        private DbEntityDefinition MergeDependents(DbEntityDefinition entity, DbEntityDefinition anotherEntity)
        {
            var dependents = new[]
            {
                entity.DependentsEntities,
                anotherEntity.DependentsEntities
            }
            .Merge()
            .GroupBy(e => e.EntityType);

            var members = new[]
            {
                entity.Members,
                anotherEntity.Members
            }
            .Merge()
            .GroupBy(m => m.Member)
            .Select(g => g.First());

            var uniques = dependents.Where(g => g.Count() == 1);
            var toMerge = dependents.Where(g => g.Count() > 1);

            var entityMerge = new DbEntityDefinition()
            {
                EntityType = entity.EntityType,
                DependencyEntity = entity.DependencyEntity,
                DependencyMember = entity.DependencyMember,
                Alias = entity.Alias
            };

            foreach (var m in members)
            {
                m.Entity = entityMerge;
                entityMerge.Members.Add(m);
            }

            foreach (var g in uniques)
            {
                var dependentsEntity = g.First();
                dependentsEntity.DependencyEntity = entityMerge;
                entityMerge.DependentsEntities.Add(dependentsEntity);
            }

            foreach (var g in toMerge)
            {
                var dependentsEntity = MergeDependents(g.First(), g.Last());
                dependentsEntity.DependencyEntity = entityMerge;
                entityMerge.DependentsEntities.Add(dependentsEntity);
            }

            return entityMerge;
        }
    }

    internal class DbStatementDefinitionEnumerator : IEnumerator<DbStatementDefinition>
    {
        private DbStatementDefinition _current;
        private DbStatementDefinition _root;

        public DbStatementDefinitionEnumerator(DbStatementDefinition root)
            => _root = root;

        public DbStatementDefinition Current => _current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _current = null;
            _root = null;
        }

        public bool MoveNext()
        {
            if (Current is null)
                return (_current = _root) != null;

            return (_current = Current.SubStatementDefinition) != null;
        }

        public void Reset()
            => _current = _root;
    }
}