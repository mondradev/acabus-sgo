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

    /// <summary>
    /// Define la estructura de una devolución de dinero.
    /// </summary>
    [Entity(TableName = "RefundOfMoney")]
    public sealed class RefundOfMoney : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'CashDestiny'.
        /// </summary>
        private CashDestiny _cashDestiny;

        /// <summary>
        /// Campo que provee a la propiedad 'ID'.
        /// </summary>
        private UInt64 _id;

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
            if (incidence.Device?.Type != DeviceType.KVR)
                throw new ArgumentException("La incidencia debe pertenecer a un Kvr.");

            _incidence = incidence;
        }

        /// <summary>
        /// Crea una instancia básica de <see cref="RefundOfMoney"/>.
        /// </summary>
        public RefundOfMoney() { }

        /// <summary>
        /// Obtiene o establece el destino del dinero.
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_CashDestiny_ID")]
        public CashDestiny CashDestiny {
            get => _cashDestiny;
            set {
                _cashDestiny = value;
                OnPropertyChanged("CashDestiny");
            }
        }

        /// <summary>
        /// Obtiene o establece el identificador de la devolución.
        /// </summary>
        [Column(IsPrimaryKey = true, IsAutonumerical = true)]
        public UInt64 ID {
            get => _id;
            set {
                _id = value;
                OnPropertyChanged(nameof(ID));
            }
        }

        /// <summary>
        /// Obtiene o establece la incidencia a la que corresponde la devolución.
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_Folio")]
        public Incidence Incidence {
            get => _incidence;
            set {
                if (value is null || value.Device is null || value.Device.Type == DeviceType.KVR)
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