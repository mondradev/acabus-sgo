using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;

namespace Opera.Acabus.Core.Services.ModelServices
{
    /// <summary>
    /// Mantiene sincronizada la entidad <see cref="Bus"/>.
    /// </summary>
    public sealed class BusLocalSync : EntityLocalSyncBase<Bus>
    {
        /// <summary>
        /// Crea una nueva instancia.
        /// </summary>
        public BusLocalSync() : base("Route") { }

        /// <summary>
        /// Obtiene el identificador del campo utilizado para el ID de la entidad.
        /// </summary>
        protected override int IDField => 14;

        /// <summary>
        /// Obtiene el identificador del campo utilizado para almacenar esta entidad en bytes.
        /// </summary>
        protected override int SourceField => 61;

        /// <summary>
        /// Obtiene un autobus a partir de una secuencia de bytes.
        /// </summary>
        /// <param name="source">Fuente de datos.</param>
        /// <returns>Una instancia de autobus.</returns>
        protected override Bus Deserialize(IAdaptiveMessage source)
            => DataHelper.GetBus(source.GetBytes(SourceField));

        /// <summary>
        /// Asigna las propiedades del autobus en los campos del mensaje utilizado para la creación
        /// en el servidor.
        /// </summary>
        /// <param name="bus">Instancia del autobus.</param>
        /// <param name="message">Mensaje de la petición de creación.</param>
        protected override void ToMessage(Bus bus, IAdaptiveMessage message)
        {
            message.SetEnum(12, bus.Type);
            message[17] = bus.EconomicNumber;
            message[13] = bus.Route?.ID ?? 0;
            message.SetEnum(36, bus.Status);
            message[14] = bus.ID;
        }

        /// <summary>
        /// Obtiene una secuencia de datos a partir de la instancia de autobus.
        /// </summary>
        /// <param name="instance">Instancia de autobus.</param>
        /// <returns>Una secuencia de datos.</returns>
        protected override byte[] Serialize(Bus instance)
            => instance.Serialize();
    }
}