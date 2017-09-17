using InnSyTech.Standard.Database;
using InnSyTech.Standard.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Opera.Acabus.Core.Models
{
    /// <summary>
    /// Esta clase define toda estación en el sistema BRT.
    /// </summary>
    [Entity(TableName = "Stations")]
    public sealed class Station : NotifyPropertyChanged, IAssignableSection, ILocation, IComparable<Station>, IComparable
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="AssignedSection"/>
        /// </summary>
        private String _assignedSection;

        ///<summary>
        /// Campo que provee a la propiedad <see cref="Devices"/>.
        ///</summary>
        private ObservableCollection<Device> _devices;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ID" />.
        /// </summary>
        private UInt64 _id;

        ///<summary>
        /// Campo que provee a la propiedad <see cref="Name"/>.
        ///</summary>
        private String _name;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Route" />.
        /// </summary>
        private Route _route;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="StationNumber"/>.
        /// </summary>
        private UInt16 _stationNumber;

        /// <summary>
        /// Crea una instancia persistente de <see cref="Station"/>.
        /// </summary>
        /// <param name="id">Identificador único de estación.</param>
        /// <param name="stationNumber">Número de estación.</param>
        public Station(ulong id, ushort stationNumber)
        {
            _id = id;
            _stationNumber = stationNumber;
        }

        /// <summary>
        /// Crea una instancia de <see cref="Station"/>.
        /// </summary>
        public Station() { }

        /// <summary>
        /// Obtiene o establece la sección de atención a esta ubicación.
        /// </summary>
        public String AssignedSection {
            get => _assignedSection ?? (_assignedSection = String.Empty);
            set {
                _assignedSection = value;
                OnPropertyChanged(nameof(AssignedSection));
            }
        }

        /// <summary>
        /// Obtiene la lista de todos los dispositivos asignados a esta ubicación.
        /// </summary>
        [Column(ForeignKeyName = "Fk_Station_ID")]
        public ICollection<Device> Devices
                      => _devices ?? (_devices = new ObservableCollection<Device>());

        /// <summary>
        /// Obtiene el identificador único de estación.
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
        /// Obtiene o establece el nombre de esta ubicación.
        /// </summary>
        public String Name {
            get => _name ?? (_name = string.Empty);
            set {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Obtiene o establece la ruta a la cual está asignada esta estación.
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
        /// Obtiene o establece el número de estación.
        /// </summary>
        public UInt16 StationNumber {
            get => _stationNumber;
            private set {
                _stationNumber = value;
                OnPropertyChanged(nameof(StationNumber));
            }
        }

        /// <summary>
        /// Compara dos instancias de <see cref="Station"/> y determina si son diferentes.
        /// </summary>
        /// <param name="station">Una estación a comparar.</param>
        /// <param name="anotherStation">Otra estación a comparar.</param>
        /// <returns>Un valor <see cref="true"/> si las estaciones son diferentes.</returns>
        public static bool operator !=(Station station, Station anotherStation)
        {
            if (station is null && anotherStation is null) return false;
            if (station is null || anotherStation is null) return true;

            return station.CompareTo(anotherStation) != 0;
        }

        /// <summary>
        /// Compara dos instancias de <see cref="Station"/> y determina si son iguales.
        /// </summary>
        /// <param name="station">Una estación a comparar.</param>
        /// <param name="anotherStation">Otra estación a comparar.</param>
        /// <returns>Un valor <see cref="true"/> si las estaciones son iguales.</returns>
        public static bool operator ==(Station station, Station anotherStation)
        {
            if (station is null && anotherStation is null) return true;
            if (station is null || anotherStation is null) return false;

            return station.Equals(anotherStation);
        }

        /// <summary>
        /// Compara la instancia <see cref="Station"/> actual con otra instancia <see cref="Station"/> y
        /// devuelve un entero que indica si la posición de la instancia actual es anterior,
        /// posterior o igual que la del otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="other">Otra instancia <see cref="Station"/>.</param>
        /// <returns>
        /// Un valor 0 si las instancias son iguales, 1 si la instancia es mayor que la otra y -1 si
        /// la instancia menor que la otra.
        /// </returns>
        public int CompareTo(Station other)
        {
            if (other is null) return 1;
            if (other.Route == Route)
                return StationNumber.CompareTo(other.StationNumber);
            return Route.CompareTo(other.Route);
        }

        /// <summary>
        /// Compara la instancia <see cref="Station"/> actual con otra instancia y
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
            return CompareTo(other as Station);
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

            var anotherStation = obj as Station;

            return CompareTo(anotherStation) == 0;
        }

        /// <summary>
        /// Devuelve el código hash de la instancia actual.
        /// </summary>
        /// <returns>Un código hash que representa la instancia actual.</returns>
        public override int GetHashCode()
            => Tuple.Create(StationNumber, Route).GetHashCode();

        /// <summary>
        /// Devuelve el código de la estación actual que es formado a partir de el número de ruta y de estación.
        /// </summary>
        /// <returns>Un código de estación.</returns>
        public String GetStationCode()
            => String.Format("{0:D2}{1:D2}", Route.RouteNumber, StationNumber);

        /// <summary>
        /// Representa en una cadena la instancia de <see cref="Station"/> actual.
        /// </summary>
        /// <returns>Una cadena que representa una instancia <see cref="Station"/>.</returns>
        public override string ToString()
            => Name;
    }
}