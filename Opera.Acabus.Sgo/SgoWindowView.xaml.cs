using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        /// Una lista de mensajes que han sido mostrados mediante el Snackbar.
        /// </summary>
        private Queue<String> _messages = new Queue<String>();


        /// <summary>
        /// Crea una instancia de la ventana principal.
        /// </summary>
        public SgoWindowView()
        {
            InitializeComponent();
            DataContext = new SgoWindowModelView(this);
        }

        /// <summary>
        /// Permite añadir un botón a la barra de herramientas.
        /// </summary>
        /// <param name="name">Nombre del componente o modulo.</param>
        /// <param name="command">Comando para hacer su llamada.</param>
        /// <param name="buttonContent">Contenido del botón.</param>
        /// <param name="tooltip">Tip de la herramienta.</param>
        /// <param name="isSecundary">Es un modulo secundario.</param>
        internal void AddToolButton(String name, ICommand command, FrameworkElement buttonContent, String tooltip, Boolean isSecundary)
        {
            foreach (var item in _mainToolBar.Children)
                if (item is Button)
                    if (((Button)item).Name == name)
                        throw new ArgumentException($"Ya existe un botón con el mismo nombre '{name}'");

            buttonContent.Height = 24;
            buttonContent.Width = 24;
            buttonContent.Margin = new Thickness(0);

            Button newButton = new Button()
            {
                Content = buttonContent,
                Command = command,
                Name = name,
                ToolTip = tooltip,
                Style = TryFindResource("MaterialDesignToolButton") as Style,
                Margin = new Thickness(16),
                Padding = new Thickness(0),
                Foreground = TryFindResource("IdealForegroundColorBrush") as Brush
            };
            if (isSecundary)
                DockPanel.SetDock(newButton, Dock.Right);
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
        /// Agrega un mensaje a la cola de notificaciones en la aplicación.
        /// </summary>
        /// <param name="message">Mensaje que se agregará.</param>
        /// <param name="action">Acción que realiza el Snackbar al hacer clic en el botón.</param>
        /// <param name="actionName">Nombre de la acción a realizar.</param>
        internal void AddMessage(String message, Action action = null, String actionName = "OCULTAR")
        {
            if (_messages.Contains(message.ToUpper())) return;

            Application.Current?.Invoke(() =>
            {
                _snackBar.MessageQueue.Enqueue(message.ToUpper(), actionName, action);
                _messages.Enqueue(message.ToUpper());
                new Task(() =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(3));
                    _messages.Dequeue();
                }).Start();
            });


        }
    }
}
