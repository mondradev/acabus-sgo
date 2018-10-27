using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Utils;
using Opera.Acabus.Core.Services;
using System;
using System.Collections;
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
            methods = methods.Where(x => x.Name == funcName).ToArray();

            IEnumerator enumerator = methods.GetEnumerator();

            MethodInfo method = null;

            while (enumerator.MoveNext() && !ValidateMethod(message, method = enumerator.Current as MethodInfo)) ;

            if (method is null)
                return;

            ParameterInfo[] parameters = method.GetParameters();
            Object[] parametersValues = new Object[parameters.Length];

            for (int i = 0; i < parameters.Length - 1; i++)
            {
                int id = parameters[i].GetCustomAttribute<ParameterFieldAttribute>().ID;

                Type type = parameters[i].ParameterType;

                if (type == typeof(Int16))
                    parametersValues[i] = Int16.Parse(message[id].ToString());
                else if (type == typeof(Int32))
                    parametersValues[i] = Int32.Parse(message[id].ToString());
                else if (type == typeof(Int64))
                    parametersValues[i] = Int64.Parse(message[id].ToString());
                else if (type == typeof(UInt16))
                    parametersValues[i] = Convert.ToUInt16(message[id]);
                else if (type == typeof(UInt32))
                    parametersValues[i] = Convert.ToUInt32(message[id]);
                else if (type == typeof(UInt64))
                    parametersValues[i] = Convert.ToUInt64(message[id]);
                else if (type == typeof(Double))
                    parametersValues[i] = Double.Parse(message[id].ToString());
                else if (type == typeof(Single))
                    parametersValues[i] = Single.Parse(message[id].ToString());
                else if (type == typeof(DateTime))
                    parametersValues[i] = DateTime.Parse(String.Format("{0}{1}-{2}-{3} {4}:{5}:{6}", message[id].ToString().Cut(7)));
                else if (type == typeof(TimeSpan))
                    parametersValues[i] = TimeSpan.Parse(String.Format("{0}:{1}:{2}", message[id].ToString().Cut(3)));
                else if (type == typeof(Decimal))
                    parametersValues[i] = Decimal.Parse(message[id].ToString());
                else if (type.IsEnum)
                    parametersValues[i] = Enum.Parse(parameters[i].ParameterType, message[id].ToString());
                else if (type == typeof(bool))
                    parametersValues[i] = BitConverter.ToBoolean(message[id] as byte[] ?? BitConverter.GetBytes(default(bool)), 0);
                else
                    parametersValues[i] = Convert.ChangeType(message[id], parameters[i].ParameterType);
            }

            parametersValues[parametersValues.Length - 1] = message;

            method.Invoke(null, parametersValues);
        }

        /// <summary>
        /// Valida si el método corresponde a la petición realizada.
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        /// <param name="method"></param>
        /// <returns>Un valor de true si la petición es compatible con el método.</returns>
        private static bool ValidateMethod(IMessage message, MethodInfo method)
        {
            bool valid = true;

            ParameterInfo[] parameters = method.GetParameters();
            IEnumerable<int> fields = parameters.Where(x => x.GetCustomAttribute<ParameterFieldAttribute>() != null)
                .Select(x => x.GetCustomAttribute<ParameterFieldAttribute>().ID);

            foreach (int field in fields)
                valid &= message.IsSet(field);

            return valid;
        }

        /// <summary>
        /// Procesa el mensaje para tratarlo como una enumeración.
        /// </summary>
        /// <param name="message">Mensaje que representa la enumeración.</param>
        /// <param name="count">Cantidad total de la numeración.</param>
        /// <param name="enumeratingFunc">Función para procesar el elemento actual.</param>
        public static void Enumerating(IMessage message, Int32 count, Action<Int32> enumeratingFunc)
        {
            if (!message.IsSet(AcabusAdaptiveMessageFieldID.IsEnumerable.ToInt32()))
            {
                message[AcabusAdaptiveMessageFieldID.IsEnumerable.ToInt32()] = BitConverter.GetBytes(true);
                message[AcabusAdaptiveMessageFieldID.EnumerableCount.ToInt32()] = count;
                message[AcabusAdaptiveMessageFieldID.CurrentPosition.ToInt32()] = 0;
            }
            else if (message.GetValue(AcabusAdaptiveMessageFieldID.EnumerableOperation.ToInt32(), x => Convert.ToInt32(x)) == 0)
                message[AcabusAdaptiveMessageFieldID.CurrentPosition.ToInt32()] = message.GetValue(AcabusAdaptiveMessageFieldID.CurrentPosition.ToInt32(), x => Convert.ToInt32(x)) + 1;
            else if (message.GetValue(AcabusAdaptiveMessageFieldID.CurrentPosition.ToInt32(), x => Convert.ToInt32(x)) == 1)
            {
                message[AcabusAdaptiveMessageFieldID.CurrentPosition.ToInt32()] = 0;
                message[AcabusAdaptiveMessageFieldID.EnumerableOperation.ToInt32()] = 0;
            }

            enumeratingFunc?.Invoke(Convert.ToInt32(message[AcabusAdaptiveMessageFieldID.CurrentPosition.ToInt32()]));
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
            methods = methods.Where(x => x.Name == funcName).ToArray();

            IEnumerator enumerator = methods.GetEnumerator();

            MethodInfo method = null;

            while (enumerator.MoveNext() && !ValidateMethod(message, method = enumerator.Current as MethodInfo)) ;

            if (method is null)
                return false;

            return true;
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