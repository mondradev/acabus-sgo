using InnSyTech.Standard.Utils;
using System;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages
{
    /// <summary>
    /// Proporciona funciones para la conversión de los campos de los mensajes <see cref="IAdaptiveMessage"/>.
    /// </summary>
    public static class AdaptiveMessageExtension
    {
        /// <summary>
        /// Obtiene el valor Boolean del campo especificado.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <returns>El valor del campo.</returns>
        public static Boolean GetBoolean(this IAdaptiveMessage src, int id)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            if (src.TryGetValue(id, out Boolean value, x => BitConverter.ToBoolean(x as byte[], 0)))
                return value;

            throw new FormatException("No se logró realizar la conversión a Boolean");
        }

        /// <summary>
        /// Obtiene el valor del campo Byte especificado. El campo deberá ser del tipo <see cref="FieldDefinition.FieldType.Binary"/>.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <return>Valor del campo.</return>
        public static Byte[] GetBytes(this IAdaptiveMessage src, int id)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            return src[id] as byte[];
        }

        /// <summary>
        /// Obtiene el valor DateTime del campo especificado.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <returns>El valor del campo.</returns>
        public static DateTime GetDateTime(this IAdaptiveMessage src, int id)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            String value = src[id].ToString();
            String dateTimeText = String.Format("{0}{1}-{2}-{3} {4}:{5}:{6}", value.Cut(7));

            if (DateTime.TryParse(dateTimeText, out DateTime result))
                return result;

            throw new FormatException("No se logró realizar la conversión a DateTime");
        }

        /// <summary>
        /// Obtiene el valor derivado de Enum del campo especificado.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <returns>El valor del campo.</returns>
        public static T GetEnum<T>(this IAdaptiveMessage src, int id) where T : struct
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            if (src.TryGetValue(id, out int value, x => Convert.ToInt16(x)))
                return (T)Convert.ChangeType(value, typeof(T));

            throw new FormatException("No se logró realizar la conversión a " + typeof(T).Name);
        }

        /// <summary>
        /// Obtiene el valor Int16 del campo especificado.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <returns>El valor del campo.</returns>
        public static Int16 GetInt16(this IAdaptiveMessage src, int id)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            if (src.TryGetValue(id, out Int16 value, x => Convert.ToInt16(x)))
                return value;

            throw new FormatException("No se logró realizar la conversión a Int16");
        }

        /// <summary>
        /// Obtiene el valor Int32 del campo especificado.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <returns>El valor del campo.</returns>
        public static Int32 GetInt32(this IAdaptiveMessage src, int id)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            if (src.TryGetValue(id, out Int32 value, x => Convert.ToInt32(x)))
                return value;

            throw new FormatException("No se logró realizar la conversión a Int32");
        }

        /// <summary>
        /// Obtiene el valor Int64 del campo especificado.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <returns>El valor del campo.</returns>
        public static Int64 GetInt64(this IAdaptiveMessage src, int id)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            if (src.TryGetValue(id, out Int64 value, x => Convert.ToInt64(x)))
                return value;

            throw new FormatException("No se logró realizar la conversión a Int64");
        }

        /// <summary>
        /// Obtiene el valor String del campo especificado.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <returns>El valor del campo.</returns>
        public static String GetString(this IAdaptiveMessage src, int id)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            if (src.TryGetValue(id, out String value, x => x.ToString()))
                return value;

            throw new FormatException("No se logró realizar la conversión a String");
        }

        /// <summary>
        /// Obtiene el valor TimeSpan del campo especificado.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <returns>El valor del campo.</returns>
        public static TimeSpan GetTimeSpan(this IAdaptiveMessage src, int id)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            String value = src[id].ToString();
            String timeSpanText = String.Format("{0}:{1}:{2}", value.Cut(3));

            if (TimeSpan.TryParse(timeSpanText, out TimeSpan result))
                return result;

            throw new FormatException("No se logró realizar la conversión a TimeSpan");
        }

        /// <summary>
        /// Obtiene el valor UInt16 del campo especificado.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <returns>El valor del campo.</returns>
        public static UInt16 GetUInt16(this IAdaptiveMessage src, int id)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            if (src.TryGetValue(id, out UInt16 value, x => Convert.ToUInt16(x)))
                return value;

            throw new FormatException("No se logró realizar la conversión a UInt16");
        }

        /// <summary>
        /// Obtiene el valor UInt32 del campo especificado.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <returns>El valor del campo.</returns>
        public static UInt32 GetUInt32(this IAdaptiveMessage src, int id)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            if (src.TryGetValue(id, out UInt32 value, x => Convert.ToUInt32(x)))
                return value;

            throw new FormatException("No se logró realizar la conversión a UInt32");
        }

        /// <summary>
        /// Obtiene el valor UInt64 del campo especificado.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <returns>El valor del campo.</returns>
        public static UInt64 GetUInt64(this IAdaptiveMessage src, int id)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            if (src.TryGetValue(id, out UInt64 value, x => Convert.ToUInt64(x)))
                return value;

            throw new FormatException("No se logró realizar la conversión a UInt64");
        }

        /// <summary>
        /// Establece el valor del campo Boolean especificado. El campo deberá ser del tipo <see cref="FieldDefinition.FieldType.Binary"/>.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <param name="value">Valor del campo.</param>
        public static void SetBoolean(this IAdaptiveMessage src, int id, Boolean value)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            src[id] = BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Establece el valor del campo DateTime especificado. El campo deberá ser del tipo <see cref="FieldDefinition.FieldType.Numeric"/>.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <param name="value">Valor del campo.</param>
        public static void SetDateTime(this IAdaptiveMessage src, int id, DateTime value)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            String dateTimeText = String.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second);

            src[id] = dateTimeText;
        }

        /// <summary>
        /// Establece el valor del campo derivado Enum especificado. El campo deberá ser del tipo <see cref="FieldDefinition.FieldType.Numeric"/>.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <param name="value">Valor del campo.</param>
        public static void SetEnum<T>(this IAdaptiveMessage src, int id, T value) where T : struct
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            src[id] = Convert.ChangeType(value, typeof(int));
        }

        /// <summary>
        /// Establece el valor del campo TimeSpan especificado. El campo deberá ser del tipo <see cref="FieldDefinition.FieldType.Numeric"/>.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <param name="value">Valor del campo.</param>
        public static void SetTimeSpan(this IAdaptiveMessage src, int id, TimeSpan value)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            String timeSpanText = String.Format("{0}{1}{2}", value.Hours, value.Minutes, value.Seconds);

            src[id] = timeSpanText;
        }

        /// <summary>
        /// Indica si tiene el campo de nombre de la función activo.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        public static Boolean HasFunctionName(this IAdaptiveMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return message.IsSet((int)AdaptiveMessageFieldID.FunctionName);
        }

        /// <summary>
        /// Obtiene el nombre de la función en el mensaje.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <returns>Nombre de la función.</returns>
        public static string GetFunctionName(this IAdaptiveMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return message.GetString((int)AdaptiveMessageFieldID.FunctionName);
        }

        /// <summary>
        /// Establece el nombre de la función en el mensaje.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <param name="functionName">Nombre de la función.</param>
        public static void SetFunctionName(this IAdaptiveMessage message, string functionName)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            message[(int)AdaptiveMessageFieldID.FunctionName] = functionName;
        }

        /// <summary>
        /// Establece el token de la aplicación requerido para la autenticación de un nodo válido.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <param name="appToken">Token de aplicación.</param>
        public static void SetAPIToken(this IAdaptiveMessage message, byte[] appToken)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            message[(int)AdaptiveMessageFieldID.APIToken] = appToken;
        }

        /// <summary>
        /// Establece el hash de las reglas utilizadas por el sistema de mensajes.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <param name="hashRules">Hash de las reglas de mensajes.</param>
        public static void SetHashRules(this IAdaptiveMessage message, byte[] hashRules)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            message[(int)AdaptiveMessageFieldID.HashRules] = hashRules;
        }

        /// <summary>
        /// Establece el nombre del módulo.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <param name="moduleName">Nombre del módulo.</param>
        public static void SetModuleName(this IAdaptiveMessage message, string moduleName)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            message[(int)AdaptiveMessageFieldID.ModuleName] = moduleName;
        }

        /// <summary>
        /// Obtiene el nombre del módulo.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        public static string GetModuleName(this IAdaptiveMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return message.GetString((int)AdaptiveMessageFieldID.ModuleName);
        }


        /// <summary>
        /// Indica si tiene el campo de nombre del módulo activo.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        public static Boolean HasModuleName(this IAdaptiveMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return message.IsSet((int)AdaptiveMessageFieldID.ModuleName);
        }

        /// <summary>
        /// Obtiene el código de respuesta de la petición.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <returns>El código de respuesta de la última petición.</returns>
        public static AdaptiveMessageResponseCode GetResponseCode(this IAdaptiveMessage message)
        {

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return message.GetEnum<AdaptiveMessageResponseCode>((int)AdaptiveMessageFieldID.ResponseCode);
        }

        /// <summary>
        /// Obtiene el mensaje de respuesta de la petición.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <returns>El mensaje de respuesta de la última petición.</returns>
        public static string GetResponseMessage(this IAdaptiveMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return message.GetString((int)AdaptiveMessageFieldID.ResponseMessage);
        }

        /// <summary>
        /// Obtiene la cantidad de elementos de la enumeración.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <returns>Cantidad de elementos de la colección.</returns>
        public static int GetEnumerableCount(this IAdaptiveMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return message.GetInt32((int)AdaptiveMessageFieldID.EnumerableCount);
        }

        /// <summary>
        /// Establece la respuesta a una petición.
        /// </summary>
        /// <param name="message">Mensaja adaptativo.</param>
        /// <param name="response">Respuesta de la petición.</param>
        /// <param name="code">Código de respuesta a la petición.</param>
        public static void SetResponse(this IAdaptiveMessage message, string response, AdaptiveMessageResponseCode code)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            message[(int)AdaptiveMessageFieldID.ResponseMessage] = response;
            message[(int)AdaptiveMessageFieldID.ResponseCode] = code;
        }

        /// <summary>
        /// Obtiene si el mensaje contiene una colección.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <returns>Un valor true si es enumerable.</returns>
        public static Boolean IsEnumerable(this IAdaptiveMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));


            return message.IsSet((int)AdaptiveMessageFieldID.IsEnumerable);
        }

        /// <summary>
        /// Establece el mensaje como enumerable y se posiciona al comienzo de la enumeración.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <param name="count">Cantidad de elementos de la colección.</param>
        public static void SetAsEnumerable(this IAdaptiveMessage message, int count)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            message[(int)AdaptiveMessageFieldID.IsEnumerable] = BitConverter.GetBytes(true);
            message[(int)AdaptiveMessageFieldID.EnumerableCount] = count;
            message[(int)AdaptiveMessageFieldID.CurrentPosition] = 0;
        }

        /// <summary>
        /// Obtiene la posición actual de la colección. En caso de no ser una colección devuelve un valor -1.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <returns>Posición en el recorrido de la enumeración.</returns>
        public static int GetPosition(this IAdaptiveMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (!message.IsEnumerable())
                return -1;

            return message.GetInt32((int)AdaptiveMessageFieldID.CurrentPosition);
        }


        /// <summary>
        /// Establece la posición actual de la colección.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <param name="position">Posición de la colección.</param>
        public static void SetPosition(this IAdaptiveMessage message, int position)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (message.IsEnumerable())
                message[(int)AdaptiveMessageFieldID.CurrentPosition] = position;
        }

        /// <summary>
        /// Establece la operación a realizar en la enumeración.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <param name="op">Operación a realizar.</param>
        public static void SetEnumOp(this IAdaptiveMessage message, AdaptiveMessageEnumOp op)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (message.IsEnumerable())
                message[(int)AdaptiveMessageFieldID.EnumerableOperation] = op;
        }

        /// <summary>
        /// Obtiene la operación a realizar en la enumeración.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <returns>Operación a efecturar.</returns>
        public static AdaptiveMessageEnumOp GetEnumOp(this IAdaptiveMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (message.IsEnumerable())
                return (AdaptiveMessageEnumOp)message[(int)AdaptiveMessageFieldID.EnumerableOperation];

            return (AdaptiveMessageEnumOp)(-1);
        }

        /// <summary>
        /// Indica si tiene el campo de APIToken activo.
        /// </summary>
        /// <param name="message">Mensaje adaptivo.</param>
        /// <returns>Un valor true si tiene el campo activo.</returns>
        public static Boolean HasAPIToken(this IAdaptiveMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return message.IsSet((int)AdaptiveMessageFieldID.APIToken);
        }

        /// <summary>
        /// Indica si tiene el campo de HashRules activo.
        /// </summary>
        /// <param name="message">Mensaje adaptivo.</param>
        /// <returns>Un valor true si tiene el campo activo.</returns>
        public static Boolean HasHashRules(this IAdaptiveMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return message.IsSet((int)AdaptiveMessageFieldID.HashRules);
        }

        /// <summary>
        /// Obtiene el token de aplicación.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <returns>Token de aplicación.</returns>
        public static byte[] GetAPIToken(this IAdaptiveMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return message.GetBytes((int)AdaptiveMessageFieldID.APIToken);
        }

        /// <summary>
        /// Obtiene el hash de las reglas de mensajes.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <returns>Hash de las reglas de mensajes.</returns>
        public static byte[] GetHashRules(this IAdaptiveMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return message.GetBytes((int)AdaptiveMessageFieldID.HashRules);
        }
    }
}