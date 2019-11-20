using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Serializers;
using InnSyTech.Standard.Utils;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages
{
    /// <summary>
    /// Representa la composición de los posibles campos de los mensajes adaptativos. Los campos 0
    /// al 9 no se pueden definir con la plantilla ya que estos se utilizan para la validación de la comunicación.
    /// </summary>
    public sealed class AdaptiveMessageRules : IEnumerable<FieldDefinition>, ICollection<FieldDefinition>
    {
        /// <summary>
        /// Campos básicos necesarios para el manejo de mensajes.
        /// </summary>
        private static readonly FieldDefinition[] _header;

        /// <summary>
        /// Lista de serializadores.
        /// </summary>
        private static readonly SortedList<FieldType, IAdaptiveSerializer> _serializers;

        /// <summary>
        /// Lista de definición de campos.
        /// </summary>
        private readonly SortedList<int, FieldDefinition> _definitions;

        /// <summary>
        /// Constructor estático.
        /// </summary>
        static AdaptiveMessageRules()
        {
            _serializers = new SortedList<FieldType, IAdaptiveSerializer>
            {
                { FieldType.Numeric, new NumericSerializer() },
                { FieldType.Text, new TextSerializer() },
                { FieldType.Binary, new BinarySerializer() }
            };

            _header = new FieldDefinition[] {
                new FieldDefinition((int)AdaptiveMessageFieldID.APIToken, FieldType.Binary, 32, false, _serializers[FieldType.Binary]), // Token de aplicación gestionados por el servidor
                new FieldDefinition((int)AdaptiveMessageFieldID.ResponseCode, FieldType.Numeric, 2, false,  _serializers[FieldType.Numeric]), // Código de respuesta de la petición
                new FieldDefinition((int)AdaptiveMessageFieldID.ResponseMessage, FieldType.Text, 255, true,  _serializers[FieldType.Text]), // Mensaje de respuesta de la petición
                new FieldDefinition((int)AdaptiveMessageFieldID.ModuleName, FieldType.Text, 50, true,  _serializers[FieldType.Text]), // Nombre del módulo
                new FieldDefinition((int)AdaptiveMessageFieldID.FunctionName, FieldType.Text, 50, true,  _serializers[FieldType.Text]), // Nombre de la función
                new FieldDefinition((int)AdaptiveMessageFieldID.Count, FieldType.Numeric, 8, false,  _serializers[FieldType.Numeric]), // Número total de registros
                new FieldDefinition((int)AdaptiveMessageFieldID.Position, FieldType.Numeric, 8, false,  _serializers[FieldType.Numeric]), // Posición actual del enumerable
            };
        }

        /// <summary>
        /// Crea una instancia nueva de definición de mensajes.
        /// </summary>
        public AdaptiveMessageRules()
        {
            _definitions = new SortedList<int, FieldDefinition>();
            _header.ForEach(f => _definitions.Add(f.ID, f));
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
        /// Obtiene la definición del campo especificado.
        /// </summary>
        /// <param name="id">ID del campo.</param>
        /// <returns>La definición del campo.</returns>
        public FieldDefinition this[int id] => _definitions[id];

        /// <summary>
        /// Carga desde un archivo JSON la definición del mensaje a transmitir.
        /// </summary>
        /// <param name="path">Ruta del archivo JSON.</param>
        /// <returns>Una definición de mensaje.</returns>
        public static AdaptiveMessageRules Load(string path)
        {
            AdaptiveMessageRules rules = JsonConvert.DeserializeObject<AdaptiveMessageRules>(File.ReadAllText(path));

            rules.RemoveForced(_header.Select(x => x.ID).ToArray());

            _header.ForEach(f => rules._definitions.Add(f.ID, f));

            return rules;
        }

        /// <summary>
        /// Registra un serializador nuevo para un tipo distinto a los que presenta la enumeración
        /// <see cref="FieldType"/>.
        /// </summary>
        /// <param name="type">Tipo del campo.</param>
        /// <param name="serializer">Serializador para el campo</param>
        /// <seealso cref="FieldType"/>
        /// <seealso cref="IAdaptiveSerializer"/>
        /// <seealso cref="IAdaptiveMessage"/>
        public static void RegisterSerializer(int type, IAdaptiveSerializer serializer)
        {
            if (_serializers[(FieldType)type].IsNull())
                _serializers.Add((FieldType)type, serializer);
        }

        /// <summary>
        /// Añade una definición de campo si esta no existe aún.
        /// </summary>
        /// <param name="definition">Definición del campo.</param>
        public void Add(FieldDefinition definition)
        {
            if (!IsReadOnly && !Contains(definition))
                _definitions.Add(definition.ID, new FieldDefinition(definition.ID, definition.Type, definition.MaxLength,
                    definition.IsVarLength, _serializers[definition.Type]));
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
        /// <param name="item">Definición de campo a buscar.</param>
        /// <returns>Un valor true si la definición existe.</returns>
        public bool Contains(FieldDefinition item)
            => _definitions.ContainsKey(item.ID);

        /// <summary>
        /// Copia todas las definiciones de campo a un vector.
        /// </summary>
        /// <param name="array">Vector destino de las definiciones.</param>
        /// <param name="arrayIndex">
        /// Indice de partida a copiar los elementos, no confundir con el ID de campo.
        /// </param>
        public void CopyTo(FieldDefinition[] array, int arrayIndex)
            => _definitions.ToArray().CopyTo(array, arrayIndex);

        /// <summary>
        /// Obtiene el enumerador de la colección.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<FieldDefinition> GetEnumerator()
            => _definitions.Values.GetEnumerator();

        /// <summary>
        /// Obtiene el enumerador de la colección.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// Elimina la definición de campo de esta colección. Si intenta remover las definiciones de
        /// los campos 0 al 9, la función no podrá continuar devolviendo false.
        /// </summary>
        /// <param name="item">Campo a eliminar.</param>
        /// <returns>Un valor true si se encontró y eliminó las definición.</returns>
        public bool Remove(FieldDefinition item)
        {
            if (IsReadOnly || _header.Select(x => x.ID).Contains(item.ID))
                return false;

            return _definitions.Remove(item.ID);
        }

        /// <summary>
        /// Elimina todos las definiciones que corresponden a los ID especificados. Si intenta
        /// remover las definiciones de los campos 0 al 9, la función no podrá continuar devolviendo false.
        /// </summary>
        /// <param name="ids">Conjunto de ID de campo.</param>
        /// <returns>Un valor true si se eliminaron todas las definiciones especificadas.</returns>
        public bool Remove(params int[] ids)
        {
            if (IsReadOnly || ids.Any(id => _header.Select(f => f.ID).Contains(id)))
                return false;

            return RemoveForced(ids);
        }

        /// <summary>
        /// Elimina todos las definiciones especificados por el identificador del campo.
        /// </summary>
        /// <param name="ids">Conjunto de ID de campo.</param>
        /// <returns>Un valor true si se eliminaron todas las definiciones.</returns>
        private bool RemoveForced(params int[] ids)
        {
            bool removedAll = true;

            ids.ForEach(id =>
            {
                if (_definitions.ContainsKey(id))
                    removedAll &= _definitions.Remove(id);
            });

            return removedAll;
        }
    }
}