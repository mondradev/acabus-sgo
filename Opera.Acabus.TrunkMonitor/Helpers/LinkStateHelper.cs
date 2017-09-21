using Opera.Acabus.TrunkMonitor.Models;
using System;

namespace Opera.Acabus.TrunkMonitor.Helpers
{
    /// <summary>
    /// Provee de funciones extras a la enumeración <see cref="LinkState"/>.
    /// </summary>
    public static class LinkStateHelper
    {
        /// <summary>
        /// Obtiene un estado de enlace a partir de otros dos.
        /// </summary>
        /// <param name="linkstate">Estado de enlace a evaluar.</param>
        /// <param name="anotherLinkState">Otro estado de enlace a evaluar.</param>
        /// <returns>Un estado de enlace resultado.</returns>
        public static LinkState And(this LinkState linkstate, LinkState anotherLinkState)
        {
            if (linkstate == anotherLinkState)
                return linkstate;
            if (linkstate < anotherLinkState)
                return linkstate;
            return anotherLinkState;
        }

        /// <summary>
        /// Obtiene el estado de conexión determinado por la latencia optima y máxima aceptable.
        /// </summary>
        /// <param name="ping">Latencia de la conexión.</param>
        /// <param name="optimalPing">Latencia optima.</param>
        /// <param name="acceptablePing">Latencia máxima aceptable.</param>
        /// <returns>Si la latencia revasa el máximo devuelve un <see cref="LinkState.BAD"/>,
        ///          si revasa el mínimo devuelve un <see cref="LinkState.MEDIUM"/>,
        ///          si es menor a cero devuelve un <see cref="LinkState.DISCONNECTED"/>,
        ///          en otro caso devuelve un <see cref="LinkState.GOOD"/>.</returns>
        public static LinkState CalculateLinkState(Int16 ping, UInt16 optimalPing, UInt16 acceptablePing)
        {
            if (ping > (Int16)acceptablePing)
                return LinkState.BAD;
            if (ping > (Int16)optimalPing)
                return LinkState.MEDIUM;
            if (ping < 0)
                return LinkState.DISCONNECTED;
            return LinkState.GOOD;
        }
    }
}