using Acabus.DataAccess;
using Acabus.Window;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace Acabus.Modules.Configurations.Views
{
    /// <summary>
    /// Lógica de interacción para ConfigurationView.xaml
    /// </summary>
    public partial class ConfigurationView : UserControl
    {
        static ConfigurationView()
        {
            AcabusData.LoadConfigModules();
            AcabusControlCenterViewModel.AddModule(new ConfigurationView(), new PackIcon() { Kind = PackIconKind.Settings }, "Configuración");
        }

        public ConfigurationView()
        {
            InitializeComponent();
        }
    }
}