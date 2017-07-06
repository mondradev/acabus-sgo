using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.DataAccess;
using System.Collections.Generic;

namespace Opera.Acabus.Core.Modules.Configurations.ViewModels
{
    /// <summary>
    /// Esta clase define el modelo de la vista <see cref="Opera.Acabus.Core.Modules.Configurations.Views.ConfigurationView"/>.
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
        public ICollection<IConfigurable> Configurables => AcabusData.Configurables;
    }
}