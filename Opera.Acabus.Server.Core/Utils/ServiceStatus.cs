namespace Opera.Acabus.Server.Core.Utils
{
    /// <summary>
    /// Especifica los posibles estados de algún servicio del servidor de aplicación.
    /// </summary>
    public enum ServiceStatus
    {
        /// <summary>
        /// Apagado
        /// </summary>
        OFF,

        /// <summary>
        /// Encendido
        /// </summary>
        ON,

        /// <summary>
        /// Con error
        /// </summary>
        ERROR,

        /// <summary>
        /// Con alerta
        /// </summary>
        WARN
    }
}