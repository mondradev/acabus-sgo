using InnSyTech.Debug;
using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using System;

namespace Acabus.Modules.CctvReports.Models
{
    public enum RefundOfMoneyStatus
    {
        UNCOMMIT,
        COMMIT
    }

    public class RefundOfMoneyStatusConverter : DbEnumConverter<RefundOfMoneyStatus> { }

    public class CashDestinyConverter : IDbConverter
    {
        public object ConverterFromDb(object data)
        {
            if (data.GetType() != typeof(String))
                throw new ArgumentException("Valor incorrecto");

            return new CashDestiny()
            {
                Description = data.ToString(),
                Type = CashType.GENERAL
            };
        }

        public object ConverterToDbData(object property)
        {
            return ((CashDestiny)property).ToString();
        }
    }

    [Entity]
    public sealed class RefundOfMoney
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Quantity'.
        /// </summary>
        private Double _quantity;

        /// <summary>
        /// Obtiene o establece la cantidad de la devolución.
        /// </summary>
        public Double Quantity {
            get => _quantity;
            set {
                _quantity = value;
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'CashDestiny'.
        /// </summary>
        private CashDestiny _cashDestiny;

        /// <summary>
        /// Obtiene o establece el destino del dinero.
        /// </summary>
        [Column(Converter = typeof(CashDestinyConverter))]
        public CashDestiny CashDestiny {
            get => _cashDestiny;
            set {
                _cashDestiny = value;
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'Incidence'.
        /// </summary>
        private Incidence _incidence;

        /// <summary>
        /// Obtiene o establece la incidencia a la que corresponde la devolución.
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_Folio")]
        public Incidence Incidence {
            get => _incidence;
            set {
                _incidence = value;
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'Status'.
        /// </summary>
        private RefundOfMoneyStatus _status;

        /// <summary>
        /// Obtiene o establece el estado de la devolución de dinero.
        /// </summary>
        [Column(Converter = typeof(RefundOfMoneyStatusConverter))]
        public RefundOfMoneyStatus Status {
            get => _status;
            set {
                _status = value;
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'RefundDate'.
        /// </summary>
        private DateTime? _refundDate;

        /// <summary>
        /// Obtiene o establece la fecha/hora de la devolución del dinero.
        /// </summary>
        [Column(Converter = typeof(SQLiteDateTimeConverter))]
        public DateTime? RefundDate {
            get => _refundDate;
            set {
                _refundDate = value;
            }
        }

        [Column(IsPrimaryKey = true, IsAutonumerical = true)]
        public Int64 ID { get; private set; }

        public RefundOfMoney()
        {

        }

        public RefundOfMoney(Int64 id)
        {
            ID = id;
        }

        public RefundOfMoney(Incidence incidence)
        {

            _incidence = incidence;
        }
    }
}
