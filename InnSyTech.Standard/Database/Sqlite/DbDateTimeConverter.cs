using System;

namespace InnSyTech.Standard.Database.Sqlite
{
    /// <summary>
    /// Esta clase transforma en una cadena de texto valido cualquier instancia de tipo <see cref="DateTime"/>.
    /// </summary>
    public class SQLiteDateTimeConverter : IDbConverter
    {
        /// <summary>
        /// Obtiene una instancia <see cref="DateTime"/> de la cadena pasa como argumento.
        /// </summary>
        /// <param name="data">Cadena de texto obtenida de la base de datos.</param>
        /// <returns>Una instancia <see cref="DateTime"/>.</returns>
        public object ConverterFromDb(object data)
        {

            if (data.GetType() != typeof(String))
                throw new ArgumentException("El parametro 'data' debe ser una cadena de texto.");

            if (String.IsNullOrEmpty(data.ToString().Trim()))
                throw new ArgumentException("El parametro 'data' no puede ser una cadena vacia o nula.");

            if (!DateTime.TryParse(data.ToString(), out DateTime result))
                throw new FormatException("La cadena no contiene el formato adecuado.");

            return result;
        }

        /// <summary>
        /// Obtiene una cadena valida para un campo de fecha y tiempo en una base de datos SQLite.
        /// </summary>
        /// <param name="property">Propiedad a convertir a cadena.</param>
        /// <returns>Una cadena valida para un campo DATETIME de SQLite.</returns>
        public object ConverterToDbData(object property)
        {
            if (!(property is DateTime))
                throw new ArgumentException("La propiedad debe ser del tipo 'DateTime'");

            return ((DateTime)property).ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }
}