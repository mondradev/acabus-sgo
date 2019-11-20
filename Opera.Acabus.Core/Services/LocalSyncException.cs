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
        /// Obtiene el nombre de la función donde surgió la excepción remota.
        /// </summary>
        public string Function { get; }

        /// <summary>
        /// Obtiene el nombre del servicio donde surgió la excepción remota.
        /// </summary>
        public string Service { get; }

        /// <summary>
        /// Obtiene mensaje de la excepción remota.
        /// </summary>
        public string Error { get; }

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

        /// <summary>
        /// Crea una nueva excepción especificando los datos de la excepción remota.
        /// </summary>
        /// <param name="error">Mensaje de error de la excepción.</param>
        /// <param name="service">Nombre del servicio.</param>
        /// <param name="function">Nombre de la función.</param>
        public LocalSyncException(string error, string service, string function) : this("Ocurrió una excepción remota")
        {
            Error = error;
            Service = service;
            Function = function;
        }

        /// <summary>
        /// Obtiene un mensaje que describe la excepción actual.
        /// </summary>
        public override string Message => InnerException != null ? $"{base.Message} ({InnerException.Message})" : base.Message;
    }
}