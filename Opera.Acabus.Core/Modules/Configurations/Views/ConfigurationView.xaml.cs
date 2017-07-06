using MaterialDesignThemes.Wpf;
using Opera.Acabus.Core.DataAccess;
using System.Windows.Controls;

namespace Opera.Acabus.Core.Modules.Configurations.Views
{
    /// <summary>
    /// Define la lógica de la vista <see cref="ConfigurationView"/>.
    /// </summary>
    public partial class ConfigurationView : UserControl
    {
        /// <summary>
        /// Constructor estático de <see cref="ConfigurationView"/>.
        /// </summary>
        static ConfigurationView()
        {
            AcabusData.AddModule(
                typeof(ConfigurationView),
                new PackIcon() { Kind = PackIconKind.Settings },
                "Configuración",
                true);

            AcabusData.LoadConfigurables();
        }

        /// <summary>
        /// Crea una instancia nueva de <see cref="ConfigurationView"/>.
        /// </summary>
        public ConfigurationView()
        {
            InitializeComponent();
        }
    }
}