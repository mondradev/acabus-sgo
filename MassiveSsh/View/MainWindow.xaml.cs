using MahApps.Metro.Controls;
using MassiveSsh.ViewModel;
using System;
using System.Windows.Controls;

namespace MassiveSsh.View
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (this.DataContext as MVMassiveSsh).KillAll();
        }

        private void ScrollToEndOnTextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }

        private void WindowOnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.SshCommand.Focus();
        }

        private void GetFocusOnIsEnabledChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue is Boolean && ((Boolean)e.NewValue))
                this.SshCommand.Focus();
        }
    }
}
