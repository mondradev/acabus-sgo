using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages
{
    /// <summary>
    /// Representa la composición de los posibles campos de los mensajes adaptativos. Los campos 0 al
    /// 9 no se pueden definir con la plantilla ya que estos se utilizan para la validación de la comunicación.
    /// </summary>
    public sealed class AdaptiveMessageRules : IEnumerable<FieldDefinition>, ICollection<FieldDefinition>
    {
        /// <summary>
        /// Campos básicos necesarios para el manejo de mensajes.
        /// </summary>
        private static readonly FieldDefinition[] _header = new FieldDefinition[] {
            new FieldDefinition(1, FieldType.Binary, 32, false, "Token de aplicación"), // Token de aplicación gestionados por el servidor
            new FieldDefinition(2, FieldType.Binary, 32, false, "Hash de reglas"), // Versión de la regla
            new FieldDefinition(3, FieldType.Numeric, 3, false, "Código de respuesta"), // Código de respuesta de la petición
            new FieldDefinition(4, FieldType.Text, 255, true, "Mensaje de respuesta"), // Mensaje de respuesta de la petición
            new FieldDefinition(5, FieldType.Text, 100, true, "Nombre del módulo"), // Nombre del módulo
            new FieldDefinition(6, FieldType.Text, 50, true, "Nombre de la función"), // Nombre de la función
            new FieldDefinition(7, FieldType.Binary, 1, false, "Es enumerable"), // La respuesta es enumerable
            new FieldDefinition(8, FieldType.Numeric, 100, true, "Registros totales del enumerable"), // Número total de registros
            new FieldDefinition(9, FieldType.Numeric, 100, true, "Posición del enumerable"), // Posición actual del enumerable
            new FieldDefinition(10, FieldType.Numeric, 1, false, "Operaciones del enumerable (Siguiente|Inicio)") // Operación del enumerador
        };

        /// <summary>
        /// Lista de definición de campos.
        /// </summary>
        private readonly List<FieldDefinition> _definitions;

        /// <summary>
        /// Obtiene la definición del campo especificado.
        /// </summary>
        /// <param name="id">ID del campo.</param>
        /// <returns>La definición del campo.</returns>
        public FieldDefinition this[int id] => _definitions.FirstOrDefault(f => f.ID == id);

        /// <summary>
        /// Crea una instancia nueva de definición de mensajes.
        /// </summary>
        public AdaptiveMessageRules()
        {
            _definitions = new List<FieldDefinition>();
            _definitions.AddRange(_header);
        }

        /// <summary>
        /// Obtiene la cantidad de definiciones de campos.
        /// </summary>
        public int Count => _definitions?.Count ?? 0;

        /// <summary>
        /// Obtiene si la definición de mensajes es de solo lectura.
        /// </summary>
        public bool IsReadOnly { get; internal set; }

        /// <summary>
        /// Carga desde un archivo JSON la definición del mensaje a transmitir.
        /// </summary>
        /// <param name="path"> Ruta del archivo JSON. </param>
        /// <returns> Una definición de mensaje. </returns>
        public static AdaptiveMessageRules Load(String path)
        {
            AdaptiveMessageRules rules = JsonConvert.DeserializeObject<AdaptiveMessageRules>(File.ReadAllText(path));
            
            rules.RemoveForced(_header.Select(x => x.ID).ToArray());
            rules._definitions.AddRange(_header);

            return rules;
        }

        /// <summary>
        /// Añade una definición de campo si esta no existe aún.
        /// </summary>
        /// <param name="item">  </param>
        public void Add(FieldDefinition item)
        {
            if (IsReadOnly)
                return;

            if (!_definitions.Any(x => x.ID == item.ID))
                _definitions.Add(item);
        }

        /// <summary>
        /// Elimina toda las definiciones de campos.
        /// </summary>
        public void Clear()
        {
            if (!IsReadOnly)
                _definitions.Clear();
        }

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
        /// <param name="arrayIndex"> Indice de partida a copiar los elementos, no confundir con el ID de campo. </param>
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
        /// Elimina la definición de campo de esta colección. Si intenta
        /// remover las definiciones de los campos 0 al 9, la función no podrá continuar devolviendo false.
        /// </summary>
        /// <param name="item"> Campo a eliminar. </param>
        /// <returns> Un valor true si se encontró y eliminó las definición. </returns>
        public bool Remove(FieldDefinition item)
        {
            if (IsReadOnly)
                return false;

            if (_header.Select(x => x.ID).Contains(item.ID))
                return false;

            return _definitions.Remove(item);
        }

        /// <summary>
        /// Elimina todos las definiciones que corresponden a los ID especificados. Si intenta
        /// remover las definiciones de los campos 0 al 9, la función no podrá continuar devolviendo false.
        /// </summary>
        /// <param name="id">Conjunto de ID de campo.</param>
        /// <returns>Un valor true si se eliminaron todas las definiciones especificadas.</returns>
        public bool Remove(params int[] id)
        {
            if (IsReadOnly)
                return false;

            if (id.Any(x => _header.Select(y => y.ID).Contains(x)))
                return false;

            return RemoveForced(id);
        }

        /// <summary>
        /// Elimina todos las definiciones especificados por el identificador del campo.
        /// </summary>
        /// <param name="id">Conjunto de ID de campo.</param>
        /// <returns>Un valor true si se eliminaron todas las definiciones.</returns>
        private bool RemoveForced(params int[] id)
        {
            while (_definitions.Where(x => id.Contains(x.ID)).Any())
                _definitions.Remove(_definitions.First(x => id.Contains(x.ID)));

            return true;
        }
    }
}