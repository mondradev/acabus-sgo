using Acabus.Utils.Mvvm;
using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using System;

namespace Acabus.Models
{
    [Entity(TableName = "FaultCategories")]
    public sealed class DeviceFaultCategory : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Description'.
        /// </summary>
        private String _description;

        /// <summary>
        /// Campo que provee a la propiedad 'DeviceType'.
        /// </summary>
        private DeviceType _deviceType;

        /// <summary>
        /// Campo que provee a la propiedad 'ID'.
        /// </summary>
        private UInt64 _id;

        /// <summary>
        /// Obtiene o establece la descripción de la categoría.
        /// </summary>
        public String Description {
            get => _description;
            set {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Obtiene o establece el tipo de dispositivo a la que corresponde esta categoría de fallas.
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<DeviceType>))]
        public DeviceType DeviceType {
            get => _deviceType;
            set {
                _deviceType = value;
                OnPropertyChanged("DeviceType");
            }
        }

        /// <summary>
        /// Obtiene el identificador de la categoría de fallas.
        /// </summary>
        [Column(IsPrimaryKey = true, IsAutonumerical = true)]
        public UInt64 ID {
            get => _id;
            private set {
                _id = value;
                OnPropertyChanged("ID");
            }
        }

       
    }
}