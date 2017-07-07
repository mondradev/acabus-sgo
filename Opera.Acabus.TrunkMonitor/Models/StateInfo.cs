﻿using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.Models;

namespace Opera.Acabus.TrunkMonitor.Models
{
    /// <summary>
    /// Administra la información de estado de la estación especificada.
    /// </summary>
    public class StationStateInfo : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="State" />.
        /// </summary>
        private LinkState _state;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Station"/>.
        /// </summary>
        private Station _station;

        /// <summary>
        /// Crea una instancia nueva de <see cref="StationStateInfo"/>.
        /// </summary>
        /// <param name="station">Estación la que será administrada por esta instancia.</param>
        public StationStateInfo(Station station)
        {
            _station = station;
        }

        /// <summary>
        /// Obtiene o establece el estado de la estación.
        /// </summary>
        public LinkState State {
            get => _state;
            set {
                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }

        /// <summary>
        /// Obtiene la estación administrada por esta instancia.
        /// </summary>
        public Station Station
            => _station;
    }
}