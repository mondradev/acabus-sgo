using Acabus.Utils.Mvvm;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Acabus.Window
{
    /// <summary>
    /// Provee de un modelo de la vista para la ventana principal de la aplicación.
    /// </summary>
    public sealed class AcabusControlCenterViewModel : NotifyPropertyChanged
    {
        /// <summary>
        /// Contiene los mensajes de error omitidos en la aplicación.
        /// </summary>
        private List<String> messageSkiped = new List<string>();

        /// <summary>
        /// Define una clase que implementa TraceListener, utilizada para mostrar en el Snackbar
        /// de la ventana los errores producidos.
        /// </summary>
        private class TraceListenerImp : TraceListener
        {
            /// <summary>
            /// Escribe un mensaje en el snackbar.
            /// </summary>
            /// <param name="message">Mensaje a escribir</param>
            public override void Write(string message) => WriteLine(message);

            /// <summary>
            /// Escribe un mensaje en el Snackbar de la ventana principal, si esta pertenece 
            /// a la categoría de errores.
            /// </summary>
            /// <param name="message">Mensaje a escribir.</param>
            public override void WriteLine(string message)
            {
                if (Instance == null) return;

                File.AppendAllText("acabus.log", DateTime.Now + "\t" + message + "\n\r");

                String[] messageData = message.Split(new Char[] { ':' }, 2);
                if (messageData.Length > 0 && messageData[0] == "NOTIFY" && !Instance.messageSkiped.Contains(messageData[1]))
                    Instance._view.AddMessage(
                        messageData[1],
                        () => Instance.messageSkiped.Add(messageData[1]),
                        "OMITIR");
            }
        }

        /// <summary>
        /// Agrega un escucha al Trace para poder capturar los mensajes y mostrar los de error en la Snackbar
        /// de la ventana.
        /// </summary>
        static AcabusControlCenterViewModel()
        {
            Trace.Listeners.Add(new TraceListenerImp());
        }

        /// <summary>
        /// Campo que provee a la propiedad 'Instance'.
        /// </summary>
        private static AcabusControlCenterViewModel _instance;

        /// <summary>
        /// Instancia de la vista de la ventana principal.
        /// </summary>
        private AcabusControlCenterView _view;

        /// <summary>
        /// Obtiene el valor de esta propiedad.
        /// </summary>
        public static AcabusControlCenterViewModel Instance => _instance;

        /// <summary>
        /// Permite añadir un modulo a la aplicación.
        /// </summary>
        /// <param name="module">Instancia del modulo.</param>
        /// <param name="icon">Icono a visualizar en el botón.</param>
        /// <param name="tooltip">Tip de la herramienta.</param>
        /// <param name="isSecundary">Si el modulo es una función secundaria.</param>
        public static void AddModule(UserControl module, FrameworkElement icon, String tooltip, Boolean isSecundary = false)
        {
            var moduleTemp = module;
            Instance?._view.AddToolButton(module.Name, new CommandBase((parameter) =>
            {
                Instance?._view.ShowContent(module);
            }), icon, tooltip, isSecundary);
        }

        /// <summary>
        /// Muestra una vista en la aplicación como si se tratase de un modulo.
        /// </summary>
        public static void ShowContent(UserControl view)
        {
            Instance?._view.ShowContent(view);
        }

        /// <summary>
        /// Carga todos los modulos leidos del archivo de configuración de la aplicación.
        /// </summary>
        private static void LoadModule()
        {
            foreach (var moduleName in DataAccess.AcabusData.Modules)
            {
                var type = Type.GetType(moduleName);
                type.GetMethod("LoadModule")?.Invoke(null, null);
            }
        }

        /// <summary>
        /// Crea una instancia del modelo de la vista de la ventana principal.
        /// </summary>
        /// <param name="view">Ventana principal.</param>
        public AcabusControlCenterViewModel(AcabusControlCenterView view)
        {
            _view = view;
            _instance = this;
            LoadModule();
        }

        /// <summary>
        /// Agrega un mensaje de notificación a la cola de notificaciones del Snackbar.
        /// </summary>
        /// <param name="message">Mensaje que se desea notificar.</param>
        public static void AddNotify(String message)
        {
            Instance?._view.AddMessage(message);
        }

        /// <summary>
        /// Permite mostrar un mensaje en pantalla.
        /// </summary>
        /// <param name="message">Mensaje simple para mostrar.</param>
        public async static void ShowDialog(String message)
        {
            try
            {
                DialogHost.CloseDialogCommand.Execute(null, null);
                System.Threading.Thread.Sleep(1000);
                await DialogHost.Show(new DialogTemplateView() { Message = message });
            }
            catch
            {
                AddNotify(message);
            }
        }

        /// <summary>
        /// Muestra el cuadro de dialog con el contenido especificado.
        /// </summary>
        /// <param name="dialogContent">Contenido del cuadro de dialogo.</param>
        public static async void ShowDialog(UserControl dialogContent, Action<Object> callback = null)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);

            Object response = await DialogHost.Show(dialogContent);
            GC.SuppressFinalize(dialogContent);

            callback?.Invoke(response);
        }
    }
}
