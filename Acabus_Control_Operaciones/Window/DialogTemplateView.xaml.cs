using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Acabus.Window
{
    /// <summary>
    /// Lógica de interacción para DialogTemplate.xaml
    /// </summary>
    public partial class DialogTemplateView : UserControl
    {
        public String Message {
            get { return (String)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Message.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(String), typeof(DialogTemplateView), new PropertyMetadata(""));
        
        public DialogTemplateView()
        {
            InitializeComponent();
        }
    }
}
