using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
