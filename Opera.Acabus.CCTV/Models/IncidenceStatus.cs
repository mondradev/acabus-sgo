namespace Opera.Acabus.Cctv.Models
{
    /// <summary>
    /// Estados posibles de una incidencia.
    /// </summary>
    public enum IncidenceStatus
    {
        /// <summary>
        /// Toda incidencia nueva se considera abierta.
        /// </summary>
        OPEN = 0,

        /// <summary>
        /// Caudno una incidencia a finalizado su actividad, esta pasa a estar cerrada.
        /// </summary>
        CLOSE = 1,

        /// <summary>
        /// Cuando se requiere confirmar el cambio de estado de la incidencia, se establece en sin confirmar.
        /// </summary>
        UNCOMMIT = 2,

        /// <summary>
        /// Cuando una incidencia se considera que no será posible cerrarse en el corto plazo, se
        /// identifica como pendiente.
        /// </summary>
        PENDING = 4,

        /// <summary>
        /// Cuando una incidencia ocurre nuevamente en un lapso corto, se considera re-abierta.
        /// </summary>
        RE_OPEN = 8,

        /// <summary>
        /// Cuando un equipo presenta dinero fuera de las alcancías y este se requiere trasladar, la
        /// incidencia se considera devolución en tránsito.
        /// </summary>
        REFUND_IN_TRANSIT = 16
    }
}