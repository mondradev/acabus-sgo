using Acabus.Models;
using Acabus.Utils.Mvvm;
using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using System;

namespace Acabus.Modules.CctvReports.Models
{
    /// <summary>
    /// Define los estados de una devolución de dinero.
    /// </summary>
    public enum RefundOfMoneyStatus
    {
        UNCOMMIT,
        COMMIT
    }

    [Entity(TableName = "RefundOfMoney")]
    public sealed class RefundOfMoney : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'CashDestiny'.
        /// </summary>
        private CashDestiny _cashDestiny;

        /// <summary>
        /// Campo que provee a la propiedad 'Incidence'.
        /// </summary>
        private Incidence _incidence;

        /// <summary>
        /// Campo que provee a la propiedad 'Quantity'.
        /// </summary>
        private Single _quantity;

        /// <summary>
        /// Campo que provee a la propiedad 'RefundDate'.
        /// </summary>
        private DateTime? _refundDate;

        /// <summary>
        /// Campo que provee a la propiedad 'Status'.
        /// </summary>
        private RefundOfMoneyStatus _status;

        /// <summary>
        /// Crea una instancia de <see cref="RefundOfMoney"/> indicando su asociación a una <see cref="Acabus.Modules.CctvReports.Models.Incidence"/>.
        /// </summary>
        /// <param name="incidence">
        /// Instancia de <see cref="Acabus.Modules.CctvReports.Models.Incidence"/> a la que está asociada.
        /// </param>
        public RefundOfMoney(Incidence incidence)
        {
            if (!(incidence.Device is Kvr)) throw new ArgumentException("La incidencia debe pertenecer a un Kvr.");

            _incidence = incidence;
        }

        /// <summary>
        /// Crea una instancia básica de <see cref="RefundOfMoney"/>.
        /// </summary>
        public RefundOfMoney() { }

        /// <summary>
        /// Obtiene o establece el destino del dinero.
        /// </summary>
        [Column(Converter = typeof(CashDestinyConverter))]
        public CashDestiny CashDestiny {
            get => _cashDestiny ?? new CashDestiny();
            set {
                _cashDestiny = value;
                OnPropertyChanged("CashDestiny");
            }
        }

        /// <summary>
        /// Obtiene o establece el tipo de dinero que se manipula en la devolución.
        /// </summary>
        public CashType CashType {
            get => CashDestiny.Type;
            set {
                CashDestiny.Type = value;
                OnPropertyChanged("CashDestiny");
                OnPropertyChanged("CashType");
            }
        }

        /// <summary>
        /// Obtiene o establece la incidencia a la que corresponde la devolución.
        /// </summary>
        public Incidence Incidence {
            get => _incidence;
            set {
                if (value.Device is Kvr)
                {
                    _incidence = value;
                    OnPropertyChanged("Incidence");
                }
            }
        }

        /// <summary>
        /// Obtiene o establece la cantidad de la devolución.
        /// </summary>
        public Single Quantity {
            get => _quantity;
            set {
                _quantity = value;
                OnPropertyChanged("Quantity");
            }
        }

        /// <summary>
        /// Obtiene o establece la fecha/hora de la devolución del dinero.
        /// </summary>
        public DateTime? RefundDate {
            get => _refundDate;
            set {
                _refundDate = value;
                OnPropertyChanged("RefundDate");
            }
        }

        /// <summary>
        /// Obtiene o establece el estado de la devolución de dinero.
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<RefundOfMoneyStatus>))]
        public RefundOfMoneyStatus Status {
            get => _status;
            set {
                _status = value;
                OnPropertyChanged("Status");
            }
        }
    }
}