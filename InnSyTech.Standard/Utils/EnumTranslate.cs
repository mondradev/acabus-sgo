using System;
using System.Collections.Generic;

namespace InnSyTech.Standard.Utils
{
    /// <summary>
    /// Esta clase permite la traducción de una enumeración a un texto que puede ser leido por una ser humano.
    /// </summary>
    /// <typeparam name="T">Tipo de la enumaración a traducir.</typeparam>
    public class EnumTranslator<T>
    {
        /// <summary>
        /// Crea una instancia de traductor de enumeración especificando el diccionario que utilizará.
        /// </summary>
        /// <param name="dictionary">Diccionario para la traducción.</param>
        public EnumTranslator(Dictionary<T, String> dictionary)
            => Dictionary = dictionary;

        /// <summary>
        /// Obtiene el diccionario de palabras para la traducción de la enumeración.
        /// </summary>
        public Dictionary<T, String> Dictionary { get; }

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

            throw new ArgumentException($"El texto no se encuentra dentro del diccionario --> '{text}'", "text");
        }
    }
}