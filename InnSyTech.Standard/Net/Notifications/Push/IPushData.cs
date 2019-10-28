namespace InnSyTech.Standard.Net.Notifications.Push
{
    /// <summary>
    /// Provee de las funciones básicas de los datos para las notificaciones push.
    /// </summary>
    public interface IPushData
    {
        /// <summary>
        /// Nombre de la entidad del evento.
        /// </summary>
        string EntityName { get; }

        /// <summary>
        /// Identificador de la entidad del evento.
        /// </summary>
        ulong ID { get; }

        /// <summary>
        /// Representa a la instancia actual en una cadena de caracteres.
        /// </summary>
        /// <returns>Una cadena de caracteres.</returns>
        string ToString();
    }
}