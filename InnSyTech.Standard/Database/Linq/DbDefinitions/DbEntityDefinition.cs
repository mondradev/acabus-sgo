using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace InnSyTech.Standard.Database.Linq.DbDefinitions
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
}