namespace Opera.Acabus.Core.Services
{
    /// <summary>
    /// Especifica los mensajes posibles entre el cliente y el servidor.
    /// </summary>
    public enum Messages
    {
        #region Authentication\x00

        /// <summary>
        /// Solicitud de conexión al servidor.
        /// </summary>
        REQUEST_CONNECT,

        /// <summary>
        /// Solicitud de credenciales al cliente.
        /// </summary>
        NEED_AUTHENTICATE,

        /// <summary>
        /// Enviando credenciales.
        /// </summary>
        SEND_CREDENTIALS,

        /// <summary>
        /// Sesión aceptada.
        /// </summary>
        ACCEPT,

        /// <summary>
        /// Sesión rechazada.
        /// </summary>
        REJECT,

        /// <summary>
        /// Señal de presencia.
        /// </summary>
        IS_ALIVE_SIGNAL,

        /// <summary>
        /// Señal de vida.
        /// </summary>
        ALIVE_SIGNAL,

        #endregion

        #region Errors\x10

        /// <summary>
        /// Error: Mala petición.
        /// </summary>
        BAD_REQUEST = '\x10',

        /// <summary>
        /// Error: Servicio no response.
        /// </summary>
        SERVICE_NOT_RESPONSE,

        /// <summary>
        /// Error: Petición desconocida.
        /// </summary>
        UNKNOWN_REQUEST = '\x1F',

        #endregion

        #region Requests\x20

        /// <summary>
        /// Solicitud de incidencias.
        /// </summary>
        REQUEST_INCIDENCES = '\x20',

        #endregion

        #region DynamicResponse\x30

        /// <summary>
        /// Envío de la respuesta.
        /// </summary>
        SEND_RESPONSE = '\x30',

        /// <summary>
        /// Inicio de la respuesta
        /// </summary>
        BEGIN_RESPONSE,

        /// <summary>
        /// Fin de respuesta.
        /// </summary>
        END_RESPONSE,

        /// <summary>
        /// Confirmación de recepción.
        /// </summary>
        COMMIT_RESPONSE,

        /// <summary>
        /// Respuesta de espera.
        /// </summary>
        WAIT_RESPONSE,

        #endregion

        #region DbOperations\x40

        /// <summary>
        /// Solicitud de sincronización de catálogos.
        /// </summary>
        REQUEST_SYNC = '\x40',

        /// <summary>
        /// Comienza sincronización de catálogos.
        /// </summary>
        BEGIN_SYNC,

        /// <summary>
        /// Fin de la sincronización.
        /// </summary>
        END_SYNC,

        /// <summary>
        /// Crea datos nuevos para almacenar en la base de datos.
        /// </summary>
        CREATE_DATA,

        /// <summary>
        /// Actualiza datos almacenados en la base de datos.
        /// </summary>
        UPDATE_DATA,

        #endregion

        #region Disconnect\xF0

        /// <summary>
        /// Solicitud de desconexión.
        /// </summary>
        REQUEST_DISCONNECT = '\xF0',

        /// <summary>
        /// Desconectando servidor.
        /// </summary>
        DISCONNECTING_SERVER,

        /// <summary>
        /// Desconexión aceptada.
        /// </summary>
        DISCONNECTED

        #endregion
    }
}