
using Acabus.Utils;
using Acabus.Utils.MVVM;
using System;

namespace Acabus.Models
{
    /// <summary>
    /// Define la estructura de un enlace entre estaciones.
    /// </summary>
    public sealed class Link : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'StationA'.
        /// </summary>
        private Station _stationA;

        /// <summary>
        /// Obtiene o establece la estación del punto A del enlace.
        /// </summary>
        public Station StationA {
            get => _stationA;
            set {
                _stationA = value;
                OnPropertyChanged("StationA");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'StationB'.
        /// </summary>
        private Station _stationB;

        /// <summary>
        /// Obtiene o establece la estación del punto B del enlace.
        /// </summary>
        public Station StationB {
            get => _stationB;
            set {
                _stationB = value;
                OnPropertyChanged("StationB");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'State'.
        /// </summary>
        private StateValue _state;

        /// <summary>
        /// Obtiene o establece el estado del enlace de comunicación.
        /// </summary>
        [XmlAnnotation(Ignore = true)]
        public StateValue State {
            get => _state;
            set {
                _state = value;
                OnPropertyChanged("State");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'Ping'.
        /// </summary>
        private Int16 _ping;

        /// <summary>
        /// Obtiene o establece la latencia del enlace.
        /// </summary>
        public Int16 Ping {
            get => _ping;
            set {
                _ping = value;
                OnPropertyChanged("Ping");
            }
        }

        /// <summary>
        /// Crea una instancia de enlace indicando sus estaciones A y B.
        /// </summary>
        /// <param name="a">Estación A.</param>
        /// <param name="b">Estación B.</param>
        public Link(Station a, Station b)
        {
            StationA = a;
            StationB = b;
            StationA.AddLink(this);
        }

        /// <summary>
        /// Crea una instancia de enlace indicando sus estaciones A y B.
        /// </summary>
        /// <param name="a">Estación A</param>
        /// <param name="b">Estación B</param>
        /// <returns>Un enlace entre estaciones.</returns>
        public static Link CreateLink(Station a, Station b)
        {
            return new Link(a, b);
        }

        /// <summary>
        /// Obtiene una cadena que representa el enlace.
        /// </summary>
        /// <returns>Una cadena que representa el enlace.</returns>
        public override string ToString()
        {
            return String.Format("{0} <--> {1}", StationA, StationB);
        }
    }
}
