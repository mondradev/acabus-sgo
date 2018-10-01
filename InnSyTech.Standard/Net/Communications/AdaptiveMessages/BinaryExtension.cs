using System;
using System.Collections.Generic;
using System.Linq;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages
{
    /// <summary>
    /// Provee de funcionalidad extra a los vectores de byte.
    /// </summary>
    internal static class BinaryExtension
    {
        /// <summary>
        /// Alinea los bytes a la derecha e inserta 0x00 a la izquierda hasta alcanzar la longitud especificada.
        /// </summary>
        /// <param name="src">Vector unidimensional a alinear.</param>
        /// <param name="minLength">Longitud minima del vector.</param>
        /// <param name="padding">Caracter de relleno.</param>
        /// <returns>Un vector con la longitud alcanzada.</returns>
        public static byte[] PadLeft(this byte[] src, int minLength, byte padding = 0x00)
        {
            List<Byte> dest = new List<byte>(src);

            while (dest.Count < minLength)
                dest.Insert(0, padding);

            return dest.ToArray();
        }

        /// <summary>
        /// Alinea los bytes a la izquierdad e inserta 0xFF a la derecha hasta alcanzar la longitud especificada.
        /// </summary>
        /// <param name="src">Vector unidimensional a alinear.</param>
        /// <param name="minLength">Longitud minima del vector.</param>
        /// <param name="padding">Caracter de relleno.</param>
        /// <returns>Un vector con la longitud alcanzada.</returns>
        public static byte[] PadRight(this byte[] src, int minLength, byte padding = 0xFF)
        {
            List<Byte> dest = new List<byte>(src);

            while (dest.Count < minLength)
                dest.Add(padding);

            return dest.ToArray();
        }

        /// <summary>
        /// Ajusta el tamaño del vector y rellena con el elemento especificado.
        /// </summary>
        /// <param name="src">Vector unidimensional a redimensionar.</param>
        /// <param name="width">Longitud del vector.</param>
        /// <param name="padding">Elmento de relleno.</param>
        /// <returns>Vector redimensionado.</returns>
        public static byte[] Resize(this byte[] src, int width, byte padding = 0x00)
        {
            byte[] dist = src;

            while (dist.Length > width)
                dist = dist.TrimStart();

            while (dist.Length < width)
                dist = dist.PadLeft(width);

            return dist;
        }

        /// <summary>
        /// Remueve todos los elementos al final del vector que sean igual a 0xFF.
        /// </summary>
        /// <param name="src">Vector unidimensional.</param>
        /// <returns>Vector sin los elementos 0xFF del final removidos.</returns>
        public static byte[] TrimEnd(this byte[] src)
        {
            List<byte> dest = new List<byte>(src);

            while (dest.Last() == 0xFF && dest.Any())
                dest.RemoveAt(dest.Count - 1);

            return dest.ToArray();
        }

        /// <summary>
        /// Remueve todos los elementos al final del vector que sean igual a 0xFF.
        /// </summary>
        /// <param name="src">Vector unidimensional.</param>
        /// <returns>Vector sin los elementos 0xFF del final removidos.</returns>
        public static byte[] TrimStart(this byte[] src)
        {
            List<byte> dest = new List<byte>(src);

            while (dest.First() == 0x00 && dest.Any())
                dest.RemoveAt(0);

            return dest.ToArray();
        }
    }
}