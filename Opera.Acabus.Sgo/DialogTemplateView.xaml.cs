using System;
using System.Windows;
using System.Windows.Controls;

namespace Opera.Acabus.Sgo
{
    /// <summary>
    /// Lógica de interacción para DialogTemplate.xaml
    /// </summary>
    public partial class DialogTemplateView : UserControl
    {
        // Using a DependencyProperty as the backing store for Message.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(String), typeof(DialogTemplateView), new PropertyMetadata(""));

        public DialogTemplateView()
        {
            InitializeComponent();
        }

        public String Message {
            get { return (String)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
    }
}