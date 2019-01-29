using InnSyTech.Standard.Translates;
using System.Collections.Generic;

namespace Opera.Acabus.Cctv.Models
{
    /// <summary>
    /// Representa un traductor al idioma español de la enumeración <see cref="CashType"/>.
    /// </summary>
    public sealed class CashTypeTraslator : EnumTranslator<CashType>
    {
        /// <summary>
        /// Crea una nueva instancia.
        /// </summary>
        public CashTypeTraslator() : base(new Dictionary<CashType, string>()
        {
            { CashType.BILL , "BILLETE" },
            { CashType.MONEY, "MONEDAS" }
        })
        { }
    }
}