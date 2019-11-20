using System;
using System.Collections;
using System.Collections.Generic;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets
{
    /// <summary>
    /// Controla las enumeraciones que fluyen a través del canal de comunicación por <see cref="IAdaptiveMessage"/>
    /// </summary>
    /// <typeparam name="TResult">Tipo de dato que maneja la colección.</typeparam>
    public sealed class AdaptiveMessageCollection<TResult> : IReadOnlyCollection<TResult>, IDisposable
    {
        /// <summary>
        /// Enumerador de la colección.
        /// </summary>
        private readonly AdaptiveMessageEnumerator<TResult> _enumerator;

        /// <summary>
        /// Indica si fue liberada la conexión al servicio remoto.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Crea una instancia de mensaje para transferir una colección.
        /// </summary>
        /// <param name="message">Mensaje de la colección.</param>
        /// <param name="request">Controlador de la petición</param>
        /// <param name="converter">Función de conversión del contenido del mensaje a el tipo de dato <typeparamref name="TResult"/></param>
        internal AdaptiveMessageCollection(IAdaptiveMessage message, AdaptiveMessageRequest request, Func<IAdaptiveMessage, TResult> converter)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            Message = message;

            _enumerator = new AdaptiveMessageEnumerator<TResult>(message, request, converter);
        }

        /// <summary>
        /// Obtiene la cantidad total de elementos de la colección a transmitir.
        /// </summary>
        public int Count => Message.GetCount();

        /// <summary>
        /// Obtiene el mensaje la colección.
        /// </summary>
        public IAdaptiveMessage Message { get; }

        /// <summary>
        /// Libera la conexión al servicio remoto.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _enumerator.Dispose();
        }

        /// <summary>
        /// Obtiene el enumerador genérico de la colección.
        /// </summary>
        /// <returns>Enumerador de la colección.</returns>
        public IEnumerator<TResult> GetEnumerator()
            => _enumerator;

        /// <summary>
        /// Obtiene el enumerador no genérico de la colección.
        /// </summary>
        /// <returns>Enumerador de la colección.</returns>
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}