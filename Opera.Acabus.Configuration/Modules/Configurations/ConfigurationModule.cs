using MaterialDesignThemes.Wpf;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Modules.Configurations.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Opera.Acabus.Core.Modules.Configurations
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
        /// Carga la configuración del módulo <see cref="Opera.Acabus.Core.Modules.Configurations"/>.
        /// </summary>
        private static void LoadConfigurables()
        {
            AcabusData.FillList(ref _configurablesInfo, SettingToConfigurable, "Configurables", "Tuple");

            foreach (Tuple<String, String, String> configurableInfo in _configurablesInfo)
            {
                Trace.WriteLine($"Cargando configurable: '{configurableInfo.Item1}'...", "DEBUG");

                Assembly assembly = Assembly.LoadFrom(configurableInfo.Item3);
                Type configurableClass = assembly.GetType(configurableInfo.Item2);
                Configurables.Add((IConfigurable)Activator.CreateInstance(configurableClass));
            }
        }

        /// <summary>
        /// Convierte una instancia <see cref="ISetting"/> en una <see cref="Tuple{String, String,
        /// String}"/> donde el primer valor es el nombre del configurable, el segundo es el nombre
        /// completo de la clase y el tercero es el nombre del ensamblado del configurable.
        /// </summary>
        /// <param name="arg">Instancia <see cref="ISetting"/> a convertir.</param>
        /// <returns>Una instancia de <see cref="Tuple{String, String, String}"/>.</returns>
        private static Tuple<String, String, String> SettingToConfigurable(ISetting arg)
            => new Tuple<string, string, string>(
                arg["Item1"].ToString(),
                arg["Item2"].ToString(),
                arg["Item3"].ToString()
        );
    }
}