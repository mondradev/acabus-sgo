using InnSyTech.Standard.Mvvm;
using System.Collections.Generic;

namespace Opera.Acabus.Configurations.ViewModels
{
    /// <summary>
    /// Esta clase define el modelo de la vista <see cref="Views.ConfigurationView"/>.
    /// </summary>
    public sealed class ConfigurationViewModel : ViewModelBase
    {
        /// <summary>
        /// Crea una instancia nueva de <see cref="ConfigurationViewModel"/>.
        /// </summary>
        public ConfigurationViewModel() { }

        /// <summary>
        /// Obtiene una lista de las vistas configurables.
        /// </summary>
        public ICollection<IConfigurable> Configurables => ConfigurationModule.Configurables;
    }
}