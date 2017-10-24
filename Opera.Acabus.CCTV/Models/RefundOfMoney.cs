using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.Models;
using System;

namespace Opera.Acabus.Cctv.Models
{
    /// <summary>
    /// Define los estados de una devolución de dinero.
    /// </summary>
    public enum RefundStatus
    {
        FOR_DELIVERY,
        DELIVERED
    }

    /// <summary>
    /// Define la estructura de una devolución de dinero.
    /// </summary>
    [Entity(TableName = "RefundOfMoney")]
    public sealed class RefundOfMoney : NotifyPropertyChanged, IComparable, IComparable<RefundOfMoney>
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="CashDestiny"/>
        /// </summary>
        private CashDestiny _cashDestiny;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ID"/>
        /// </summary>
        private UInt64 _id;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Incidence"/>
        /// </summary>
        private Incidence _incidence;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Quantity"/>
        /// </summary>
        private Single _quantity;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="RefundDate"/>
        /// </summary>
        private DateTime? _refundDate;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Status"/>
        /// </summary>
        private RefundStatus _status;

        /// <summary>
        /// Crea una instancia de <see cref="RefundOfMoney"/> indicando su asociación a una <see cref="Incidence"/>.
        /// </summary>
        /// <param name="incidence">
        /// Instancia de <see cref="Incidence"/> a la que está asociada.
        /// </param>
        public RefundOfMoney(Incidence incidence) : this(0, incidence) { }

        /// <summary>
        /// Crea una instancia básica de <see cref="RefundOfMoney"/>.
        /// </summary>
        public RefundOfMoney() : this(null) { }

        /// <summary>
        /// Crea una instancia persistida de <see cref="RefundOfMoney"/> indicando su <see cref="Incidence"/> asociada.
        /// </summary>
        /// <param name="id">Identificador único de la devolución de dinero.</param>
        /// <param name="incidence">Incidencia asociada a la devolución.</param>
        public RefundOfMoney(UInt64 id, Incidence incidence)
        {
            if (incidence != null && incidence.Device?.Type != DeviceType.KVR)
                throw new ArgumentException("La incidencia debe pertenecer a un Kiosko.");

            _incidence = incidence;
            _id = id;
        }

        /// <summary>
        /// Obtiene o establece el destino del dinero.
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_CashDestiny_ID")]
        public CashDestiny CashDestiny {
            get => _cashDestiny;
            set {
                _cashDestiny = value;
                OnPropertyChanged(nameof(CashDestiny));
            }
        }

        /// <summary>
        /// Obtiene o establece el identificador de la devolución.
        /// </summary>
        [Column(IsPrimaryKey = true, IsAutonumerical = true)]
        public UInt64 ID {
            get => _id;
            private set {
                _id = value;
                OnPropertyChanged(nameof(ID));
            }
        }

        /// <summary>
        /// Obtiene o establece la incidencia a la que corresponde la devolución.
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_Incidence_Folio")]
        public Incidence Incidence {
            get => _incidence;
            private set {
                _incidence = value;
                OnPropertyChanged(nameof(Incidence));
            }
        }

        /// <summary>
        /// Obtiene o establece la cantidad de la devolución.
        /// </summary>
        public Single Quantity {
            get => _quantity;
            set {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        /// <summary>
        /// Obtiene o establece la fecha/hora de la devolución del dinero.
        /// </summary>
        public DateTime? RefundDate {
            get => _refundDate;
            set {
                _refundDate = value;
                OnPropertyChanged(nameof(RefundDate));
            }
        }

        /// <summary>
        /// Obtiene o establece el estado de la devolución de dinero.
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<RefundStatus>))]
        public RefundStatus Status {
            get => _status;
            set {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        /// Compara dos instancias de <see cref="RefundOfMoney"/> y determina si son diferentes.
        /// </summary>
        /// <param name="left">Una devolución de dinero a comparar.</param>
        /// <param name="right">Otra devolución de dinero a comparar.</param>
        /// <returns>Un valor true si las devoluciones de dinero son diferentes.</returns>
        public static bool operator !=(RefundOfMoney left, RefundOfMoney right)
        {
            if (left is null && right is null) return false;
            if (left is null || right is null) return true;

            return !left.Equals(right);
        }

        /// <summary>
        /// Compara dos instancias de <see cref="RefundOfMoney"/> y determina si son iguales.
        /// </summary>
        /// <param name="left">Una devolución de dinero a comparar.</param>
        /// <param name="right">Otra devolución de dinero a comparar.</param>
        /// <returns>Un valor true si las devoluciones de dinero son iguales.</returns>
        public static bool operator ==(RefundOfMoney left, RefundOfMoney right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Compara la instancia actual con otra y devuelve un entero que indica si la posición de la
        /// instancia actual es anterior, posterior o igual que la del otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="other">Objeto que se va a comparar con esta instancia.</param>
        /// <returns>Un valor que indica el orden relativo de los objetos que se están comparando.</returns>
        public int CompareTo(RefundOfMoney other)
        {
            if (other == null) return -1;

            if (RefundDate == other.RefundDate)
                if (Incidence == other.Incidence)
                    if (CashDestiny == other.CashDestiny)
                        return Quantity.CompareTo(other.Quantity);
                    else
                        return CashDestiny.CompareTo(other.CashDestiny);
                else
                    return Incidence.CompareTo(other.Incidence);

            return RefundDate.Value.CompareTo(other.RefundDate.Value);
        }

        /// <summary>
        /// Compara la instancia actual con otro objeto del mismo tipo y devuelve un entero que
        /// indica si la posición de la instancia actual es anterior, posterior o igual que la del
        /// otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="obj">Objeto que se va a comparar con esta instancia.</param>
        /// <returns>Un valor que indica el orden relativo de los objetos que se están comparando.</returns>
        public int CompareTo(object obj)
        {
            if (obj == null) return -1;

            if (obj.GetType() != typeof(RefundOfMoney)) return -1;

            return CompareTo(obj as RefundOfMoney);
        }

        /// <summary>
        /// Determina si la instancia actual es igual al objeto especificado.
        /// </summary>
        /// <param name="obj">Otro objeto a comparar.</param>
        /// <returns>Un valor true si los objetos son iguales.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            RefundOfMoney anotherInstance = (RefundOfMoney)obj;

            return CompareTo(anotherInstance) == 0;
        }

        /// <summary>
        /// Obtiene el código hash del objeto actual.
        /// </summary>
        /// <returns>Código hash de la instancia.</returns>
        public override int GetHashCode()
            => Tuple.Create(RefundDate, Incidence, CashDestiny, Quantity).GetHashCode();

        /// <summary>
        /// Representa la instancia actual en una cadena.
        /// </summary>
        /// <returns>Una cadena que representa la instancia.</returns>
        public override string ToString()
            => String.Format("Devolución a {0} de $ {1:d2} en {2}", CashDestiny?.ToString() ?? "(sin definir)", Quantity, CashDestiny?.CashType.Translate() ?? "(sin definir)");
    }
}