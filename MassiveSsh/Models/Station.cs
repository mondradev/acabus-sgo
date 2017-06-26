using Acabus.Utils;
using InnSyTech.Standard.Database;
using System;
using System.Collections.ObjectModel;

namespace Acabus.Models
{
    /// <summary>
    /// Esta clase define la estructura de una estación perteneciente a una ruta troncal.
    /// </summary>
    [Entity(TableName = "Stations")]
    public sealed class Station : AssignableSection
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Devices'.
        /// </summary>
        private ObservableCollection<Device> _devices;

        /// <summary>
        /// Campo que provee el identificador de estación a la propiedad 'ID'.
        /// </summary>
        private UInt16 _id;

        /// <summary>
        /// Campo que provee a la propiedad 'IsConnected'.
        /// </summary>
        private Boolean _isConnected;

        /// <summary>
        /// Campo que provee a la propiedad 'Links'.
        /// </summary>
        private ObservableCollection<Link> _links;

        /// <summary>
        /// Campo que provee a la propiedad 'PingMax'.
        /// </summary>
        private UInt16 _pingMax;

        /// <summary>
        /// Campo que provee a la propiedad 'PingMin'.
        /// </summary>
        private UInt16 _pingMin;

        /// <summary>
        /// Campo que provee la ruta a la propiedad 'Trunk'.
        /// </summary>
        private Route _route;

        /// <summary>
        /// Campo que provee a la propiedad 'State'.
        /// </summary>
        private StateValue _state;

        /// <summary>
        /// Campo que provee a la propiedad 'StationNumber'.
        /// </summary>
        private UInt16 _stationNumber;

        /// <summary>
        /// Crea una instancia de una estación indicando la ruta troncal
        /// a la que pertence.
        /// </summary>
        /// <param name="trunk">Ruta troncal a la que pertenece la estación.</param>
        /// <param name="id">Identificador único de la estación.</param>
        /// <param name="stationNumber">Número de la estación.</param>
        public Station(Route trunk, UInt16 id, UInt16 stationNumber)
        {
            _route = trunk;
            _id = id;
            _stationNumber = stationNumber;
            State = StateValue.DISCONNECTED;
        }

        /// <summary>
        /// Crea una instancia básica de <see cref="Station"/>.
        /// </summary>
        public Station()
        {
            State = StateValue.DISCONNECTED;
        }

        /// <summary>
        /// Obtiene o establece los dispositivos que están asiganados a la estación actual.
        /// </summary>
        [Column(ForeignKeyName = "Fk_Station_ID")]
        public ObservableCollection<Device> Devices {
            get {
                if (_devices == null)
                    _devices = new ObservableCollection<Device>();
                return _devices;
            }
        }

        /// <summary>
        /// Obtiene el identificador de estación.
        /// </summary>
        [Column(IsPrimaryKey = true, IsAutonumerical = true)]
        public UInt16 ID {
            get => _id;
            private set {
                _id = value;
                OnPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Obtiene o establece si la estación tiene comunicación.
        /// </summary>
        public Boolean IsConnected {
            get => _isConnected;
            set {
                _isConnected = value;
                OnPropertyChanged("IsConnected");
            }
        }

        /// <summary>
        /// Obtiene una lista de los enlaces que tiene la estación actual.
        /// </summary>
        [Column(IsIgnored = true)]
        [XmlAnnotation(Ignore = true)]
        public ObservableCollection<Link> Links {
            get {
                if (_links == null)
                    _links = new ObservableCollection<Link>();
                return _links;
            }
        }

        /// <summary>
        /// Obtiene o establece la latencia máxima de la estación.
        /// </summary>
        public UInt16 PingMax {
            get => _pingMax;
            set {
                _pingMax = value;
                OnPropertyChanged("PingMax");
            }
        }

        /// <summary>
        /// Obtiene o establece la latencia mínima de la estación.
        /// </summary>
        public UInt16 PingMin {
            get => _pingMin;
            set {
                _pingMin = value;
                OnPropertyChanged("PingMin");
            }
        }

        /// <summary>
        /// Obtiene la ruta troncal a la que pertence la estación.
        /// </summary>
        [XmlAnnotation(Ignore = true)]
        [Column(IsForeignKey = true, Name = "Fk_Route_ID")]
        public Route Route {
            get => _route;
            set {
                _route = value;
                OnPropertyChanged("Route");
            }
        }

        /// <summary>
        /// Obtiene o establece el estado de la comunicación con la estación.
        /// </summary>
        [Column(IsIgnored = true)]
        [XmlAnnotation(Ignore = true)]
        public StateValue State {
            get => _state;
            set {
                _state = value;
                OnPropertyChanged("State");
            }
        }

        /// <summary>
        /// Obtiene el número de estación.
        /// </summary>
        public UInt16 StationNumber {
            get => _stationNumber;
            private set {
                _stationNumber = value;
                OnPropertyChanged("StationNumber");
            }
        }

        /// <summary>
        /// Crea una instancia de estación especificando su ID y  la ruta a la que pertenece.
        /// </summary>
        /// <param name="trunk">Ruta troncal a la que pertenece.</param>
        /// <param name="id">ID de la estación.</param>
        /// <param name="stationNumber">Número de la estación.</param>
        /// <returns>Una instancia de estación.</returns>
        public static Station CreateStation(Route trunk, UInt16 id, UInt16 stationNumber) => new Station(trunk, id, stationNumber);

        /// <summary>
        /// Compara dos instancias de <see cref="Station"/> y determina si son diferentes.
        /// </summary>
        public static bool operator !=(Station stationA, Station stationB)
            => !(stationA == stationB);

        /// <summary>
        /// Compara dos instancias de <see cref="Station"/> y determina si son iguales.
        /// </summary>
        public static bool operator ==(Station stationA, Station stationB)
            => (stationA is null && stationB is null)
                || (
                !(stationA is null)
                && !(stationB is null)
                && stationA.ID == stationB.ID
                && stationA.StationNumber == stationB.StationNumber);

        /// <summary>
        /// Añade un dispositivo a la estación.
        /// </summary>
        /// <param name="device">Dispositivo a agregar.</param>
        public void AddDevice(Device device)
        {
            if (!Devices.Contains(device))
                Devices.Add(device);
        }

        /// <summary>
        /// Añade un enlace a la estación.
        /// </summary>
        /// <param name="link">Enlace a agregar.</param>
        public void AddLink(Link link)
        {
            Links.Add(link);
        }

        /// <summary>
        /// Obtiene el número total de dispositivos en la estación.
        /// </summary>
        /// <returns>El número total de dispositivos.</returns>
        public UInt16 DeviceCount() => (UInt16)(Devices.Count);

        /// <summary>
        /// Compara la instancia actual con una instancia cualquiera y determina si son iguales.
        /// </summary>
        /// <param name="obj">Instancia a comparar.</param>
        /// <returns>Un valor <see cref="true"/> si las instancias son iguales.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != GetType()) return false;

            return this == (Station)obj;
        }

        /// <summary>
        /// Obtiene el primer dispositivo de la estación que cumpla con el predicado.
        /// </summary>
        /// <param name="predicate">Predicato utilizado para validar el dispositivo.</param>
        /// <returns>Un dispositivo de la estación.</returns>
        public Device FindDevice(Predicate<Device> predicate)
        {
            foreach (Device device in Devices)
                if (predicate.Invoke(device))
                    return device;
            return null;
        }

        /// <summary>
        /// Obtiene el dispositivo que coincide con el ID especificado.
        /// </summary>
        /// <param name="idDevice">ID del equipo a buscar</param>
        /// <returns>Un dispositivo de la estación.</returns>
        public Device GetDevice(UInt16 idDevice)
        {
            foreach (var item in Devices)
                if (item.ID == idDevice)
                    return item;
            return null;
        }

        /// <summary>
        /// Obtiene una código hash de la instancia actual.
        /// </summary>
        /// <returns>Un código hash de la instancia.</returns>
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Obtiene el ID de la estación en vía.
        /// </summary>
        /// <returns>ID de la estación en vía.</returns>
        public String GetViaID() => String.Format("{0:00}{1:00}", Route?.RouteNumber, StationNumber);

    }
}