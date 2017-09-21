using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.Models;
using System;

namespace Opera.Acabus.TrunkMonitor.Models
{
    /// <summary>
    /// Administra la información de estado de un dispositivo en especifico.
    /// </summary>
    public sealed class DeviceStateInfo : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="Device" />.
        /// </summary>
        private Device _device;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Ping" />.
        /// </summary>
        private Int16 _ping;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="State" />.
        /// </summary>
        private LinkState _state;

        /// <summary>
        /// Crea una instancia que administra la información de monitoreo del dispositivo.
        /// </summary>
        /// <param name="owner">Dispositivo que será administrado por la instancia.</param>
        public DeviceStateInfo(Device owner)
        {
            _device = owner;
        }

        /// <summary>
        /// Obtiene el dispositivo que es administrado por la instancia.
        /// </summary>
        public Device Device
            => _device;

        /// <summary>
        /// Obtiene o establece la duración del eco al dispositivo.
        /// </summary>
        public Int16 Ping {
            get => _ping;
            set {
                _ping = value;
                OnPropertyChanged(nameof(Ping));
            }
        }

        /// <summary>
        /// Obtiene o establece el estado de conexión del dispositivo.
        /// </summary>
        public LinkState State {
            get => _state;
            set {
                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }
    }
}