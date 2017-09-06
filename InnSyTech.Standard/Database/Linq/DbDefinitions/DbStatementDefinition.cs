using InnSyTech.Standard.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InnSyTech.Standard.Database.Linq.DbDefinitions
{
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
}