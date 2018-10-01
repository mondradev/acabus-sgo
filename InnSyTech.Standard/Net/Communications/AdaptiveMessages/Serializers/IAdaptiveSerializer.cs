namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Serializers
{
    /// <summary>
    /// Provee de una interfaz para definir un tipo de convertidor de campos para mensajes adaptativos.
    /// </summary>
    internal interface IAdaptiveSerializer
    {
        /// <summary>
        /// Convierte una vector de bytes en una instancia de campo a partir de la definición
        /// especificada. Al finalizar el proceso, el vector de bytes le serán extraidos los bytes utilizados.
        /// </summary>
        /// <param name="src"> Vector de bytes origen donde se encuentra el campo. </param>
        /// <param name="definition"> Caracteristicas que definen el campo. </param>
        /// <returns> Un campo generado a partir del vector de bytes y la definición especificada. </returns>
        Field Deserialize(ref byte[] src, FieldDefinition definition);

        /// <summary>
        /// Obtiene los bytes del campo especificado a partir de la definición proporcionada.
        /// </summary>
        /// <param name="src"> Campo a obtener los bytes. </param>
        /// <param name="definition">
        /// Caracteristicas que definen el campo. Al finalizar el proceso, la definición actualiza
        /// su atributo longitud.
        /// </param>
        /// <returns> Un vector de bytes que representan al campo. </returns>
        byte[] Serialize(Field src, FieldDefinition definition);
    }
}