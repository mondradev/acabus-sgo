using System;
using System.Linq;

namespace InnSyTech.Standard.Net.Communications.AdaptativeMessages.Serializers
{
    /// <summary>
    /// Provee de un convertidor de campos tipo <see cref="FieldDefinition.FieldType.Binary"/> para
    /// mensajes adaptativos.
    /// </summary>
    internal class BinarySerializer : HexaSerializer
    {
        /// <summary>
        /// Convierte una vector de bytes en una instancia de campo del tipo binario a partir de la
        /// definición especificada. Al finalizar el proceso, el vector de bytes le serán extraidos
        /// los bytes utilizados.
        /// </summary>
        /// <param name="src">Vector de bytes origen donde se encuentra el campo.</param>
        /// <param name="definition">Caracteristicas que definen el campo.</param>
        /// <returns>Un campo generado a partir del vector de bytes y la definición especificada.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// La definición no expresa las caracteristicas para un campo binario.
        /// </exception>
        /// <exception cref="ArgumentNullException">Ningun argumento puede ser nulo.</exception>
        public override Field Deserialize(ref byte[] src, FieldDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException("definition");

            if (src == null)
                throw new ArgumentNullException("src");

            if (definition.Type != FieldDefinition.FieldType.Binary)
                throw new ArgumentOutOfRangeException("definition", "La definición del campo debe representar un campo binario");

            if (src.Length == 0)
                throw new ArgumentException("src", "El vector no contiene elementos");

            int length = GetLengthFromBytes(src, definition, out int lvarSize);

            byte[] dest = src;

            dest = dest.Skip(lvarSize).Take(length).ToArray();

            if (!definition.IsVarLength)
                dest = dest.TrimEnd();

            src = src.Skip(lvarSize + length).ToArray();

            return new Field(definition.ID, dest);
        }

        /// <summary>
        /// Obtiene los bytes del campo del tipo binario especificado a partir de la definición proporcionada.
        /// </summary>
        /// <param name="src">Campo a obtener los bytes.</param>
        /// <param name="definition">
        /// Caracteristicas que definen el campo. Al finalizar el proceso, la definición actualiza su longitud.
        /// </param>
        /// <returns>Un vector de bytes que representan al campo.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// La definición no expresa las caracteristicas para un campo binario.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// El valor del campo no es un vector unidimensional del tipo Byte.
        /// </exception>
        /// <exception cref="ArgumentNullException">Ningun argumento puede ser nulo.</exception>
        /// <exception cref="InvalidOperationException">
        /// El ID de la definición y el campo no coinciden.
        /// </exception>
        public override byte[] Serialize(Field src, FieldDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException("definition");

            if (src == null)
                throw new ArgumentNullException("src");

            if (definition.Type != FieldDefinition.FieldType.Binary)
                throw new ArgumentOutOfRangeException("definition", "La definición del campo debe representar un campo binario");

            if (src.ID != definition.ID)
                throw new InvalidOperationException("No es posible utilizar la definición para este campo. Los ID no coinciden");

            if (!(src.Value is byte[]))
                throw new ArgumentException("src", "El valor del campo no es un vector de bytes.");

            byte[] dest = src.Value as byte[];
            int length = dest.Length;

            if (definition.IsVarLength)
            {
                int lvarSize = GetLVarSize(definition);
                int maxLength = definition.MaxLength - lvarSize;
                length = length > maxLength ? maxLength : length;

                dest = BitConverter.GetBytes(length).Reverse().ToArray().PadLeft(lvarSize).Concat(dest.Take(length)).ToArray();
            }
            else
            {
                length = length > definition.MaxLength ? definition.MaxLength : length;
                dest = dest.Take(length).ToArray();
            }

            definition.Length = dest.Length;

            return dest;
        }
    }
}