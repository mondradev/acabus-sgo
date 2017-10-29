using InnSyTech.Standard.Mvvm.Converters;
using Opera.Acabus.Cctv.Models;
using System;
using System.Collections.Generic;
using System.Windows.Data;

namespace Opera.Acabus.Cctv.Converters
{
    /// <summary>
    /// Convertidor para la traducción de la enumeración <see cref="CashType" />.
    /// </summary>
    [ValueConversion(typeof(CashType), typeof(String))]
    public sealed class CashTypeSpanishConverter : TranslateEnumConverter<CashType>
    {
        /// <summary>
        /// Crea una instancia nueva del convertidor.
        /// </summary>
        public CashTypeSpanishConverter() : base(new Dictionary<CashType, string>()
        {
            { CashType.BILL , "BILLETE" },
            { CashType.MONEY, "MONEDAS" }
        })
        {
        }
    }
}