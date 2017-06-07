using InnSyTech.Standard.Database;
using System;

namespace Acabus.Models
{
    public sealed class DeviceBus : Device
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Description'.
        /// </summary>
        private String _description;

        public DeviceBus()
        {

        }

        public DeviceBus(UInt16 id, String numeSeri) : base(id, DeviceType.NONE, null, numeSeri)
        {
        }

        /// <summary>
        /// Obtiene o establece la descripción del equipo a bordo.
        /// </summary>
        [Column(IsIgnored = true)]
        public String Description {
            get => _description?.ToUpper();
            set {
                _description = value;
                OnPropertyChanged("Description");
            }
        }
        /// <summary>
        /// Obtiene o establece el vehículo donde se encuentra el dispositivo.
        /// </summary>
        [Column(IsForeignKey = true, IsAutonumerical = true, Name = "Fk_Vehicle_No_Econ")]
        public Vehicle Vehicle {
            get => _vehicle;
            set {
                _vehicle = value;
                OnPropertyChanged("Vehicle");
            }
        }
        /// <summary>
        /// Campo que provee a la propiedad 'Vehicle'.
        /// </summary>
        private Vehicle _vehicle;

        /// <summary>
        /// 
        /// </summary>
        [Column(IsIgnored = true)]
        public new Station Station { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Description;
    }
}