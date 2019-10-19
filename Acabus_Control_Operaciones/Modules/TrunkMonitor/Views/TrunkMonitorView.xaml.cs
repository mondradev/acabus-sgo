using Acabus.Window;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace Acabus.Modules.TrunkMonitor
{
    /// <summary>
    /// Provee de la estructura de la vista del monitor de vía.
    /// </summary>
    public partial class TrunkMonitorView : UserControl
    {
        /// <summary>
        /// Crea una instancia nueva de la vista del monitor de vía.
        /// </summary>
        public TrunkMonitorView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor estático de la clase, permite la carga del modulo en la aplicación.
        /// </summary>
        public static void LoadModule()
        {
            AcabusControlCenterViewModel.AddModule(
                new TrunkMonitorView(),
                new PackIcon() { Kind = PackIconKind.SourceMerge },
                "Monitor de vía y equipos externos"
                );
        }

    }
}
