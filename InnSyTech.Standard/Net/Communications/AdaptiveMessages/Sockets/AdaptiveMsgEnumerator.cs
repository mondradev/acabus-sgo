using System;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets
{
    /// <summary>
    /// Implementación del controlador de las enumeraciones que fluyen a través del canal de comunicación por AdaptiveMessages.
    /// </summary>
    internal sealed class AdaptiveMsgEnumerator : IAdaptiveMsgEnumerator
    {
        /// <summary>
        /// Mensaje que corresponde a la enumeración.
        /// </summary>
        private IMessage _message;

        /// <summary>
        /// Crea una nueva instancia especificando el mensaje.
        /// </summary>
        /// <param name="message">Mensaje que contiene la secuencia.</param>
        public AdaptiveMsgEnumerator(IMessage message)
        {
            _message = message;
        }

        /// <summary>
        /// Obtiene el valor actual en la secuencia.
        /// </summary>
        public IMessage Current {
            get {
                return _message;
            }
        }

        /// <summary>
        /// Obtiene si la secuencia está siendo interrumpida.
        /// </summary>
        public bool Breaking { get; private set; }
        
        /// <summary>
        /// Interrumpe el bucle del recorrido de la secuencia.
        /// </summary>
        public void Break()
            => Breaking = true;

        /// <summary>
        /// Reinicia la secuencia.
        /// </summary>
        public void Reset()
            => _message[10] = 1;
    }
}