using System;
using System.Collections;
using System.Collections.Generic;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages
{
    /// <summary>
    /// Representa una interfaz para la implementación de mensajes adaptativos. Se utiliza un
    /// conjunto de reglas para definir el comportamiento de cada campo en el mensaje, permitiendo
    /// ahorra espacio en el tamaño final del mensaje a enviar a través de un flujo de datos.
    /// </summary>
    public interface IMessage : IEnumerable, IEnumerable<Field>, ICollection<Field>
    {
        /// <summary>
        /// Obtiene, establece o agrega el valor del campo con el ID especificado.
        /// </summary>
        /// <param name="id">ID del campo a manipular.</param>
        /// <returns>El valor del campo.</returns>
        object this[int id] { get; set; }
        
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
        /// <exception cref="FormatException">El formato del dato no es compatible con el tipo de dato destino.</exception>
        TResult GetValue<TResult>(int id, Func<object, TResult> converter = null);

        /// <summary>
        /// Determina si existe un campo dentro del mensaje con el ID especificado.
        /// </summary>
        /// <param name="id">ID a buscar en el mensaje.</param>
        /// <returns>Un valor true si existe un campo con el ID especificado.</returns>
        bool IsSet(int id);

        /// <summary>
        /// Quita la primera aparición del campo que coincide con el ID especificado.
        /// </summary>
        /// <param name="id">ID del campo a quitar.</param>
        /// <returns>Un valor true en caso de remover el campo.</returns>
        bool Remove(int id);

        /// <summary>
        /// Convierte el mensaje a una secuencia de bytes para ser transmitido o almacenado.
        /// </summary>
        /// <returns>Una secuencia de bytes que representa al mensaje.</returns>
        byte[] Serialize();

        /// <summary>
        /// Intenta obtener el valor del campo especificado a través del ID, de ser necesario puede
        /// utilizarse una función para procesar el valor del campo y realizar la conversión.
        /// </summary>
        /// <typeparam name="TResult">Tipo de dato resultado.</typeparam>
        /// <param name="id">ID del campo.</param>
        /// <param name="value">Valor del campo.</param>
        /// <param name="converter">Función convertidora.</param>
        /// <returns>Un valor true si se obtuvo el valor del campo.</returns>
        bool TryGetValue<TResult>(int id, out TResult value, Func<object, TResult> converter = null);
    }
}