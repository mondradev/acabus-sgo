using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets;
using InnSyTech.Standard.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

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
        /// <param name="request">Controlador de la petición.</param>
        /// <param name="functionsClass">Clase de las funciones.</param>
        public static void CallFunc(IAdaptiveMessageReceivedArgs request, Type functionsClass)
        {
            IAdaptiveMessage message = request.Data;

            String funcName = message.GetFunctionName();

            if (String.IsNullOrEmpty(funcName))
                throw new InvalidOperationException("No se especificó el nombre de la función.");

            MethodInfo method = null;

            MethodInfo[] methods = functionsClass.GetMethods(BindingFlags.Static | BindingFlags.Public);
            methods = methods.Where(x => x.Name == funcName).ToArray();

            IEnumerator enumerator = methods.GetEnumerator();

            while (enumerator.MoveNext() && !ValidateMethod(message, method = enumerator.Current as MethodInfo)) ;

            if (method is null)
                throw new InvalidOperationException("No se encontró la función especificada.");

            ParameterInfo[] parameters = method.GetParameters();
            Object[] parametersValues = new Object[parameters.Length];

            try
            {
                ProcessParameters(message, parameters, parametersValues);

                parametersValues[parametersValues.Length - 1] = request;

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
        /// Procesa los parametros que requiere la función llamada a través de <see cref="CallFunc(IAdaptiveMessage, Type)"/>
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        /// <param name="parameters">Vector con la información de los parametros de la función.</param>
        /// <param name="parametersValues">Vector con los valores para los parametros de la función.</param>
        private static void ProcessParameters(IAdaptiveMessage message, ParameterInfo[] parameters, object[] parametersValues)
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

                parametersValues[i] = DeserializeParameter(message[id], parameters[i], type);
            }
        }

        /// <summary>
        /// Deserializa el valor de un campo en el mensaje correspondiente al parametro de la función llamada.
        /// </summary>
        /// <param name="data">Dato del campo.</param>
        /// <param name="parameter">Información del parámetro de la función.</param>
        /// <param name="type">Tipo de dato del campo.</param>
        /// <returns>El valor del parametro.</returns>
        private static object DeserializeParameter(object data, ParameterInfo parameter, Type type)
        {
            if (type == typeof(bool))
                return BitConverter.ToBoolean(data as byte[] ?? BitConverter.GetBytes(default(bool)), 0);
            else if (type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static) != null)
            {
                if (type.IsEnum)
                    return Enum.Parse(parameter.ParameterType, data.ToString());
                if (type == typeof(DateTime))
                    return DateTime.Parse(String.Format("{0}{1}-{2}-{3} {4}:{5}:{6}", data.ToString().Cut(7)));
                else if (type == typeof(TimeSpan))
                    return TimeSpan.Parse(String.Format("{0}:{1}:{2}", data.ToString().Cut(3)));
                else
                    return type.GetMethod("Parse").Invoke(null, new object[] { data.ToString() });
            }
            else
                return Convert.ChangeType(data, parameter.ParameterType);
        }

        /// <summary>
        /// Envía una respuesta de error al cliente definido en los argumentos de la petición.
        /// </summary>
        /// <param name="e">Datos de la petición.</param>
        /// <param name="exception">Excepción que se enviará como respuesta.</param>
        public static void SendException(this IAdaptiveMessageReceivedArgs e, ServiceException exception)
        {
            string response = String.Format("{0} [Error=\"{1}\", Servicio=\"{2}\", Función={3}]",
                  exception.InnerException != null ? exception.Message : "Ocurrió un error al procesar la petición",
                  exception.InnerException != null ? exception.InnerException.Message : exception.Message,
                   exception.ModuleName,
                   exception.FunctionName
                   );

            IAdaptiveMessage message = e.CreateMessage();

            message.SetResponse(response, exception.Code);

            e.Response(message);
        }

        /// <summary>
        /// Envía una colección a través de <see cref="IAdaptiveMessage"/>.
        /// </summary>
        /// <typeparam name="T">Tipo de dato de la colección.</typeparam>
        /// <param name="collection">Colección a enviar.</param>
        /// <param name="args">Controlador de la petición.</param>
        /// <param name="idItem">Identificador del campo utilizado para transmitir los elementos.</param>
        /// <param name="serializer">Función serializadora.</param>
        public static void SendCollection<T>(ICollection<T> collection, IAdaptiveMessageReceivedArgs args, uint idItem, Func<T, object> serializer)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            if (!args.Data.IsEnumerable())
                args.Data.SetAsEnumerable(collection.Count);

            args.Data.SetPosition(-1);
            args.Data.SetResponse(String.Empty, AdaptiveMessageResponseCode.PARTIAL_CONTENT);

            args.Response();

            while (args.Connection.Connected)
            {
                AdaptiveMessageSocketHelper.ReadBuffer(args.Connection, args.Data.Rules)?
                    .CopyTo(args.Data);

                switch (args.Data.GetEnumOp())
                {
                    case AdaptiveMessageEnumOp.NEXT:
                        if (args.Data.GetPosition() < -1)
                            args.Data.SetResponse("Operación no válida al recorrer la colección",
                                AdaptiveMessageResponseCode.BAD_REQUEST);
                        else if (args.Data.GetPosition() >= collection.Count)
                            args.Data.Remove((int)idItem);
                        else
                        {
                            args.Data.SetPosition(args.Data.GetPosition() + 1);
                            args.Data[(int)idItem] = serializer(collection.ElementAt(args.Data.GetPosition()));
                        }

                        args.Response();
                        break;

                    case AdaptiveMessageEnumOp.RESET:
                        args.Data.SetPosition(-1);
                        args.Response();
                        break;

                    case (AdaptiveMessageEnumOp)(-1):
                        args.Data.SetResponse("Operación no válida", AdaptiveMessageResponseCode.METHOD_NOT_ALLOWED);
                        args.Response();
                        break;

                    default:
                        Thread.Sleep(10);
                        break;
                }
            }
        }

        /// <summary>
        /// Valida la solicitud sea compatible con la función destino.
        /// </summary>
        /// <param name="message">Mensaje de petición.</param>
        /// <param name="functionsClass">Clase que contiene a la función.</param>
        /// <returns>Un valor true si la petición es compatible con la función.</returns>
        public static bool ValidateRequest(IAdaptiveMessage message, Type functionsClass)
        {
            String funcName = message.GetFunctionName();

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
        private static bool ValidateMethod(IAdaptiveMessage message, MethodInfo method)
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