using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Serializers;
using InnSyTech.Standard.Utils;
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
        private readonly SortedSet<Field> _fields;

        /// <summary>
        /// Crea una instancia nueva de un mensaje.
        /// </summary>
        /// <param name="rules">Reglas de composición del mensaje.</param>
        public AdaptiveMessage(AdaptiveMessageRules rules)
        {
            _fields = new SortedSet<Field>(new FieldComparer());
            Rules = rules ?? new AdaptiveMessageRules();

            Rules.IsReadOnly = true;
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
        public AdaptiveMessageRules Rules { get; }

        /// <summary>
        /// Obtiene el tamaño en bytes del mensaje.
        /// </summary>
        public int Size => Serialize().Length;

        /// <summary>
        /// Obtiene, establece o agrega el valor del campo con el ID especificado.
        /// </summary>
        /// <param name="id">ID del campo a manipular.</param>
        /// <returns>El valor del campo.</returns>
        public object this[int id] {
            get => GetValue<object>(id, null);
            set => Add(id, value);
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
            IList<int> fieldsID = new List<int>();

            while (headerRead > 1)
            {
                UInt64 res = headerRead % 2;
                headerRead /= 2;

                if (res == 1)
                    fieldsID.Add(index + 1);
                index++;
            }

            fieldsID.Add(index + 1);

            return DeserializeBody(message, body, fieldsID);
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
            => Add(new Field(value, Rules[id]));

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
            if (Rules[field.ID].IsNull())
                throw new ArgumentException($"El campo: {field.ID} no está definido para el mensaje.");

            if (this[field.ID].IsNull())
                _fields.Add(field);
            else
                _fields.First(x => x.ID == field.ID).Value = field.Value;
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
            => IsSet(item.ID) && GetValue<object>(item.ID) == item.Value;

        /// <summary>
        /// Copia los campos del mensaje dentro un vector de <see cref="Field"/> a partir del indice especificado ordenados por ID de forma ascendente.
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
                    => _fields.GetEnumerator();

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
            if (!IsSet(id))
                return default;

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
                    => IsSet(id) ? _fields.RemoveWhere(x => x.ID == id) > 0 : false;

        /// <summary>
        /// Quita la primera aparición del campo que coincide con el ID y el valor especificado.
        /// </summary>
        /// <param name="field">Campo a quitar.</param>
        /// <returns>Un valor true en caso de remover el campo.</returns>
        public bool Remove(Field field)
            => Remove(field.ID);

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
                FieldDefinition definition = field.Definition;
                IAdaptiveSerializer serializer = definition.Serializer;

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

            if (!IsSet(id))
                return false;

            try
            {
                value = GetValue(id, converter);

                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Procesa la secuencia de bytes para decodificar el contenido del mensaje a partir de la
        /// definición especificada.
        /// </summary>
        /// <param name="message">Mensaje a decodificar.</param>
        /// <param name="body">Secuencia de bytes que contiene los valores de los campos.</param>
        /// <param name="rules">Reglas de composición del mensaje.</param>
        private static AdaptiveMessage DeserializeBody(AdaptiveMessage message, byte[] body, IList<int> fieldsID)
        {
            try
            {
                foreach (var field in fieldsID)
                {
                    FieldDefinition definition = message.Rules.Any(f => f.ID == field) ? message.Rules[field] : null;

                    if (definition == null)
                        continue;

                    IAdaptiveSerializer converter = definition.Serializer;

                    var value = converter.Deserialize(ref body, definition)?.Value;

                    message.Add(new Field(value, definition));
                }

                return message;
            }
            catch (Exception ex)
            {
                throw new AdaptiveMessageDeserializeException("Mensaje corrompido, no es posible leer", ex);
            }
        }

        /// <summary>
        /// Representa en una cadena a la instancia actual.
        /// </summary>
        /// <returns>Una cadena Hex que representa al mensaje.</returns>
        public override string ToString() => Serialize().ToHex();

        /// <summary>
        /// Copia el contenido del mensaje a otra instancia, sobreescribiendo los campos utilizados.
        /// </summary>
        /// <param name="message">Mensaje destino.</param>
        public void CopyTo(IAdaptiveMessage dest)
        {
            foreach (Field field in this)
                dest[field.ID] = this[field.ID];
        }

        /// <summary>
        /// Estructura que define como se comparan los campos en una lista ordenada.
        /// </summary>
        private class FieldComparer : IComparer<Field>
        {
            public int Compare(Field x, Field y) => x.ID.CompareTo(y.ID);
        }
    }
}