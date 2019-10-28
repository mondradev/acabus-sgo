using System;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages
{
    /// <summary>
    /// Representa los errores ocurridos al tratar de deserializar un mensaje con formato distinto al esperado.
    /// </summary>
    public class AdaptiveMessageDeserializeException : Exception
    {
        /// <summary>
        /// Crea una nueva excepción.
        /// </summary>
        public AdaptiveMessageDeserializeException() : this("Mensaje con formato invalido, verifique que las caracteristicas sean compatibles", null, null) { }

        /// <summary>
        /// Crea una nueva excepción especificando un solo mensaje.
        /// </summary>
        public AdaptiveMessageDeserializeException(String message) : this(message, null, null) { }

        /// <summary>
        /// Crea una nueva excepción especificando un mensaje y los datos recibidos.
        /// </summary>
        public AdaptiveMessageDeserializeException(String message, byte[] dataReceived) : this(message, dataReceived, null) { }

        /// <summary>
        /// Crea una nueva excepción especificando un mensaje, los datos recibidos y una excepción interna.
        /// </summary>
        public AdaptiveMessageDeserializeException(String message, byte[] dataReceived, Exception innerException) :
            base(message, innerException)
                => DataReceived = dataReceived;

        /// <summary>
        /// Crea una nueva excepción especificando un mensaje y una excepción interna.
        /// </summary>
        public AdaptiveMessageDeserializeException(String message, Exception innerException) : this(message, null, innerException) { }

        /// <summary>
        /// Datos recibidos del flujo de datos.
        /// </summary>
        public byte[] DataReceived { get; }
    }
}