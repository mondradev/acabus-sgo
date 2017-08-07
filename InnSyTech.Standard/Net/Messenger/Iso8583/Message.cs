﻿using InnSyTech.Standard.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace InnSyTech.Standard.Net.Messenger.Iso8583
{
    /// <summary>
    /// Define los tipos de campos de un mensaje ISO8583.
    /// </summary>
    internal enum FieldType
    {
        /// <summary>
        /// Alfanumérico.
        /// </summary>
        ALPHANUMERIC,

        /// <summary>
        /// Numérico.
        /// </summary>
        NUMERIC,

        /// <summary>
        /// Longitud variable de máximo 9 caracteres.
        /// </summary>
        LVAR,

        /// <summary>
        /// Longitud variable de máximo 99 caracteres.
        /// </summary>
        LLVAR,

        /// <summary>
        /// Longitud variable de máximo 999 caracteres.
        /// </summary>
        LLLVAR,

        /// <summary>
        /// Longitud variable de máximo 9 bytes.
        /// </summary>
        LVBINARY,

        /// <summary>
        /// Longitud variable de máximo 99 bytes.
        /// </summary>
        LLVBINARY,

        /// <summary>
        /// Longitud variable de máximo 999 bytes.
        /// </summary>
        LLLVBINARY
    }

    /// <summary>
    /// Define la estructura de un mensaje ISO8583 para la comunicación de equipos remotos. Para su utilización requiere de una plantilla que indique el tipo de cada campo que se desea implementar en los mensajes.
    /// </summary>
    public sealed class Message : IDisposable
    {
        /// <summary>
        /// Plantilla de campos para el codificado y decodificado de mensajes.
        /// </summary>
        private static XmlDocument _template;

        /// <summary>
        /// Lista de campos agregados al mensaje.
        /// </summary>
        private List<Field> _fields;

        /// <summary>
        /// Indica si el mensaje ha sido desechado.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Crea un nuevo mensaje.
        /// </summary>
        public Message()
        {
            _fields = new List<Field>();
        }

        /// <summary>
        /// Convierte una cadena que representa un mensaje según la plantilla utilizada y devuelve una instancia <see cref="Message"/>.
        /// </summary>
        /// <param name="iso8583str">Cadena de un mensaje ISO8583.</param>
        /// <returns>Una instancia <see cref="Message"/>.</returns>
        /// <exception cref="InvalidOperationException">No se ha establecido una plantilla.</exception>
        public static Message Parse(String iso8583str)
        {
            if (_template is null)
                throw new InvalidOperationException("No hay una plantilla definida para la creación de la cadena ISO8583, utilice la función Message.SetTemplate");

            var header = iso8583str.Substring(0, 16);
            var bytes = header.Cut(8);
            var fields = new List<int>();
            var aux = 1;
            foreach (var item in bytes)
            {
                var @byte = Convert.ToByte(item, 16);
                string bin = Convert.ToString(@byte, 2).PadLeft(8, '0');
                for (int i = 0; i < bin.Length; i++)
                    if (bin[i] == '1')
                        fields.Add(i + aux);
                aux += 8;
            }
            Message message = new Message();
            var body = iso8583str.Substring(16);
            foreach (var field in fields)
            {
                var value = Decode(field, body, out int lenght);
                message.AddField((UInt16)field, value);
                body = body.Substring(lenght);
            }

            return message;
        }

        /// <summary>
        /// Establece la plantilla que se usará en la aplicación.
        /// </summary>
        /// <param name="xmlDoc">Plantilla de campos para el mensaje.</param>
        public static void SetTemplate(XmlDocument xmlDoc)
            => _template = xmlDoc;

        /// <summary>
        /// Añade un campo al mensaje.
        /// </summary>
        /// <param name="id">Identificador del campo, solo se puede agregar una sola vez.</param>
        /// <param name="value">Valor del campo.</param>
        /// <exception cref="ObjectDisposedException">Mensaje desechado.</exception>
        /// <exception cref="ArgumentException">
        /// Identificador no puede ser 0 o previamente agregado.
        /// </exception>
        public void AddField(UInt16 id, Object value)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(typeof(Message).FullName);

            if (id == 0)
                throw new ArgumentException("El identificador del campo no puede ser 0.");

            if (_fields.Where(field => field.ID == id).Count() > 0)
                throw new ArgumentException($"El campo ya fue agregado al mensaje --> {id}");

            _fields.Add(new Field(id, value));
        }

        /// <summary>
        /// Desecha el mensaje actual.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Si la instancia ya fue previamente desechada.</exception>
        public void Dispose()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(typeof(Message).FullName);

            _fields.Clear();
            _isDisposed = true;
        }

        /// <summary>
        /// Obtiene el valor de un campo especificado del mensaje.
        /// </summary>
        /// <param name="id">Identificador del campo.</param>
        /// <returns>El valor del campo especificado.</returns>
        /// <exception cref="ObjectDisposedException">Mensaje ya ha sido desechado.</exception>
        /// <exception cref="ArgumentException">El identificador del campo es 0.</exception>
        public Object GetField(UInt16 id)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(typeof(Message).FullName);

            if (id == 0)
                throw new ArgumentException("El identificador del campo no puede ser 0.");

            return _fields.FirstOrDefault(field => field.ID == id).Value;
        }

        /// <summary>
        /// Remueve del mensaje el campo especificado.
        /// </summary>
        /// <param name="id">Identificador del campo del mensaje.</param>
        /// <returns>Un true en caso de remover el campo.</returns>
        /// <exception cref="ObjectDisposedException">Mensaje desechado previamente.</exception>
        /// <exception cref="ArgumentException">Identificador no puede ser 0.</exception>
        public bool RemoveField(Int16 id)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(typeof(Message).FullName);

            if (id == 0)
                throw new ArgumentException("El identificador del campo no puede ser 0.");

            var fieldToRemove = _fields.FirstOrDefault(field => field.ID == id);

            if (fieldToRemove != null)
                return _fields.Remove(fieldToRemove);
            return false;
        }

        /// <summary>
        /// Obtiene un vector de bytes utilizados para el envío de información a travéz de sockets TCP/IP.
        /// </summary>
        /// <returns>Un vector de bytes.</returns>
        public byte[] ToBytes()
            => Encoding.UTF8.GetBytes(ToString());

        /// <summary>
        /// Crea una cadena que representa el mensaje codificado en ISO8583.
        /// </summary>
        /// <returns>El mensaje codificado en ISO8583.</returns>
        public override string ToString()
            => Encode();

        /// <summary>
        /// Decodifica el campo especificado a partir del cuerpo del mensaje en texto.
        /// </summary>
        /// <param name="field">El id del campo a decodificar.</param>
        /// <param name="body">El cuerpo del mensaje, iniciando del campo a decodificar.</param>
        /// <param name="length">Longitud total del campo decodificado.</param>
        /// <returns>Valor del campo decodificado.</returns>
        /// <exception cref="InvalidOperationException">La plantilla no se puede aplicar al mensaje.</exception>
        private static object Decode(int field, string body, out int length)
        {
            GetAttributes(_template, field, out FieldType fieldType, out int lengthField);
            switch (fieldType)
            {
                case FieldType.ALPHANUMERIC:
                    length = lengthField;
                    return body.Substring(0, lengthField).Trim();

                case FieldType.NUMERIC:
                    length = lengthField;
                    var numeric = body.Substring(0, lengthField);
                    if (numeric.Contains('.'))
                        return Double.Parse(numeric);
                    return Int32.Parse(numeric);

                case FieldType.LVAR:
                    length = int.Parse(body.Substring(0, 1)) + 1;
                    return body.Substring(1, length - 1);

                case FieldType.LLVAR:
                    length = int.Parse(body.Substring(0, 2)) + 2;
                    return body.Substring(2, length - 2);

                case FieldType.LLLVAR:
                    length = int.Parse(body.Substring(0, 3)) + 3;
                    return body.Substring(3, length - 3);

                case FieldType.LVBINARY:
                    length = int.Parse(body.Substring(0, 1)) * 2 + 1;
                    return ToBytes(body.Substring(1, length - 1));

                case FieldType.LLVBINARY:
                    length = int.Parse(body.Substring(0, 2)) * 2 + 2;
                    return ToBytes(body.Substring(2, length - 2));

                case FieldType.LLLVBINARY:
                    length = int.Parse(body.Substring(0, 3)) * 2 + 3;
                    return ToBytes(body.Substring(3, length - 3));
            }
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Codifica el campo especificado.
        /// </summary>
        /// <param name="field">Identificador del campo.</param>
        /// <returns>Cadena que representa el campo codificado.</returns>
        /// <exception cref="InvalidOperationException">La plantilla no se puede aplicar al campo.</exception>
        private static String EncodeField(Field field)
        {
            GetAttributes(_template, field.ID, out FieldType type, out int length);
            String encoded;
            var value = field.Value;
            switch (type)
            {
                case FieldType.ALPHANUMERIC:
                    encoded = value.ToString().PadLeft(length, ' ');
                    return encoded.Substring(encoded.Length - length, length);

                case FieldType.NUMERIC:
                    encoded = value.ToString().PadLeft(length, '0');
                    return encoded.Substring(encoded.Length - length, length);

                case FieldType.LVAR:
                    if (value.ToString().Length > 9)
                        value = value.ToString().Substring(0, 9);
                    return String.Format("{0}{1}", value.ToString().Length, value);

                case FieldType.LLVAR:
                    if (value.ToString().Length > 99)
                        value = value.ToString().Substring(0, 99);
                    return String.Format("{0:D2}{1}", value.ToString().Length, value);

                case FieldType.LLLVAR:
                    if (value.ToString().Length > 999)
                        value = value.ToString().Substring(0, 999);
                    return String.Format("{0:D3}{1}", value.ToString().Length, value);

                case FieldType.LVBINARY:
                    return EncodeLengthVar((byte[])value, 9);

                case FieldType.LLVBINARY:
                    return EncodeLengthVar((byte[])value, 99);

                case FieldType.LLLVBINARY:
                    return EncodeLengthVar((byte[])value, 999);
            }
            throw new InvalidOperationException("La plantilla no se puede aplicar para la codificación del mensaje.");
        }

        /// <summary>
        /// Codifica un campo de longitud variable.
        /// </summary>
        /// <param name="value">Valor del campo.</param>
        /// <param name="maxLength">Longitud máxima del campo.</param>
        /// <returns>Una cadena que representa el campo codificado.</returns>
        private static String EncodeLengthVar(byte[] value, int maxLength)
        {
            if (value.Length > maxLength)
                Array.Resize(ref value, maxLength);

            var length = maxLength.ToString().Length;

            return String.Format($"{{0:D{length}}}{{1}}", value.Length, ToString(value));
        }

        /// <summary>
        /// Obtiene los atributos del campo a partir de la plantilla especificada.
        /// </summary>
        /// <param name="template">Plantilla de campos.</param>
        /// <param name="idField">Identificador del campo.</param>
        /// <param name="type">Tipo de campo.</param>
        /// <param name="length">Longitud del campo.</param>
        private static void GetAttributes(XmlDocument template, int idField, out FieldType type, out int length)
        {
            var node = template.GetElementsByTagName("Field");
            var fieldNode = node?.Cast<XmlNode>()
                ?.FirstOrDefault(xmlNode => xmlNode.Attributes.GetNamedItem("ID")?.Value == idField.ToString());
            type = (FieldType)Enum.Parse(typeof(FieldType), fieldNode.Attributes.GetNamedItem("Type")?.Value);
            length = 0;
            if (type == FieldType.ALPHANUMERIC || type == FieldType.NUMERIC)
                length = Int32.Parse(fieldNode.Attributes.GetNamedItem("Length").Value);
        }

        /// <summary>
        /// Convierte una cadena que contiene pares de caracteres que representan un byte de datos en un vector de Bytes. El valor máximo por par es FF.
        /// </summary>
        /// <param name="byteString">Cadena que contiene los pares de caracteres.</param>
        /// <returns>Un vector de bytes.</returns>
        private static byte[] ToBytes(string byteString)
        {
            var bytes = byteString.Cut(byteString.Length / 2);
            var byteList = new List<Byte>();
            foreach (var @byte in bytes)
                byteList.Add(Convert.ToByte(@byte, 16));
            return byteList.ToArray();
        }

        /// <summary>
        /// Convierte un vector de bytes en una cadena de caracteres que los representan en hexadecimal.
        /// </summary>
        /// <param name="bytes">Vector a convertir en cadena.</param>
        /// <returns>Una cadena que representa el vector en hexadecimal.</returns>
        private static String ToString(byte[] bytes)
        {
            var builder = new StringBuilder();
            foreach (var @byte in bytes)
                builder.Append(Convert.ToString(@byte, 16).PadLeft(2, '0'));
            return builder.ToString().ToUpper();
        }

        /// <summary>
        /// Codifica el mensaje actual a partir de una plantilla.
        /// </summary>
        /// <returns>Una cadena que representa el campo.</returns>
        /// <exception cref="InvalidOperationException">La plantilla no ha sido definida.</exception>
        private string Encode()
        {
            StringBuilder iso8583Str = new StringBuilder();

            if (_template is null)
                throw new InvalidOperationException("No hay una plantilla definida para la creación de la cadena ISO8583, utilice la función Message.SetTemplate");

            bool[] bitmap = new bool[64];

            foreach (var field in _fields)
                bitmap[field.ID - 1] = true;

            foreach (var bit in bitmap)
                iso8583Str.AppendFormat("{0}", bit ? 1 : 0);

            var bytes = iso8583Str.ToString().Cut(8);
            iso8583Str.Clear();

            foreach (var @byte in bytes)
            {
                int value = 0;
                for (int i = 0; i < @byte.Length; i++)
                    value += (int)(int.Parse(@byte[i].ToString()) * Math.Pow(2, @byte.Length - i - 1));
                iso8583Str.AppendFormat("{0:D2}", Convert.ToString(value, 16).PadLeft(2, '0'));
            }

            foreach (var field in _fields.OrderBy(f => f.ID))
                iso8583Str.Append(EncodeField(field));

            return iso8583Str.ToString();
        }
    }

    /// <summary>
    /// Define la estructura básica de una campo para un mensaje ISO8583.
    /// </summary>
    internal class Field
    {
        /// <summary>
        /// Identificador del campo.
        /// </summary>
        private ushort _id;

        /// <summary>
        /// Valor del campo.
        /// </summary>
        private object _value;

        /// <summary>
        /// Crea una instancia nueva de <see cref="Field"/>.
        /// </summary>
        /// <param name="id">Identificador del campo.</param>
        /// <param name="value">Valor del campo.</param>
        public Field(ushort id, object value)
        {
            _id = id;
            _value = value;
        }

        /// <summary>
        /// Obtiene el identificador del campo.
        /// </summary>
        public ushort ID => _id;

        /// <summary>
        /// Obtiene el valor del campo.
        /// </summary>
        public object Value => _value;
    }
}