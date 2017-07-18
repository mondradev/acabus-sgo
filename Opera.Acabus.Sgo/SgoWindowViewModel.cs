using InnSyTech.Standard.Mvvm;
using MaterialDesignThemes.Wpf;
using Opera.Acabus.Core.DataAccess;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        /// Campo que provee a la propiedad <see cref="Modules" />.
        /// </summary>
        private static ICollection<IModuleInfo> _module;

        /// <summary>
        /// Instancia de la vista de la ventana principal.
        /// </summary>
        private SgoWindowView _view;

        /// <summary>
        /// Contiene los mensajes de error omitidos en la aplicación.
        /// </summary>
        private List<String> messageSkiped = new List<string>();

        /// <summary>
        /// Agrega un escucha al Trace para poder capturar los mensajes y mostrar los de error en la Snackbar
        /// de la ventana.
        /// </summary>
        static SgoWindowModelView()
        {
            Trace.Listeners.Add(new TraceListenerImp());
        }

        /// <summary>
        /// Crea una instancia del modelo de la vista de la ventana principal.
        /// </summary>
        /// <param name="view">Ventana principal.</param>
        public SgoWindowModelView(SgoWindowView view)
        {
            _view = view;
            _instance = this;

            AcabusData.RequestingShowContent += arg =>
             {
                 Instance?._view.ShowContent(arg.Content);
             };

            AcabusData.RequestingShowDialog += async arg =>
            {
                DialogHost.CloseDialogCommand.Execute(null, null);

                Object response = await DialogHost.Show(arg.Content);
                GC.SuppressFinalize(arg.Content);

                arg.Callback?.Invoke(response);
            };

            AcabusData.RequestingSendMessageOrNotify += async arg =>
            {
                switch (arg.SendType)
                {
                    case RequestSendMessageArg.RequestSendType.MESSAGE:
                        DialogHost.CloseDialogCommand.Execute(null, null);
                        System.Threading.Thread.Sleep(1000);
                        await DialogHost.Show(new DialogTemplateView() { Message = arg.Message });
                        break;

                    case RequestSendMessageArg.RequestSendType.NOTIFY:
                        Instance?._view.AddMessage(arg.Message);
                        break;
                }
            };

            LoadModule();
        }

        /// <summary>
        /// Obtiene el valor de esta propiedad.
        /// </summary>
        public static SgoWindowModelView Instance => _instance;

        /// <summary>
        /// Obtiene una lista de los modulos actualmente añadidos al SGO.
        /// </summary>
        public static ICollection<IModuleInfo> Modules
            => _module ?? (_module = new ObservableCollection<IModuleInfo>());

        /// <summary>
        /// Agrega una módulo al SGO.
        /// </summary>
        /// <param name="module">Vista principal del módulo a agregar.</param>
        /// <param name="icon">Icono que usará para mostrarse en el menú principal.</param>
        /// <param name="moduleName">Nombre del módulo.</param>
        /// <param name="secundaryModule">
        /// Si es <see cref="true"/>, el botón aparecerá del lado derecho de la barra de menú.
        /// </param>
        public static void AddModule(Type moduleClass, System.Windows.FrameworkElement icon,
            String moduleName, Boolean secundaryModule)
        {
            ModuleInfo moduleInfo = new ModuleInfo()
            {
                Icon = icon,
                IsLoaded = false,
                IsSecundary = secundaryModule,
                Name = moduleName,
                Type = moduleClass
            };

            if (Modules.Where(module => module.Name == moduleInfo.Name).Count() < 1)
                Modules.Add(moduleInfo);
        }

        /// <summary>
        /// Carga todos los modulos leidos del archivo de configuración de la aplicación.
        /// </summary>
        private static void LoadModule()
        {
            foreach (var moduleName in AcabusData.ModulesNames)
            {
                try
                {
                    Trace.WriteLine($"Cargando el módulo: '{moduleName.Item1}'...", "DEBUG");
                    Assembly assembly = Assembly.LoadFrom(moduleName.Item3);
                    var type = assembly.GetType(moduleName.Item2);
                    var moduleInfo = Activator.CreateInstance(type);

                    var viewType = type.GetProperty("View");
                    var icon = type.GetProperty("Icon");
                    var name = type.GetProperty("Name");
                    var isSecundary = type.GetProperty("IsSecundary");
                    var isService = type.GetProperty("IsService");

                    if (!(bool)isService.GetValue(moduleInfo))
                    {
                        AddModule(viewType.GetValue(moduleInfo) as Type,
                            icon.GetValue(moduleInfo) as System.Windows.FrameworkElement,
                            name.GetValue(moduleInfo).ToString(),
                            (bool)isSecundary.GetValue(moduleInfo));
                    }

                    var moduleLoaded = type.GetMethod("LoadModule")?.Invoke(moduleInfo, null);
                    if (moduleLoaded is Boolean && (Boolean)moduleLoaded)
                        AcabusData.SendNotify($"Módulo '{moduleName.Item1}' cargado");
                    else
                        AcabusData.SendNotify($"No se logró cargar el módulo '{moduleName.Item1}'");
                }
                catch (FileNotFoundException)
                {
                    AcabusData.SendNotify($"No se encontró el módulo '{moduleName.Item1}'");
                }
            }

            foreach (IModuleInfo moduleInfo in Modules)
            {
                UserControl moduleView = null;
                Instance?._view.AddToolButton(moduleInfo.Name.Replace(' ', '_'), new Command(parameter =>
                 {
                     if (!moduleInfo.IsLoaded)
                     {
                         moduleView = (UserControl)Activator.CreateInstance(moduleInfo.Type);
                         moduleInfo.IsLoaded = true;
                     }

                     Instance?._view.ShowContent(moduleView);
                 }), moduleInfo.Icon, moduleInfo.Name, moduleInfo.IsSecundary);
            }
        }

        /// <summary>
        /// Define la estructura de la información de un módulo del SGO.
        /// </summary>
        private sealed class ModuleInfo : IModuleInfo
        {
            /// <summary>
            /// Obtiene o establece icono del módulo.
            /// </summary>
            public System.Windows.FrameworkElement Icon { get; set; }

            /// <summary>
            /// Obtiene o establece si el módulo está cargado.
            /// </summary>
            public Boolean IsLoaded { get; set; }

            /// <summary>
            /// Obtiene o establece si el módulo es secundario.
            /// </summary>
            public Boolean IsSecundary { get; set; }

            /// <summary>
            /// Obtiene o establece el nombre de módulo.
            /// </summary>
            public String Name { get; set; }

            /// <summary>
            /// Obtiene o establece clase de la vista principal del módulo.
            /// </summary>
            public Type Type { get; set; }
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
                if (messageData.Length > 0 && messageData[0] == "NOTIFY" && !Instance.messageSkiped.Contains(messageData[1].ToUpper()))
                    Instance._view.AddMessage(
                        messageData[1].ToUpper(),
                        () => Instance.messageSkiped.Add(messageData[1].ToUpper()),
                        "OMITIR");
            }
        }
    }
}