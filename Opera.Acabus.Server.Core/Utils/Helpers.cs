using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Opera.Acabus.Server.Core.Utils
{
    /// <summary>
    /// Contiene funciones auxiliares para la validación de los datos al realizar las peticiones.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Realiza la llamada a la función especificada.
        /// </summary>
        /// <param name="message">Mensaje de la petición</param>
        /// <param name="functionsClass">Clase de las funciones.</param>
        public static void CallFunc(IMessage message, Type functionsClass)
        {
            String funcName = message[6]?.ToString();

            if (String.IsNullOrEmpty(funcName))
                return;

            MethodInfo[] methods = functionsClass.GetMethods(BindingFlags.Static | BindingFlags.Public);
            MethodInfo method = methods.FirstOrDefault(x => x.Name == funcName);

            if (method is null)
                return;

            ParameterInfo[] parameters = method.GetParameters();
            Object[] parametersValues = new Object[parameters.Length];

            for (int i = 0; i < parameters.Length - 1; i++)
            {
                int id = parameters[i].GetCustomAttribute<ParameterFieldAttribute>().ID;
                parametersValues[i] = message[id];
            }

            parametersValues[parametersValues.Length - 1] = message;

            method.Invoke(null, parametersValues);
        }

        /// <summary>
        /// Valida la solicitud sea compatible con la función destino.
        /// </summary>
        /// <param name="message">Mensaje de petición.</param>
        /// <param name="functionsClass">Clase que contiene a la función.</param>
        /// <returns>Un valor true si la petición es compatible con la función.</returns>
        public static bool ValidateRequest(IMessage message, Type functionsClass)
        {
            String funcName = message[6]?.ToString();

            if (String.IsNullOrEmpty(funcName))
                return false;

            MethodInfo[] methods = functionsClass.GetMethods(BindingFlags.Static | BindingFlags.Public);
            MethodInfo method = methods.FirstOrDefault(x => x.Name == funcName);

            if (method is null)
                return false;

            bool valid = true;

            ParameterInfo[] parameters = method.GetParameters();
            IEnumerable<Int32> fields = parameters.Where(x=> x.GetCustomAttribute<ParameterFieldAttribute>() != null)
                .Select(x => x.GetCustomAttribute<ParameterFieldAttribute>().ID);

            foreach (Int32 field in fields)
                valid &= message.IsSet(field);

            return valid;
        }
    }

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
        public ParameterFieldAttribute(int id)
        {
            ID = id;
        }

        /// <summary>
        /// Obtiene el ID del campo requerido por el parametro.
        /// </summary>
        public int ID { get; }
    }
}