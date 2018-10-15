using InnSyTech.Standard.Utils;
using System;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages
{
    /// <summary>
    /// Proporciona funciones para la conversión de los campos de los mensajes <see cref="IMessage"/>.
    /// </summary>
    public static class AdaptiveMsgExtension
    {
        /// <summary>
        /// Obtiene el valor Boolean del campo especificado.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <returns>El valor del campo.</returns>
        public static Boolean GetBoolean(this IMessage src, int id)
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
        public static Byte[] GetBytes(this IMessage src, int id)
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
        public static DateTime GetDateTime(this IMessage src, int id)
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
        /// Obtiene el valor Int16 del campo especificado.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <returns>El valor del campo.</returns>
        public static Int16 GetInt16(this IMessage src, int id)
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
        public static Int32 GetInt32(this IMessage src, int id)
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
        public static Int64 GetInt64(this IMessage src, int id)
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
        public static String GetString(this IMessage src, int id)
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
        public static TimeSpan GetTimeSpan(this IMessage src, int id)
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
        public static UInt16 GetUInt16(this IMessage src, int id)
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
        public static UInt32 GetUInt32(this IMessage src, int id)
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
        public static UInt64 GetUInt64(this IMessage src, int id)
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
        public static void SetBoolean(this IMessage src, int id, Boolean value)
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
        public static void SetDateTime(this IMessage src, int id, DateTime value)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            String dateTimeText = String.Format("{0}{1}{2}{3}{4}{5}", value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second);

            src[id] = dateTimeText;
        }

        /// <summary>
        /// Establece el valor del campo TimeSpan especificado. El campo deberá ser del tipo <see cref="FieldDefinition.FieldType.Numeric"/>.
        /// </summary>
        /// <param name="src">Mensaje origen.</param>
        /// <param name="id">Identificador del campo.</param>
        /// <param name="value">Valor del campo.</param>
        public static void SetTimeSpan(this IMessage src, int id, TimeSpan value)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            String timeSpanText = String.Format("{0}{1}{2}", value.Hours, value.Minutes, value.Seconds);

            src[id] = timeSpanText;
        }
    }
}