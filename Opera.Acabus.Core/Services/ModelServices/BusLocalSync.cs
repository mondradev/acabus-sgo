using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;
using System;
using System.Linq;

namespace Opera.Acabus.Core.Services.ModelServices
{
    /// <summary>
    /// Mantiene sincronizada la entidad <see cref="Bus"/>.
    /// </summary>
    public sealed class BusLocalSync : EntityLocalSyncBase<Bus>
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
        /// Determina si el autobus existe localmente.
        /// </summary>
        /// <param name="source">Secuencia local de datos.</param>
        /// <param name="instance">Autobus a verificar.</param>
        /// <returns></returns>
        protected override bool Exists(IQueryable<Bus> source, Bus instance)
            => source.Where(x => x.ID == instance.ID).ToList().Any();

        /// <summary>
        /// Obtiene un autobus a partir de una secuencia de bytes.
        /// </summary>
        /// <param name="source">Fuente de datos.</param>
        /// <returns>Una instancia de autobus.</returns>
        protected override Bus FromBytes(byte[] source)
            => ModelHelper.GetBus(source);

        /// <summary>
        /// Obtiene el identificador del autobus.
        /// </summary>
        /// <param name="instance">Instancia actual.</param>
        /// <returns>Identificador del autobus.</returns>
        protected override object GetID(Bus instance)
        {
            if (instance == null)
                throw new LocalSyncException("La instancia de Bus no puede ser nulo.");

            return instance.ID;
        }

        /// <summary>
        /// Asigna las propiedades del autobus en los campos del mensaje utilizado para la creación
        /// en el servidor.
        /// </summary>
        /// <param name="bus">Instancia del autobus.</param>
        /// <param name="message">Mensaje de la petición de creación.</param>
        protected override void InstanceToMessage(Bus bus, IMessage message)
        {
            message[12] = (int)bus.Type;
            message[17] = bus.EconomicNumber;
            message[13] = bus.Route?.ID ?? 0;
            message[36] = (int)bus.Status;
        }

        /// <summary>
        /// Obtiene una instancia almacenada en el contexto local la cual coincide con el identificador especificado.
        /// </summary>
        /// <typeparam name="TID">Tipo del identificador.</typeparam>
        /// <param name="id">Identificador de la instancia.</param>
        /// <returns>Instancia leida del contexto local.</returns>
        protected override Bus LocalReadByID<TID>(TID id)
        {
            UInt64 busID = UInt64.Parse(id.ToString());
            return LocalContext.Read<Bus>().FirstOrDefault(x => x.ID == busID);
        }

        /// <summary>
        /// Obtiene una secuencia de datos a partir de la instancia de autobus.
        /// </summary>
        /// <param name="instance">Instancia de autobus.</param>
        /// <returns>Una secuencia de datos.</returns>
        protected override byte[] ToBytes(Bus instance)
            => instance.Serialize();
    }
}