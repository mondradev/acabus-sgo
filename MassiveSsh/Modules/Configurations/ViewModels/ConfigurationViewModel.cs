using Acabus.DataAccess;
using Acabus.Utils.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Acabus.Modules.Configurations.ViewModels
{
    public sealed class ConfigurationViewModel : ViewModelBase
    {


        /// <summary>
        /// Obtiene una lista de las vistas configurables.
        /// </summary>
        public ICollection<IConfigurable> Configurables => AcabusData.Configurables;

        public ConfigurationViewModel()
        {
        }
    }
}
