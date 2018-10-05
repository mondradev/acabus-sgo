namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets
{
    /// <summary>
    /// Controla las enumeraciones que fluyen a través del canal de comunicación por AdaptiveMessages.
    /// </summary>
    public interface IAdaptiveMsgEnumerator
    {
        /// <summary>
        /// Obtiene el valor actual en la secuencia.
        /// </summary>
        IMessage Current { get; }

        /// <summary>
        /// Interrumpe el bucle del recorrido de la secuencia.
        /// </summary>
        void Break();

        /// <summary>
        /// Reinicia la secuencia.
        /// </summary>
        void Reset();
    }
}