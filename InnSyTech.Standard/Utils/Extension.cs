using System.Collections.Generic;

namespace InnSyTech.Standard.Utils
{
    /// <summary>
    /// Provee de funciones de utilidad a diversas estructuras.
    /// </summary>
    public static class Extensions
    {
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