using InnSyTech.Standard.Utils;
using System;
using System.Linq;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Serializers
{
    /// <summary>
    /// Provee de un convertidor de campos tipo <see cref="FieldType.Numeric"/> para
    /// mensajes adaptativos. El convetidor utiliza valores enteros sin signos.
    /// </summary>
    internal class NumericSerializer : IAdaptiveSerializer
    {
        /// <summary>
        /// Convierte un vector unidimensional en una instancia de campo del tipo numerico a partir
        /// de la definición especificada. Al finalizar el proceso, el vector de bytes le serán
        /// extraidos los bytes utilizados.
        /// </summary>
        /// <param name="src">Vector de bytes origen donde se encuentra el campo.</param>
        /// <param name="definition">Caracteristicas que definen el campo.</param>
        /// <returns>Un campo generado a partir del vector de bytes y la definición especificada.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// La definición no expresa las caracteristicas para un campo numérico.
        /// </exception>
        /// <exception cref="ArgumentNullException">Ningun argumento puede ser nulo.</exception>
        public Field Deserialize(ref byte[] src, FieldDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException("definition");

            if (src == null)
                throw new ArgumentNullException("src");

            if (definition.Type != FieldType.Numeric)
                throw new ArgumentOutOfRangeException("definition", "La definición del campo debe representar un campo numérico");

            if (src.Length == 0)
                throw new ArgumentException("El vector no contiene elementos", nameof(src));

            byte[] dest = src;
            byte[] buff = dest.Slice(0, definition.MaxLength);

            src = dest.Slice(definition.MaxLength, dest.Length - definition.MaxLength);

            int length = RealSize(definition.MaxLength);

            buff = buff.Slice(0, length);

            return new Field(GetNumeric(buff, length), definition);
        }

        /// <summary>
        /// Obtiene los bytes del campo del tipo numérico especificado a partir de la definición proporcionada.
        /// </summary>
        /// <param name="src">Campo a obtener los bytes.</param>
        /// <param name="definition">
        /// Caracteristicas que definen el campo. Al finalizar el proceso, la definición actualiza su longitud.
        /// </param>
        /// <returns>Un vector de bytes que representan al campo.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// La definición no expresa las caracteristicas para un campo numérico.
        /// </exception>
        /// <exception cref="ArgumentNullException">Ningun argumento puede ser nulo.</exception>
        /// <exception cref="InvalidOperationException">
        /// El ID de la definición y el campo no coinciden.
        /// </exception>
        public byte[] Serialize(Field src, FieldDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException("definition");

            if (src == null)
                throw new ArgumentNullException("src");

            if (definition.Type != FieldType.Numeric)
                throw new ArgumentOutOfRangeException("definition", "La definición del campo debe representar un campo numérico");

            if (src.ID != definition.ID)
                throw new InvalidOperationException("No es posible utilizar la definición para este campo. Los ID no coinciden");

            if (!Validate(src.Value, definition))
                throw new ArgumentOutOfRangeException("El tipo de campo FieldType.Numeric no es compatible con valores no numéricos o con punto decimal, utilice campo tipo FieldType.Text o FieldType.Binary");

            int length = definition.MaxLength;

            byte[] buffer = GetBytes(src.Value);

            return buffer.Slice(0, length).PadRight(length);
        }

        /// <summary>
        /// Obtiene una cadena que representa al valor numérico del campo.
        /// </summary>
        /// <param name="value">Valor del campo.</param>
        /// <param name="definition">Caracteristicas del campo.</param>
        public string ToString(object value, FieldDefinition definition)
            => Validate(value, definition) ? value.ToString() : "(NOT SUPPORT)";

        /// <summary>
        /// Determina si un valor es compatible con el campo especificado por la definición.
        /// </summary>
        /// <param name="value">Valor a validar.</param>
        /// <param name="definition">Definición del campo.</param>
        public bool Validate(object value, FieldDefinition definition)
        {
            if (definition.Type != FieldType.Numeric)
                throw new InvalidOperationException("El tipo de campo no es FieldType.Numeric");

            if (!value.IsInteger())
                return false;

            if (Type.GetTypeCode(value.GetType()) == TypeCode.UInt64)
            {
                ulong number = Convert.ToUInt64(value);

                if (number <= Convert.ToUInt64(sbyte.MaxValue) && definition.MaxLength >= 1)
                    return true;
                if (number <= Convert.ToUInt64(short.MaxValue) && definition.MaxLength >= 2)
                    return true;
                if (number <= Convert.ToUInt64(int.MaxValue) && definition.MaxLength >= 4)
                    return true;
                if (number <= Convert.ToUInt64(long.MaxValue) && definition.MaxLength >= 8)
                    return true;

                if (number.Between(byte.MinValue, byte.MaxValue) && definition.MaxLength >= 1)
                    return true;
                if (number.Between(ushort.MinValue, ushort.MaxValue) && definition.MaxLength >= 2)
                    return true;
                if (number.Between(uint.MinValue, uint.MaxValue) && definition.MaxLength >= 4)
                    return true;

                if (number.Between(ulong.MinValue, ulong.MaxValue) && definition.MaxLength >= 8)
                    return true;
            }
            else
            {
                long number = Convert.ToInt64(value);

                if (number.Between(sbyte.MinValue, sbyte.MaxValue) && definition.MaxLength >= 1)
                    return true;
                if (number.Between(short.MinValue, short.MaxValue) && definition.MaxLength >= 2)
                    return true;
                if (number.Between(int.MinValue, int.MaxValue) && definition.MaxLength >= 4)
                    return true;
                if (number.Between(long.MinValue, long.MaxValue) && definition.MaxLength >= 8)
                    return true;

                if (number.Between(byte.MinValue, byte.MaxValue) && definition.MaxLength >= 1)
                    return true;
                if (number.Between(ushort.MinValue, ushort.MaxValue) && definition.MaxLength >= 2)
                    return true;
                if (number.Between(uint.MinValue, uint.MaxValue) && definition.MaxLength >= 4)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Obtiene el valor numérico de la secuencia de bytes.
        /// </summary>
        /// <param name="buff">Secuencia de bytes.</param>
        /// <param name="maxLength">Longitud máxima de la secuencia.</param>
        private static object GetNumeric(byte[] buff, int maxLength)
        {
            if (maxLength >= 8)
                return BitConverter.ToUInt64(buff.PadRight(8), 0);
            if (maxLength >= 4)
                return BitConverter.ToUInt32(buff.PadRight(4), 0);
            if (maxLength >= 2)
                return BitConverter.ToUInt16(buff.PadRight(2), 0);
            if (maxLength == 1)
                return buff.First();

            throw new OverflowException("No se puede obtener el valor numerico de una secuencia vacía.");
        }

        /// <summary>
        ///  Obtiene los bytes del valor numérico.
        /// </summary>
        /// <param name="value">Valor numérico</param>
        /// <returns>Una secuencia de bytes</returns>
        private static byte[] GetBytes(object value)
        {
            if (Type.GetTypeCode(value?.GetType()) == TypeCode.UInt64)
                return BitConverter.GetBytes(Convert.ToUInt64(value));
            else
                return BitConverter.GetBytes(Convert.ToInt64(value));
        }

        /// <summary>
        /// Obtiene el tamaño de un tipo de dato numérico.
        /// </summary>
        /// <param name="length">Longitud de buffer.</param>
        /// <returns>Longitud real del buffer.</returns>
        private static int RealSize(int length)
        {
            if (length >= 8) return 8;
            if (length >= 4) return 4;
            if (length >= 2) return 2;
            if (length >= 1) return 1;

            throw new OverflowException("El tamaño no puede ser 0");
        }
    }
}