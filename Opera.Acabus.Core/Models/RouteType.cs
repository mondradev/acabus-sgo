namespace Opera.Acabus.Core.Models
{
    /// <summary>
    /// Enumera todos los tipos de rutas que existen.
    /// </summary>
    public enum RouteType
    {
        /// <summary>
        /// Sin tipo de ruta (valor predeterminado).
        /// </summary>
        NONE = 0,

        /// <summary>
        /// Ruta tipo alimentador.
        /// </summary>
        ALIM = 1,

        /// <summary>
        /// Ruta tipo troncal.
        /// </summary>
        TRUNK = 2
    }
}