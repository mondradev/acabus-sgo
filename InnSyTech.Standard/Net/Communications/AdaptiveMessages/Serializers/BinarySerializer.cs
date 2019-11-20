using InnSyTech.Standard.Utils;
using System;
using System.Linq;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Serializers
{
    /// <summary>
    /// Provee de un convertidor de campos tipo <see cref="FieldType.Binary"/> para mensajes adaptativos.
    /// </summary>
    public class BinarySerializer : IAdaptiveSerializer
    {
        /// <summary>
        /// Convierte una vector de bytes en una instancia de campo del tipo binario a partir de la
        /// definición especificada. Al finalizar el proceso, el vector de bytes le serán extraidos
        /// los bytes utilizados.
        /// </summary>
        /// <param name="src">Vector de bytes origen donde se encuentra el campo.</param>
        /// <param name="definition">Caracteristicas que definen el campo.</param>
        /// <returns>Un campo generado a partir del vector de bytes y la definición especificada.</returns>
        public virtual Field Deserialize(ref byte[] src, FieldDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException("definition");

            if (src == null)
                throw new ArgumentNullException("src");

            if (definition.Type != FieldType.Binary)
                throw new ArgumentException("La definición del campo debe representar un campo binario", "definition");

            if (src.Length == 0)
                throw new ArgumentException("El vector no contiene elementos", nameof(src));

            byte[] dest = src;

            int lvarSize = definition.IsVarLength ? GetLvarSize(definition.MaxLength) : 0;
            int length = definition.IsVarLength ? GetLvarLength(ref dest, lvarSize) : definition.MaxLength;

            byte[] buff = dest.Slice(0, length);

            src = dest.Slice(length, dest.Length - length);

            return new Field(buff, definition);
        }

        /// <summary>
        /// Obtiene los bytes del campo del tipo binario especificado a partir de la definición proporcionada.
        /// </summary>
        /// <param name="src">Campo a obtener los bytes.</param>
        /// <param name="definition">
        /// Caracteristicas que definen el campo. Al finalizar el proceso, la definición actualiza su longitud.
        /// </param>
        /// <returns>Un vector de bytes que representan al campo.</returns>
        public virtual byte[] Serialize(Field src, FieldDefinition definition)
        {
            if (src == null)
                throw new ArgumentNullException("src");

            if (definition == null)
                throw new ArgumentNullException("definition");

            if (src.ID != definition.ID)
                throw new InvalidOperationException("No es posible utilizar la definición para este campo. Los ID no coinciden");

            if (!Validate(src.Value, definition))
                throw new ArgumentException("El tipo de campo FieldType.Binary es compatible unicamente con vectores unidimensionales de Bytes.");

            byte[] buffer = src.Value as byte[];

            if (buffer.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(src), "El vector no pude estar vacío");

            return definition.IsVarLength ? SerializeLvar(buffer, definition.MaxLength) : buffer.PadRight(definition.MaxLength, 0).Slice(0, definition.MaxLength);
        }

        /// <summary>
        /// Determina si un valor es compatible con el campo especificado por la definición.
        /// </summary>
        /// <param name="value">Valor a validar.</param>
        /// <param name="definition">Definición del campo.</param>
        public virtual bool Validate(object value, FieldDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException("definition");

            if (definition.Type != FieldType.Binary)
                throw new ArgumentOutOfRangeException("definition", "La definición del campo debe representar un campo binario");

            if (typeof(byte[]) == value.GetType())
                return (value as byte[]).Length > 0;

            return false;
        }

        /// <summary>
        /// Obtiene la longitud real de un campo con longitud variable.
        /// </summary>
        /// <param name="src">Secuencia de bytes.</param>
        /// <param name="lvarSize">Tamaño del valor de LVar</param>
        private static int GetLvarLength(ref byte[] src, int lvarSize)
        {
            if (lvarSize <= 0)
                return 0;

            byte[] buff = src.Slice(0, lvarSize);

            src = src.Slice(lvarSize, src.Length - lvarSize);

            return BitConverter.ToInt32(buff.PadRight(4, 0), 0);
        }

        /// <summary>
        /// Obtiene el tamaño del valor que indica la cantidad de bytes utilizados para el campo.
        /// </summary>
        /// <param name="maxLength">Tamaño máximo del campo.</param>
        private static int GetLvarSize(int maxLength)
        {
            if (maxLength.Between(0, byte.MaxValue))
                return 1;
            if (maxLength.Between(0, ushort.MaxValue))
                return 2;
            if (maxLength.Between(0, int.MaxValue))
                return 4;

            throw new InvalidOperationException($"No es posible utilizar una longitud mayor a {int.MaxValue}");
        }

        /// <summary>
        /// Obtiene una secuencia de bytes donde se incluye la longitud del vector seguido del vector.
        /// </summary>
        /// <param name="buffer">Datos a serializar</param>
        /// <param name="maxLength">Longitud máxima.</param>
        private static byte[] SerializeLvar(byte[] buffer, int maxLength)
        {
            if (buffer.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(buffer), "El vector no pude estar vacío");

            int lvarSize = GetLvarSize(maxLength);
            int length = buffer.Length > maxLength ? maxLength : buffer.Length;

            byte[] binLvarSize = BitConverter.GetBytes(length).Slice(0, lvarSize);

            buffer = buffer.Length > maxLength ? buffer.Slice(0, maxLength) : buffer;

            return binLvarSize.Concat(buffer).ToArray();
        }

        /// <summary>
        /// Obtiene una cadena que representa la secuencia de bytes del campo.
        /// </summary>
        /// <param name="value">Secuencia de bytes.</param>
        /// <param name="definition">Caracteristicas del campo.</param>
        public virtual string ToString(object value, FieldDefinition definition)
            => Validate(value, definition) ? "0x" + (value as byte[]).ToHex() : "(NOT SUPPORT)";
    }
}