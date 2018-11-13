using Opera.Acabus.Core.Services;
using System;
using System.Threading.Tasks;

namespace Opera.Acabus.Server.Core
{
    /// <summary>
    /// Provee de funciones que permiten notificar a los usuarios activos de los cambios realizados
    /// en el servidor.
    /// </summary>
    public static class ServerNotify
    {
        /// <summary>
        /// Evento que surge cuando se notifica algo a la red.
        /// </summary>
        public static event EventHandler<PushAcabus> Notified;

        /// <summary>
        /// Notifica algún cambio a la red.
        /// </summary>
        /// <param name="push">Datos del cambio.</param>
        public static void Notify(PushAcabus push)
            => Task.Run(() => Notified?.Invoke(null, push));
    }
}