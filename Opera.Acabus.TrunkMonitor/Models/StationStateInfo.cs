using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Opera.Acabus.TrunkMonitor.Models
{
    /// <summary>
    /// Administra la información de estado de una estación en especifico.
    /// </summary>
    public class StationStateInfo : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="Links" />.
        /// </summary>
        private ICollection<Link> _links;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="MaximunAcceptablePing" />.
        /// </summary>
        private UInt16 _maximunAcceptablePing;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="MaximunPing" />.
        /// </summary>
        private UInt16 _maximunPing;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Ping" />.
        /// </summary>
        private Int16 _ping;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Station"/>.
        /// </summary>
        private Station _station;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Status" />.
        /// </summary>
        private LinkState _status;

        /// <summary>
        /// Crea una instancia nueva de <see cref="StationStateInfo"/>.
        /// </summary>
        /// <param name="owner">Estación la que será administrada por esta instancia.</param>
        public StationStateInfo(Station owner)
        {
            _station = owner;
            _maximunAcceptablePing = 600;
            _maximunPing = 100;
        }

        /// <summary>
        /// Obtiene una lista de los enlaces de esta estación.
        /// </summary>
        public ICollection<Link> Links
            => _links ?? (_links = new ObservableCollection<Link>());

        /// <summary>
        /// Obtiene o establece el valor máximo aceptable para la duración del eco.
        /// </summary>
        public UInt16 MaximunAcceptablePing {
            get => _maximunAcceptablePing;
            set {
                _maximunAcceptablePing = value;
                OnPropertyChanged(nameof(MaximunAcceptablePing));
            }
        }

        /// <summary>
        /// Obtiene o establece el valor máximo optimo para la duración del eco.
        /// </summary>
        public UInt16 MaximunPing {
            get => _maximunPing;
            set {
                _maximunPing = value;
                OnPropertyChanged(nameof(MaximunPing));
            }
        }

        /// <summary>
        /// Obtiene o establece el valor del tiempo del eco.
        /// </summary>
        public Int16 Ping {
            get => _ping;
            set {
                _ping = value;
                OnPropertyChanged(nameof(Ping));
            }
        }

        /// <summary>
        /// Obtiene la estación administrada por esta instancia.
        /// </summary>
        public Station Station
            => _station;

        /// <summary>
        /// Obtiene o establece el estado de la estación.
        /// </summary>
        public LinkState Status {
            get => _status;
            set {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
    }
}