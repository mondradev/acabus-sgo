using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Opera.Acabus.Core.DataAccess
{
    /// <summary>
    /// Exporta información a archivos Csv.
    /// </summary>
    public static class CsvDump
    {
        /// <summary>
        /// Exporta la información contenida dentro de la secuencia de diccionarios a un archivo Csv.
        /// </summary>
        /// <param name="source">Origen de los datos.</param>
        /// <param name="filename">Nombre del archivo Csv a generar.</param>
        public static void Export(IEnumerable<Dictionary<String, Object>> source, String filename)
        {
            File.Delete(filename);

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source.Count() == 0)
                throw new ArgumentOutOfRangeException(nameof(source), "El origen de datos no presenta información.");

            StringBuilder builder = new StringBuilder();
            foreach (var header in source.First().Keys)
                builder.AppendFormat("'{0}', ", header);

            builder.Remove(builder.Length - 2, 2);
            builder.Append("\n");

            File.AppendAllText(filename, builder.ToString(), Encoding.UTF8);

            builder.Clear();

            foreach (var item in source)
            {
                foreach (var header in item.Keys)
                    builder.AppendFormat("'{0}', ", item[header]);

                builder.Remove(builder.Length - 2, 2);
                builder.Append("\n");

                File.AppendAllText(filename, builder.ToString(), Encoding.UTF8);

                builder.Clear();
            }
        }
    }
}