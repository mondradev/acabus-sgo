using InnSyTech.Standard.Mvvm.Converters;

namespace Opera.Acabus.Cctv.Models
{
    /// <summary>
    /// Representa un convertidor para la traducción al idioma español de la enumeración <see cref="CashType"/>.
    /// </summary>
    public sealed class CashTypeTraslatorConverter : TranslateEnumConverter<CashType>
    {
        /// <summary>
        /// Crea una nueva instancia.
        /// </summary>
        public CashTypeTraslatorConverter() : base(new CashTypeTraslator())
        { }
    }
}