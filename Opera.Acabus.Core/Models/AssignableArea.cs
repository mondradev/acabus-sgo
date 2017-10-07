namespace Opera.Acabus.Core.Models
{
    /// <summary>
    /// Define las áreas asignables de actividades.
    /// </summary>
    public enum AssignableArea
    {
        /// <summary>
        /// Toda las areas (Valor predeterminado).
        /// </summary>
        EVERYBODY = 0,

        /// <summary>
        /// Área de mantenimiento.
        /// </summary>
        MANTTO = 1,

        /// <summary>
        /// Área de supervisión de mantenimiento.
        /// </summary>
        SUPERVISOR = 2,

        /// <summary>
        /// Área de soporte técnico.
        /// </summary>
        SUPPORT = 4,

        /// <summary>
        /// Área de base de datos.
        /// </summary>
        DATABASE = 8,

        /// <summary>
        /// Área de gerencia TI.
        /// </summary>
        IT_MANAGER = 16
    }
}