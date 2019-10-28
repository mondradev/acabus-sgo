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
        /// Crea una nueva instancia.
        /// </summary>
        public StationLocalSync() : base("Route") { }

        /// <summary>
        /// Obtiene el identificador del campo utilizado para el ID de la entidad.
        /// </summary>
        protected override int IDField => 14;

        /// <summary>
        /// Obtiene el identificador del campo utilizado para almacenar esta entidad en bytes.
        /// </summary>
        protected override int SourceField => 61;

        /// <summary>
        /// Obtiene una estación a partir de una secuencia de bytes.
        /// </summary>
        /// <param name="source">Fuente de datos.</param>
        /// <returns>Una instancia de estación.</returns>
        protected override Station Deserialize(IAdaptiveMessage source)
            => DataHelper.GetStation(source.GetBytes(SourceField));

        /// <summary>
        /// Asigna las propiedades de la estación en los campos del mensaje utilizado para la creación
        /// en el servidor.
        /// </summary>
        /// <param name="station">Instancia de la estación.</param>
        /// <param name="message">Mensaje de la petición de creación.</param>
        protected override void ToMessage(Station station, IAdaptiveMessage message)
        {
            message[12] = station.StationNumber;
            message[17] = station.Name;
            message[13] = station.Route?.ID ?? 0;
            message.SetBoolean(23, station.IsExternal);

            if (!String.IsNullOrEmpty(station.AssignedSection))
                message[18] = station.AssignedSection;
        }

        /// <summary>
        /// Obtiene una secuencia de datos a partir de la instancia de estación.
        /// </summary>
        /// <param name="instance">Instancia de estación.</param>
        /// <returns>Una secuencia de datos.</returns>
        protected override byte[] Serialize(Station instance)
            => instance.Serialize();
    }
}