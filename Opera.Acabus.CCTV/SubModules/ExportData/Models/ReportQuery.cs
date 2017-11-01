using System;

namespace Opera.Acabus.Cctv.SubModules.ExportData.Models
{
    /// <summary>
    /// Define la estructura para manipular la configuración de los reportes disponibles.
    /// </summary>
    public sealed class ReportQuery
    {
        /// <summary>
        /// Obtiene o establece la descripción del reporte.
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// Obtiene o establece la consulta del reporte.
        /// </summary>
        public String Query { get; set; }

        /// <summary>
        /// Representa la instancia en una cadena de texto.
        /// </summary>
        /// <returns> Cadena que representa el objeto. </returns>
        public override string ToString()
        {
            return Description;
        }
    }
}