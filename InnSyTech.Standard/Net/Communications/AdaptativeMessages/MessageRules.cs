using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InnSyTech.Standard.Net.Communications.AdaptativeMessages
{
    /// <summary>
    /// Representa la composición de los posibles campos de los mensajes adaptativos.
    /// </summary>
    public sealed class MessageRules : IEnumerable<FieldDefinition>, ICollection<FieldDefinition>
    {
        /// <summary>
        /// Lista de definición de campos.
        /// </summary>
        private List<FieldDefinition> _definitions;

        /// <summary>
        /// Crea una instancia nueva de definición de mensajes.
        /// </summary>
        public MessageRules()
        {
            _definitions = new List<FieldDefinition>();
        }

        /// <summary>
        /// Obtiene la cantidad de definiciones de campos.
        /// </summary>
        public int Count => _definitions?.Count ?? 0;

        /// <summary>
        /// Obtiene si la definición de mensajes es de solo lectura.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Carga desde un archivo JSON la definición del mensaje a transmitir.
        /// </summary>
        /// <param name="path"> Ruta del archivo JSON. </param>
        /// <returns> Una definición de mensaje. </returns>
        public static MessageRules Load(String path)
            => JsonConvert.DeserializeObject<MessageRules>(File.ReadAllText(path));

        /// <summary>
        /// Añade una definición de campo si esta no existe aún.
        /// </summary>
        /// <param name="item">  </param>
        public void Add(FieldDefinition item)
        {
            if (!_definitions.Any(x => x.ID == item.ID))
                _definitions.Add(item);
        }

        /// <summary>
        /// Elimina toda las definiciones de campos.
        /// </summary>
        public void Clear()
            => _definitions.Clear();

        /// <summary>
        /// Determina si la definición de campo existe en la colección.
        /// </summary>
        /// <param name="item"> Definición de campo a buscar. </param>
        /// <returns> Un valor true si la definición existe. </returns>
        public bool Contains(FieldDefinition item)
            => _definitions.Contains(item);

        /// <summary>
        /// Copia todas las definiciones de campo a un vector.
        /// </summary>
        /// <param name="array"> Vector destino de las definiciones. </param>
        /// <param name="arrayIndex"> Indice de partida a copiar los elementos. </param>
        public void CopyTo(FieldDefinition[] array, int arrayIndex)
            => _definitions.CopyTo(array, arrayIndex);

        /// <summary>
        /// Obtiene el enumerador de la colección.
        /// </summary>
        /// <returns>  </returns>
        public IEnumerator<FieldDefinition> GetEnumerator()
            => _definitions.GetEnumerator();

        /// <summary>
        /// Obtiene el enumerador de la colección.
        /// </summary>
        /// <returns>  </returns>
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// Elimina la definición de campo de esta colección.
        /// </summary>
        /// <param name="item"> Campo a eliminar. </param>
        /// <returns> Un valor true si se encontró y eliminó las definición. </returns>
        public bool Remove(FieldDefinition item)
                    => _definitions.Remove(item);
    }
}