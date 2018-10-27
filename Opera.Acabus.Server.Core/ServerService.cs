using Opera.Acabus.Core.Services;
using System;
using System.Threading.Tasks;

namespace Opera.Acabus.Server.Core
{
    /// <summary>
    /// Provee de los servicios globales del servidor. Esta clase funciona como puente entre los
    /// submódulos y el Core del servidor para realizar las funciones aunque no se haga referencia a
    /// la librería.
    /// </summary>
    public static class ServerService
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
            => Task.Run(()=> Notified?.Invoke(null, push));
    }
}