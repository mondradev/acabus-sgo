using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Serializers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages
{
    /// <summary>
    /// Representa un mensaje adaptativo en tamaño de la trama, unicamente como obligatorio se
    /// requiere la cabecera de 8 bytes en la cual se representa el campo activo. Este tipo de
    /// mensajes se basa en el ISO8583 pero con la diferencia que las caracteristicas de los campos
    /// son variables y son definidos por la clase <see cref="AdaptiveMessageRules"/> la cual puede ser
    /// cargada desde un archivo JSON a través de la función <see cref="AdaptiveMessageRules.Load(string)"/>.
    /// </summary>
    internal sealed class AdaptiveMessage : IAdaptiveMessage
    {
        /// <summary>
        /// Lista de campos.
        /// </summary>
        private readonly List<Field> _fields;

        /// <summary>
        /// Reglas de composición del mensaje.
        /// </summary>
        private readonly AdaptiveMessageRules _messageRules;

        /// <summary>
        /// Crea una instancia nueva de un mensaje.
        /// </summary>
        /// <param name="rules">Reglas de composición del mensaje.</param>
        public AdaptiveMessage(AdaptiveMessageRules rules)
        {
            _fields = new List<Field>();
            _messageRules = rules ?? new AdaptiveMessageRules();

            _messageRules.IsReadOnly = true;
        }

        /// <summary>
        /// Obtiene el total de campos activos en el mensaje.
        /// </summary>
        public int Count => _fields?.Count ?? 0;

        /// <summary>
        /// Obtiene si el mensaje es de solo lectura (siempre es false).
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Obtiene las reglas con las que se creó el mensaje.
        /// </summary>
        public AdaptiveMessageRules Rules => throw new NotImplementedException();

        /// <summary>
        /// Obtiene, establece o agrega el valor del campo con el ID especificado.
        /// </summary>
        /// <param name="id">ID del campo a manipular.</param>
        /// <returns>El valor del campo.</returns>
        public object this[int id] {
            get => _fields.FirstOrDefault(x => x.ID == id)?.Value;
            set {
                if (_fields.Any(x => x.ID == id))
                    _fields.FirstOrDefault(x => x.ID == id).Value = value;
                else
                    Add(id, value);
            }
        }

        /// <summary>
        /// Genera el mensaje a partir de un vector unidimensional de bytes donde se considera que se
        /// encuentra codificado un mensaje de este tipo.
        /// </summary>
        /// <param name="src">Vecto de bytes con el mensaje a deserializar.</param>
        /// <param name="rules">Reglas de composición del mensaje a deserializar.</param>
        /// <returns>Un mensaje rearmado a partir del vector de bytes.</returns>
        public static AdaptiveMessage Deserialize(byte[] src, AdaptiveMessageRules rules)
        {
            if (src == null)
                throw new AdaptiveMessageDeserializeException("No se recibieron datos para construir el mensaje", new ArgumentNullException(nameof(src)));

            if (src.Length < 8)
                throw new AdaptiveMessageDeserializeException("El vector debe contener al menos 8 elementos que correspondan al cabecera del mensaje", src);

            UInt64 headerRead = BitConverter.ToUInt64(src.Take(8)?.Reverse().ToArray(), 0);
            byte[] body = src.Skip(8)?.ToArray();

            if (headerRead == 0)
                throw new AdaptiveMessageDeserializeException("El encabezado del mensaje indica un mensaje vacío", src);

            AdaptiveMessage message = new AdaptiveMessage(rules ?? new AdaptiveMessageRules());

            int index = 0;

            while (headerRead > 1)
            {
                UInt64 res = headerRead % 2;
                headerRead /= 2;

                if (res == 1)
                    message.Add(index + 1, new object());

                index++;
            }

            message.Add(index + 1, new object());

            Processor(message, body, rules);

            return message;
        }

        /// <summary>
        /// Agrega campos al mensaje estableciendo el valor del mismo. Este campo deberá estar
        /// incluido en las reglas del mensaje <seealso cref="AdaptiveMessageRules"/>.
        /// </summary>
        /// <param name="id">ID del campo.</param>
        /// <param name="value">Valor del campo.</param>
        /// <exception cref="ArgumentException">
        /// El campo ya existe o no se encuentra en la definición del mensaje.
        /// </exception>
        public void Add(int id, object value)
            => _fields.Add(new Field(id, value));

        /// <summary>
        /// Agrega campos al mensaje. Este campo deberá estar incluido en las reglas del mensaje
        /// <seealso cref="AdaptiveMessageRules"/>.
        /// </summary>
        /// <param name="field">Campo a agregar.</param>
        /// <exception cref="ArgumentException">
        /// El campo ya existe o no se encuentra en la definición del mensaje.
        /// </exception>
        public void Add(Field field)
        {
            if (_fields.Any(x => x.ID == field.ID))
                throw new ArgumentException("El ID ya existe en el mensaje actual");

            if (!_messageRules.Any(x => x.ID == field.ID))
                throw new ArgumentException($"El campo: {field.ID} no está definido para el mensaje.");

            if (_fields.Count == 0)
                _fields.Add(field);
            else
            {
                Field minID = _fields.Where(x => x.ID > field.ID).Aggregate((x1, x2) => x1.ID < x2.ID ? x1 : x2);
                int minIDIndex = _fields.IndexOf(minID);

                if (minIDIndex == -1)
                    _fields.Add(field);
                else
                    _fields.Insert(minIDIndex, field);
            }
        }

        /// <summary>
        /// Elimina todos los campos en el mensaje.
        /// </summary>
        public void Clear()
            => _fields.Clear();

        /// <summary>
        /// Determina si el campo considerando el ID y valor de este se encuentra contenido en el mensaje.
        /// </summary>
        /// <param name="item">Campo a a buscar.</param>
        /// <returns>Un valor true si el campo con el ID y valor coincide dentro del mensaje.</returns>
        public bool Contains(Field item)
            => _fields.Any(x => x.ID == item.ID && x.Value == item.Value);

        /// <summary>
        /// Copia los campos del mensaje dentro un vector de <see cref="Field"/> a partir del indice especificado.
        /// </summary>
        /// <param name="array">Vector destino de la colección.</param>
        /// <param name="arrayIndex">Indice de partida del copiado.</param>
        public void CopyTo(Field[] array, int arrayIndex = 0)
            => _fields.Skip(arrayIndex).ToList().CopyTo(array);

        /// <summary>
        /// Devuelve un enumerador genérico que recorre en iteración al mensaje.
        /// </summary>
        /// <returns>Un enumerador de iteración.</returns>
        public IEnumerator<Field> GetEnumerator()
                    => _fields.OrderBy(x => x.ID).GetEnumerator();

        /// <summary>
        /// Devuelve un enumerador no genérico que recorre en iteración al mensaje.
        /// </summary>
        /// <returns>Un enumerador de iteración.</returns>
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// Devuelve el valor del campo especificado y realiza la conversión al tipo de dato
        /// <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="TResult">
        /// Tipo de datos destino, este deberá ser compatible con el campo.
        /// </typeparam>
        /// <param name="id">Identificador del campo.</param>
        /// <param name="converter">
        /// Función de conversión que recibe el valor <see cref="object"/> y devuelve la conversión realizada.
        /// </param>
        /// <returns>Valor convertido del campo.</returns>
        public TResult GetValue<TResult>(int id, Func<object, TResult> converter = null)
        {
            if (!_fields.Any(x => x.ID == id))
                throw new ArgumentException($"No existe el campo con el identificador ID: {id}");

            if (converter == null)
                return (TResult)_fields.First(x => x.ID == id).Value;

            return converter.Invoke(_fields.First(x => x.ID == id).Value);
        }

        /// <summary>
        /// Determina si existe un campo dentro del mensaje con el ID especificado.
        /// </summary>
        /// <param name="id">ID a buscar en el mensaje.</param>
        /// <returns>Un valor true si existe un campo con el ID especificado.</returns>
        public bool IsSet(int id)
            => _fields.Any(x => x.ID == id);

        /// <summary>
        /// Quita la primera aparición del campo que coincide con el ID especificado.
        /// </summary>
        /// <param name="id">ID del campo a quitar.</param>
        /// <returns>Un valor true en caso de remover el campo.</returns>
        public bool Remove(int id)
                    => _fields.Any(x => x.ID == id)
                ? _fields.Remove(_fields.First(x => x.ID == id))
                : false;

        /// <summary>
        /// Quita la primera aparición del campo que coincide con el ID y el valor especificado.
        /// </summary>
        /// <param name="field">Campo a quitar.</param>
        /// <returns>Un valor true en caso de remover el campo.</returns>
        public bool Remove(Field field)
            => _fields.Any(x => x.ID == field.ID && field.Value == x.Value)
                ? _fields.Remove(_fields.First(x => x.ID == field.ID && field.Value == x.Value))
                : false;

        /// <summary>
        /// Convierte el mensaje en una secuencia de bytes para ser transmitido o almacenado.
        /// </summary>
        /// <returns>Una secuencia de bytes que representa al mensaje.</returns>
        public byte[] Serialize()
        {
            ///9223372036854775808
            ulong header = 0;
            byte[] body = new byte[] { };

            foreach (Field field in this)
            {
                FieldDefinition definition = _messageRules.FirstOrDefault(x => x.ID == field.ID);
                IAdaptiveSerializer serializer = GetSerializer(definition);

                header += (ulong)Math.Pow(2, definition.ID - 1);
                body = body.Concat(serializer.Serialize(field, definition)).ToArray();
            }

            return BitConverter.GetBytes(header).Reverse().ToArray().PadLeft(8).Concat(body).ToArray();
        }

        /// <summary>
        /// Intenta obtener el valor del campo especificado a través del ID, de ser necesario puede
        /// utilizarse una función para procesar el valor del campo y realizar la conversión.
        /// </summary>
        /// <typeparam name="TResult">Tipo de dato resultado.</typeparam>
        /// <param name="id">ID del campo.</param>
        /// <param name="value">Valor del campo.</param>
        /// <param name="converter">Función convertidora.</param>
        /// <returns>Un valor true si se obtuvo el valor del campo.</returns>
        public bool TryGetValue<TResult>(int id, out TResult value, Func<object, TResult> converter = null)
        {
            value = default;

            if (!_fields.Any(x => x.ID == id))
                return false;

            try
            {
                value = GetValue(id, converter);
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Identifica y obtiene el serializador especificado en la definición.
        /// </summary>
        /// <param name="definition">Definición del campo.</param>
        /// <returns>Un serializador adaptativo.</returns>
        private static IAdaptiveSerializer GetSerializer(FieldDefinition definition)
        {
            if (definition == null)
                return null;

            return definition.Type switch
            {
                FieldType.Numeric => new NumericSerializer(),

                FieldType.Text => new TextSerializer(),

                FieldType.Binary => new BinarySerializer(),

                _ => throw new AdaptiveMessageDeserializeException("El tipo de campo no es válido para la conversión"),
            };
        }

        /// <summary>
        /// Procesa la secuencia de bytes para decodificar el contenido del mensaje a partir de la
        /// definición especificada.
        /// </summary>
        /// <param name="message">Mensaje a decodificar.</param>
        /// <param name="body">Secuencia de bytes que contiene los valores de los campos.</param>
        /// <param name="rules">Reglas de composición del mensaje.</param>
        private static void Processor(AdaptiveMessage message, byte[] body, AdaptiveMessageRules rules)
        {
            try
            {
                foreach (var field in message)
                {
                    FieldDefinition definition = rules.FirstOrDefault(x => x.ID == field.ID);

                    if (definition == null)
                        continue;

                    IAdaptiveSerializer converter = GetSerializer(definition);

                    field.Value = converter.Deserialize(ref body, definition)?.Value;
                }
            }
            catch (Exception ex)
            {
                throw new AdaptiveMessageDeserializeException("No se logró generar el mensaje desde los datos recibidos", ex);
            }
        }

        /// <summary>
        /// Representa en una cadena a la instancia actual.
        /// </summary>
        /// <returns>Una cadena Hex que representa al mensaje.</returns>
        public override string ToString()
            => BitConverter.ToString(Serialize()).Replace("-", "");


        /// <summary>
        /// Copia el contenido del mensaje a otra instancia, sobreescribiendo los campos utilizados.
        /// </summary>
        /// <param name="message">Mensaje destino.</param>
        public void CopyTo(IAdaptiveMessage dest)
        {
            foreach(Field field in this)
                dest[field.ID] = dest[field.ID];
        }
    }
}