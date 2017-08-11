using InnSyTech.Standard.Configuration;
using MaterialDesignThemes.Wpf;
using Opera.Acabus.Configurations.Views;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui.Modules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Opera.Acabus.Configurations
{
    /// <summary>
    /// Define la información del módulo de configuraciones de la aplicación.
    /// </summary>
    public class ConfigurationModule : ISgoModule
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="Configurables"/>.
        /// </summary>
        private static ICollection<IConfigurable> _configurables;

        /// <summary>
        /// Una lista que contiene la información para generar la colección de <see cref="AcabusData.Configurables"/>.
        /// </summary>
        private static ICollection<Tuple<String, String, String>> _configurablesInfo;

        /// <summary>
        /// Obtiene una lista de el valor de esta propiedad.
        /// </summary>
        public static ICollection<IConfigurable> Configurables {
            get {
                if (_configurables == null)
                    _configurables = new ObservableCollection<IConfigurable>();
                return _configurables;
            }
        }

        /// <summary>
        /// Obtiene el icono del módulo.
        /// </summary>
        public FrameworkElement Icon => new PackIcon() { Kind = PackIconKind.Settings };

        /// <summary>
        /// Indica si el módulo es secundario o de configuración.
        /// </summary>
        public bool IsSecundary => true;

        /// <summary>
        /// Indica si el módulo es de solo servicio.
        /// </summary>
        public bool IsService => false;

        /// <summary>
        /// Obtiene el nombre con el cual se presenta el módulo.
        /// </summary>
        public string Name => "Configuración";

        /// <summary>
        /// Obtiene el tipo de la vista principal del módulo.
        /// </summary>
        public Type View => typeof(ConfigurationView);

        /// <summary>
        /// Permite la carga inicial del módulo.
        /// </summary>
        /// <returns>Un valor <see cref="true"/> si este cargó correctamente.</returns>
        public bool LoadModule()
        {
            try
            {
                LoadConfigurables();
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Carga la configuración del módulo <see cref="Core.Modules.Configurations"/>.
        /// </summary>
        private static void LoadConfigurables()
        {
            var confs = ConfigurationManager.Settings.GetSettings("Configuration", "Configurables");

            foreach (ConfigurableInfo configurableInfo in confs)
            {
                Trace.WriteLine($"Cargando configurable: '{configurableInfo.Name}'...", "DEBUG");

                Assembly assembly = Assembly.LoadFrom(configurableInfo.AssemblyFilename);
                Type configurableClass = assembly.GetType(configurableInfo.TypeClass);
                Configurables.Add((IConfigurable)Activator.CreateInstance(configurableClass));
            }
        }
        
    }
}