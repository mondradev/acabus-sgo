using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using InnSyTech.Standard.Mvvm;
using InnSyTech.Standard.Mvvm.Converters;
using InnSyTech.Standard.Translates;
using System;
using System.Collections.Generic;

namespace Opera.Acabus.Cctv.Models
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
    /// Provee de funciones extra a la enumeración <see cref="CashType"/>.
    /// </summary>
    public static class CashTypeExtesion
    {
        /// <summary>
        /// Traduce el valor de la enumeración al idioma español.
        /// </summary>
        /// <param name="cashType">Enumeración a traducir.</param>
        /// <returns>Una cadena que representa al objeto en español.</returns>
        public static String Translate(this CashType cashType)
            => new CashTypeTraslator().Translate(cashType);
    }

    /// <summary>
    /// Define la estructura de un destino de devolución de dinero.
    /// </summary>
    [Entity(TableName = "CashDestinies")]
    public class CashDestiny : NotifyPropertyChanged, IComparable, IComparable<CashDestiny>
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="CashType"/>
        /// </summary>
        private CashType _cashType;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Description"/>
        /// </summary>
        private String _description;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ID"/>
        /// </summary>
        private Int64 _id;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="RequiresMoving" />.
        /// </summary>
        private bool _requiresMoving;

        /// <summary>
        /// Obtiene o establece el tipo de dinero valido para este destino.
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<CashType>))]
        public CashType CashType {
            get => _cashType;
            private set {
                _cashType = value;
                OnPropertyChanged(nameof(CashType));
            }
        }

        /// <summary>
        /// Obtiene o establece la descripción del destino de dinero.
        /// </summary>
        public String Description {
            get => _description;
            set {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        /// <summary>
        /// Obtiene o establece el identificador unico del destino de dinero.
        /// </summary>
        [Column(IsPrimaryKey = true, IsAutonumerical = true)]
        public Int64 ID {
            get => _id;
            private set {
                _id = value;
                OnPropertyChanged(nameof(ID));
            }
        }

        /// <summary>
        /// Obtiene o establece si el destino requiere ser desplazado a otra ubicación.
        /// </summary>
        public bool RequiresMoving {
            get => _requiresMoving;
            set {
                _requiresMoving = value;
                OnPropertyChanged(nameof(RequiresMoving));
            }
        }

        /// <summary>
        /// Compara dos instancias de <see cref="CashDestiny"/> y determina si son diferentes.
        /// </summary>
        /// <param name="left">Un destino de dinero a comparar.</param>
        /// <param name="right">Otro destino de dinero a comparar.</param>
        /// <returns>Un valor true si los destinos de dinero son diferentes.</returns>
        public static bool operator !=(CashDestiny left, CashDestiny right)
        {
            if (left is null && right is null) return false;
            if (left is null || right is null) return true;

            return !left.Equals(right);
        }

        /// <summary>
        /// Compara dos instancias de <see cref="CashDestiny"/> y determina si son iguales.
        /// </summary>
        /// <param name="left">Un destino de dinero a comparar.</param>
        /// <param name="right">Otro destino de dinero a comparar.</param>
        /// <returns>Un valor true si los destinos de dinero son iguales.</returns>
        public static bool operator ==(CashDestiny left, CashDestiny right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;

            return left.Equals(right);
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

            if (obj.GetType() != typeof(CashDestiny)) return -1;

            return CompareTo(obj as CashDestiny);
        }

        /// <summary>
        /// Compara la instancia actual con otra y devuelve un entero que indica si la posición de la
        /// instancia actual es anterior, posterior o igual que la del otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="other">Objeto que se va a comparar con esta instancia.</param>
        /// <returns>Un valor que indica el orden relativo de los objetos que se están comparando.</returns>
        public int CompareTo(CashDestiny other)
        {
            if (other == null) return -1;

            if (Description == other.Description)
                return CashType.CompareTo(other.CashType);

            return Description.CompareTo(other.Description);
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

            CashDestiny anotherInstance = (CashDestiny)obj;

            return CompareTo(anotherInstance) == 0;
        }

        /// <summary>
        /// Obtiene el código hash del objeto actual.
        /// </summary>
        /// <returns>Código hash de la instancia.</returns>
        public override int GetHashCode()
            => Tuple.Create(Description, CashType).GetHashCode();

        /// <summary>
        /// Representa la instancia actual en una cadena.
        /// </summary>
        /// <returns>Una cadena que representa la instancia.</returns>
        public override string ToString()
            => Description;
    }

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