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
        private static XmlDocument template;
        private List<Field> _fields;
        private bool _isDisposed;

        public static Message Parse(String iso8583str)
        {
            if (template is null)
                throw new InvalidOperationException("No hay una plantilla definida para la creación de la cadena ISO8583, utilice la función Message.SetTemplate");

            return null;
        }

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

        public void GetField(UInt16 id)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(typeof(Message).FullName);

            if (id == 0)
                throw new ArgumentException("El identificador del campo no puede ser 0.");

            _fields.FirstOrDefault(field => field.ID == id);
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

            if (template is null)
                throw new InvalidOperationException("No hay una plantilla definida para la creación de la cadena ISO8583, utilice la función Message.SetTemplate");

            bool[] bitmap = new bool[64];

            foreach (var field in _fields)
                bitmap[field.ID - 1] = true;

            foreach (var bit in bitmap)
                iso8583Str.AppendFormat("{0}", bit ? 1 : 0);

            foreach (var field in _fields)
                iso8583Str.Append(field.Encode(template));

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
            throw new NotImplementedException();
        }
    }
}