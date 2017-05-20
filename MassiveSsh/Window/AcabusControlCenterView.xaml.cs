using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Acabus.Window
{
    /// <summary>
    /// Ventana principal de la aplicación Acabus Control Center.
    /// </summary>
    public partial class AcabusControlCenterView : MetroWindow
    {
        /// <summary>
        /// Una lista de mensajes que han sido mostrados mediante el Snackbar.
        /// </summary>
        private Queue<String> _messages = new Queue<String>();


        /// <summary>
        /// Crea una instancia de la ventana principal.
        /// </summary>
        public AcabusControlCenterView()
        {
            InitializeComponent();
            DataContext = new AcabusControlCenterViewModel(this);
        }

        /// <summary>
        /// Permite añadir un botón a la barra de herramientas.
        /// </summary>
        /// <param name="name">Nombre del componente o modulo.</param>
        /// <param name="command">Comando para hacer su llamada.</param>
        /// <param name="buttonContent">Contenido del botón.</param>
        /// <param name="tooltip">Tip de la herramienta.</param>
        public void AddToolButton(String name, ICommand command, FrameworkElement buttonContent, String tooltip)
        {
            foreach (var item in _mainToolBar.Items)
                if (item is Button)
                    if (((Button)item).Name == name)
                        throw new ArgumentException($"Ya existe un botón con el mismo nombre '{name}'");

            buttonContent.Width = 24;
            buttonContent.Height = 24;

            _mainToolBar.Items.Add(new Button()
            {
                Content = buttonContent,
                Command = command,
                Name = name,
                ToolTip = tooltip
            });
        }

        /// <summary>
        /// Permite mostrar el contenido del modulo a visualizar.
        /// </summary>
        /// <param name="content">Modulo a visualizar.</param>
        public void ShowContent(UserControl content)
        {
            _content.Content = content;
        }

        /// <summary>
        /// Agrega un mensaje a la cola de notificaciones en la aplicación.
        /// </summary>
        /// <param name="message">Mensaje que se agregará.</param>
        /// <param name="action">Acción que realiza el Snackbar al hacer clic en el botón.</param>
        /// <param name="actionName">Nombre de la acción a realizar.</param>
        internal void AddMessage(String message, Action action = null, String actionName = "OCULTAR")
        {
            if (_messages.Contains(message)) return;

            App.Current?.Invoke(() =>
            {
                _snackBar.MessageQueue.Enqueue(message, actionName, action);
                _messages.Enqueue(message);
                new Task(() =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(3));
                    _messages.Dequeue();
                }).Start();
            });
        }
    }
}
