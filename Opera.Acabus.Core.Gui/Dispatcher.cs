using System;
using System.Windows.Input;
using static Opera.Acabus.Core.Gui.Dispatcher.RequestSendMessageArg;

namespace Opera.Acabus.Core.Gui
{
    /// <summary>
    /// Gestiona toda la comunicación con los visores de datos de la aplicación.
    /// </summary>
    public static class Dispatcher
    {
        /// <summary>
        /// Representa el método que controla el evento para la petición envío de mensajes y
        /// notificaciones a la interfaz.
        /// </summary>
        /// <param name="arg">Argumentos del evento.</param>
        public delegate void RequestSendMessageHandler(RequestSendMessageArg arg);

        /// <summary>
        /// Representa el método que controla el evento para la petición de muestra de contenido.
        /// </summary>
        /// <param name="arg">Argumentos del evento.</param>
        public delegate void RequestShowContentHandler(RequestShowContentArg arg);

        /// <summary>
        /// Evento que se desencadena cuando se realiza una petición para envíar un mensaje o
        /// notificación a la interfaz.
        /// </summary>
        public static event RequestSendMessageHandler RequestingSendMessageOrNotify;

        /// <summary>
        /// Evento que se desencadena cuando se realiza una petición para mostrar el contenido.
        /// </summary>
        public static event RequestShowContentHandler RequestingShowContent;

        /// <summary>
        /// Evento que se desencadena cuando se realiza una petición para mostrar el contenido a
        /// través de un cuadro de diálogo.
        /// </summary>
        public static event RequestShowContentHandler RequestingShowDialog;

        /// <summary>
        /// Obtiene o establece el comando que cierra el cuadro de dialogo abierto actualmente.
        /// </summary>
        public static ICommand CloseDialogCommand { get; set; }

        /// <summary>
        /// Obtiene o establece el comando que abre el cuadro de dialogo.
        /// </summary>
        public static ICommand OpenDialogCommand { get; set; }

        /// <summary>
        /// Solicita al controlador de interfaz mostrar la vista pasada por parametro.
        /// </summary>
        public static void RequestShowContent(System.Windows.Controls.UserControl content)
            => RequestingShowContent?.Invoke(new RequestShowContentArg(content));

        /// <summary>
        /// Solicita a la ventana principal del SGO mostrar el contenido especificado y realizar una
        /// función cuando este sea ocultado.
        /// </summary>
        /// <param name="content">Contenido a mostrar.</param>
        /// <param name="execute">Acción a realizar al ocultar el contenido.</param>
        public static void RequestShowDialog(System.Windows.Controls.UserControl content, Action<object> execute = null)
            => RequestingShowDialog?.Invoke(new RequestShowContentArg(content, execute));

        /// <summary>
        /// Envía una mensaje a la interfaz gráfica.
        /// </summary>
        /// <param name="message">Mensaje a envíar.</param>
        public static void SendMessageToGUI(string message)
            => RequestingSendMessageOrNotify?
                .Invoke(new RequestSendMessageArg(message, RequestSendType.MESSAGE));

        /// <summary>
        /// Envía una notificación a la interfaz gráfica.
        /// </summary>
        /// <param name="message">Mensaje a notificar.</param>
        public static void SendNotify(string message)
            => RequestingSendMessageOrNotify?
                .Invoke(new RequestSendMessageArg(message, RequestSendType.NOTIFY));

        /// <summary>
        /// Define la estructura de los argumentos utilizados para la solicitud de envío de mensajes
        /// o notificaciones a la interfaz.
        /// </summary>
        public sealed class RequestSendMessageArg : EventArgs
        {
            /// <summary>
            /// Crea una nueva instancia de <see cref="RequestSendMessageArg"/>.
            /// </summary>
            /// <param name="message">Mensaje a mostrar.</param>
            /// <param name="type">Tipo la petición de envío.</param>
            public RequestSendMessageArg(String message, RequestSendType type)
            {
                Message = message;
                SendType = type;
            }

            /// <summary>
            /// Define los tipos de petición de envío.
            /// </summary>
            public enum RequestSendType
            {
                /// <summary>
                /// Petición de envío tipo mensaje. Muestra un pequeño cuadro de diálogo con el
                /// mensaje a mostrar.
                /// </summary>
                MESSAGE,

                /// <summary>
                /// Petición de envío tipo notificación. Muestra una notificación en la parte
                /// inferior de la ventana.
                /// </summary>
                NOTIFY
            }

            /// <summary>
            /// Obtiene el mensaje a mostrar.
            /// </summary>
            public String Message { get; }

            /// <summary>
            /// Obtiene el tipo de petición de envío.
            /// </summary>
            public RequestSendType SendType { get; }
        }

        /// <summary>
        /// Define la estructura de los argumentos utilizados para la solicitud de muestra de
        /// contenido o cuadros de dialogos con función de llamada de vuelta.
        /// </summary>
        public sealed class RequestShowContentArg : EventArgs
        {
            /// <summary>
            /// Crea una instancia nueva de <see cref="RequestShowContentArg"/>.
            /// </summary>
            /// <param name="content">Contenido a que se solicita mostrar.</param>
            /// <param name="callback">Función a ejecutar al ocultar el contenido.</param>
            public RequestShowContentArg(System.Windows.Controls.UserControl content, Action<Object> callback = null)
            {
                Content = content;
                Callback = callback;
            }

            /// <summary>
            /// Obtiene la función de llamada devuelta que se ejecuta cuando el contenido se oculta.
            /// </summary>
            public Action<Object> Callback { get; }

            /// <summary>
            /// Obtiene el contenido a mostrar.
            /// </summary>
            public System.Windows.Controls.UserControl Content { get; }
        }
    }
}