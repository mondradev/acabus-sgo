using Opera.Acabus.Server.Core.Gui;
using Opera.Acabus.Server.Core.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;

namespace Opera.Acabus.Server.Config.Components
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ServiceList : UserControl
    {

        public static readonly DependencyProperty ServicesProperty =
            DependencyProperty.Register("Services",
                typeof(ObservableCollection<IServiceModule>),
                typeof(ServiceList),
                new PropertyMetadata(new ObservableCollection<IServiceModule>(), OnServiceListChanged));

        public ServiceList()
        {
            InitializeComponent();
        }

        public ObservableCollection<IServiceModule> Services {
            get { return (ObservableCollection<IServiceModule>)GetValue(ServicesProperty); }
            set { SetValue(ServicesProperty, value); }
        }

        private static void OnServiceListChanged(DependencyObject dependency, DependencyPropertyChangedEventArgs args)
        {
            var instance = dependency as ServiceList;

            if (args.NewValue != null)
                (args.NewValue as ObservableCollection<IServiceModule>).CollectionChanged += instance.OnUpdateModules;

            instance.OnUpdateModules(instance, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnUpdateModules(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move) return;

            _mainLayout.Children.Clear();

            foreach (IServiceModule service in Services)
            {
                Grid container = new Grid()
                {
                    DataContext = service,
                    Height = 32,
                    Margin = new Thickness(0, 0, 0, 16)
                };
                Label caption = new Label()
                {
                    Height = 26
                };
                Ellipse statusIndicator = new Ellipse()
                {
                    Margin = new Thickness(0, 0, 4, 4),
                    Width = 24,
                    Height = 24,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                container.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                container.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(8, GridUnitType.Pixel) });
                container.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                Grid.SetColumn(statusIndicator, 2);

                container.Children.Add(caption);
                container.Children.Add(statusIndicator);

                caption.SetBinding(ContentProperty, new Binding(nameof(service.ServiceName)));
                statusIndicator.SetBinding(Shape.FillProperty, new Binding(nameof(service.Status)) { Converter = new StatusToColorConverter() });

                _mainLayout.Children.Add(container);
            }
        }
    }
}