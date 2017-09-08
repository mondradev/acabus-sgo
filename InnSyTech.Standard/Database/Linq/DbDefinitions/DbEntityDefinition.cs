using InnSyTech.Standard.Database.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InnSyTech.Standard.Database.Linq.DbDefinitions
{
    /// <summary>
    /// Representa una estructura que ayuda a definir las entitidades involucradas en una sentencia
    /// SQL para la lectura de datos.
    /// </summary>
    internal sealed class DbEntityDefinition
    {
        /// <summary>
        /// Entidad de dependencia.
        /// </summary>
        private DbEntityDefinition _dependencyEntity;

        /// <summary>
        /// Campo de la dependencia.
        /// </summary>
        private DbFieldDefinition _dependencyField;

        /// <summary>
        /// Crea una nueva instancia de <see cref="DbEntityDefinition"/>.
        /// </summary>
        public DbEntityDefinition()
        {
            DependentsEntities = new List<DbEntityDefinition>();
            Fields = new List<DbFieldDefinition>();
        }

        /// <summary>
        /// Crea una nueva instancia de <see cref="DbEntityDefinition"/> especificando su dependencia.
        /// </summary>
        /// <param name="dependencyEntity">Entidad de dependencia.</param>
        /// <param name="dependencyField">Campo de la referencia a la dependencia.</param>
        public DbEntityDefinition(DbEntityDefinition dependencyEntity, DbFieldDefinition dependencyField) : this()
        {
            _dependencyEntity = dependencyEntity;
            _dependencyField = dependencyField;
        }

        /// <summary>
        /// Obtiene o establece el Alias utilizado en la consulta SQL.
        /// </summary>
        public String Alias { get; set; }

        /// <summary>
        /// Obtiene la entidad de dependencia que se relaciona con esta.
        /// </summary>
        public DbEntityDefinition DependencyEntity => _dependencyEntity;

        /// <summary>
        /// Obtiene el campo utilizado como referencia para relacionar esta entidad con la dependencia.
        /// </summary>
        public DbFieldDefinition DependencyField => _dependencyField;

        /// <summary>
        /// Obtiene un listado de entidad dependientes de esta.
        /// </summary>
        public List<DbEntityDefinition> DependentsEntities { get; }

        /// <summary>
        /// Obtiene el tipo de entidad.
        /// </summary>
        public Type EntityType { get; set; }

        /// <summary>
        /// Obtiene el listado de los campos involucrados en la consulta a definir.
        /// </summary>
        public List<DbFieldDefinition> Fields { get; }

        /// <summary>
        /// Crea una nueva entidad de dependencia.
        /// </summary>
        /// <param name="member">Miembro utilizado para relacionar la dependencia.</param>
        /// <returns>La dependencia de esta entidad.</returns>
        public DbEntityDefinition CreateDependencyEntity(MemberInfo member)
        {
            if (member == null)
                return null;

            var entity = new DbEntityDefinition();

            _dependencyEntity = entity;
            _dependencyField = entity.CreateMember(member);

            entity.DependentsEntities.Add(this);

            return entity;
        }

        /// <summary>
        /// Crea una nueva entidad dependiente.
        /// </summary>
        /// <param name="member">Miembro que relaciona con la entidad a crear.</param>
        /// <returns>La nueva entidad dependiente.</returns>
        public DbEntityDefinition CreateDependentsEntity(MemberInfo member)
        {
            if (member == null)
                return null;

            var entity = new DbEntityDefinition();

            DependentsEntities.Add(entity);

            entity._dependencyEntity = this;
            entity._dependencyField = CreateMember(member);

            return entity;
        }

        /// <summary>
        /// Crea un campo de la entidad.
        /// </summary>
        /// <param name="member">Miembro que representa el campo.</param>
        /// <returns>El campo nuevo.</returns>
        public DbFieldDefinition CreateMember(MemberInfo member)
            => new DbFieldDefinition(member, this);

        /// <summary>
        /// Obtiene el nodo raiz del árbol de relación de la entidad.
        /// </summary>
        /// <returns>La entidad padre.</returns>
        public DbEntityDefinition GetRoot()
        {
            var entity = DependencyEntity ?? this;

            while (entity.DependencyEntity != null)
                entity = entity.DependencyEntity;

            return entity;
        }

        /// <summary>
        /// Establece una nueva dependencia a la entidad.
        /// </summary>
        /// <param name="dependency">Dependencia nueva.</param>
        public void SetDependency(DbEntityDefinition dependency)
        {
            _dependencyEntity = dependency;

            dependency.DependentsEntities
                .RemoveAll(e => e.EntityType == EntityType && e.DependencyField.Member == DependencyField.Member);

            dependency.DependentsEntities.Add(this);
        }

        /// <summary>
        /// Representa con una cadena la instancia actual.
        /// </summary>
        /// <returns>Una cadena que representa a la instancia.</returns>
        public override string ToString()
        {
            StringBuilder format = new StringBuilder();

            format.AppendFormat("Type: {0}, ", EntityType?.Name ?? "null");

            if (!String.IsNullOrEmpty(Alias))
                format.AppendFormat("Alias: {0}, ", Alias);

            if (DependencyEntity != null)
                format.AppendFormat("Dependency: {{ Entity: {0}, Field: {1}}}, ", DependencyEntity.EntityType?.Name ?? "null", DependencyField.ToString() ?? "null");

            format.AppendFormat("Dependents: {0}, Fields: {1}", DependentsEntities.Count, Fields.Count);

            return format.ToString();
        }
    }
}