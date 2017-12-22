///////////////////////////////////////////////////////////
//  FieldTypeIso8583.cs
//  Implementation of the Enumeration FieldTypeIso8583
//  Generated by Enterprise Architect
//  Created on:      12-dic.-2017 06:17:16 p. m.
//  Original author: Javier de J. Flores Mondrag�n
///////////////////////////////////////////////////////////

namespace InnSyTech.Standard.Net.Communication.Iso8583
{
    /// <summary>
    /// Representa los tipos validos de formatos de los campos para el env�o de mensajes a trav�s del
    /// ISO 8583.
    /// </summary>
    public enum FieldFormat
    {
        /// <summary>
        /// Campo con formato hexadecimal.
        /// </summary>
        Hexadecimal = 1,

        /// <summary>
        /// Campo con formato decimal codificado en binario.
        /// </summary>
        BinaryCodedDecimal = 2
    }

    /// <summary>
    /// Representa los tipos de longitudes validas para los campos en un mensaje ISO 8583.
    /// </summary>
    public enum FieldLength
    {
        /// <summary>
        /// Campo con longitud fija.
        /// </summary>
        Fixed = 1,

        /// <summary>
        /// Campo con longitud variable [1..9].
        /// </summary>
        Lvar = 2,

        /// <summary>
        /// Campo con longitud variable [1..99].
        /// </summary>
        Llvar = 4,

        /// <summary>
        /// Campo con longitud variable [1..999].
        /// </summary>
        Lllvar = 8,
    }

    /// <summary>
    /// Representa los tipos de campos disponibles para el env�o de mensajes a trav�s del ISO 8583.
    /// </summary>
    public enum FieldType
    {
        /// <summary>
        /// Campo tipo n�merico.
        /// </summary>
        Numeric = 1,

        /// <summary>
        /// Campo tipo alfanum�rico.
        /// </summary>
        Alphanumeric = 2,

        /// <summary>
        /// Campo tipo caracteres especiales.
        /// </summary>
        Special = 4,

        /// <summary>
        /// Campo tipo binario.
        /// </summary>
        Binary = 8,
    }
}