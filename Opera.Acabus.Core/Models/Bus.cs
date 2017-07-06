using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using InnSyTech.Standard.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Opera.Acabus.Core.Models
{
    /// <summary>
    /// Define los estados posibles de un autobus.
    /// </summary>
    public enum BusStatus
    {
        /// <summary>
        /// En operación (Valor predeterminado).
        /// </summary>
        OPERATIONAL,

        /// <summary>
        /// En reparación o taller mecánico.
        /// </summary>
        IN_REPAIR,

        /// <summary>
        /// Sin energía en baterías.
        /// </summary>
        WITHOUT_ENERGY,

        /// <summary>
        /// Otras razones.
        /// </summary>
        OTHERS_REASONS
    }

    /// <summary>
    /// Enumera todos los tipos de autobuses.
    /// </summary>
    public enum BusType
    {
        /// <summary>
        /// Sin tipo de autobus (Valor predeterminado).
        /// </summary>
        NONE,

        /// <summary>
        /// Autobus tipo convencional.
        /// </summary>
        CONVENTIONAL,

        /// <summary>
        /// Autobus tipo padrón.
        /// </summary>
        STANDARD,

        /// <summary>
        /// Autobus tipo articulado.
        /// </summary>
        ARTICULATED
    }

    /// <summary>
    /// Esta clase define los autobuses que circulan por el BRT.
    /// </summary>
    [Entity(TableName = "Buses")]
    public sealed class Bus : NotifyPropertyChanged, ILocation, IComparable<Bus>, IComparable
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="Devices" />.
        /// </summary>
        private ICollection<Device> _devices;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="EconomicNumber" />.
        /// </summary>
        private String _economicNumber;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ID" />.
        /// </summary>
        private UInt64 _id;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Route" />.
        /// </summary>
        private Route _route;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Status" />.
        /// </summary>
        private BusStatus _status;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Type" />.
        /// </summary>
        private BusType _type;

        private object economicNumber;

        /// <summary>
        /// Crea una nueva instancia persistente de <see cref="Bus"/>.
        /// </summary>
        /// <param name="id">Identificador único de autobus.</param>
        /// <param name="economicNumber">Número económico de estación.</param>
        public Bus(ulong id, object economicNumber)
        {
            ID = id;
            this.economicNumber = economicNumber;
        }

        /// <summary>
        /// Obtiene una lista de los dispositivos asignados a este autobus.
        /// </summary>
        [Column(ForeignKeyName = "Fk_Bus_ID")]
        public ICollection<Device> Devices
             => _devices ?? (_devices = new ObservableCollection<Device>());

        /// <summary>
        /// Obtiene o establece el número económico del autobus.
        /// </summary>
        public String EconomicNumber {
            get => _economicNumber;
            private set {
                _economicNumber = value;
                OnPropertyChanged(nameof(EconomicNumber));
            }
        }

        /// <summary>
        /// Obtiene o establece el identificador único del autobus.
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
        /// Obtiene el nombre de este autobus.
        /// </summary>
        public String Name {
            get => _economicNumber;
            set => EconomicNumber = value;
        }

        /// <summary>
        /// Obtiene o establece la ruta a la que este autobus está asignado.
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_Route_ID")]
        public Route Route {
            get => _route;
            set {
                _route = value;
                OnPropertyChanged(nameof(Route));
            }
        }

        /// <summary>
        /// Obtiene o establece el estado actual de la unidad.
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<BusStatus>))]
        public BusStatus Status {
            get => _status;
            set {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        /// Obtiene o establece el tipo de autobus.
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<BusType>))]
        public BusType Type {
            get => _type;
            set {
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        /// <summary>
        /// Compara dos instancias de <see cref="Bus"/> y determina si son diferentes.
        /// </summary>
        /// <param name="bus">Un autobus a comparar.</param>
        /// <param name="anotherBus">Otro autobus a comparar.</param>
        /// <returns>Un valor <see cref="true"/> si los autobuses son diferentes.</returns>
        public static bool operator !=(Bus bus, Bus anotherBus)
        {
            if (bus is null && anotherBus is null) return false;
            if (bus is null || anotherBus is null) return true;

            if (bus.Type != anotherBus.Type) return true;
            if (bus.EconomicNumber != anotherBus.EconomicNumber) return true;

            return false;
        }

        /// <summary>
        /// Compara dos instancias de <see cref="Bus"/> y determina si son iguales.
        /// </summary>
        /// <param name="bus">Una estación a comparar.</param>
        /// <param name="anotherBus">Otra estación a comparar.</param>
        /// <returns>Un valor <see cref="true"/> si las estaciones son iguales.</returns>
        public static bool operator ==(Bus bus, Bus anotherBus)
        {
            if (bus is null && anotherBus is null) return true;
            if (bus is null || anotherBus is null) return false;

            if (bus.Type != anotherBus.Type) return false;
            if (bus.EconomicNumber != anotherBus.EconomicNumber) return false;

            return true;
        }

        /// <summary>
        /// Compara la instancia <see cref="Bus"/> actual con otra instancia <see cref="Bus"/> y
        /// devuelve un entero que indica si la posición de la instancia actual es anterior,
        /// posterior o igual que la del otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="other">Otra instancia <see cref="Bus"/>.</param>
        /// <returns>
        /// Un valor 0 si las instancias son iguales, 1 si la instancia es mayor que la otra y -1 si
        /// la instancia menor que la otra.
        /// </returns>
        public int CompareTo(Bus other)
        {
            if (other is null) return 1;
            if (Type != other.Type)
                return Type.CompareTo(other.Type);
            return EconomicNumber.CompareTo(other.EconomicNumber);
        }

        /// <summary>
        /// Compara la instancia <see cref="Bus"/> actual con otra instancia y
        /// devuelve un entero que indica si la posición de la instancia actual es anterior,
        /// posterior o igual que la del otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="other">Otra instancia.</param>
        /// <returns>
        /// Un valor 0 si las instancias son iguales, 1 si la instancia es mayor que la otra y -1 si
        /// la instancia menor que la otra.
        /// </returns>
        public int CompareTo(object other)
        {
            if (other is null) return 1;
            if (other.GetType() != GetType()) return 1;
            return CompareTo(other as Bus);
        }

        /// <summary>
        /// Determina si la instancia actual es igual a la pasada por argumento de la función.
        /// </summary>
        /// <param name="obj">Instancia a comparar con la actual.</param>
        /// <returns>Un valor <see cref="true"/> si la instancia es igual a la actual.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != GetType()) return false;

            var anotherBus = obj as Bus;

            if (anotherBus.Type != Type) return false;
            if (anotherBus.EconomicNumber != EconomicNumber) return false;

            return true;
        }

        /// <summary>
        /// Devuelve el código hash de la instancia actual.
        /// </summary>
        /// <returns>Un código hash que representa la instancia actual.</returns>
        public override int GetHashCode()
            => Tuple.Create(EconomicNumber, Type).GetHashCode();

        /// <summary>
        /// Obtiene el número económico y la ruta asignada a este autobus.
        /// </summary>
        /// <returns>El código de ruta y el número económico.</returns>
        public String GetRouteAssignedAndEconomic()
            => String.Format("{0} {1}", Route?.GetRouteCode(), EconomicNumber).Trim();

        /// <summary>
        /// Representa en una cadena la instancia de <see cref="Bus"/> actual.
        /// </summary>
        /// <returns>Una cadena que representa una instancia <see cref="Bus"/>.</returns>
        public override string ToString()
            => EconomicNumber;
    }
}