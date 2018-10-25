using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;
using System;
using System.Linq;

namespace Opera.Acabus.Core.Services.ModelServices
{
    /// <summary>
    /// Mantiene sincronizada la entidad <see cref="Device"/>.
    /// </summary>
    public sealed class DeviceLocalSync : EntityLocalSyncBase<Device>
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
        /// Determina si el equipo existe localmente.
        /// </summary>
        /// <param name="source">Secuencia local de datos.</param>
        /// <param name="instance">Equipo a verificar.</param>
        /// <returns>Un valor true si existe el equipo.</returns>
        protected override bool Exists(IQueryable<Device> source, Device instance)
            => source.Where(x => x.ID == instance.ID).ToList().Any();

        /// <summary>
        /// Obtiene un equipo a partir de una secuencia de bytes.
        /// </summary>
        /// <param name="source">Fuente de datos.</param>
        /// <returns>Una instancia de equipo.</returns>
        protected override Device FromBytes(byte[] source)
            => ModelHelper.GetDevice(source);

        /// <summary>
        /// Obtiene el identificador del equipo.
        /// </summary>
        /// <param name="instance">Instancia actual.</param>
        /// <returns>Identificador del equipo.</returns>
        protected override object GetID(Device instance)
        {
            if (instance == null)
                throw new LocalSyncException("La instancia de Device no puede ser nulo.");

            return instance.ID;
        }

        /// <summary>
        /// Asigna las propiedades del equipo en los campos del mensaje utilizado para la creación
        /// en el servidor.
        /// </summary>
        /// <param name="device">Instancia del equipo.</param>
        /// <param name="message">Mensaje de la petición de creación.</param>
        protected override void InstanceToMessage(Device device, IMessage message)
        {
            message[12] = device.Type;
            message[17] = device.SerialNumber;
            message[13] = device.Station?.ID ?? 0;
            message[36] = device.Bus?.ID ?? 0;
            message[18] = device.IPAddress.ToString();
        }

        /// <summary>
        /// Obtiene una instancia almacenada en el contexto local la cual coincide con el identificador especificado.
        /// </summary>
        /// <typeparam name="TID">Tipo del identificador.</typeparam>
        /// <param name="id">Identificador de la instancia.</param>
        /// <returns>Instancia leida del contexto local.</returns>
        protected override Device LocalReadByID<TID>(TID id)
        {
            UInt64 deviceID = UInt64.Parse(id.ToString());
            return LocalContext.Read<Device>().FirstOrDefault(x => x.ID == deviceID);
        }

        /// <summary>
        /// Obtiene una secuencia de datos a partir de la instancia de equipo.
        /// </summary>
        /// <param name="instance">Instancia de equipo.</param>
        /// <returns>Una secuencia de datos.</returns>
        protected override byte[] ToBytes(Device instance)
            => instance.Serialize();
    }
}