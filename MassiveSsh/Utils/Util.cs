using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Acabus.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static String ToString<T>(IEnumerable<T> collection, String format = "{0}", Func<T, String> translator = null)
        {
            int index = 0;
            if (translator == null)
                translator = (item) => item.ToString();

            StringBuilder builder = new StringBuilder();

            foreach (var item in collection)
            {
                builder.Append(String.Format(format, translator?.Invoke(item)));
                index++;
                if (index < collection.Count())
                    builder.Append(",");
            }
            return builder.ToString();
        }

        public static ICollection<T> SelectFromList<T>(this ICollection<T> collection, Predicate<T> predicate)
        {
            ICollection<T> listTemp = (IList<T>)Activator.CreateInstance(collection.GetType());
            foreach (var item in collection)
                if (predicate.Invoke(item))
                    listTemp.Add(item);
            return listTemp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSvg"></param>
        /// <returns></returns>
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
                Fill = Brushes.Black
            });
            return new Viewbox()
            {
                Child = canvas
            };
        }
    }
}
