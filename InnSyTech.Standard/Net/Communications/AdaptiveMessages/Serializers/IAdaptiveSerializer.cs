namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Serializers
{
    /// <summary>
    /// Define dos funciones para realizar la conversión a una secuencia de bytes a un <see
    /// cref="Field"/> y viceversa.
    /// </summary>
    public interface IAdaptiveSerializer
    {
        /// <summary>
        /// Obtiene un campo a partir de una secuencia de bytes en base a la definición especificada.
        /// </summary>
        /// <param name="src">Secuencia de bytes</param>
        /// <param name="definition">Definición del campo</param>
        /// <returns>Un campo generado desde una secuencia de bytes.</returns>
        Field Deserialize(ref byte[] src, FieldDefinition definition);

        /// <summary>
        /// Obtiene una secuencia de bytes a partir de un campo en base a la definición especificada.
        /// </summary>
        /// <param name="src">Campo a serializar</param>
        /// <param name="definition">Definición de campo</param>
        /// <returns>Campo serializa en una secuencia de bytes.</returns>
        byte[] Serialize(Field src, FieldDefinition definition);

        /// <summary>
        /// Determina si un valor es compatible con el campo especificado por la definición.
        /// </summary>
        /// <param name="value">Valor a validar.</param>
        /// <param name="definition">Definición del campo.</param>
        bool Validate(object value, FieldDefinition definition);

        /// <summary>
        /// Obtiene una cadena que representa al valor del campo.
        /// </summary>
        /// <param name="value">Valor del campo.</param>
        /// <param name="definition">Caracteristicas del campo.</param>
        string ToString(object value, FieldDefinition definition);
    }
}