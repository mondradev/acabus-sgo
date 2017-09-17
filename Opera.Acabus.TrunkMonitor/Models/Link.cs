using InnSyTech.Standard.Database;
using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.Models;
using Opera.Acabus.TrunkMonitor.Helpers;
using System;

namespace Opera.Acabus.TrunkMonitor.Models
{
    /// <summary>
    /// Define la estructura de un enlace de comunicación entre una estación en extremo A y otra en extremo B.
    /// </summary>
    [Entity(TableName = "Links")]
    public sealed class Link : NotifyPropertyChanged, IComparable<Link>, IComparable
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="ID" />.
        /// </summary>
        private UInt64 _id;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Ping" />.
        /// </summary>
        private Int16 _ping;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="State" />.
        /// </summary>
        private LinkState _state;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="StationA" />.
        /// </summary>
        private Station _stationA;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="StationB" />.
        /// </summary>
        private Station _stationB;

        /// <summary>
        /// Crea una nueva instancia persistente de <see cref="Link"/>.
        /// </summary>
        /// <param name="id">Identificador único del enlace.</param>
        /// <param name="stationA">Estación del extremo A.</param>
        /// <param name="stationB">Estación del extremo B.</param>
        public Link(UInt64 id, Station stationA, Station stationB)
        {
            _id = id;
            _stationA = stationA;
            _stationB = stationB;
        }

        /// <summary>
        /// Crea una nueva instancia de <see cref="Link"/>.
        /// </summary>
        /// <param name="stationA">Estación del extremo A.</param>
        /// <param name="stationB">Estación del extremo B.</param>
        public Link(Station stationA, Station stationB)
        {
            _stationA = stationA;
            _stationB = stationB;
        }

        /// <summary>
        /// Crea una nueva instancia de <see cref="Link"/>.
        /// </summary>
        public Link() { }

        /// <summary>
        /// Obtiene o establece el identificador único del enlace.
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
        /// Obtiene o establece el valor de latencia del enlace.
        /// </summary>
        [Column(IsIgnored = true)]
        public Int16 Ping {
            get => _ping;
            set {
                _ping = value;
                OnPropertyChanged(nameof(Ping));
            }
        }

        /// <summary>
        /// Obtiene o establece el estado del enlace.
        /// </summary>
        [Column(IsIgnored = true)]
        public LinkState State {
            get => _state;
            set {
                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }

        /// <summary>
        /// Obtiene o establece la estación de un extremo (Estación A).
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_StationA_ID")]
        public Station StationA {
            get => _stationA;
            set {
                if (_stationA != null && value == null)
                    _stationA?.RemoveLink(this);

                _stationA = value;

                if (value != null)
                    _stationA?.AddLink(this);

                OnPropertyChanged(nameof(StationA));
            }
        }

        /// <summary>
        /// Obtiene o establece la estación de un extremo (Estación B).
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_StationB_ID")]
        public Station StationB {
            get => _stationB;
            set {
                _stationB = value;
                OnPropertyChanged(nameof(StationB));
            }
        }

        /// <summary>
        /// Compara dos instancias de <see cref="Link"/> y determina si son diferentes.
        /// </summary>
        /// <param name="link">Un enlace a comparar.</param>
        /// <param name="anotherLink">Otro enlace a comparar.</param>
        /// <returns>Un valor true si los enlaces son diferentes.</returns>
        public static bool operator !=(Link link, Link anotherLink)
        {
            if (link is null && anotherLink is null) return false;
            if (link is null || anotherLink is null) return true;

            return link.CompareTo(anotherLink) != 0;
        }

        /// <summary>
        /// Compara dos instancias de <see cref="Link"/> y determina si son iguales.
        /// </summary>
        /// <param name="link">Un enlace a comparar.</param>
        /// <param name="anotherLink">Otro enlace a comparar.</param>
        /// <returns>Un valor true si los enlaces son iguales.</returns>
        public static bool operator ==(Link link, Link anotherLink)
        {
            if (link is null && anotherLink is null) return true;
            if (link is null || anotherLink is null) return false;

            return link.Equals(anotherLink);
        }

        /// <summary>
        /// Compara la instancia <see cref="Link"/> actual con otra instancia y
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
            return CompareTo(other as Link);
        }

        /// <summary>
        /// Compara la instancia <see cref="Link"/> actual con otra instancia <see cref="Link"/> y
        /// devuelve un entero que indica si la posición de la instancia actual es anterior,
        /// posterior o igual que la del otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="other">Otra instancia <see cref="Link"/>.</param>
        /// <returns>
        /// Un valor 0 si las instancias son iguales, 1 si la instancia es mayor que la otra y -1 si
        /// la instancia menor que la otra.
        /// </returns>
        public int CompareTo(Link other)
        {
            if (other is null) return 1;

            var comparer = StationA?.CompareTo(other.StationA) ?? -1;

            if (comparer == 0)
                return StationB?.CompareTo(other.StationB) ?? -1;

            return comparer;
        }

        /// <summary>
        /// Determina si la instancia actual es igual a la pasada por argumento de la función.
        /// </summary>
        /// <param name="obj">Instancia a comparar con la actual.</param>
        /// <returns>Un valor true si la instancia es igual a la actual.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != GetType()) return false;

            var anotherLink = obj as Link;

            return CompareTo(anotherLink) == 0;
        }

        /// <summary>
        /// Devuelve el código hash de la instancia actual.
        /// </summary>
        /// <returns>Un código hash que representa la instancia actual.</returns>
        public override int GetHashCode()
            => Tuple.Create(StationA, StationB).GetHashCode();

        /// <summary>
        /// Representa en una cadena la instancia actual.
        /// </summary>
        /// <returns>Una cadena que representa el enlace.</returns>
        public override string ToString()
            => String.Format("{0} <--> {1}", StationA, StationB);
    }
}