using InnSyTech.Standard.Net.Notifications.Push;
using System.Text.RegularExpressions;

namespace Opera.Acabus.Core.Services
{
    /// <summary>
    /// Notificación Push para las notificaciones del servidor.
    /// </summary>
    public sealed class PushAcabus : IPushData
    {
        /// <summary>
        /// Crea una nueva instancia.
        /// </summary>
        public PushAcabus(string entityName, string id, LocalSyncOperation operation)
        {
            EntityName = entityName;
            ID = id;
            Operation = operation;
        }

        /// <summary>
        /// Nombre de la entidad del evento.
        /// </summary>
        public string EntityName { get; private set; }

        /// <summary>
        /// Identificador de la entidad del evento.
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// Operación que desencadenó el evento.
        /// </summary>
        public LocalSyncOperation Operation { get; private set; }

        /// <summary>
        /// Convierte una cadena de texto con el formato valido en una instancia de <see cref="PushAcabus"/>.
        /// </summary>
        /// <param name="src">Cadena de texto con formato valido.</param>
        /// <returns>Una instancia de <see cref="PushAcabus"/>.</returns>
        public static PushAcabus Parse(string src)
        {
            GroupCollection group = Regex.Match(src, "(.*):(.*):(.*)").Groups;
            PushAcabus push = new PushAcabus(
                group[1]?.Value,
                group[2]?.Value,
                (LocalSyncOperation)int.Parse(group[3]?.Value)
            );

            return push;
        }

        /// <summary>
        /// Representa a la instancia actual en una cadena de caracteres.
        /// </summary>
        /// <returns>Una cadena de caracteres.</returns>
        public override string ToString()
            => string.Format("{0}:{1}:{2}", EntityName, ID, (int)Operation);
    }
}