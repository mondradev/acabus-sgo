using Opera.Acabus.Core.Modules.Configurations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace Opera.Acabus.Core.DataAccess
{
    /***
     * Esta clase proporciona la comunicación con el módulo de <see cref="Opera.Acabus.Core.Modules.Configurations"/>.
     */

    public static partial class AcabusData
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
        /// Carga la configuración del módulo <see cref="Opera.Acabus.Core.Modules.Configurations"/>.
        /// </summary>
        internal static void LoadConfigurables()
        {
            FillList(ref _configurablesInfo, SettingToConfigurable, "Configurables", "Tuple");

            foreach (Tuple<String, String, String> configurableInfo in _configurablesInfo)
            {
                Trace.WriteLine($"Cargando configurable {configurableInfo.Item1}...");

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