using InnSyTech.Standard.Configuration;
using InnSyTech.Standard.Mvvm;
using MaterialDesignThemes.Wpf;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Gui.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Controls;

namespace Opera.Acabus.Sgo
{
    /// <summary>
    /// Provee de un modelo de la vista para la ventana principal de la aplicación.
    /// </summary>
    public sealed class SgoWindowModelView : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Instance'.
        /// </summary>
        private static SgoWindowModelView _instance;

        /// <summary>
        /// Indica si está activa el botón de configuración.
        /// </summary>
        private bool _configurationAvailable;

        /// <summary>
        /// Lista de todos los módulos cargados en el visor.
        /// </summary>
        private List<IModuleInfo> _modules;

        /// <summary>
        /// Instancia de la vista de la ventana principal.
        /// </summary>
        private SgoWindowView _view;

        /// <summary>
        /// Crea una instancia del modelo de la vista de la ventana principal.
        /// </summary>
        /// <param name="view">Ventana principal.</param>
        public SgoWindowModelView(SgoWindowView view)
        {
            if (_instance != null)
                return;

            _view = view;
            _instance = this;
            _modules = new List<IModuleInfo>();

            Dispatcher.CloseDialogCommand = DialogHost.CloseDialogCommand;

            Dispatcher.OpenDialogCommand = new Command(param => OpenDialg(param));

            Dispatcher.RequestingShowContent += arg
                => _view.ShowContent(arg.Content);

            Dispatcher.RequestingShowDialog += arg
                => Dispatcher.OpenDialogCommand?.Execute(arg);

            Dispatcher.RequestingSendMessageOrNotify += arg =>
            {
                switch (arg.SendType)
                {
                    case Dispatcher.RequestSendMessageArg.RequestSendType.MESSAGE:
                        OpenDialg(new DialogTemplateView() { Message = arg.Message });
                        break;

                    case Dispatcher.RequestSendMessageArg.RequestSendType.NOTIFY:
                        Trace.WriteLine(arg.Message?.ToUpper(), "NOTIFY");
                        break;
                }
            };

            Trace.Listeners.Add(new TraceListenerImp());

            LoadModule();
        }

        /// <summary>
        /// Obtiene el valor de esta propiedad.
        /// </summary>
        public static SgoWindowModelView Instance => _instance;

        /// <summary>
        /// Obtiene una lista de los módulos cargados en el visor.
        /// </summary>
        public IReadOnlyList<IModuleInfo> ModulesLoaded => _modules;

        /// <summary>
        /// Carga todos los modulos leidos del archivo de configuración de la aplicación.
        /// </summary>
        private void LoadModule()
        {
            foreach (ISetting module in AcabusDataContext.ConfigContext["modules"]?.GetSettings("module"))
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(module.ToString("assembly"));
                    var type = assembly.GetType(module.ToString("fullname"));

                    if (type is null)
                        throw new Exception($"Libería no contiene módulo especificado ---> {module.ToString("fullname")}");

                    IModuleInfo moduleInfo = (IModuleInfo)Activator.CreateInstance(type);

                    _modules.Add(moduleInfo);
                }
                catch (FileNotFoundException)
                {
                    Dispatcher.SendNotify($"No se encontró el módulo '{module.ToString("fullname")}'");
                }
                catch (Exception)
                {
                    Dispatcher.SendNotify($"No se encontró módulo '{module.ToString("fullname")}' en libería '{module.ToString("assembly")}'");
                }
            }

            foreach (IModuleInfo moduleInfo in _modules)
            {
                if (moduleInfo.ModuleType == ModuleType.SERVICE)
                    if (!moduleInfo.LoadModule())
                        Dispatcher.SendNotify($"El servicio '{moduleInfo.Name} no se logró iniciar.");

                if (moduleInfo.ModuleType == ModuleType.VIEWER)
                {
                    UserControl moduleView = null;
                    _view.CreateToolButton(moduleInfo.CodeName, new Command(delegate
                    {
                        if (moduleView == null)
                            moduleView = (UserControl)Activator.CreateInstance(moduleInfo.ViewType);
                        _view.ShowContent(moduleView);
                    }), moduleInfo.Icon, moduleInfo.Name);

                    if (!moduleInfo.LoadModule())
                        Dispatcher.SendNotify($"El módulo '{moduleInfo.Name} no se logró iniciar.");
                }

                if (moduleInfo.ModuleType == ModuleType.CONFIGURATION)
                {
                    if (!_configurationAvailable)
                    {
                        _view.CreateSettingsButton();

                        _configurationAvailable = true;
                    }

                    UserControl moduleView = null;

                    _view.AddSetting(moduleInfo.Name, new Command(delegate
                    {
                        if (moduleView == null)
                            moduleView = (UserControl)Activator.CreateInstance(moduleInfo.ViewType);

                        _view.ShowContent(moduleView);
                    }));
                }
            }
        }

        /// <summary>
        /// Muestra un cuadro de diálogo en la ventana actual.
        /// </summary>
        /// <param name="parameters">Parametros del cuadro de dialogo.</param>
        private void OpenDialg(object parameters)
        {
            var dialogHost = _view.DialogHost;

            DialogClosingEventHandler callback = async (sender, eventArgs) =>
            {
                if (parameters is Dispatcher.RequestShowContentArg)
                {
                    Dispatcher.RequestShowContentArg arg = parameters as Dispatcher.RequestShowContentArg;
                    Object response = await DialogHost.Show(arg.Content);
                    GC.SuppressFinalize(arg.Content);
                    arg.Callback?.Invoke(response);
                }
                else
                    await DialogHost.Show(parameters);
            };

            if (dialogHost.IsOpen)
                dialogHost.DialogClosingCallback = callback;
            else
                callback.Invoke(_view, null);
        }

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

                String[] messageData = message.Split(new Char[] { ':' }, 2);
                if (messageData.Length > 0 && messageData[0] == "NOTIFY")
                    Instance?._view.AddMessage(messageData[1].ToUpper());
            }
        }
    }
}