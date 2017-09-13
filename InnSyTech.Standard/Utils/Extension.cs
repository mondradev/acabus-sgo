using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace InnSyTech.Standard.Utils
{
    /// <summary>
    /// Provee de funciones de utilidad a diversas estructuras.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Cortar la cadena en 'n' partes iguales.
        /// </summary>
        /// <param name="str">Cadena a partir.</param>
        /// <param name="parts">Número de veces a partir la cadena.</param>
        /// <returns>Un vector de cadenas.</returns>
        public static String[] Cut(this String str, int parts)
        {
            List<String> partsStr = new List<string>();

            var lengthPart = str.Length / parts;

            for (int i = 0; i < parts; i++)
                partsStr.Add(str.Substring(i * lengthPart, lengthPart));

            return partsStr.ToArray();
        }

        /// <summary>
        /// Elimina los saltos de linea en un texto.
        /// </summary>
        /// <param name="str">Texto a unificar lineas.</param>
        /// <returns>Una cadena de una sola linea.</returns>
        public static String JoinLines(this String str)
            => Regex.Replace(str, "[\r\n\r]+", " ");

        /// <summary>
        /// Fusiona una instancia <see cref="IEnumerable{T}"/> de <see cref="IEnumerable{T}"/>
        /// volviendola en una sola.
        /// </summary>
        /// <typeparam name="T">Tipo de los elementos de las instancias <see cref="IEnumerable{T}"/>.</typeparam>
        /// <param name="enumerables">Instancia de <see cref="IEnumerable{T}"/> de <see cref="IEnumerable{T}"/>.</param>
        /// <returns>
        /// Una instancia <see cref="IEnumerable{T}"/> con todos los elementos de cada una de las
        /// instancias <see cref="IEnumerable{T}"/>.
        /// </returns>
        public static IEnumerable<T> Merge<T>(this IEnumerable<IEnumerable<T>> enumerables)
        {
            foreach (var enumerable in enumerables)
                foreach (var item in enumerable)
                    yield return item;
        }

    }
}