using Acabus.Models;
using Acabus.Utils.Mvvm;
using System;

namespace Acabus.Modules.CctvReports.Models
{
    public enum RefundOfMoneyStatus
    {
        UNCOMMIT,
        COMMIT
    }

    public sealed class RefundOfMoney : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Quantity'.
        /// </summary>
        private Single _quantity;

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
        /// Campo que provee a la propiedad 'CashDestiny'.
        /// </summary>
        private CashDestiny _cashDestiny;

        /// <summary>
        /// Obtiene o establece el destino del dinero.
        /// </summary>
        public CashDestiny CashDestiny {
            get => _cashDestiny;
            set {
                _cashDestiny = value;
                OnPropertyChanged("CashDestiny");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'Incidence'.
        /// </summary>
        private Incidence _incidence;

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
        /// Campo que provee a la propiedad 'Status'.
        /// </summary>
        private RefundOfMoneyStatus _status;

        /// <summary>
        /// Obtiene o establece el estado de la devolución de dinero.
        /// </summary>
        public RefundOfMoneyStatus Status {
            get => _status;
            set {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'RefundDate'.
        /// </summary>
        private DateTime? _refundDate;

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

        public RefundOfMoney(Incidence incidence)
        {
            if (!(incidence.Device is Kvr)) throw new ArgumentException("La incidencia debe pertenecer a un Kvr.");

            _incidence = incidence;
        }
    }
}
