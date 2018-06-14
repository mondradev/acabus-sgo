using System;
using System.Collections.Generic;
using System.Linq;

namespace InnSyTech.Standard.Net.Communications.AdaptativeMessages.Converters
{
    /// <summary>
    /// Provee de un convertidor de campos tipo <see cref="FieldDefinition.FieldType.Binary" /> para
    /// mensajes adaptativos.
    /// </summary>
    internal class BinaryConverter : IAdaptativeConverter
    {
        /// <summary>
        /// Convierte una vector de bytes en una instancia de campo del tipo binario a partir de la
        /// definición especificada.
        /// </summary>
        /// <param name="src"> Vector de bytes origen donde se encuentra el campo. </param>
        /// <param name="definition"> Caracteristicas que definen el campo. </param>
        /// <returns> Un campo generado a partir del vector de bytes y la definición especificada. </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// La definición no expresa las caracteristicas para un campo binario.
        /// </exception>
        /// <exception cref="ArgumentNullException"> Ningun argumento puede ser nulo. </exception>
        public Field Convert(byte[] src, FieldDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException("definition");

            if (src == null)
                throw new ArgumentNullException("src");

            if (definition.Type != FieldDefinition.FieldType.Binary)
                throw new ArgumentOutOfRangeException("definition", "La definición del campo debe representar un campo binario");

            if (src.Length == 0)
                throw new ArgumentException("src", "El vector no contiene elementos");

            int length = src.Length;
            byte[] dest = src;

            if (length > definition.MaxLength)
                dest = dest.Take(definition.MaxLength).ToArray();

            if (definition.IsVarLength)
                dest = TrimEnd(dest);

            return new Field(definition.ID, dest);
        }

        /// <summary>
        /// Obtiene los bytes del campo del tipo binario especificado a partir de la definición proporcionada.
        /// </summary>
        /// <param name="src"> Campo a obtener los bytes. </param>
        /// <param name="definition">
        /// Caracteristicas que definen el campo. Al finalizar el proceso, la definición actualiza su longitud.
        /// </param>
        /// <returns> Un vector de bytes que representan al campo. </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// La definición no expresa las caracteristicas para un campo binario.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// El valor del campo no es un vector unidimensional del tipo Byte.
        /// </exception>
        /// <exception cref="ArgumentNullException"> Ningun argumento puede ser nulo. </exception>
        /// <exception cref="InvalidOperationException">
        /// El ID de la definición y el campo no coinciden.
        /// </exception>
        public byte[] GetBytes(Field src, FieldDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException("definition");

            if (src == null)
                throw new ArgumentNullException("src");

            if (definition.Type != FieldDefinition.FieldType.Binary)
                throw new ArgumentOutOfRangeException("definition", "La definición del campo debe representar un campo binario");

            if (src.ID == definition.ID)
                throw new InvalidOperationException("No es posible utilizar la definición para este campo. Los ID no coinciden");

            if (!(src.Value is byte[]))
                throw new ArgumentException("src", "El valor del campo no es un vector de bytes.");

            byte[] dest = src.Value as byte[];
            int length = dest.Length;

            if (length > definition.MaxLength)
                dest = dest.Take(definition.MaxLength).ToArray();

            if (!definition.IsVarLength)
                dest = PadRight(dest, definition.MaxLength);

            definition.Length = dest.Length;

            return dest;
        }

        /// <summary>
        /// Alinea los bytes a la izquierdad e inserta 0xFF a la derecha hasta alcanzar la longitud especificada.
        /// </summary>
        /// <param name="src"> Vector unidimensional a alinear. </param>
        /// <param name="maxLength"> Longitud minima del vector. </param>
        /// <returns> Un vector con la longitud alcanzada. </returns>
        private byte[] PadRight(byte[] src, int minLength)
        {
            List<Byte> dest = new List<byte>(src);

            while (dest.Count > minLength)
                dest.Add(0xFF);

            return dest.ToArray();
        }

        /// <summary>
        /// Remueve todos los elementos al final del vector que sean igual a 0xFF.
        /// </summary>
        /// <param name="src"> Vector unidimensional. </param>
        /// <returns> Vector sin los elementos 0xFF del final removidos. </returns>
        private byte[] TrimEnd(byte[] src)
        {
            List<byte> dest = new List<byte>(src);

            while (dest.Last() != 0xFF && dest.Any())
                dest.RemoveAt(dest.Count - 1);

            return dest.ToArray();
        }
    }
}