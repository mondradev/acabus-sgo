using System;
using System.Text;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Serializers
{
    /// <summary>
    /// Provee de un convertidor de campos tipo <see cref="FieldType.Text"/> para
    /// mensajes adaptativos. El convetidor utiliza la función <see cref="object.ToString()"/> para
    /// la conversión del valor a texto y una codificación UTF-8.
    /// </summary>
    internal class TextSerializer : BinarySerializer
    {
        /// <summary>
        /// Convierte un vector unidimensional en una instancia de campo del tipo Text a partir de
        /// la definición especificada. Al finalizar el proceso, el vector de bytes le serán
        /// extraidos los bytes utilizados.
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

            if (definition.Type != FieldType.Text)
                throw new ArgumentException("La definición del campo debe representar un campo de texto", "definition");

            if (src.Length == 0)
                throw new ArgumentException("El vector no contiene elementos", nameof(src));

            FieldDefinition binaryDefinition = new FieldDefinition(
                definition.ID,
                FieldType.Binary,
                definition.MaxLength,
                definition.IsVarLength,
                new BinarySerializer()
            );

            Field field = base.Deserialize(ref src, binaryDefinition);

            string value = Encoding.UTF8.GetString(field.Value as byte[]);

            return new Field(value.Trim(), definition);
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
            if (src == null)
                throw new ArgumentNullException("src");

            if (definition == null)
                throw new ArgumentNullException("definition");

            if (src.ID != definition.ID)
                throw new InvalidOperationException("No es posible utilizar la definición para este campo. Los ID no coinciden");

            if (definition.Type != FieldType.Text)
                throw new ArgumentOutOfRangeException("definition", "La definición del campo debe representar un campo texto");

            string dest = src.Value?.ToString();

            if (string.IsNullOrEmpty(dest))
                throw new ArgumentException("No es permitido enviar valores nulos o cadenas vacias.", nameof(src));

            FieldDefinition binaryDefinition = new FieldDefinition(
               definition.ID,
               FieldType.Binary,
               definition.MaxLength,
               definition.IsVarLength,
               new BinarySerializer()
           );

            byte[] destBin = Encoding.UTF8.GetBytes(dest);

            return base.Serialize(new Field(destBin.PadRight(definition.IsVarLength ? dest.Length : definition.MaxLength, 0x20), binaryDefinition), binaryDefinition);
        }

        /// <summary>
        /// Obtiene una cadena que representa al valor del campo.
        /// </summary>
        /// <param name="value">Valor del campo.</param>
        /// <param name="definition">Caracteristicas del campo.</param>
        public override string ToString(object value, FieldDefinition definition)
            => value?.ToString() ?? "(NOT SUPPORT)";

        /// <summary>
        /// Determina si un valor es compatible con el campo especificado por la definición.
        /// </summary>
        /// <param name="value">Valor a validar.</param>
        /// <param name="definition">Definición del campo.</param>
        public override bool Validate(object value, FieldDefinition definition)
            => true;
    }
}