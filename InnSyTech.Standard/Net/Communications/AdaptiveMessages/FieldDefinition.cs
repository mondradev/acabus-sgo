using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Serializers;
using System;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages
{
    /// <summary>
    /// Representa un conjunto de caracteristicas que permiten definir el tipo de campo a manipular.
    ///
    /// <code>
    ///     {
    ///         ID: 21,
    ///         MaxLength: 50,
    ///         IsVarLength: true,
    ///         Type: 2
    ///     }
    /// </code>
    /// <seealso cref="FieldType"/>
    /// <seealso cref="IAdaptiveMessage"/>
    /// <seealso cref="AdaptiveMessageRules"/>
    /// <seealso cref="AdaptiveMessageResponseCode"/>
    /// <seealso cref="AdaptiveMessageFieldID"/>
    /// </summary>
    public sealed class FieldDefinition
    {
        /// <summary>
        /// Crea una nueva instancia definiendo las propiedades del campo a manipular.
        /// </summary>
        /// <param name="id"> Identificador único del campo. </param>
        /// <param name="type"> Tipo de campo. </param>
        /// <param name="maxLength"> Longitud máxima del campo. </param>
        /// <param name="isVarLength"> Indicador si el campo es de longitud variable. </param>
        /// <param name="serializer">Serializador del campo.</param>
        public FieldDefinition(int id, FieldType type, int maxLength, bool isVarLength = false, IAdaptiveSerializer serializer = null)
        {
            ID = id;
            Type = type;
            MaxLength = maxLength;
            IsVarLength = isVarLength;
            Serializer = serializer;
        }

        /// <summary>
        /// Obtiene el identificador del campo.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// Obtiene un valor true si el campo tiene una longitud variable.
        /// </summary>
        public bool IsVarLength { get; }

        /// <summary>
        /// Obtiene la longitud máxima del campo en bytes.
        /// </summary>
        public int MaxLength { get; }

        /// <summary>
        /// Serializador del campo.
        /// </summary>
        public IAdaptiveSerializer Serializer { get; }

        /// <summary>
        /// Obtiene el tipo de campo <seealso cref="FieldType" />.
        /// </summary>
        public FieldType Type { get; }

        /// <summary>
        /// Obtiene una cadena que representa al valor del campo en función del serializador.
        /// </summary>
        /// <param name="value">Valor del campo.</param>
        public string ToString(object value)
            => Serializer.ToString(value, this);

        /// <summary>
        /// Determina si el valor es compatible con el campo definido en función del serializador.
        /// </summary>
        /// <param name="value">Valor a validar.</param>
        public bool Validate(object value)
            => Serializer.Validate(value, this);
    }
}