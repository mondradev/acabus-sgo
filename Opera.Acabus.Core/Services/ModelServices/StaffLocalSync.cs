using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;
using System;
using System.Linq;

namespace Opera.Acabus.Core.Services.ModelServices
{
    /// <summary>
    /// Mantiene sincronizada la entidad <see cref="Staff"/>.
    /// </summary>
    public sealed class StaffLocalSync : EntityLocalSyncBase<Staff>
    {
        /// <summary>
        /// Obtiene el identificador del campo utilizado para el ID de la entidad.
        /// </summary>
        protected override int IDField => 14;

        /// <summary>
        /// Obtiene el identificador del campo utilizado para almacenar esta entidad en bytes.
        /// </summary>
        protected override int SourceField => 61;

        /// <summary>
        /// Determina si el personal existe localmente.
        /// </summary>
        /// <param name="source">Secuencia local de datos.</param>
        /// <param name="instance">Personal a verificar.</param>
        /// <returns>Un valor true si existe el personal.</returns>
        protected override bool Exists(IQueryable<Staff> source, Staff instance)
            => source.Where(x => x.ID == instance.ID).ToList().Any();

        /// <summary>
        /// Obtiene un miembro del personal a partir de una secuencia de bytes.
        /// </summary>
        /// <param name="source">Fuente de datos.</param>
        /// <returns>Una instancia de personal.</returns>
        protected override Staff FromBytes(byte[] source)
            => ModelHelper.GetStaff(source);

        /// <summary>
        /// Obtiene el identificador del personal.
        /// </summary>
        /// <param name="instance">Instancia actual.</param>
        /// <returns>Identificador del personal.</returns>
        protected override object GetID(Staff instance)
        {
            if (instance == null)
                throw new LocalSyncException("La instancia de Staff no puede ser nulo.");

            return instance.ID;
        }

        /// <summary>
        /// Asigna las propiedades del personal en los campos del mensaje utilizado para la creación
        /// en el servidor.
        /// </summary>
        /// <param name="staff">Instancia del personal.</param>
        /// <param name="message">Mensaje de la petición de creación.</param>
        protected override void InstanceToMessage(Staff staff, IMessage message)
        {
            message[12] = (int)staff.Area;
            message[17] = staff.Name;
            message[23] = staff.Active;
        }

        /// <summary>
        /// Obtiene una instancia almacenada en el contexto local la cual coincide con el identificador especificado.
        /// </summary>
        /// <typeparam name="TID">Tipo del identificador.</typeparam>
        /// <param name="id">Identificador de la instancia.</param>
        /// <returns>Instancia leida del contexto local.</returns>
        protected override Staff LocalReadByID<TID>(TID id)
        {
            UInt64 staffID = UInt64.Parse(id.ToString());
            return LocalContext.Read<Staff>().FirstOrDefault(x => x.ID == staffID);
        }

        /// <summary>
        /// Obtiene una secuencia de datos a partir de la instancia de personal.
        /// </summary>
        /// <param name="instance">Instancia de personal.</param>
        /// <returns>Una secuencia de datos.</returns>
        protected override byte[] ToBytes(Staff instance)
            => instance.Serialize();
    }
}