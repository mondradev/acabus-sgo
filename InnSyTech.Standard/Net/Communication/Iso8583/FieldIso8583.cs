///////////////////////////////////////////////////////////
//  FieldIso8583.cs
//  Implementation of the Class FieldIso8583
//  Generated by Enterprise Architect
//  Created on:      12-dic.-2017 06:17:16 p. m.
//  Original author: Javier de J. Flores Mondragón
///////////////////////////////////////////////////////////

namespace InnSyTech.Standard.Net.Communication.Iso8583
{
    public class FieldIso8583
    {
        private int _id;
        private object _value;

        /// <param name="id"></param>
        /// <param name="value"></param>
        public FieldIso8583(int id, object value)
        {
            _id = id;
            _value = value;
        }

        ~FieldIso8583()
        {
            _id = 0;
            _value = null;
        }

        public int ID
            => _id;

        public object Value
            => _value;

        ///
        /// <param name="size"></param>
        /// <param name="type"></param>
        public FieldIso8583 Decode(int size, FieldTypeIso8583 type)
        {
            return null;
        }

        ///
        /// <param name="size"></param>
        /// <param name="type"></param>
        public byte[] Encode(int size, FieldTypeIso8583 type)
        {
            return null;
        }
    }
}