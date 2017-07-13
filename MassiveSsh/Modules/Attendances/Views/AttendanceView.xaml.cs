using Acabus.DataAccess;
using Acabus.Window;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace Acabus.Modules.Attendances.Views
{
    /// <summary>
    /// Lógica de interacción para AttendanceView.xaml
    /// </summary>
    public partial class AttendanceView : UserControl
    {
        public static void LoadModule()
        {
            AcabusData.LoadAttendanceModule();
            AcabusControlCenterViewModel.AddModule(
                new AttendanceView(),
                new PackIcon() { Kind = PackIconKind.AccountCheck },
                "Asistencia de técnicos"
                );
        }

        public AttendanceView()
        {
            InitializeComponent();
        }
    }
}