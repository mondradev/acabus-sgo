using InnSyTech.Standard.Database;
using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.Models;
using Opera.Acabus.TrunkMonitor.Utils;
using System;

namespace Opera.Acabus.TrunkMonitor.Models
{
    /// <summary>
    /// Define la estructura de un enlace de comunicación entre una estación en extremo A y otra en extremo B.
    /// </summary>
    [Entity(TableName = "Links")]
    public sealed class Link : NotifyPropertyChanged
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
                _stationA = value;
                OnPropertyChanged(nameof(StationA));
                if (value != null)
                    _stationA.AddLink(this);
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
        /// Representa en una cadena la instancia actual.
        /// </summary>
        /// <returns>Una cadena que representa el enlace.</returns>
        public override string ToString()
            => String.Format("{0} <--> {1}", StationA, StationB);
    }
}