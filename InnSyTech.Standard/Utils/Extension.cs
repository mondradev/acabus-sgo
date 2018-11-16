using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InnSyTech.Standard.Utils
{
    /// <summary>
    /// Provee de funciones de utilidad a diversas estructuras.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Convierte una secuencia de un tipo a otro a traves de una función convertidora.
        /// </summary>
        /// <typeparam name="TSource">Tipo de dato de la secuencia origen.</typeparam>
        /// <typeparam name="TResult">Tipo de dato de la secuencia destino.</typeparam>
        /// <param name="converter">Función convertidora.</param>
        /// <returns>La secuencia convertida al tipo de datos definido por la funcion convertidora.</returns>
        public static IEnumerable<TResult> Convert<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> converter)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            foreach (var item in source)
                yield return converter(item);
        }

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
        /// Recorre toda la secuencia llamando a la función proporcionada como parametro por cada uno
        /// de los elementos de la misma.
        /// </summary>
        /// <typeparam name="T">Tipo de la secuencia.</typeparam>
        /// <param name="src">Secuencia origen.</param>
        /// <param name="action">Acción a realizar por cada uno de los elementos.</param>
        public static void ForEach<T>(this IEnumerable<T> src, Action<T> action)
        {
            IEnumerator<T> enumerator = src.GetEnumerator();

            while (enumerator.MoveNext())
                action?.Invoke(enumerator.Current);
        }

        /// <summary>
        /// Obtiene un elemento de la secuencia de manera aleatoría que cumple con el predicado especificado.
        /// </summary>
        /// <typeparam name="TData">Tipo de dato de la secuencia.</typeparam>
        /// <param name="source">Secuencia de origen.</param>
        /// <param name="predicate">Predicado a validar con los elementos.</param>
        /// <returns>Elemento devuelto por la secuencia.</returns>
        public static TData GetRandom<TData>(this IEnumerable<TData> source, Func<TData, bool> predicate)
        {
            IEnumerable<TData> valids = source.Where(predicate);
            var count = valids.Count();

            Random random = new Random((int)DateTime.Now.Ticks);
            var index = (int)(random.NextDouble() * count);

            if (index >= valids.Count())
                return default(TData);

            return valids.ElementAt(index);
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

        /// <summary>
        /// Obtiene el mensaje de una instancia <see cref="Exception"/> desde su excepción más profunda.
        /// </summary>
        /// <returns>Un mensaje de la excepción ocurrida.</returns>
        public static String PrintMessage(this Exception exception)
        {
            while (exception.InnerException != null)
                exception = exception.InnerException;

            return exception.Message + "\n" + exception.StackTrace.Trim();
        }

        /// <summary>
        /// Representa el valor boolean a través de un SI o NO.
        /// </summary>
        /// <param name="source">Valor booleano.</param>
        public static String ToStringFriendly(this Boolean source)
            => source ? "SI" : "NO";

        /// <summary>
        /// Convierte una secuencia de caracteres a texto plano.
        /// </summary>
        /// <param name="src">Secuencia de datos origen.</param>
        /// <returns>Una cadena que representa a la cadena.</returns>
        public static String ToTextPlain(this byte[] src)
        {
            StringBuilder builder = new StringBuilder();
            src.Select(x => x.ToString("x2")).ForEach(x => builder.Append(x));

            return builder.ToString();
        }
    }
}