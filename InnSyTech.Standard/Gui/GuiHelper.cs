using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace InnSyTech.Standard.Gui
{
    /// <summary>
    /// Provee de funciones auxiliares para el manejo de interfaces gráficas.
    /// </summary>
    public static class GuiHelper
    {
        /// <summary>
        /// Genera un componente que permite mostrar un icono con las dimesiones de 24x24
        /// especificando la cadena SVG que lo define.
        /// </summary>
        /// <param name="dataSvg">Cadena SVG del icono.</param>
        /// <returns>Un componente que muestra el icono.</returns>
        public static Viewbox CreateIcon(String dataSvg)
        {
            Canvas canvas = new Canvas()
            {
                Width = 24,
                Height = 24
            };
            canvas.Children.Add(new Path()
            {
                Data = Geometry.Parse(dataSvg),
            });
            (canvas.Children[0] as Path).SetBinding(Shape.FillProperty, new Binding("Foreground")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Control), 1)
            });
            return new Viewbox()
            {
                Child = canvas
            };
        }
    }
}