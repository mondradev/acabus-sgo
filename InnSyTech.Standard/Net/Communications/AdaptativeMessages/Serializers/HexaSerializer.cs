using System;
using System.Collections.Generic;
using System.Linq;

namespace InnSyTech.Standard.Net.Communications.AdaptativeMessages.Serializers
{
    /// <summary>
    /// Provee de un convertidor de campos tipos que se almacenan en bytes para mensajes adaptativos.
    /// </summary>
    internal abstract class HexaSerializer : IAdaptativeSerializer
    {
        /// <summary>
        /// Convierte una vector de bytes en una instancia de campo a partir de la definición
        /// especificada. Al finalizar el proceso, el vector de bytes le serán extraidos los bytes utilizados.
        /// </summary>
        /// <param name="src">Vector de bytes origen donde se encuentra el campo.</param>
        /// <param name="definition">Caracteristicas que definen el campo.</param>
        /// <returns>Un campo generado a partir del vector de bytes y la definición especificada.</returns>
        public abstract Field Deserialize(ref byte[] src, FieldDefinition definition);

        /// <summary>
        /// Obtiene los bytes del campo especificado a partir de la definición proporcionada.
        /// </summary>
        /// <param name="src">Campo a obtener los bytes.</param>
        /// <param name="definition">
        /// Caracteristicas que definen el campo. Al finalizar el proceso, la definición actualiza su
        /// atributo longitud.
        /// </param>
        /// <returns>Un vector de bytes que representan al campo.</returns>
        public abstract byte[] Serialize(Field src, FieldDefinition definition);
        
        /// <summary>
        /// Obtiene la longitud del campo.
        /// </summary>
        /// <param name="src">Vector de bytes que contiene la información.</param>
        /// <param name="definition">Caracteristicas que definen el campo.</param>
        /// <returns>La longitud real del campo.</returns>
        protected int GetLengthFromBytes(byte[] src, FieldDefinition definition, out int lvarSize)
        {
            lvarSize = 0;

            if (!definition.IsVarLength)
                return definition.MaxLength;

            lvarSize = GetLVarSize(definition);

            return BitConverter.ToInt32(src.Take(lvarSize).Reverse().ToArray(), 0);
        }

        /// <summary>
        /// Obtiene el tamaño en bytes de la longitud variable del campo.
        /// </summary>
        /// <param name="definition">Definición con las caracteristicas del campo.</param>
        /// <returns>Tamaño de la longitud variable.</returns>
        protected int GetLVarSize(FieldDefinition definition)
        {
            if (!definition.IsVarLength)
                return 0;

            byte[] lvar = BitConverter.GetBytes(definition.MaxLength);

            return lvar.Length;
        }
    }
}