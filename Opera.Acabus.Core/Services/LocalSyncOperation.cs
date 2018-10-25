namespace Opera.Acabus.Core.Services
{
    /// <summary>
    /// Representa las operaciones que desencadena un evento en la clase <see cref="EntityLocalSyncBase{T}"/>.
    /// </summary>
    public enum LocalSyncOperation
    {
        /// <summary>
        /// Se produce cuando una instancia es creada en el servidor.
        /// </summary>
        CREATE,

        /// <summary>
        /// Se produce cuando una instancia es actualizada en el servidor.
        /// </summary>
        UPDATE,

        /// <summary>
        /// Se produce cuando una instancia es eliminada del servidor.
        /// </summary>
        DELETE
    }
}