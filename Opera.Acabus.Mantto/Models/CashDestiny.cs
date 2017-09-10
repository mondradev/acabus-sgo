using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using InnSyTech.Standard.Mvvm;
using InnSyTech.Standard.Mvvm.Utils;
using System;
using System.Collections.Generic;

namespace Opera.Acabus.Mantto.Models
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
    
    public sealed class CashTypeTraslatorConverter : TranslateEnumConverter<CashType>
    {
        public CashTypeTraslatorConverter() : base(new Dictionary<CashType, string>()
        {
            { CashType.BILL , "BILLETE" },
            { CashType.MONEY, "MONEDAS" }
        })
        {
        }
    }

    /// <summary>
    /// Define la estructura de un destino de devolución de dinero.
    /// </summary>
    [Entity(TableName = "CashDestinies")]
    public class CashDestiny : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'CashType'.
        /// </summary>
        private CashType _cashType;

        /// <summary>
        /// Campo que provee a la propiedad 'Description'.
        /// </summary>
        private String _description;

        /// <summary>
        /// Campo que provee a la propiedad 'ID'.
        /// </summary>
        private Int64 _id;

        /// <summary>
        /// Obtiene o establece el tipo de dinero valido para este destino.
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<CashType>))]
        public CashType CashType {
            get => _cashType;
            set {
                _cashType = value;
                OnPropertyChanged("CashType");
            }
        }

        /// <summary>
        /// Obtiene o establece la descripción de la devolución de dinero.
        /// </summary>
        public String Description {
            get => _description;
            set {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Obtiene o establece el identificador unico del destino de dinero.
        /// </summary>
        [Column(IsPrimaryKey = true, IsAutonumerical = true)]
        public Int64 ID {
            get => _id;
            set {
                _id = value;
                OnPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Representa la instancia actual en una cadena.
        /// </summary>
        /// <returns>Una cadena que representa la instancia.</returns>
        public override string ToString() => Description;
    }
}