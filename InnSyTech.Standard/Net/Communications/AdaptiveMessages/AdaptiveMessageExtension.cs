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
        /// Obtiene el valor Boolean del campo especificado.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <returns>El valor del campo.</returns>
        public static Boolean GetBoolean(this IAdaptiveMessage src, int id)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            if (!src.IsSet(id))
                return default;

            if (src.TryGetValue(id, out Boolean value, x => BitConverter.ToBoolean(x as byte[], 0)))
                return value;

            throw new FormatException("No se logró realizar la conversión a Boolean");
        }

        /// <summary>
        /// Obtiene el valor del campo Byte especificado. El campo deberá ser del tipo <see cref="FieldType.Binary"/>.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <return>Valor del campo.</return>
        public static Byte[] GetBytes(this IAdaptiveMessage src, int id)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            if (!src.IsSet(id))
                return default;

            return src[id] as byte[];
        }

        /// <summary>
        /// Obtiene la cantidad de elementos de la enumeración.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <returns>Cantidad de elementos de la colección.</returns>
        public static int GetCount(this IAdaptiveMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return message.GetInt32((int)AdaptiveMessageFieldID.Count);
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

            if (!src.IsSet(id))
                return default;

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

            if (!src.IsSet(id))
                return default;

            if (src.TryGetValue(id, out T value, x => (T)Enum.Parse(typeof(T), x.ToString(), true)))
                return value;

            throw new FormatException("No se logró realizar la conversión a " + typeof(T).Name);
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
        /// Obtiene el valor Int16 del campo especificado.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <returns>El valor del campo.</returns>
        public static Int16 GetInt16(this IAdaptiveMessage src, int id)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            if (!src.IsSet(id))
                return default;

            if (src.TryGetValue(id, out short value, x => ConvertTo(x, 2, BitConverter.ToInt16)))
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

            if (!src.IsSet(id))
                return default;

            if (src.TryGetValue(id, out int value, x => ConvertTo(x, 4, BitConverter.ToInt32)))
                return value;

            throw new FormatException("No se logró realizar la conversión a Int32");
        }

        /// <summary>
        /// Obtiene el valor Int64 del campo especificado.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <returns>El valor del campo.</returns>
        public static long GetInt64(this IAdaptiveMessage src, int id)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            if (!src.IsSet(id))
                return default;

            if (src.TryGetValue(id, out long value, x => ConvertTo(x, 8, BitConverter.ToInt64)))
                return value;

            throw new FormatException("No se logró realizar la conversión a Int64");
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

            return message.GetInt32((int)AdaptiveMessageFieldID.Position);
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

            if (!message.IsSet((int)AdaptiveMessageFieldID.ResponseCode))
                return AdaptiveMessageResponseCode.SERVICE_UNAVAILABLE;

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

            if (!message.IsSet((int)AdaptiveMessageFieldID.ResponseMessage))
                return "(Servicio no disponible)";

            return message.GetString((int)AdaptiveMessageFieldID.ResponseMessage);
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

            if (!src.IsSet(id))
                return default;

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

            if (!src.IsSet(id))
                return default;

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

            if (!src.IsSet(id))
                return default;

            if (src.TryGetValue(id, out UInt16 value, x => ConvertTo(x, 2, BitConverter.ToUInt16)))
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

            if (!src.IsSet(id))
                return default;

            if (src.TryGetValue(id, out UInt32 value, x => ConvertTo(x, 4, BitConverter.ToUInt32)))
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

            if (!src.IsSet(id))
                return default;

            if (src.TryGetValue(id, out UInt64 value, x => ConvertTo(x, 8, BitConverter.ToUInt64)))
                return value;

            throw new FormatException("No se logró realizar la conversión a UInt64");
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
        /// Obtiene si el mensaje contiene una colección.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <returns>Un valor true si es enumerable.</returns>
        public static Boolean IsEnumerable(this IAdaptiveMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return message.IsSet((int)AdaptiveMessageFieldID.ResponseCode)
                && message.GetResponseCode() == AdaptiveMessageResponseCode.PARTIAL_CONTENT;
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
        /// Establece el mensaje como enumerable y se posiciona al comienzo de la enumeración.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <param name="count">Cantidad de elementos de la colección.</param>
        public static void SetAsEnumerable(this IAdaptiveMessage message, int count)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            message[(int)AdaptiveMessageFieldID.ResponseCode] = AdaptiveMessageResponseCode.PARTIAL_CONTENT;
            message[(int)AdaptiveMessageFieldID.Count] = count;
            message[(int)AdaptiveMessageFieldID.Position] = -1;
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

            src[id] = ulong.Parse(dateTimeText);
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
        /// Establece la posición actual de la colección.
        /// </summary>
        /// <param name="message">Mensaje adaptativo.</param>
        /// <param name="position">Posición de la colección.</param>
        public static void SetPosition(this IAdaptiveMessage message, int position)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (message.IsEnumerable())
                message[(int)AdaptiveMessageFieldID.Position] = position;
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

            message[(int)AdaptiveMessageFieldID.ResponseCode] = code;

            if (String.IsNullOrEmpty(response))
                message.Remove((int)AdaptiveMessageFieldID.ResponseMessage);
            else
                message[(int)AdaptiveMessageFieldID.ResponseMessage] = response;
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
        /// Realiza la conversión de valor numéricos.
        /// </summary>
        /// <typeparam name="T">Tipo de valor destino.</typeparam>
        /// <param name="value">Valor numérico a convertir.</param>
        /// <param name="memSize">Tamaño de la memoria del tipo de dato.</param>
        /// <param name="bitConverterFn">Función convertidora.</param>
        private static T ConvertTo<T>(object value, int memSize, Func<byte[], int, T> bitConverterFn)
        {
            if (!value.IsInteger())
                throw new ArgumentException("El valor a convertir debe ser del tipo numérico");

            byte[] buffer;

            if (Type.GetTypeCode(value.GetType()) == TypeCode.UInt64)
                buffer = BitConverter.GetBytes(Convert.ToUInt64(value));
            else
                buffer = BitConverter.GetBytes(Convert.ToInt64(value));

            return bitConverterFn(buffer.PadRight(memSize), 0);
        }
    }
}