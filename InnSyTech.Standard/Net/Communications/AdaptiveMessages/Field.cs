using System;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages
{
    /// <summary>
    /// Representa la estructura de un campo contenido en los mensajes adaptativos <seealso
    /// cref="AdaptiveMessage" />. El campo está compuesto por un identificador que lo diferencia dentro del
    /// mensaje y su valor almacenado.
    /// </summary>
    public sealed class Field
    {

        /// <summary>
        /// Valor almacenado del campo.
        /// </summary>
        private object _value;

        /// <summary>
        /// Crea un campo nuevo especificando su identificador y el valor que representa.
        /// </summary>
        /// <param name="id"> Identificador del campo. </param>
        /// <param name="value"> Valor del campo. </param>
        public Field(int id, object value)
        {
            ID = id;
            Value = value;
        }

        /// <summary>
        /// Obtiene el identificador del campo. Este valor lo permite diferenciar dentro del mensaje.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// Obtiene o establece el valor del campo. Este valor no es permitido ser nulo, de ser
        /// necesario deberá omitirse dentro del mensaje.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// En caso de que el valor a asignar sea nulo.
        /// </exception>
        public object Value
        {
            get => _value;
            set
            {
                _value = value ?? throw new ArgumentNullException("value", "El campo no puede ser asignado con un valor nulo.");
            }
        }

        /// <summary>
        /// Representa el campo actual a través de una cadena.
        /// </summary>
        /// <returns>Un cadena que representa al campo.</returns>
        public override string ToString()
            => String.Format("Field{{ID={0}, Value={1}}}", ID, Value);
    }
}