using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using System;

namespace Opera.Acabus.Core.Services
{
    /// <summary>
    /// Extensión para los mensajes utilizados en el sistema acabus.
    /// </summary>
    public static class AcabusAdaptiveMessageExtension
    {
        /// <summary>
        /// Establece el token de disposivito requerido para la autenticación de un nodo válido.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <param name="deviceToken">Token de dispositivo.</param>
        public static void SetDeviceToken(this IMessage message, byte[] deviceToken)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            message[(int)AcabusAdaptiveMessageFieldID.DeviceToken] = deviceToken;
        }

        /// <summary>
        /// Indica si tiene el campo de DeviceToken activo.
        /// </summary>
        /// <param name="message">Mensaje adaptivo.</param>
        /// <returns>Un valor true si tiene el campo activo.</returns>
        public static Boolean HasDeviceToken(this IMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return message.IsSet((int)AcabusAdaptiveMessageFieldID.DeviceToken);
        }

        /// <summary>
        /// Obtiene el token de disposivito requerido para la autenticación de un nodo válido.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <returns>Token del dispositivo.</returns>
        public static byte[] GetDeviceToken(this IMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return message.GetBytes((int)AcabusAdaptiveMessageFieldID.DeviceToken);
        }

    }
}