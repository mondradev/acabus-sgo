using System;
using System.Windows.Input;

namespace Opera.Acabus.Core.Gui
{
    /// <summary>
    /// Gestiona toda la comunicaci�n con los visores de datos de la aplicaci�n.
    /// </summary>
    public static class Dispatcher
    {
        /// <summary>
        /// Representa el m�todo que controla el evento para la petici�n env�o de mensajes y
        /// notificaciones a la interfaz.
        /// </summary>
        /// <param name="arg">Argumentos del evento.</param>
        public delegate void RequestSendMessageHandler(RequestSendMessageArg arg);

        /// <summary>
        /// Representa el m�todo que controla el evento para la petici�n de muestra de contenido.
        /// </summary>
        /// <param name="arg">Argumentos del evento.</param>
        public delegate void RequestShowContentHandler(RequestShowContentArg arg);

        /// <summary>
        /// Evento que se desencadena cuando se realiza una petici�n para env�ar un mensaje o
        /// notificaci�n a la interfaz.
        /// </summary>
        public static event RequestSendMessageHandler RequestingSendMessageOrNotify;

        /// <summary>
        /// Evento que se desencadena cuando se realiza una petici�n para mostrar el contenido.
        /// </summary>
        public static event RequestShowContentHandler RequestingShowContent;

        /// <summary>
        /// Evento que se desencadena cuando se realiza una petici�n para mostrar el contenido a
        /// trav�s de un cuadro de di�logo.
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
        /// Cierra el cuadro de dialogo actualmente abierto en la pantalla.
        /// </summary>
        /// <param name="parameter">Valor a devolver del cierre del cuadro de dialogo.</param>
        public static void CloseDialog(Object parameter = null)
            => CloseDialogCommand?.Execute(parameter);

        /// <summary>
        /// Solicita al controlador de interfaz mostrar la vista pasada por parametro.
        /// </summary>
        public static void RequestShowContent(System.Windows.Controls.UserControl content)
            => RequestingShowContent?.Invoke(new RequestShowContentArg(content));

        /// <summary>
        /// Solicita a la ventana principal del SGO mostrar el contenido especificado y realizar una
        /// funci�n cuando este sea ocultado.
        /// </summary>
        /// <param name="content">Contenido a mostrar.</param>
        /// <param name="execute">Acci�n a realizar al ocultar el contenido.</param>
        public static void RequestShowDialog(System.Windows.Controls.UserControl content, Action<object> execute = null)
            => RequestingShowDialog?.Invoke(new RequestShowContentArg(content, execute));

        /// <summary>
        /// Env�a una mensaje a la interfaz gr�fica.
        /// </summary>
        /// <param name="message">Mensaje a env�ar.</param>
        public static void SendMessageToGUI(string message)
            => RequestingSendMessageOrNotify?
                .Invoke(new RequestSendMessageArg(message, RequestSendMessageArg.RequestSendType.MESSAGE));

        /// <summary>
        /// Env�a una notificaci�n a la interfaz gr�fica.
        /// </summary>
        /// <param name="message">Mensaje a notificar.</param>
        public static void SendNotify(string message)
            => RequestingSendMessageOrNotify?
                .Invoke(new RequestSendMessageArg(message, RequestSendMessageArg.RequestSendType.NOTIFY));

        /// <summary>
        /// Define la estructura de los argumentos utilizados para la solicitud de env�o de mensajes
        /// o notificaciones a la interfaz.
        /// </summary>
        public sealed class RequestSendMessageArg : EventArgs
        {
            /// <summary>
            /// Crea una nueva instancia de <see cref="RequestSendMessageArg"/>.
            /// </summary>
            /// <param name="message">Mensaje a mostrar.</param>
            /// <param name="type">Tipo la petici�n de env�o.</param>
            public RequestSendMessageArg(String message, RequestSendType type)
            {
                Message = message;
                SendType = type;
            }

            /// <summary>
            /// Define los tipos de petici�n de env�o.
            /// </summary>
            public enum RequestSendType
            {
                /// <summary>
                /// Petici�n de env�o tipo mensaje. Muestra un peque�o cuadro de di�logo con el
                /// mensaje a mostrar.
                /// </summary>
                MESSAGE,

                /// <summary>
                /// Petici�n de env�o tipo notificaci�n. Muestra una notificaci�n en la parte
                /// inferior de la ventana.
                /// </summary>
                NOTIFY
            }

            /// <summary>
            /// Obtiene el mensaje a mostrar.
            /// </summary>
            public String Message { get; }

            /// <summary>
            /// Obtiene el tipo de petici�n de env�o.
            /// </summary>
            public RequestSendType SendType { get; }
        }

        /// <summary>
        /// Define la estructura de los argumentos utilizados para la solicitud de muestra de
        /// contenido o cuadros de dialogos con funci�n de llamada de vuelta.
        /// </summary>
        public sealed class RequestShowContentArg : EventArgs
        {
            /// <summary>
            /// Crea una instancia nueva de <see cref="RequestShowContentArg"/>.
            /// </summary>
            /// <param name="content">Contenido a que se solicita mostrar.</param>
            /// <param name="callback">Funci�n a ejecutar al ocultar el contenido.</param>
            public RequestShowContentArg(System.Windows.Controls.UserControl content, Action<Object> callback = null)
            {
                Content = content;
                Callback = callback;
            }

            /// <summary>
            /// Obtiene la funci�n de llamada devuelta que se ejecuta cuando el contenido se oculta.
            /// </summary>
            public Action<Object> Callback { get; }

            /// <summary>
            /// Obtiene el contenido a mostrar.
            /// </summary>
            public System.Windows.Controls.UserControl Content { get; }
        }
    }
}