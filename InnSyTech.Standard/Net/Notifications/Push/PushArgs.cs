namespace InnSyTech.Standard.Net.Notifications.Push
{
    /// <summary>
    /// Representa los datos que surgen al desencadenarse los eventos en <see cref="PushService{T}"/>.
    /// </summary>
    public sealed class PushArgs
    {
        /// <summary>
        /// Crea una nueva instancia especificando sus datos.
        /// </summary>
        /// <param name="data">Datos del evento.</param>
        public PushArgs(object data)
        {
            Data = data;
        }

        /// <summary>
        /// Obtiene los datos del evento.
        /// </summary>
        public object Data { get; }
    }
}