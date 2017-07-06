﻿using InnSyTech.Standard.Mvvm;
using MaterialDesignThemes.Wpf;
using Opera.Acabus.Core.DataAccess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// Carga todos los modulos leidos del archivo de configuración de la aplicación.
        /// </summary>
        private static void LoadModule()
        {
            foreach (var moduleName in AcabusData.ModulesNames)
            {
                Trace.WriteLine($"Cargando el módulo: '{moduleName.Item1}'...", "DEBUG");
                Assembly assembly = Assembly.LoadFrom(moduleName.Item3);
                var type = assembly.GetType(moduleName.Item2);
                var instanceTemp = Activator.CreateInstance(type);
                GC.SuppressFinalize(instanceTemp);
            }

            foreach (IModuleInfo moduleInfo in AcabusData.Modules)
            {
                UserControl moduleView = null;
                Instance?._view.AddToolButton(moduleInfo.Name, new Command(parameter =>
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
                if (messageData.Length > 0 && messageData[0] == "NOTIFY" && !Instance.messageSkiped.Contains(messageData[1]))
                    Instance._view.AddMessage(
                        messageData[1],
                        () => Instance.messageSkiped.Add(messageData[1]),
                        "OMITIR");
            }
        }
    }
}