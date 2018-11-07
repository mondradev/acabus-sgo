namespace Opera.Acabus.Core.Models
{
    /// <summary>
    /// Define los estados posibles de un autobus.
    /// </summary>
    public enum BusStatus
    {
        /// <summary>
        /// En operación (Valor predeterminado).
        /// </summary>
        OPERATIONAL = 0,

        /// <summary>
        /// En reparación o taller mecánico.
        /// </summary>
        IN_REPAIR = 1,

        /// <summary>
        /// Sin energía en baterías.
        /// </summary>
        WITHOUT_ENERGY = 2,

        /// <summary>
        /// Otras razones.
        /// </summary>
        OTHERS_REASONS = 4
    }
}