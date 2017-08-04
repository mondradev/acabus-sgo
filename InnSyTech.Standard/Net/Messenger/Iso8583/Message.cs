using InnSyTech.Standard.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace InnSyTech.Standard.Net.Messenger.Iso8583
{
    public enum FieldType
    {
        ALPHANUMERIC,
        NUMERIC,
        LVAR,
        LLVAR,
        LLLVAR,
        LVBINARY,
        LLVBINARY,
        LLLVBINARY
    }

    public sealed class Message : IDisposable
    {
        private static XmlDocument _template;

        private List<Field> _fields;

        private bool _isDisposed;

        public Message()
        {
            _fields = new List<Field>();
        }

        public static Message Parse(String iso8583str)
        {
            if (_template is null)
                throw new InvalidOperationException("No hay una plantilla definida para la creación de la cadena ISO8583, utilice la función Message.SetTemplate");

            return null;
        }

        public static void SetTemplate(XmlDocument xmlDoc)
            => _template = xmlDoc;

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

        public void Dispose()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(typeof(Message).FullName);

            _fields.Clear();
            _isDisposed = true;
        }

        public Object GetField(UInt16 id)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(typeof(Message).FullName);

            if (id == 0)
                throw new ArgumentException("El identificador del campo no puede ser 0.");

            return _fields.FirstOrDefault(field => field.ID == id).Value;
        }

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

        public override string ToString()
            => CreateIsoString();

        private string CreateIsoString()
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
                byte value = 0;
                for (int i = 0; i < @byte.Length; i++)
                    value += (byte)(int.Parse(@byte[i].ToString()) * Math.Pow(2, i));
                iso8583Str.AppendFormat("{0:D2}", value);
            }

            foreach (var field in _fields)
                iso8583Str.Append(field.Encode(_template));

            return iso8583Str.ToString();
        }
    }

    internal class Field
    {
        private ushort _id;

        private object _value;

        public Field(ushort id, object value)
        {
            _id = id;
            _value = value;
        }

        public ushort ID => _id;

        public object Value => _value;

        public String Encode(XmlDocument template)
        {
            try
            {
                var node = template.SelectSingleNode("Template").SelectSingleNode("Fields");
                var fieldNode = node.ChildNodes.Cast<XmlNode>()
                    .FirstOrDefault(xmlNode => xmlNode.Attributes.GetNamedItem("ID")?.Value == ID.ToString());
                var type = (FieldType)Enum.Parse(typeof(FieldType), fieldNode.Attributes.GetNamedItem("Type")?.Value);
                var length = (UInt32)0;
                if (type == FieldType.ALPHANUMERIC || type == FieldType.NUMERIC)
                    length = UInt32.Parse(fieldNode.Attributes.GetNamedItem("Length").Value);
                return EncodeInternal(Value, type, (Int32)length);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("La plantilla no se puede aplicar para la codificación del mensaje.");
            }
        }

        private static string ArrayToString<T>(IEnumerable<T> padded)
        {
            StringBuilder str = new StringBuilder();
            foreach (var item in padded)
                str.Append(item);
            return str.ToString();
        }

        private static string EncodeInternal(Object value, FieldType type, Int32 length)
        {
            String encoded;
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
                    return EncodeLengthVar((byte[])value, length, 9);

                case FieldType.LLVBINARY:
                    return EncodeLengthVar((byte[])value, length, 99);

                case FieldType.LLLVBINARY:
                    return EncodeLengthVar((byte[])value, length, 999);
            }
            throw new InvalidOperationException();
        }

        private static String EncodeLengthVar(byte[] value, int length, int maxLength)
        {
            if (value.Length > maxLength)
                Array.Resize(ref value, maxLength);

            return ArrayToString(value);
        }

    }
}