using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using Opera.Acabus.Core.Models.ModelsBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Opera.Acabus.Core.Models
{
    /// <summary>
    /// Esta clase define toda ruta que circula por el sistema BRT.
    /// </summary>
    [Entity(TableName = "Routes")]
    public class Route : AcabusEntityBase, IAssignableSection, IComparable<Route>, IComparable
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="AssignedSection"/>.
        /// </summary>
        private String _assignedSection;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Buses" />.
        /// </summary>
        private ICollection<Bus> _buses;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ID"/>.
        /// </summary>
        private UInt64 _id;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Name"/>.
        /// </summary>
        private String _name;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="RouteNumber"/>.
        /// </summary>
        private UInt16 _routeNumber;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Stations" />.
        /// </summary>
        private ICollection<Station> _stations;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Type"/>.
        /// </summary>
        private RouteType _type;

        /// <summary>
        /// Crea una instancia persistente de <see cref="Route"/>.
        /// </summary>
        /// <param name="id">Identificador único de la ruta.</param>
        /// <param name="number">Número de ruta.</param>
        /// <param name="type">Tipo de ruta.</param>
        public Route(ulong id, ushort number, RouteType type)
        {
            _id = id;
            _routeNumber = number;
            _type = type;
        }

        /// <summary>
        /// Crea una instancia de <see cref="Route"/>.
        /// </summary>
        public Route() { }

        /// <summary>
        /// Obtiene o establece la sección asignada para la atención de la ruta.
        /// </summary>
        public string AssignedSection {
            get => _assignedSection;
            set {
                _assignedSection = value;
                OnPropertyChanged(nameof(AssignedSection));
            }
        }

        /// <summary>
        /// Obtiene una lista de los autobuses asignados a esta ruta.
        /// </summary>
        [Column(ForeignKeyName = "Fk_Route_ID")]
        public ICollection<Bus> Buses
               => _buses ?? (_buses = new ObservableCollection<Bus>());

        /// <summary>
        /// Obtiene o establece identificador único de la ruta.
        /// </summary>
        [Column(IsPrimaryKey = true, IsAutonumerical = true)]
        override public UInt64 ID {
            get => _id;
            protected set {
                _id = value;
                OnPropertyChanged(nameof(ID));
            }
        }

        /// <summary>
        /// Obtiene o establece el nombre de la ruta.
        /// </summary>
        public String Name {
            get => _name;
            set {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Obtiene o establece número de la ruta.
        /// </summary>
        public UInt16 RouteNumber {
            get => _routeNumber;
            private set {
                _routeNumber = value;
                OnPropertyChanged(nameof(RouteNumber));
            }
        }

        /// <summary>
        /// Obtiene una lista de la lista de estaciones asignadas a esta ruta.
        /// </summary>
        [Column(ForeignKeyName = "Fk_Route_ID")]
        public ICollection<Station> Stations
              => _stations ?? (_stations = new ObservableCollection<Station>());

        /// <summary>
        /// Obtiene o establece el tipo de la ruta.
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<RouteType>))]
        public RouteType Type {
            get => _type;
            private set {
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        /// <summary>
        /// Determina si dos instancias de <see cref="Route"/> son diferentes.
        /// </summary>
        /// <param name="route">Una instancia <see cref="Route"/> a comparar.</param>
        /// <param name="anotherRoute">Otra instancia <see cref="Route"/> a comparar.</param>
        /// <returns>Un valor true si ambas instancias son diferentes.</returns>
        public static bool operator !=(Route route, Route anotherRoute)
        {
            if (route is null && anotherRoute is null) return false;
            if (route is null || anotherRoute is null) return true;

            return route.CompareTo(anotherRoute) != 0;
        }

        /// <summary>
        /// Determina si dos instancias de <see cref="Route"/> son iguales.
        /// </summary>
        /// <param name="route">Una instancia <see cref="Route"/> a comparar.</param>
        /// <param name="anotherRoute">Otra instancia <see cref="Route"/> a comparar.</param>
        /// <returns>Un valor true si ambas instancias son iguales.</returns>
        public static bool operator ==(Route route, Route anotherRoute)
        {
            if (route is null && anotherRoute is null) return true;
            if (route is null || anotherRoute is null) return false;

            return route.Equals(anotherRoute);
        }

        /// <summary>
        /// Compara la instancia <see cref="Route"/> actual con otra instancia <see cref="Route"/> y
        /// devuelve un entero que indica si la posición de la instancia actual es anterior,
        /// posterior o igual que la del otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="other">Otra instancia <see cref="Route"/>.</param>
        /// <returns>
        /// Un valor 0 si las instancias son iguales, 1 si la instancia es mayor que la otra y -1 si
        /// la instancia menor que la otra.
        /// </returns>
        public int CompareTo(Route other)
        {
            if (other is null) return 1;
            if (Type.Equals(other.Type))
                return RouteNumber.CompareTo(other.RouteNumber);
            else
                return Type.CompareTo(other.Type);
        }

        /// <summary>
        /// Compara la instancia <see cref="Route"/> actual con otra instancia y devuelve un entero
        /// que indica si la posición de la instancia actual es anterior, posterior o igual que la
        /// del otro objeto en el criterio de ordenación.
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
            return CompareTo(other as Route);
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

            var anotherObject = obj as Route;

            return CompareTo(anotherObject) == 0;
        }

        /// <summary>
        /// Devuelve el código hash de la instancia actual.
        /// </summary>
        /// <returns>Un código hash que representa la instancia actual.</returns>
        public override int GetHashCode()
            => Tuple.Create(Name, RouteNumber, Type).GetHashCode();

        /// <summary>
        /// Obtiene el código de la ruta actual.
        /// </summary>
        /// <returns>Una cadena que representa una instancia.</returns>
        public String GetRouteCode()
            => String.Format("R{0}{1}", Enum.GetName(typeof(RouteType), Type)?[0], RouteNumber);

        /// <summary>
        /// Representa en una cadena la instancia de <see cref="Route"/> actual.
        /// </summary>
        /// <returns>Una cadena que representa una instancia <see cref="Route"/>.</returns>
        public override String ToString()
            => String.Format("RUTA {0}{1} - {2}",
                Enum.GetName(typeof(RouteType), Type)?[0], RouteNumber.ToString("D2"), Name);
    }
}