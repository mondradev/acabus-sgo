using System.Windows;

namespace Acabus
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            DataAccess.AcabusData.LoadConfiguration();
        }
    }
}
