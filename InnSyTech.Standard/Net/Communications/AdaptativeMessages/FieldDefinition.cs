namespace InnSyTech.Standard.Net.Communications.AdaptativeMessages
{
    /// <summary>
    /// Representa un conjunto de caracteristicas que permiten definir el tipo de campo a manipular.
    ///
    /// <code>
    ///     {
    ///         ID: 21,
    ///         Length: 20,
    ///         MaxLength: 50,
    ///         IsVarLength: true,
    ///         Type: { Numeric, Text, Binary }
    ///     }
    /// </code>
    /// </summary>
    internal class FieldDefinition
    {
        /// <summary>
        /// Crea una nueva instancia definiendo las propiedades del campo a manipular.
        /// </summary>
        /// <param name="id"> Identificador único del campo. </param>
        /// <param name="type"> Tipo de campo. </param>
        /// <param name="maxLength"> Longitud máxima del campo. </param>
        /// <param name="isVarLength"> Indicador si el campo es de longitud variable. </param>
        public FieldDefinition(int id, FieldType type, int maxLength, bool isVarLength = false)
        {
            ID = id;
            Type = type;
            MaxLength = maxLength;
            IsVarLength = IsVarLength;
            Length = maxLength;
        }

        /// <summary>
        /// Define los tipos de campos que pueden ser utilizados en los mensajes adaptativos.
        /// </summary>
        public enum FieldType
        {
            /// <summary>
            /// Representa un campo numérico en base 10.
            /// </summary>
            Numeric,

            /// <summary>
            /// Representa un campo de texto.
            /// </summary>
            Text,

            /// <summary>
            /// Representa un campo de tipo binario.
            /// </summary>
            Binary
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
        /// Obtiene o establece la longitud actual del campo en bytes.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Obtiene la longitud máxima del campo en bytes.
        /// </summary>
        public int MaxLength { get; }

        /// <summary>
        /// Obtiene el tipo de campo <seealso cref="FieldType" />.
        /// </summary>
        public FieldType Type { get; }
    }
}