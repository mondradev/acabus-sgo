using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Serializers
{
    /// <summary>
    /// Provee de un convertidor de campos tipo <see cref="FieldDefinition.FieldType.Numeric"/> para
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

            if (definition.Type != FieldDefinition.FieldType.Numeric)
                throw new ArgumentOutOfRangeException("definition", "La definición del campo debe representar un campo numérico");

            if (src.Length == 0)
                throw new ArgumentException("El vector no contiene elementos", nameof(src));

            ulong length = (ulong)src.Length;
            byte[] dest = src;
            ulong value = 0;

            if (definition.IsVarLength)
            {
                int lvarSize = GetNumberLvarSize(definition);
                byte[] lengthVar = dest.Take(lvarSize).ToArray();

                length = BCDToUInt64(lengthVar);

                dest = dest.Skip(lvarSize).Take((int)length).ToArray();

                value = BCDToUInt64(dest);

                src = src.Skip(lvarSize + ((int)length)).ToArray();

                return new Field(definition.ID, value);
            }

            dest = dest.Take(definition.MaxLength).ToArray();

            value = BCDToUInt64(dest);

            src = src.Skip(definition.MaxLength).ToArray();

            return new Field(definition.ID, value);
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

            if (definition.Type != FieldDefinition.FieldType.Numeric)
                throw new ArgumentOutOfRangeException("definition", "La definición del campo debe representar un campo numérico");

            if (src.ID != definition.ID)
                throw new InvalidOperationException("No es posible utilizar la definición para este campo. Los ID no coinciden");

            UInt64 value = Convert.ToUInt64(src.Value);

            if (definition.IsVarLength)
            {
                int lvarSize = ToBCD((UInt64)definition.MaxLength).Length;
                int maxLength = (definition.MaxLength - lvarSize) * 2;
                byte[] body = ToBCD(value);

                return BitConverter.GetBytes(body.Length).Reverse().ToArray().Resize(lvarSize).Concat(body).ToArray();
            }

            value = BCDResize(value, definition.MaxLength * 2);

            return ToBCD(value, definition.MaxLength * 2);
        }

        /// <summary>
        /// Re-ajusta la cantidad de caracteres que conforman al número haciendolo más pequeño en
        /// caso de necesitarlo.
        /// </summary>
        /// <param name="number">Número a reajustar.</param>
        /// <param name="maxSize">Tamaño máximo de caracteres del valor númerico.</param>
        /// <returns>Un nuevo valor númerico.</returns>
        private ulong BCDResize(ulong number, int maxSize)
        {
            List<Byte> dest = new List<byte>();

            string numberText = number.ToString();

            if (numberText.Length > maxSize)
                numberText = numberText.Substring(0, maxSize);

            return UInt64.Parse(numberText);
        }

        /// <summary>
        /// Realiza la conversión de un vector de bytes en un valor numérico tomando los datos como BCD.
        /// </summary>
        /// <param name="lengthVar">Vector de bytes origen.</param>
        /// <returns>Valor numérico obtenido del vector.</returns>
        private UInt64 BCDToUInt64(byte[] lengthVar)
        {
            string hexa = BitConverter.ToString(lengthVar).Replace("-", "");

            if (!Regex.IsMatch(hexa, "[0-9]"))
                throw new ArgumentOutOfRangeException(nameof(lengthVar),
                    "Los valores del vector de bytes solo pueden estar conformados por números del 0 al 9");

            return UInt64.Parse(hexa);
        }

        /// <summary>
        /// Obtiene el tamaño de indicador del tamaño del campo.
        /// </summary>
        /// <param name="definition">Caracteristicas que definen al campo.</param>
        /// <returns>Tamaño del LVar.</returns>
        private int GetNumberLvarSize(FieldDefinition definition)
        {
            if (!definition.IsVarLength)
                return definition.MaxLength;

            int length = definition.MaxLength.ToString().Length;
            length = (length % 2) + length;

            return length / 2;
        }

        /// <summary>
        /// Realiza la conversión de un valor numérico a un vector de bytes almacenando los datos en
        /// formato BCD.
        /// </summary>
        /// <param name="number">Número a convertir en formato BCD.</param>
        /// <param name="width">Longitud minima del vector.</param>
        /// <param name="padding">Caracter de relleno.</param>
        /// <returns>Un vector que representa al número especificado.</returns>
        private byte[] ToBCD(UInt64 number, int width = 2, char padding = '0')
        {
            List<Byte> dest = new List<byte>();

            width = width > number.ToString().Length ? width : number.ToString().Length;

            if (width % 2 != 0)
                width += 1;

            string numberText = number.ToString().PadLeft(width, padding);

            for (int i = 0; i < numberText.Length; i += 2)
                dest.Add(Byte.Parse(numberText.Substring(i, 1) + numberText.Substring(i + 1, 1), NumberStyles.AllowHexSpecifier));

            return dest.ToArray();
        }
    }
}