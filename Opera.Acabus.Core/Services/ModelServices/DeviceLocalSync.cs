using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;
using System;

namespace Opera.Acabus.Core.Services.ModelServices
{
    /// <summary>
    /// Mantiene sincronizada la entidad <see cref="Device"/>.
    /// </summary>
    public sealed class DeviceLocalSync : EntityLocalSyncBase<Device>
    {
        /// <summary>
        /// Crea una nueva instancia.
        /// </summary>
        public DeviceLocalSync() : base("Station", "Bus") { }

        /// <summary>
        /// Obtiene el identificador del campo utilizado para el ID de la entidad.
        /// </summary>
        protected override int IDField => 14;

        /// <summary>
        /// Obtiene el identificador del campo utilizado para almacenar esta entidad en bytes.
        /// </summary>
        protected override int SourceField => 61;

        /// <summary>
        /// Obtiene un equipo a partir de una secuencia de bytes.
        /// </summary>
        /// <param name="source">Fuente de datos.</param>
        /// <returns>Una instancia de equipo.</returns>
        protected override Device Deserialize(byte[] source)
            => DataHelper.GetDevice(source);

        /// <summary>
        /// Asigna las propiedades del equipo en los campos del mensaje utilizado para la creación
        /// en el servidor.
        /// </summary>
        /// <param name="device">Instancia del equipo.</param>
        /// <param name="message">Mensaje de la petición de creación.</param>
        protected override void ToMessage(Device device, IMessage message)
        {
            if (!String.IsNullOrEmpty(device.SerialNumber))
                message[17] = device.SerialNumber;

            message.SetEnum(14, device.Type);
            message[13] = device.Station?.ID ?? 0;
            message[36] = device.Bus?.ID ?? 0;
            message[18] = device.IPAddress.ToString();
        }

        /// <summary>
        /// Obtiene una secuencia de datos a partir de la instancia de equipo.
        /// </summary>
        /// <param name="instance">Instancia de equipo.</param>
        /// <returns>Una secuencia de datos.</returns>
        protected override byte[] Serialize(Device instance)
            => instance.Serialize();
    }
}