using Acabus.Modules.TrunkMonitor;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Acabus.View
{
    /// <summary>
    /// Lógica de interacción para Window1.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static MainWindow Instance { get; private set; }

        private Action _navigeted = null;


        internal static void LoadPage(Object page, Action callback)
        {
            Instance._navigeted = callback;
        }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            Closing += (sender, args) =>
            {
                //Page page = TryFindResource("TrunkMonitor") as TrunkMonitor;
                //(page.DataContext as ViewModel.VMTrunkMonitor).StopCommand.Execute(null);
            };
        }

        private void NavigetedHandler(Object sender, EventArgs eventArgs) =>
            _navigeted?.Invoke();

        
    }
}
