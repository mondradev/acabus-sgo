using InnSyTech.Standard.Database.Utils;
using System;
using System.Reflection;

namespace InnSyTech.Standard.Database.Linq.DbDefinitions
{
    /// <summary>
    /// Representa una estructura que ayuda a definir campos involucradas en una sentencia SQL para
    /// la lectura de datos.
    /// </summary>
    internal sealed class DbFieldDefinition
    {
        /// <summary>
        /// Entidad propietario del campo.
        /// </summary>
        private DbEntityDefinition _ownerEntity;

        /// <summary>
        /// Crea una nueva instancia de <see cref="DbFieldDefinition"/>.
        /// </summary>
        /// <param name="member">Miembro a representar.</param>
        /// <param name="ownerEntity">Entidad propietaria del campo.</param>
        public DbFieldDefinition(MemberInfo member, DbEntityDefinition ownerEntity)
        {
            _ownerEntity = ownerEntity;
            Member = member;
            OwnerEntity.Fields.Add(this);
        }

        /// <summary>
        /// Obtiene el miembro que representa el campo.
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// Obtiene la entidad propietaria del campo.
        /// </summary>
        public DbEntityDefinition OwnerEntity => _ownerEntity;

        /// <summary>
        /// Obtiene el nombre del campo en la base de datos.
        /// </summary>
        /// <returns></returns>
        public String GetFieldName()
            => DbHelper.GetFieldName(Member, OwnerEntity.EntityType);

        /// <summary>
        /// Establece una nueva entidad como dueño.
        /// </summary>
        /// <param name="ownerEntity">Entidad dueño.</param>
        public void SetEntityOwner(DbEntityDefinition ownerEntity)
        {
            ownerEntity.Fields.RemoveAll(f => f.Member == Member);

            _ownerEntity = ownerEntity;

            ownerEntity.Fields.Add(this);
        }

        /// <summary>
        /// Representa con una cadena la instancia actual.
        /// </summary>
        /// <returns>Una cadena que representa la instancia.</returns>
        public override string ToString()
                    => String.Format("{0}", DbHelper.GetFieldName(Member, OwnerEntity?.EntityType));
    }
}