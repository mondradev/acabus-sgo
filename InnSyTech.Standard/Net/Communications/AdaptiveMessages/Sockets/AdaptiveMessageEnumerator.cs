using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets
{
    /// <summary>
    /// Enumerador de una colección enviada a través de <see cref="IAdaptiveMessage"/>.
    /// </summary>
    /// <typeparam name="TResult">Tipo de dato de la colección enviada.</typeparam>
    public sealed class AdaptiveMessageEnumerator<TResult> : IEnumerator<TResult>
    {
        /// <summary>
        /// Controlador de la petición.
        /// </summary>
        private readonly AdaptiveMessageRequest _request;

        /// <summary>
        /// Función de conversión de datos.
        /// </summary>
        private readonly Func<IAdaptiveMessage, TResult> _converter;

        /// <summary>
        /// Indica si fue liberada la conexión al servicio remoto.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Posición actual en la colección.
        /// </summary>
        private int _position;

        /// <summary>
        /// Crea el enumerador de la colección.
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        /// <param name="request">Controlador de la petición.</param>
        /// <param name="converter">Función de conversión del contenido del mensaje al tipo <typeparamref name="TResult"/></param>
        internal AdaptiveMessageEnumerator(IAdaptiveMessage message, AdaptiveMessageRequest request, Func<IAdaptiveMessage, TResult> converter)
        {
            this.Message = message;

            this._request = request;
            this._converter = converter;
            this._disposed = false;
            this._position = -1;
        }

        /// <summary>
        /// Obtiene el elemento de la posición actual en la colección.
        /// </summary>
        public TResult Current => _position >= 0 ? this._converter(Message) : default;

        /// <summary>
        /// Obtiene el mensaje de la colección.
        /// </summary>
        public IAdaptiveMessage Message { get; private set; }

        /// <summary>
        /// Obtiene la instancia objeto del elemento de la posición actual en la colección.
        /// </summary>
        object IEnumerator.Current => this.Current;

        /// <summary>
        /// Libera la conexión al servicio remoto.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _request.RemoteEndPoint.Shutdown(SocketShutdown.Both);
            _request.RemoteEndPoint.Disconnect(false);
            _request.RemoteEndPoint.Close();
            _request.RemoteEndPoint.Dispose();
        }

        /// <summary>
        /// Avanza a la siguiente posición en la colección.
        /// </summary>
        /// <returns>Un valor true si hay logró avanzar el recorrido en la colección.</returns>
        public bool MoveNext()
        {
            if (Message.GetCount() == 0 || !Message.IsEnumerable())
                return false;

            Message.SetPosition(Message.GetPosition() + 1);

            if (!_request.RemoteEndPoint.Connected)
                throw new SocketException((int)SocketError.NotConnected);

            int bytesTransferred = _request.RemoteEndPoint.Send(Message.Serialize());

            if (bytesTransferred <= 0)
                return false;

            Trace.TraceInformation("Bytes enviados: " + bytesTransferred);

            if (Message.GetPosition() >= Message.GetCount())
                return false;

            Message = AdaptiveMessageSocketHelper.ReadMessage(_request.RemoteEndPoint, Message.Rules);

            if (!Message.IsEnumerable())
                throw new InvalidOperationException("No se recibió un mensaje que represente una colección de datos.");

            int count = Message.GetCount();

            if (count == 0)
                return false;

            _position = Message.GetPosition();

            if (_position >= Message.GetCount() || _position < 0)
                return false;

            return true;
        }

        /// <summary>
        /// Reinicia el recorrido de la colección.
        /// </summary>
        public void Reset()
        {
            _position = -1;
        }
    }
}