using System;
using System.Linq;
using System.Text;

namespace InnSyTech.Standard.Net.Communications.AdaptativeMessages.Converters
{
    /// <summary>
    /// Provee de un convertidor de campos tipo <see cref="FieldDefinition.FieldType.Text" /> para
    /// mensajes adaptativos. El convetidor utiliza la función <see cref="object.ToString()" /> para
    /// la conversión del valor a texto y una codificación UTF-8.
    /// </summary>
    internal class TextConverter : IAdaptativeConverter
    {
        /// <summary>
        /// Convierte un vector unidimensional en una instancia de campo del tipo Text a partir de la
        /// definición especificada.
        /// </summary>
        /// <param name="src"> Vector de bytes origen donde se encuentra el campo. </param>
        /// <param name="definition"> Caracteristicas que definen el campo. </param>
        /// <returns> Un campo generado a partir del vector de bytes y la definición especificada. </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// La definición no expresa las caracteristicas para un campo texto.
        /// </exception>
        /// <exception cref="ArgumentNullException"> Ningun argumento puede ser nulo. </exception>
        public Field Convert(byte[] src, FieldDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException("definition");

            if (src == null)
                throw new ArgumentNullException("src");

            if (definition.Type != FieldDefinition.FieldType.Text)
                throw new ArgumentOutOfRangeException("definition", "La definición del campo debe representar un campo texto");

            if (src.Length == 0)
                throw new ArgumentException("src", "El vector no contiene elementos");

            int length = src.Length;
            byte[] dest = src;

            if (length > definition.MaxLength)
                dest = dest.Take(definition.MaxLength).ToArray();

            string value = Encoding.UTF8.GetString(dest);

            if (definition.IsVarLength)
                value = value.Trim();

            return new Field(definition.ID, value);
        }

        /// <summary>
        /// Obtiene los bytes del campo del tipo texto especificado a partir de la definición proporcionada.
        /// </summary>
        /// <param name="src"> Campo a obtener los bytes. </param>
        /// <param name="definition">
        /// Caracteristicas que definen el campo. Al finalizar el proceso, la definición actualiza su longitud.
        /// </param>
        /// <returns> Un vector de bytes que representan al campo. </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// La definición no expresa las caracteristicas para un campo texto.
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

            if (definition.Type != FieldDefinition.FieldType.Text)
                throw new ArgumentOutOfRangeException("definition", "La definición del campo debe representar un campo texto");

            if (src.ID == definition.ID)
                throw new InvalidOperationException("No es posible utilizar la definición para este campo. Los ID no coinciden");

            string dest = src.Value?.ToString();
            int length = dest.Length * 2;

            if (length > definition.MaxLength)
                dest = dest.Substring(0, length / 2);

            if (!definition.IsVarLength)
                dest = dest.PadRight(definition.MaxLength / 2);

            definition.Length = dest.Length * 2;

            return Encoding.UTF8.GetBytes(dest);
        }
    }
}