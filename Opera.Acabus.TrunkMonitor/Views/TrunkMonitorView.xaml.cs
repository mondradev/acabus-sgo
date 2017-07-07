using MaterialDesignThemes.Wpf;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.TrunkMonitor.DataAccess;
using System.Windows.Controls;

namespace Opera.Acabus.TrunkMonitor.Views
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
        static TrunkMonitorView()
        {
            AcabusDataExtensions.LoadTrunkMonitor();
            AcabusData.AddModule(
                typeof(TrunkMonitorView),
                new PackIcon() { Kind = PackIconKind.SourceMerge },
                "Monitor de vía y equipos externos",
                false
                );
        }

    }
}
