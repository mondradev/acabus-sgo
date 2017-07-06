using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Opera.Acabus.Core.Modules.Configurations
{
    /// <summary>
    /// Lógica de interacción para Configuration.xaml
    /// </summary>
    public partial class Configuration : UserControl
    {
        public static readonly DependencyProperty ConfigurablesProperty =
            DependencyProperty.Register("Configurables", typeof(ObservableCollection<IConfigurable>), typeof(Configuration), new PropertyMetadata(new ObservableCollection<IConfigurable>()));

        private bool _drawn;

        private List<TextBlock> _previewTexts;

        public Configuration()
        {
            _previewTexts = new List<TextBlock>();

            Loaded += UpdatePreview;

            InitializeComponent();
        }

        [Description("Obtiene o establece la lista de las vistas configurables."), Category("Común")]
        public ObservableCollection<IConfigurable> Configurables {
            get { return (ObservableCollection<IConfigurable>)GetValue(ConfigurablesProperty); }
            set {
                SetValue(ConfigurablesProperty, value);
                if (value != null)
                    value.CollectionChanged += UpdateCards;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (!_drawn)
                DrawCards();
        }

        private Card CreateCard(IConfigurable configurable)
        {
            Card confCard = new Card()
            {
                Margin = new Thickness(4, 4, 0, 0),
                Width = 350,
                Height = 200
            };
            Grid container = new Grid();
            container.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
            container.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            container.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
            confCard.Content = container;

            TextBlock title = new TextBlock()
            {
                TextWrapping = TextWrapping.WrapWithOverflow,
                Padding = new Thickness(16),
                Background = TryFindResource("AccentColorBrush") as Brush,
                Foreground = TryFindResource("IdealForegroundColorBrush") as Brush,
                Text = configurable.Title
            };

            StackPanel dataContainer = new StackPanel()
            {
                Margin = new Thickness(16, 16, 16, 0)
            };
            Grid.SetRow(dataContainer, 1);

            foreach (var item in configurable.PreviewData)
            {
                TextBlock previewData = new TextBlock()
                {
                    TextWrapping = TextWrapping.WrapWithOverflow,
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(0, 2, 0, 2),
                    DataContext = item
                };
                dataContainer.Children.Add(previewData);
                _previewTexts.Add(previewData);
            }

            StackPanel buttonsContainer = new StackPanel()
            {
                Margin = new Thickness(8, 8, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Right,
                Orientation = Orientation.Horizontal
            };

            Grid.SetRow(buttonsContainer, 2);

            foreach (var item in configurable.Commands)
            {
                Button command = new Button()
                {
                    Style = TryFindResource("MaterialDesignFlatButton") as Style,
                    Padding = new Thickness(8, 0, 8, 0),
                    Margin = new Thickness(0, 0, 8, 0),
                    Command = item.Item2,
                    Content = item.Item1,
                    MinHeight = 36
                };
                buttonsContainer.Children.Add(command);
            }

            container.Children.Add(title);
            container.Children.Add(dataContainer);
            container.Children.Add(buttonsContainer);

            return confCard;
        }

        private void DrawCards()
        {
            _container?.Children.Clear();
            _previewTexts.Clear();

            foreach (var configurable in Configurables)
                _container?.Children.Add(CreateCard(configurable));

            _drawn = true;
        }

        private void UpdateCards(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move) return;

            _drawn = false;
        }

        private void UpdatePreview(object sender, RoutedEventArgs e)
        {
            foreach (var item in _previewTexts)
            {
                var data = item.DataContext as Tuple<String, Func<Object>>;
                item.Text = String.Format("{0}: {1}", data.Item1, "Cagando...");
                Task.Run(() =>
                {
                    var result = data.Item2.Invoke().ToString();
                    Application.Current.Dispatcher.Invoke(() =>
                        item.Text = String.Format("{0}: {1}", data.Item1, result));
                });
            }
        }
    }
}