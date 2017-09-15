using InnSyTech.Standard.Mvvm;
using System;
using System.Windows.Controls;

namespace Opera.Acabus.Core.Gui.Modules
{
    /// <summary>
    /// Proporciona una clase base para los modelos de la vista de un módulo.
    /// </summary>
    public abstract class ModuleViewerBase : ViewModelBase
    {
        /// <summary>
        /// Envía una notificación a la interfaz gráfica.
        /// </summary>
        /// <param name="message">Mensaje de la notificación.</param>
        protected void SendNotify(String message)
            => Dispatcher.SendNotify(message);

        /// <summary>
        /// Muestra un contenido en el visor.
        /// </summary>
        /// <param name="content">Contenido a mostrar en el visor.</param>
        protected void ShowControlView(UserControl content)
            => Dispatcher.RequestShowContent(content);

        /// <summary>
        /// Muestra un cuadro de dialogo que presenta un contenido personalizado y ejecuta una acción
        /// al cerrar el cuadro de dialogo. Utilizar <see cref="Dispatcher.CloseDialogCommand"/> para
        /// cerrar el cuadro de dialogo.
        /// </summary>
        /// <param name="content">Contenido a mostrar en el cuadro de dialogo.</param>
        /// <param name="action">Acción a descencadenar después de cerrar el cuadro.</param>
        protected void ShowDialog(UserControl content, Action<Object> action)
            => Dispatcher.RequestShowDialog(content, action);

        /// <summary>
        /// Muestra un cuadro de dialogo que presenta un contenido personalizado. Utilizar <see
        /// cref="Dispatcher.CloseDialogCommand"/> para cerrar el cuadro de dialogo.
        /// </summary>
        /// <param name="content">Contenido a mostrar en el cuadro de dialogo.</param>
        protected void ShowDialog(UserControl content)
            => Dispatcher.RequestShowDialog(content);

        /// <summary>
        /// Muestra un mensaje en un cuadro de dialogo.
        /// </summary>
        /// <param name="message">Mensaje a mostrar.</param>
        protected void ShowMessage(String message)
            => Dispatcher.SendMessageToGUI(message);
    }
}