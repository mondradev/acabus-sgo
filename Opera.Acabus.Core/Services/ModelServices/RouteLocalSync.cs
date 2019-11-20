using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;
using System;

namespace Opera.Acabus.Core.Services.ModelServices
{
    /// <summary>
    /// Mantiene sincronizada la entidad <see cref="Route"/>.
    /// </summary>
    public sealed class RouteLocalSync : EntityLocalSyncBase<Route>
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
        /// Obtiene una ruta a partir de una secuencia de bytes.
        /// </summary>
        /// <param name="source">Fuente de datos.</param>
        /// <returns>Una instancia de ruta.</returns>
        protected override Route Deserialize(IAdaptiveMessage source)
            => DataHelper.GetRoute(source.GetBytes(SourceField));

        /// <summary>
        /// Asigna las propiedades de la ruta en los campos del mensaje utilizado para la creación
        /// en el servidor.
        /// </summary>
        /// <param name="route">Instancia de la ruta.</param>
        /// <param name="message">Mensaje de la petición de creación.</param>
        protected override void ToMessage(Route route, IAdaptiveMessage message)
        {
            message[12] = route.RouteNumber;
            message[14] = route.ID;
            message[17] = route.Name;
            message.SetEnum(13, route.Type);

            if (!String.IsNullOrEmpty(route.AssignedSection))
                message[18] = route.AssignedSection;
        }

        /// <summary>
        /// Obtiene una secuencia de datos a partir de la instancia de ruta.
        /// </summary>
        /// <param name="instance">Instancia de ruta.</param>
        /// <returns>Una secuencia de datos.</returns>
        protected override byte[] Serialize(Route instance)
            => instance.Serialize();
    }
}