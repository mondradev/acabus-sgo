namespace Opera.Acabus.TrunkMonitor.Models
{
    /// <summary>
    /// Define los estados de conexión de un enlace.
    /// </summary>
    public enum LinkState
    {
        /// <summary>
        /// Conexión con enlace desconectado.
        /// </summary>
        DISCONNECTED,

        /// <summary>
        /// Conexión con enlace malo.
        /// </summary>
        BAD,

        /// <summary>
        /// Conexión con enlace medio.
        /// </summary>
        MEDIUM,

        /// <summary>
        /// Conexión con enlace bueno.
        /// </summary>
        GOOD
    }
}