using InnSyTech.Standard.Database;
using System;

namespace Acabus.Modules.CctvReports.Models
{
    /// <summary>
    /// Define los tipos de dinero posible.
    /// </summary>
    public enum CashType
    {
        /// <summary>
        /// Monedas.
        /// </summary>
        MONEY,

        /// <summary>
        /// Billetes.
        /// </summary>
        BILL
    }

    /// <summary>
    /// Define la estructura de un destino de devolución de dinero.
    /// </summary>
    public class CashDestiny
    {
        /// <summary>
        /// Obtiene o establece la descripción de la devolución de dinero.
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// Obtiene o establece el tipo de dinero valido para este destino.
        /// </summary>
        public CashType Type { get; set; }

        /// <summary>
        /// Representa la instancia actual en una cadena.
        /// </summary>
        /// <returns>Una cadena que representa la instancia.</returns>
        public override string ToString() => Description;
    }

    /// <summary>
    /// Define el convertidor de una instancia <see cref="CashDestiny"/> a una cadena valida que pueda ser almacedena en una base de datos.
    /// </summary>
    public sealed class CashDestinyConverter : IDbConverter
    {
        /// <summary>
        /// Obtiene un campo de la base de datos convertido a una instancia valida de <see cref="CashDestiny"/>.
        /// </summary>
        /// <param name="data">Valor obtenido de la base de datos.</param>
        /// <returns>Una instancia <see cref="CashDestiny"/>.</returns>
        public object ConverterFromDb(object data)
        {
            if (data is String)
                return new CashDestiny() { Description = data.ToString() };
            throw new ArgumentException(@"El tipo 'data' debe ser System.String para la conversión a
                                                        Acabus.Modules.CctvReports.Models.CashDestiny");
        }

        /// <summary>
        /// Obtiene el valor de una instancia <see cref="CashDestiny"/> para traducirlo a un valor
        /// valido en la base de datos.
        /// </summary>
        /// <param name="property">Instancia de <see cref="CashDestiny"/>.</param>
        /// <returns>El valor para guardar en la base de datos.</returns>
        public object ConverterToDbData(object property)
        {
            if (property is CashDestiny)
                return (property as CashDestiny).Description;
            throw new ArgumentException(@"El tipo 'property' debe ser Acabus.Modules.CctvReports.Models.CashDestiny
                                                                    para realizar la convesión a System.String");
        }
    }
}