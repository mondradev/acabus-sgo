using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;

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
        /// Obtiene un miembro del personal a partir de una secuencia de bytes.
        /// </summary>
        /// <param name="source">Fuente de datos.</param>
        /// <returns>Una instancia de personal.</returns>
        protected override Staff Deserialize(IAdaptiveMessage source)
            => DataHelper.GetStaff(source.GetBytes(SourceField));

        /// <summary>
        /// Asigna las propiedades del personal en los campos del mensaje utilizado para la creación
        /// en el servidor.
        /// </summary>
        /// <param name="staff">Instancia del personal.</param>
        /// <param name="message">Mensaje de la petición de creación.</param>
        protected override void ToMessage(Staff staff, IAdaptiveMessage message)
        {
            message.SetEnum(12, staff.Area);
            message[17] = staff.Name;
        }

        /// <summary>
        /// Obtiene una secuencia de datos a partir de la instancia de personal.
        /// </summary>
        /// <param name="instance">Instancia de personal.</param>
        /// <returns>Una secuencia de datos.</returns>
        protected override byte[] Serialize(Staff instance)
            => instance.Serialize();
    }
}