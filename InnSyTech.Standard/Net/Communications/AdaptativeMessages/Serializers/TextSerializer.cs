using System;
using System.Linq;
using System.Text;

namespace InnSyTech.Standard.Net.Communications.AdaptativeMessages.Serializers
{
    /// <summary>
    /// Provee de un convertidor de campos tipo <see cref="FieldDefinition.FieldType.Text"/> para
    /// mensajes adaptativos. El convetidor utiliza la función <see cref="object.ToString()"/> para
    /// la conversión del valor a texto y una codificación UTF-8.
    /// </summary>
    internal class TextSerializer : HexaSerializer
    {
        /// <summary>
        /// Convierte un vector unidimensional en una instancia de campo del tipo Text a partir de la
        /// definición especificada. Al finalizar el proceso, el vector de bytes le serán extraidos
        /// los bytes utilizados.
        /// </summary>
        /// <param name="src">Vector de bytes origen donde se encuentra el campo.</param>
        /// <param name="definition">Caracteristicas que definen el campo.</param>
        /// <returns>Un campo generado a partir del vector de bytes y la definición especificada.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// La definición no expresa las caracteristicas para un campo texto.
        /// </exception>
        /// <exception cref="ArgumentNullException">Ningun argumento puede ser nulo.</exception>
        public override Field Deserialize(ref byte[] src, FieldDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException("definition");

            if (src == null)
                throw new ArgumentNullException("src");

            if (definition.Type != FieldDefinition.FieldType.Text)
                throw new ArgumentOutOfRangeException("definition", "La definición del campo debe representar un campo texto");

            if (src.Length == 0)
                throw new ArgumentException("src", "El vector no contiene elementos");

            int length = GetLengthFromBytes(src, definition, out int lvarSize);
            byte[] dest = src;

            dest = dest.Skip(lvarSize).Take(length).ToArray();

            string value = Encoding.UTF8.GetString(dest);

            value = value.Trim();

            src = src.Skip(lvarSize + length).ToArray();

            return new Field(definition.ID, value);
        }

        /// <summary>
        /// Obtiene los bytes del campo del tipo texto especificado a partir de la definición proporcionada.
        /// </summary>
        /// <param name="src">Campo a obtener los bytes.</param>
        /// <param name="definition">
        /// Caracteristicas que definen el campo. Al finalizar el proceso, la definición actualiza su longitud.
        /// </param>
        /// <returns>Un vector de bytes que representan al campo.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// La definición no expresa las caracteristicas para un campo texto.
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

            if (definition.Type != FieldDefinition.FieldType.Text)
                throw new ArgumentOutOfRangeException("definition", "La definición del campo debe representar un campo texto");

            if (src.ID != definition.ID)
                throw new InvalidOperationException("No es posible utilizar la definición para este campo. Los ID no coinciden");

            string dest = src.Value?.ToString();

            if (String.IsNullOrEmpty(dest))
                throw new ArgumentException("No es permitido enviar valores nulos o cadenas vacias.");

            int length = 0;

            if (definition.IsVarLength)
            {
                int lvarSize = GetLVarSize(definition);
                int maxLength = definition.MaxLength - lvarSize;

                byte[] bodyBin = Encoding.UTF8.GetBytes(dest);

                length = bodyBin.Length > maxLength ? maxLength : bodyBin.Length;

                definition.Length = length;

                return BitConverter.GetBytes(length).Reverse().ToArray().PadLeft(lvarSize).Concat(bodyBin).ToArray();
            }

            byte[] destBin = Encoding.UTF8.GetBytes(dest);

            if (destBin.Length > definition.MaxLength)
                destBin = destBin.Take(definition.MaxLength).ToArray();
            else
                destBin = destBin.PadLeft(definition.MaxLength, 0x20);

            definition.Length = destBin.Length;

            return destBin;
        }
    }
}