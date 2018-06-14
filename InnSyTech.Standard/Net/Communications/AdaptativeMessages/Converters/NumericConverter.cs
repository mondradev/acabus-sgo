using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace InnSyTech.Standard.Net.Communications.AdaptativeMessages.Converters
{
    /// <summary>
    /// Provee de un convertidor de campos tipo <see cref="FieldDefinition.FieldType.Numeric" />  para mensajes adaptativos. El convetidor
    /// utiliza valores enteros sin signos.
    /// </summary>
    internal class NumericConverter : IAdaptativeConverter
    {
        /// <summary>
        /// Convierte  un vector unidimensional en una instancia de campo del tipo numerico a partir de la
        /// definición especificada.
        /// </summary>
        /// <param name="src"> Vector de bytes origen donde se encuentra el campo. </param>
        /// <param name="definition"> Caracteristicas que definen el campo. </param>
        /// <returns> Un campo generado a partir del vector de bytes y la definición especificada. </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// La definición no expresa las caracteristicas para un campo numérico.
        /// </exception>
        /// <exception cref="ArgumentNullException"> Ningun argumento puede ser nulo. </exception>
        public Field Convert(byte[] src, FieldDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException("definition");

            if (src == null)
                throw new ArgumentNullException("src");

            if (definition.Type != FieldDefinition.FieldType.Numeric)
                throw new ArgumentOutOfRangeException("definition", "La definición del campo debe representar un campo numérico");

            if (src.Length == 0)
                throw new ArgumentException("src", "El vector no contiene elementos");

            if (!Validate(src))
                throw new ArgumentException("src", "El vector presenta valores solo numéricos");

            int length = src.Length;
            byte[] dest = src;

            if (length > definition.MaxLength)
                dest = dest.Take(definition.MaxLength).ToArray();

            string valueText = BitConverter.ToString(dest).Replace("-", "");

            ulong value = UInt64.Parse(valueText);

            return new Field(definition.ID, value);
        }

        /// <summary>
        /// Obtiene los bytes del campo del tipo numérico especificado a partir de la definición proporcionada.
        /// </summary>
        /// <param name="src"> Campo a obtener los bytes. </param>
        /// <param name="definition">
        /// Caracteristicas que definen el campo. Al finalizar el proceso, la definición actualiza su longitud.
        /// </param>
        /// <returns> Un vector de bytes que representan al campo. </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// La definición no expresa las caracteristicas para un campo numérico.
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

            if (definition.Type != FieldDefinition.FieldType.Numeric)
                throw new ArgumentOutOfRangeException("definition", "La definición del campo debe representar un campo numérico");

            if (src.ID == definition.ID)
                throw new InvalidOperationException("No es posible utilizar la definición para este campo. Los ID no coinciden");

            string dest = src.Value?.ToString();
            int length = dest.Length / 2;

            if (length > definition.MaxLength)
                dest = dest.Substring(0, length * 2);

            if (!definition.IsVarLength)
                dest = dest.PadLeft(definition.MaxLength * 2, '0');

            definition.Length = dest.Length / 2;

            if (!Regex.IsMatch(dest, "[0-9]*"))
                throw new ArgumentException("El valor del campo no es un tipo numérico entero.");

            List<Byte> destArray = new List<byte>();

            for (int i = 0; i < dest.Length; i += 2)
                destArray.Add(Byte.Parse(dest.Substring(i, 1) + dest.Substring(i + 1, 1), NumberStyles.AllowHexSpecifier));

            return destArray.ToArray();
        }

        /// <summary>
        /// Determina si los valores del vector cuenta con valores unicamente compuesto los simbolos 0-9.
        /// </summary>
        /// <param name="src"> Vector unidimensional a validar. </param>
        /// <returns> Un valor de true si el vector cuenta con simbolos 0-9. </returns>
        private bool Validate(byte[] src)
        {
            string srcText = BitConverter.ToString(src).Replace("-", "");

            return Regex.IsMatch(srcText, "[0-9]*");
        }
    }
}