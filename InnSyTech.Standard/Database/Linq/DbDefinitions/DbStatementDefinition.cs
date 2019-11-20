using InnSyTech.Standard.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InnSyTech.Standard.Database.Linq.DbDefinitions
{
    /// <summary>
    /// Representa una estructura que ayuda a definir una sentencia SQL utilizada para la lectura de datos.
    /// </summary>
    internal sealed class DbStatementDefinition : IEnumerable<DbStatementDefinition>
    {
        /// <summary>
        /// Enumerador de la definición.
        /// </summary>
        public DbStatementDefinitionEnumerator _enumerator;

        /// <summary>
        /// Crea una nueva instancia de <see cref="DbStatementDefinition"/>.
        /// </summary>
        public DbStatementDefinition()
        {
            Entities = new List<DbEntityDefinition>();
            Filters = new List<DbFieldDefinition>();
            Orders = new List<DbFieldDefinition>();

            IsEnumerable = true;
        }

        /// <summary>
        /// Obtiene o establece la cantidad de elementos que se obtendrán.
        /// </summary>
        public int CountToTake { get; set; }

        /// <summary>
        /// Indica si es un conteo de elementos.
        /// </summary>
        public Boolean IsCountFunc { get; set; }

        /// <summary>
        /// Indica si obtiene un bandera la consulta.
        /// </summary>
        public Boolean IsAnyFunc { get; set; }

        /// <summary>
        /// Obtiene un listado de todas las entidades involucradas en la sentencia.
        /// </summary>
        public List<DbEntityDefinition> Entities { get; }

        /// <summary>
        /// Obtiene un listado de todos los campos utilizados para filtrar la sentencia.
        /// </summary>
        public List<DbFieldDefinition> Filters { get; }

        /// <summary>
        /// Obtiene o establece si el resultado será una secuancia.
        /// </summary>
        public bool IsEnumerable { get; set; }

        /// <summary>
        /// Obtiene un listado de todos los campos utilizados para realizar el ordenamiento.
        /// </summary>
        public List<DbFieldDefinition> Orders { get; }

        /// <summary>
        /// Nivel de profundidad de la referencia.
        /// </summary>
        public int ReferenceDepth { get; set; }

        /// <summary>
        /// Obtiene el delegado de la salida seleccionada.
        /// </summary>
        public Delegate Select { get; set; }

        /// <summary>
        /// Obtiene la sentencia secundaría.
        /// </summary>
        public DbStatementDefinition SubStatementDefinition { get; set; }

        /// <summary>
        /// Agrega una entidad nueva fusionandola con su igual.
        /// </summary>
        /// <param name="entity">Entidad a agregar.</param>
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

        /// <summary>
        /// Convierte en una sentencia secundaria la sentencia actual.
        /// </summary>
        /// <returns>La sentencia primaria.</returns>
        public DbStatementDefinition ConvertToSubStatement()
                    => new DbStatementDefinition() { SubStatementDefinition = this };

        /// <summary>
        /// Obtiene el enumerador genérico de la sentencia.
        /// </summary>
        /// <returns>El enumerador de la sentencia.</returns>
        public IEnumerator<DbStatementDefinition> GetEnumerator()
            => _enumerator ?? (_enumerator = new DbStatementDefinitionEnumerator(this));

        /// <summary>
        /// Obtiene el enumerador de la sentencia.
        /// </summary>
        /// <returns>El enumerador de la sentencia.</returns>
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// Intenta fusionar dos entidades obteniendo como resultado la nueva con sus atributos unidos.
        /// </summary>
        /// <param name="entity">Una entidad a fusionar.</param>
        /// <param name="anotherEntity">Otra entidad a fusionar.</param>
        /// <param name="result">La entidad resultado de la fusión.</param>
        /// <returns>Un valor de true en caso de fusionar correctamente.</returns>
        public bool TryMerge(DbEntityDefinition entity, DbEntityDefinition anotherEntity, out DbEntityDefinition result)
        {
            result = null;

            if (entity.EntityType != anotherEntity.EntityType)
                return false;

            result = MergeDependents(entity, anotherEntity);

            return true;
        }

        /// <summary>
        /// Fusiona las dependientes de dos entidades y devuelve la entidad resultante.
        /// </summary>
        /// <param name="entity">Una entidad a fusionar.</param>
        /// <param name="anotherEntity">Otra entidad a fusionar.</param>
        /// <returns>Una entidad que presenta ambos conjuntos de dependientes.</returns>
        private DbEntityDefinition MergeDependents(DbEntityDefinition entity, DbEntityDefinition anotherEntity)
        {
            var dependents = new[]
            {
                entity.DependentsEntities,
                anotherEntity.DependentsEntities
            }
            .Merge()
            .GroupBy(e => String.Format("{0}:{1}", e.EntityType.FullName, e.DependencyField.GetFieldName()));

            var members = new[]
            {
                entity.Fields,
                anotherEntity.Fields
            }
            .Merge()
            .GroupBy(m => m.Member)
            .Select(g => g.First());

            var uniques = dependents.Where(g => g.Count() == 1);
            var toMerge = dependents.Where(g => g.Count() > 1);

            var entityMerge = new DbEntityDefinition(entity.DependencyEntity, entity.DependencyField)
            {
                EntityType = entity.EntityType,
                Alias = entity.Alias
            };

            new[] { entity.Fields, anotherEntity.Fields }
            .Merge()
            .ToList()
            .ForEach(m => m.SetEntityOwner(entityMerge));

            foreach (var m in members)
                m.SetEntityOwner(entityMerge);

            foreach (var g in uniques)
            {
                var dependentsEntity = g.First();
                dependentsEntity.SetDependency(entityMerge);
            }

            foreach (var g in toMerge)
            {
                var dependentsEntity = MergeDependents(g.First(), g.Last());
                dependentsEntity.SetDependency(entityMerge);
            }

            return entityMerge;
        }
    }
}