using InnSyTech.Standard.Database.Utils;
using System;
using System.Reflection;

namespace InnSyTech.Standard.Database.Linq.DbDefinitions
{
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
}