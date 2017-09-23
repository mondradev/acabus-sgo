using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace InnSyTech.Standard.Net
{
    /// <summary>
    /// Esta clase contiene funciones básicas para evaluar la disponibilidad de un
    /// equipo remoto en la red.
    /// </summary>
    public static class ConnectionTCP
    {
        /// <summary>
        /// Determina si el host está disponible en la red.
        /// </summary>
        /// <param name="host">Dirección IPv4 del host remoto.</param>
        /// <returns>Un valor true si está disponible el host remoto.</returns>
        public static Boolean IsAvaibleIP(String host)
        {
            short ping = SendToPing(host, 3);
            Trace.WriteLine(String.Format("Eco al host {0}: {1} ms", host, ping), "DEBUG");
            if (ping >= 0)
                return true;
            return false;
        }

        /// <summary>
        /// Envía un mensaje eco al host remoto el número de veces inidicado.
        /// </summary>
        /// <param name="host">Dirección IPv4 del host remoto</param>
        /// <param name="attempts">Número de intentos para realizar el eco (default: 5)</param>
        /// <returns>El tiempo en milisegundo en completar el envío del mensaje eco.</returns>
        public static Int16 SendToPing(String host, Int16 attempts = 5)
        {
            if (!IsIPv4(host))
                return -1;

            Ping ping = new Ping();
            try
            {
                PingReply pr = null;
                Int16 i = 0;
                while ((pr == null || pr.Status != IPStatus.Success) && i < attempts)
                {
                    pr = ping.Send(host);
                    i++;
                    if (i == attempts && pr.Status != IPStatus.Success)
                        return -1;
                }

                if (pr?.Status != IPStatus.Success)
                    return -1;

                return (Int16)pr.RoundtripTime;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Indica si la cadena especificada como argumento es una dirección IPv4 válida.
        /// </summary>
        /// <param name="host">Dirección IPv4 a evaluar.</param>
        /// <returns>Un valor verdadero si la cadena es una dirección IPv4 válida.</returns>
        public static Boolean IsIPv4(String host)
        {
            return IPAddress.TryParse(host, out IPAddress ipAddr);
        }

    }
}
