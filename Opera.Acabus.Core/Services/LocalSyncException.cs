using System;

namespace Opera.Acabus.Core.Services
{
    /// <summary>
    /// Representa los errores producidos en la clase <see cref="EntityLocalSyncBase{T}"/>.
    /// </summary>
    [Serializable]
    public class LocalSyncException : Exception
    {
        /// <summary>
        /// Crea una nueva excepción.
        /// </summary>
        public LocalSyncException() :
            base("Error al sincronizar la información")
        { }

        /// <summary>
        /// Crea una nueva excepción especificando el mensaje de error.
        /// </summary>
        /// <param name="message">Mensaje de error.</param>
        public LocalSyncException(string message)
            : base(message) { }

        /// <summary>
        /// Crea una nueva excepción especificando una excepción interna y un mensaje de error.
        /// </summary>
        /// <param name="message">Mensaje de error.</param>
        /// <param name="inner">Excepción interna.</param>
        public LocalSyncException(string message, Exception inner) :
            base(message, inner)
        { }
    }
}