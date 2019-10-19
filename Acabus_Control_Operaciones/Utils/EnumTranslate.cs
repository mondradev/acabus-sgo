using System;
using System.Collections.Generic;

namespace Acabus.Utils
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumTranslator<T>
    {
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<T, String> Dictionary { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dictionary"></param>
        public EnumTranslator(Dictionary<T, String> dictionary)
        {
            Dictionary = dictionary;
        }

        /// <summary>
        /// Obtiene una cadena que representa la enumeración traducida.
        /// </summary>
        public String Translate(T enumetation)
        {
            Dictionary.TryGetValue(enumetation, out String value);
            return value;
        }

        /// <summary>
        /// Obtiene la enumeración que corresponde a la cadena especificada.
        /// </summary>
        public T TranslateBack(String text)
        {
            foreach (var item in Dictionary.Keys)
                if (Dictionary[item] == text)
                    return item;

            throw new ArgumentException($"El texto no corresponde a la enumeración {typeof(T).GetType().FullName}", "text");
        }
    }
}