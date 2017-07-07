using MaterialDesignThemes.Wpf;
using Opera.Acabus.Core.Models;
using Opera.Acabus.TrunkMonitor.Models;
using Opera.Acabus.TrunkMonitor.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Opera.Acabus.TrunkMonitor
{
    /// <summary>
    /// Define la lógica del componente visual <see cref="TrunkMonitor"/>. Permite la visualización
    /// de las comunicaciones de las estaciones del SIT.
    /// </summary>
    public partial class TrunkMonitor : UserControl
    {
        // Using a DependencyProperty as the backing store for BackgroudLink.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroudLinkProperty =
            DependencyProperty.Register("BackgroundLink", typeof(Brush), typeof(TrunkMonitor), new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CFD8DC"))));

        // Using a DependencyProperty as the backing store for Links.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinksProperty =
            DependencyProperty.Register("Links", typeof(ObservableCollection<Link>), typeof(TrunkMonitor), new PropertyMetadata(new ObservableCollection<Link>()));

        /// <summary>
        /// Indica el margen del panel de enlace.
        /// </summary>
        private const Int16 MARGIN_LINK_PANEL = 16;

        /// <summary>
        /// Indica si debe dibujarse los enlaces.
        /// </summary>
        private bool _drawLinks;

        /// <summary>
        /// Indica si debe dibujarse las estaciones.
        /// </summary>
        private bool _drawStationsCards;

        /// <summary>
        /// Listado de todas las areas de enlace.
        /// </summary>
        private List<Grid> _linkAreas;

        /// <summary>
        /// Contiene una lista de todas las estaciones mostradas en el monitor.
        /// </summary>
        private List<Card> _stationCards;

        /// <summary>
        /// Crea una instancia del monitor de vía.
        /// </summary>
        public TrunkMonitor()
        {
            _linkAreas = new List<Grid>();
            _stationCards = new List<Card>();
            _drawStationsCards = true;

            Loaded += (sender, eventArgs) => DrawLinks();
            InitializeComponent();
        }

        /// <summary>
        /// Obtiene o establece el color del area de las estaciones que pertenecen a una misma cadena de enlaces.
        /// </summary>
        public Brush BackgroundLink {
            get { return (Brush)GetValue(BackgroudLinkProperty); }
            set { SetValue(BackgroudLinkProperty, value); }
        }

        /// <summary>
        /// Obtiene o establece la lista de enlaces que serán representados por el monitor de vía.
        /// </summary>
        [Description("Obtiene o establece la lista de enlaces que serán representados por el monitor de vía"), Category("Común")]
        public ObservableCollection<Link> Links {
            get { return (ObservableCollection<Link>)GetValue(LinksProperty); }
            set {
                SetValue(LinksProperty, value);
                if (value != null) value.CollectionChanged += UpdateItems;
            }
        }

        /// <summary>
        /// Método que ocurre cuando el control actual requiere ser dibujado nuevamente.
        /// </summary>
        /// <param name="drawingContext">Contexto gráfico.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_drawStationsCards)
            {
                CreateStationCards(Links, _content);
                _drawStationsCards = false;
            }

            if (_drawLinks)
                DrawLinks();
        }

        /// <summary>
        /// Crea una linea de enlace a partir de dos tarjetas y sus puntos centrales.
        /// </summary>
        /// <param name="card1">Tarjeta 1.</param>
        /// <param name="card1Origen">Punto origen de la tarjeta 1.</param>
        /// <param name="link">Enlace que las une.</param>
        /// <param name="card2">Tarjeta 2.</param>
        /// <param name="card2Origen">Punto origen de la tarjeta 2.</param>
        /// <returns>Una linea que representa un enlace entre estaciones.</returns>
        private static Line CreateLinkLine(Card card1, ref Point card1Origen, Link link, Card card2, Point card2Origen)
        {
            Double x1 = card1Origen.X + card1.ActualWidth * 0.95;
            Double y1 = card1Origen.Y + card1.ActualHeight / 2;
            Double x2 = card2Origen.X + card2.ActualWidth * 0.05;
            Double y2 = card2Origen.Y + card2.ActualHeight / 2;
            return CreateLinkLine(link, x1, y1, x2, y2);
        }

        /// <summary>
        /// Crea un enlace de linea indicando sus dos puntos gráficos y el enlace que manejará como contexto.
        /// </summary>
        /// <param name="link">Enlace que será representado por la linea.</param>
        /// <param name="x1">Coordenada X para el punto 1.</param>
        /// <param name="y1">Coordenada Y para el punto 1.</param>
        /// <param name="x2">Coordenada X para el punto 2.</param>
        /// <param name="y2">Coordenada Y para el punto 2.</param>
        /// <returns>Una linea que representa el enlace.</returns>
        private static Line CreateLinkLine(Link link, Double x1, Double y1, Double x2, Double y2)
        {
            Line linkLine = new Line()
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                StrokeThickness = 4,
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                DataContext = link
            };
            linkLine.SetBinding(Shape.StrokeProperty, new Binding("State")
            {
                Converter = new ConverterConnectionStateToBrush()
            });
            ToolTip pingTooltip = new ToolTip()
            {
                PlacementTarget = linkLine,
                Placement = PlacementMode.Mouse
            };
            pingTooltip.SetBinding(ContentProperty, new Binding("Ping")
            {
                Source = link,
                Converter = new FormatConverter("{0} {1}"),
                ConverterParameter = "ms"
            });
            linkLine.ToolTip = pingTooltip;
            return linkLine;
        }

        /// <summary>
        /// Crea una estación en forma de tarjeta (Card By Material Design).
        /// </summary>
        /// <param name="station">Estación que representa la tarjeta.</param>
        /// <returns>Una tarjeta que representa a la estación.</returns>
        private Card CreateStationCard(Station station)
        {
            Card card = new Card()
            {
                DataContext = station,
                Content = new Grid()
                {
                    Width = 150,
                    Height = 20
                },
                Foreground = (Brush)TryFindResource("AccentColorBrush"),
                Padding = new Thickness(16),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(6, 4, 0, 0)
            };

            card.SetBinding(Card.ToolTipProperty, new Binding("Name"));

            (card.Content as Grid).ColumnDefinitions.Add(new ColumnDefinition());
            (card.Content as Grid).ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(8) });
            (card.Content as Grid).ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(16) });
            (card.Content as Grid).RowDefinitions.Add(new RowDefinition());

            return card;
        }

        /// <summary>
        /// Crea todas las estaciones a partir de una lista de enlaces y el nodo padre donde se insertarán.
        /// </summary>
        /// <param name="links">Lista de enlaces.</param>
        /// <param name="parent">Nodo padre donde se insertarán las estaciones.</param>
        private void CreateStationCards(ObservableCollection<Link> links, Panel parent)
        {
            if (links == null || links.Count <= 0) return;

            StackPanel linkArea = null;
            if (links.Count > 1)
            {
                linkArea = new StackPanel();
                parent.Children.Add(linkArea);
            }

            foreach (Link link in links)
            {
                WrapPanel linkPanel = new WrapPanel()
                {
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                Card stationCard = CreateStationCard(link.StationB);

                TextBlock label = new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.WrapWithOverflow,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                label.SetBinding(TextBlock.TextProperty, new Binding("Name") { Source = stationCard.DataContext });
                (stationCard.Content as Grid).Children.Add(label);

                Ellipse indicator = new Ellipse() { Height = 16, Width = 16 };
                indicator.SetBinding(Shape.FillProperty, new Binding("State")
                {
                    Source = (stationCard.DataContext as Station).GetStateInfo(),
                    Converter = new ConverterConnectionStateToBrush()
                });
                Grid.SetColumn(indicator, 2);
                Grid.SetRow(indicator, 0);
                (stationCard.Content as Grid).Children.Add(indicator);

                linkPanel.Children.Add(stationCard);

                if (parent is StackPanel)
                {
                    Grid container = new Grid()
                    {
                        Margin = new Thickness(4)
                    };
                    container.SetBinding(Grid.BackgroundProperty, new Binding("BackgroundLink") { Source = this });
                    container.Children.Add(linkPanel);
                    linkPanel.Margin = new Thickness(MARGIN_LINK_PANEL, 0, 16, 8);
                    stationCard.Margin = new Thickness(4);
                    if (linkArea != null)
                        linkArea.Children.Add(container);
                    else
                        parent.Children.Add(container);
                    _linkAreas.Add(container);
                }
                else
                {
                    if (linkArea != null)
                        linkArea.Children.Add(linkPanel);
                    else
                        parent.Children.Add(linkPanel);
                }

                CreateStationCards(link.StationB.GetLinks(), linkPanel);
            }
        }

        /// <summary>
        /// Dibuja el primer enlace del area.
        /// </summary>
        /// <param name="card">Primera tarjeta estación del area de enlaces.</param>
        /// <param name="linkArea">Area de enlaces a la que pertenece la tarjeta de estación.</param>
        private void DrawFirstLink(Card card, Grid linkArea)
        {
            if (card == null) return;

            Point cardOrigen = card.TransformToAncestor(linkArea).Transform(new Point(0, 0));
            Point borderOrigen = new Point(cardOrigen.X - MARGIN_LINK_PANEL, cardOrigen.Y);
            Double x = cardOrigen.X + card.ActualWidth * 0.05;
            Double y = cardOrigen.Y + card.ActualHeight / 2;
            linkArea.Children.Insert(0, CreateLinkLine(FindLink(card.DataContext as Station), borderOrigen.X, borderOrigen.Y, x, y));
        }

        /// <summary>
        /// Dibuja todos los enlaces de manera recursiva.
        /// </summary>
        private void DrawLinks()
        {
            foreach (Grid linkArea in _linkAreas)
            {
                RemoveLinksLines(linkArea);
                Card[] cards = GetCardsFromArea(linkArea);

                DrawFirstLink(cards?[0], linkArea);

                for (int i = 0; i < cards.Length; i++)
                {
                    Card currentCard = cards[i];
                    Point currentCardOrigen = currentCard.TransformToAncestor(linkArea).Transform(new Point(0, 0));

                    ObservableCollection<Link> links = (currentCard.DataContext as Station).GetLinks();
                    if (links == null) continue;
                    foreach (Link link in links)
                    {
                        for (int j = i; j < cards.Length; j++)
                        {
                            Card nextCard = cards[j];
                            if (!nextCard.DataContext.Equals(link.StationB)) continue;
                            Point nextCardOrigen = nextCard.TransformToAncestor(linkArea).Transform(new Point(0, 0));
                            Line linkLine = CreateLinkLine(currentCard, ref currentCardOrigen, link, nextCard, nextCardOrigen);
                            linkArea.Children.Insert(0, linkLine);
                        }
                    }
                }
            }
            _drawLinks = false;
        }

        /// <summary>
        /// Busca el enlace inicial que su estación B sea el indicado como parametro.
        /// </summary>
        /// <param name="station">Estación B.</param>
        /// <returns>Un enlace que tiene como estación B el parametro indicado.</returns>
        private Link FindLink(Station station)
        {
            foreach (Link link in Links)
                if (link.StationB == station)
                    return link;
            return null;
        }

        /// <summary>
        /// Obtiene todas las tarjetas de estación dentro de un area de enlace.
        /// </summary>
        /// <param name="linkArea">Area de enlace a examinar.</param>
        /// <returns>Una matriz unidimencional con todas las tarjetasd de estación.</returns>
        private Card[] GetCardsFromArea(Panel linkArea)
        {
            List<Card> cards = new List<Card>();
            foreach (var item in linkArea.Children)
            {
                if (item is Card)
                    cards.Add(item as Card);
                if (item is Panel)
                    cards.AddRange(GetCardsFromArea(item as Panel));
            }
            return cards.ToArray();
        }

        /// <summary>
        /// Remueve todas las lineas de enlace que se encuentran dentro del area de enlace.
        /// </summary>
        /// <param name="linkArea">Area de enlace.</param>
        private void RemoveLinksLines(Grid linkArea)
        {
            List<Line> lines = new List<Line>();

            foreach (var item in linkArea.Children)
                if (item is Line)
                    lines.Add(item as Line);

            lines.ForEach((line) =>
            {
                linkArea.Children.Remove(line);
                GC.SuppressFinalize(line);
            });

            lines.Clear();
        }

        /// <summary>
        /// Método que es desencadenado cuando la lista de enlaces es afectada.
        /// </summary>
        /// <param name="sender">Lista de enlaces del componente.</param>
        /// <param name="e">Argumentos de la notificación de cambio.</param>
        private void UpdateItems(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move) return;

            _linkAreas.ForEach((grid) => GC.SuppressFinalize(grid));
            _linkAreas.Clear();

            _drawStationsCards = true;
            _drawLinks = true;
        }

        /// <summary>
        /// Provee un convertidor de un tipo de dato ConnectionState a Brush.
        /// Utilizado dentro de esta clase para poder colorear el indicador de la tarjeta de estación.
        /// </summary>
        private class ConverterConnectionStateToBrush : IValueConverter
        {
            /// <summary>
            /// Convierte el valor de una instancia ConnectionState a una instancia Brush.
            /// DISCONNECTED = GRAY,
            /// BAD = RED,
            /// MEDIUM = YELLOW,
            /// GOOD = GREEN,
            /// </summary>
            /// <param name="value">Instancia de ConnectionState.</param>
            /// <param name="targetType">Tipo de dato objetivo (Brush).</param>
            /// <param name="parameter">Parametro de la conversión.</param>
            /// <param name="culture">Referencia cultural para la conversión.</param>
            /// <returns>Una instancia de Brush.</returns>
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var state = (LinkState)value;
                switch (state)
                {
                    case LinkState.BAD:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"));

                    case LinkState.MEDIUM:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEB3B"));

                    case LinkState.GOOD:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));

                    case LinkState.DISCONNECTED:
                    default:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9E9E9E"));
                }
            }

            /// <summary>
            /// Función no implementada. No se requiere.
            /// </summary>
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Permite formatear el valor de una instancia para representarla como cadena.
        /// </summary>
        private class FormatConverter : IValueConverter
        {
            /// <summary>
            /// Patrón que será utilizado para el formato.
            /// </summary>
            private String pattern;

            /// <summary>
            /// Crea una nueva instancia de convertidor con formato.
            /// </summary>
            /// <param name="pattern">Patrón a utilizar para el formateo.</param>
            public FormatConverter(string pattern = "{0}")
            {
                this.pattern = pattern;
            }

            /// <summary>
            /// Convierte un valor a una cadena aplicando un formato especificado por la instancia actual del convertidor.
            /// </summary>
            /// <param name="value">Valor a convertir en cadena.</param>
            /// <param name="targetType">Tipo ojetivo de la cadena.</param>
            /// <param name="parameter">Parametro utilizado para la conversión.</param>
            /// <param name="culture">Referencia cultural para la conversión.</param>
            /// <returns>Una cadena con el formato aplicado que representa el valor indicado como paramtro.</returns>
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return String.Format(pattern, value.ToString(), parameter.ToString());
            }

            /// <summary>
            /// Función no implementada. No se requiere.
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}