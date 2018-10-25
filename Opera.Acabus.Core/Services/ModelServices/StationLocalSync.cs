using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;
using System;
using System.Linq;

namespace Opera.Acabus.Core.Services.ModelServices
{
    /// <summary>
    /// Mantiene sincronizada la entidad <see cref="Station"/>.
    /// </summary>
    public sealed class StationLocalSync : EntityLocalSyncBase<Station>
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
        /// Determina si la estación existe localmente.
        /// </summary>
        /// <param name="source">Secuencia local de datos.</param>
        /// <param name="instance">Estación a verificar.</param>
        /// <returns>Un valor true si existe la estación.</returns>
        protected override bool Exists(IQueryable<Station> source, Station instance)
            => source.Where(x => x.ID == instance.ID).ToList().Any();

        /// <summary>
        /// Obtiene una estación a partir de una secuencia de bytes.
        /// </summary>
        /// <param name="source">Fuente de datos.</param>
        /// <returns>Una instancia de estación.</returns>
        protected override Station FromBytes(byte[] source)
            => ModelHelper.GetStation(source);

        /// <summary>
        /// Obtiene el identificador de la estación.
        /// </summary>
        /// <param name="instance">Instancia actual.</param>
        /// <returns>Identificador de la estación.</returns>
        protected override object GetID(Station instance)
        {
            if (instance == null)
                throw new LocalSyncException("La instancia de Station no puede ser nulo.");

            return instance.ID;
        }

        /// <summary>
        /// Asigna las propiedades de la estación en los campos del mensaje utilizado para la creación
        /// en el servidor.
        /// </summary>
        /// <param name="station">Instancia de la estación.</param>
        /// <param name="message">Mensaje de la petición de creación.</param>
        protected override void InstanceToMessage(Station station, IMessage message)
        {
            message[12] = station.StationNumber;
            message[17] = station.Name;
            message[13] = station.Route?.ID ?? 0;
            message[18] = station.AssignedSection;
        }

        /// <summary>
        /// Obtiene una instancia almacenada en el contexto local la cual coincide con el identificador especificado.
        /// </summary>
        /// <typeparam name="TID">Tipo del identificador.</typeparam>
        /// <param name="id">Identificador de la instancia.</param>
        /// <returns>Instancia leida del contexto local.</returns>
        protected override Station LocalReadByID<TID>(TID id)
        {
            UInt64 stationID = UInt64.Parse(id.ToString());
            return LocalContext.Read<Station>().FirstOrDefault(x => x.ID == stationID);
        }

        /// <summary>
        /// Obtiene una secuencia de datos a partir de la instancia de estación.
        /// </summary>
        /// <param name="instance">Instancia de estación.</param>
        /// <returns>Una secuencia de datos.</returns>
        protected override byte[] ToBytes(Station instance)
            => instance.Serialize();
    }
}