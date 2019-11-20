using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Opera.Acabus.Sgo
{
    /// <summary>
    /// Ventana principal de la aplicación Acabus Control Center.
    /// </summary>
    public partial class SgoWindowView : MetroWindow
    {
        /// <summary>
        /// Indica el tiempo de expiración de los mensajes notificados.
        /// </summary>
        private readonly TimeSpan _expireMessageTime = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Una lista de mensajes que han sido mostrados mediante las notificaciones.
        /// </summary>
        private readonly Queue<ToastMessage> _messages = new Queue<ToastMessage>();

        /// <summary>
        /// Monitor de los mensajes de notificaciones del monitor.
        /// </summary>
        private readonly Timer _notificationMessageMonitor;

        /// <summary>
        /// Contiene los mensajes de error omitidos en la aplicación.
        /// </summary>
        private readonly List<String> _messageSkiped = new List<string>();

        /// <summary>
        /// Crea una instancia de la ventana principal.
        /// </summary>
        public SgoWindowView()
        {
            InitializeComponent();
            DataContext = new SgoWindowModelView(this);

            _dialogHost.SnackbarMessageQueue = _snackBar.MessageQueue;
            _notificationMessageMonitor = new Timer(DropExpiredMessage, null, TimeSpan.Zero, _expireMessageTime);

            Closed += CloseHandler;
        }

        /// <summary>
        /// Obtiene el controlador del cuadro de diálogo de la ventana.
        /// </summary>
        public DialogHost DialogHost => _dialogHost;

        /// <summary>
        /// Agrega un mensaje a la cola de notificaciones en la aplicación.
        /// </summary>
        /// <param name="message">Mensaje que se agregará.</param>
        /// <param name="action">Acción que realiza el Snackbar al hacer clic en el botón.</param>
        /// <param name="actionName">Nombre de la acción a realizar.</param>
        internal void AddMessage(String message, Action action = null, String actionName = "OCULTAR")
        {
            if (_messages.Any(m => m.Message == message)) return;
            if (_messageSkiped.Contains(message.ToUpper())) return;

            action ??= (() => _messageSkiped.Add(message.ToUpper()));

            Application.Current?.Invoke(() =>
            {
                var mainWindow = Application.Current.MainWindow;

                //if (mainWindow.IsActive && mainWindow.WindowState != WindowState.Minimized)
                _snackBar.MessageQueue.Enqueue(message.ToUpper(), actionName, action, true);
                /*else
                    SendToastNotification(message);*/
            });

            _messages.Enqueue(new ToastMessage(message));
        }

        /// <summary>
        /// Agrega una opción de configuración al visor.
        /// </summary>
        /// <param name="description">Descripción breve de la configuración.</param>
        /// <param name="command">Comando que desencadena al hacer clic en la configuración.</param>
        internal void AddSetting(String description, ICommand command)
        {
            PopupBox settingPopup = _mainToolBar.FindChild<PopupBox>("System_Config_SGO");

            if (settingPopup == null)
                return;

            (settingPopup.PopupContent as StackPanel).Children.Add(new Button
            {
                Content = description,
                Command = command
            });
        }

        /// <summary>
        /// Permite añadir un botón a la barra de herramientas.
        /// </summary>
        /// <param name="codeName">Nombre del componente o modulo.</param>
        /// <param name="command">Comando para hacer su llamada.</param>
        /// <param name="buttonContent">Contenido del botón.</param>
        /// <param name="tooltip">Tip de la herramienta.</param>
        /// <param name="isSecundary">Es un modulo secundario.</param>
        internal void CreateToolButton(String codeName, ICommand command, FrameworkElement buttonContent, String tooltip)
        {
            foreach (var item in _mainToolBar.Children)
                if (item is Button)
                    if (((Button)item).Name == codeName)
                        throw new ArgumentException($"Ya existe un botón con el mismo nombre código '{codeName}'");

            StackPanel content = new StackPanel()
            {
                Height = 24,
                Margin = new Thickness(0),
                Orientation = Orientation.Horizontal
            };

            if (buttonContent != null)
            {
                buttonContent.Width = 24;
                buttonContent.Height = 24;
                buttonContent.Margin = new Thickness(0);

                content.Children.Add(buttonContent);
            }

            content.Children.Add(new Label()
            {
                Content = tooltip,
                Foreground = TryFindResource("IdealForegroundColorBrush") as Brush
            });

            Button newButton = new Button()
            {
                Content = content,
                Command = command,
                Name = codeName,
                ToolTip = tooltip,
                Style = TryFindResource("MaterialDesignToolButton") as Style,
                Margin = new Thickness(16),
                Padding = new Thickness(0)
            };

            _mainToolBar.Children.Add(newButton);
        }

        /// <summary>
        /// Permite mostrar el contenido del modulo a visualizar.
        /// </summary>
        /// <param name="content">Modulo a visualizar.</param>
        internal void ShowContent(UserControl content)
        {
            _content.Children.Clear();
            _content.Children.Add(content);
        }

        /// <summary>
        /// Elimina los mensajes notificados al pasar el tiempo especificado.
        /// </summary>
        /// <param name="state">Estado del temporizador.</param>
        private void DropExpiredMessage(object state)
        {
            while (_messages.Any(m => (DateTime.Now - m.TimeCreated) > _expireMessageTime))
                _messages.Dequeue();
        }

        /// <summary>
        /// Envía una notificación a Windows.
        /// </summary>
        /// <param name="message">Mensaje a notificar.</param>
        private void SendToastNotification(string message)
        {
        }

        /// <summary>
        ///
        /// </summary>
        private void CloseHandler(object _, EventArgs __) => _notificationMessageMonitor.Dispose();

        /// <summary>
        /// Representa un mensaje de notificación.
        /// </summary>
        private class ToastMessage
        {
            /// <summary>
            /// Crea un nuevo mensaje.
            /// </summary>
            public ToastMessage(String message)
            {
                Message = message;
                TimeCreated = DateTime.Now;
            }

            /// <summary>
            /// Obtiene el mensaje de la notificación.
            /// </summary>
            public String Message { get; }

            /// <summary>
            /// Obtiene el tiempo cuando se creó la notificación.
            /// </summary>
            public DateTime TimeCreated { get; }
        }
    }
}