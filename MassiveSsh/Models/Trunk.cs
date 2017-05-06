using Acabus.Utils.MVVM;
using System;
using System.Collections.ObjectModel;

namespace Acabus.Models
{
    public sealed class Trunk : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'ID'.
        /// </summary>
        private UInt16 _id;

        /// <summary>
        /// Obtiene el ID de la ruta troncal.
        /// </summary>
        public UInt16 ID => _id;

        /// <summary>
        /// Campo que provee a la propiedad 'Stations'.
        /// </summary>
        private ObservableCollection<Station> _stations;

        /// <summary>
        /// Obtiene una lista de las estaciones de asignadas a la ruta.
        /// </summary>
        public ObservableCollection<Station> Stations {
            get {
                if (_stations == null)
                    _stations = new ObservableCollection<Station>();
                return _stations;
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'Name'.
        /// </summary>
        private String _name;

        /// <summary>
        /// Obtiene o establece nombre de la ruta troncal.
        /// </summary>
        public String Name {
            get => _name;
            set {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Crea una instancia de ruta troncal.
        /// </summary>
        /// <param name="id">Identificador de la ruta troncal.</param>
        public Trunk(UInt16 id)
        {
            this._id = id;
        }

        /// <summary>
        /// Añade una estación a la ruta.
        /// </summary>
        /// <param name="station">Estación por agregar.</param>
        public void AddStation(Station station)
        {
            Stations.Add(station);
        }

        /// <summary>
        /// Obtiene la estación con el ID especificado.
        /// </summary>
        /// <param name="idStation">ID de la estación a buscar.</param>
        /// <returns>Una estación que coincida con el número de estación.</returns>
        public Station GetStation(UInt16 idStation)
        {
            foreach (var item in Stations)
                if (item.ID == idStation)
                    return item;
            return null;
        }

        /// <summary>
        /// Obtiene el número de estaciones en la ruta.
        /// </summary>
        /// <returns>El número de estaciones asignadas a la ruta actual.</returns>
        public UInt16 StationCount() => (UInt16)(Stations.Count);

        /// <summary>
        /// Representa en una cadena la instancia actual.
        /// </summary>
        /// <returns>Una cadena que representa una instancia Trunk.</returns>
        public new String ToString() => String.Format("Ruta T{0} - {1}", ID.ToString("D2"), Name);

        /// <summary>
        /// Obtiene el número de dispositivos en la ruta troncal
        /// </summary>
        /// <returns>Número de dispositivos</returns>
        public int CountDevices()
        {
            int count = 0;
            foreach (var item in Stations)
                count += item.DeviceCount();
            return count;
        }

        /// <summary>
        /// Crea una instancia de 'Trunk' proporcionando su ID.
        /// </summary>
        /// <param name="id">ID de la ruta troncal.</param>
        /// <returns>Una instancia de ruta troncal.</returns>
        public static Trunk CreateTrunk(UInt16 id) => new Trunk(id);

    }
}
