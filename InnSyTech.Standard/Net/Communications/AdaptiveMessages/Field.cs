﻿using System;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages
{
    /// <summary>
    /// Representa la estructura de un campo contenido en los mensajes adaptativos <see
    /// cref="IAdaptiveMessage" />. El campo está compuesto por un identificador que lo diferencia dentro del
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
        /// <param name="value"> Valor del campo. </param>
        /// <param name="definition">Definición del tipo de campo</param>
        public Field(object value, FieldDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition), "La definición del campo no puede ser un valor nulo");
            ID = definition.ID;

            Value = Definition.Validate(value) ? value : throw new ArgumentNullException("value", "El campo no es compatible con el valor.");
        }

        /// <summary>i
        /// Obtiene la definición del campo.
        /// </summary>
        public FieldDefinition Definition { get; }

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
        public object Value {
            get => _value;
            set => _value = Definition.Validate(value) ? value
                : throw new ArgumentNullException("value", "El campo no es compatible con el valor.");
        }

        /// <summary>
        /// Representa el campo actual a través de una cadena.
        /// </summary>
        /// <returns>Un cadena que representa al campo.</returns>
        public override string ToString()
            => string.Format("Field[ID={0}, Value={1}, Type={2}]", ID, Definition.ToString(Value), Definition.Type.ToString());
    }
}