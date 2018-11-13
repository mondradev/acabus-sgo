using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
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
    public static class ServerHelper
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
                throw new InvalidOperationException("No se especificó el nombre de la función.");

            MethodInfo[] methods = functionsClass.GetMethods(BindingFlags.Static | BindingFlags.Public);
            methods = methods.Where(x => x.Name == funcName).ToArray();

            IEnumerator enumerator = methods.GetEnumerator();

            MethodInfo method = null;

            while (enumerator.MoveNext() && !ValidateMethod(message, method = enumerator.Current as MethodInfo)) ;

            if (method is null)
                throw new InvalidOperationException("No se encontró la función especificada.");

            ParameterInfo[] parameters = method.GetParameters();
            Object[] parametersValues = new Object[parameters.Length];

            try
            {
                for (int i = 0; i < parameters.Length - 1; i++)
                {
                    ParameterFieldAttribute parameterData = parameters[i].GetCustomAttribute<ParameterFieldAttribute>();
                    int id = parameterData.ID;

                    Type type = parameters[i].ParameterType;

                    if (!message.IsSet(id) && !parameterData.Nullable)
                        throw new ArgumentNullException("Campo " + id);

                    if (parameterData.Nullable && !message.IsSet(id))
                    {
                        parametersValues[i] = type.IsValueType ? Activator.CreateInstance(type) : null;
                        continue;
                    }

                    type = Nullable.GetUnderlyingType(type) ?? type;

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
            catch (Exception ex) when (ex is ArgumentOutOfRangeException || ex is InvalidCastException || ex is FormatException)
            {
                throw new InvalidCastException("No se logró hacer la conversión de los datos recibidos a los parametros de la función.");
            }
            catch (Exception ex) when (ex is TargetException || ex is TargetInvocationException || ex is TargetParameterCountException)
            {
                throw new InvalidOperationException("Error al realizar la llamada a la función.");
            }
        }

        /// <summary>
        /// Crea un mensaje básico de error.
        /// </summary>
        /// <param name="text">Mensaje a indicar al cliente.</param>
        /// <param name="code">Código de respuesta al cliente.</param>
        /// <param name="e">Instancia que controla el evento de la petición.</param>
        /// <returns>Una instancia de mensaje.</returns>
        public static IMessage CreateError(string text, int code, IAdaptiveMsgArgs e)
        {
            IMessage message = e.CreateMessage();

            message[AcabusAdaptiveMessageFieldID.ResponseMessage.ToInt32()] = text;
            message[AcabusAdaptiveMessageFieldID.ResponseCode.ToInt32()] = code;

            return message;
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

            if (count > 0)
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
            IEnumerable<ParameterFieldAttribute> fields = parameters
                                                .Where(x => x.GetCustomAttribute<ParameterFieldAttribute>() != null)
                                                .Select(x => x.GetCustomAttribute<ParameterFieldAttribute>());

            foreach (ParameterFieldAttribute field in fields)
                valid &= (message.IsSet(field.ID) || field.Nullable);

            return valid;
        }
    }
}