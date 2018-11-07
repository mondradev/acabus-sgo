using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using System;

namespace Opera.Acabus.Server.Core.Utils
{
    /// <summary>
    /// Representa un atributo utilizado para enlazar el parametro de una función con el campo de un mensaje <see cref="IMessage"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class ParameterFieldAttribute : Attribute
    {
        /// <summary>
        /// Especifica el campo requerido para el parametro de la función.
        /// </summary>
        /// <param name="id"></param>
        public ParameterFieldAttribute(int id, bool nullable = false)
        {
            ID = id;
            Nullable = nullable;
        }

        /// <summary>
        /// Obtiene el ID del campo requerido por el parametro.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// Obtiene un valor que indica si el valor puede recibir valores nulos.
        /// </summary>
        public bool Nullable { get; }
    }
}