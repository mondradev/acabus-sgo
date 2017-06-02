using System;

namespace Acabus.Models
{
    public sealed class DeviceBus : Device
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Description'.
        /// </summary>
        private String _description;

        public DeviceBus() : base(0, DeviceType.DEVICE_BUS, null)
        {
        }

        /// <summary>
        /// Obtiene o establece la descripción del equipo a bordo.
        /// </summary>
        public String Description {
            get => _description?.ToUpper();
            set {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        public override string ToString() => Description;
    }
}