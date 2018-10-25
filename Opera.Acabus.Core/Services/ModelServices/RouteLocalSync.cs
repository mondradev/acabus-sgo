using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;
using System;
using System.Linq;

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
        /// Determina si la ruta existe localmente.
        /// </summary>
        /// <param name="source">Secuencia local de datos.</param>
        /// <param name="instance">Ruta a verificar.</param>
        /// <returns></returns>
        protected override bool Exists(IQueryable<Route> source, Route instance)
            => source.Where(x => x.ID == instance.ID).ToList().Any();

        /// <summary>
        /// Obtiene una ruta a partir de una secuencia de bytes.
        /// </summary>
        /// <param name="source">Fuente de datos.</param>
        /// <returns>Una instancia de ruta.</returns>
        protected override Route FromBytes(byte[] source)
            => ModelHelper.GetRoute(source);

        /// <summary>
        /// Obtiene el identificador de la ruta.
        /// </summary>
        /// <param name="instance">Instancia actual.</param>
        /// <returns>Identificador de la ruta.</returns>
        protected override object GetID(Route instance)
        {
            if (instance == null)
                throw new LocalSyncException("La instancia de Route no puede ser nulo.");

            return instance.ID;
        }

        /// <summary>
        /// Asigna las propiedades de la ruta en los campos del mensaje utilizado para la creación
        /// en el servidor.
        /// </summary>
        /// <param name="route">Instancia de la ruta.</param>
        /// <param name="message">Mensaje de la petición de creación.</param>
        protected override void InstanceToMessage(Route route, IMessage message)
        {
            message[12] = route.RouteNumber;
            message[17] = route.Name;
            message[13] = (int)route.Type;
            message[18] = route.AssignedSection;
        }

        /// <summary>
        /// Obtiene una instancia almacenada en el contexto local la cual coincide con el identificador especificado.
        /// </summary>
        /// <typeparam name="TID">Tipo del identificador.</typeparam>
        /// <param name="id">Identificador de la instancia.</param>
        /// <returns>Instancia leida del contexto local.</returns>
        protected override Route LocalReadByID<TID>(TID id)
        {
            UInt64 routeID = UInt64.Parse(id.ToString());
            return LocalContext.Read<Route>().FirstOrDefault(x => x.ID == routeID);
        }

        /// <summary>
        /// Obtiene una secuencia de datos a partir de la instancia de ruta.
        /// </summary>
        /// <param name="instance">Instancia de ruta.</param>
        /// <returns>Una secuencia de datos.</returns>
        protected override byte[] ToBytes(Route instance)
            => instance.Serialize();
    }
}