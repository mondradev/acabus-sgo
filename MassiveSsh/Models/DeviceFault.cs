using Acabus.Utils.Mvvm;
using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using System;

namespace Acabus.Models
{
    [Entity(TableName = "Faults")]
    public sealed class DeviceFault : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Descripcion'.
        /// </summary>
        private String _description;

        /// <summary>
        /// Campo que provee a la propiedad 'DeviceType'.
        /// </summary>
        private DeviceType _deviceType;

        /// <summary>
        /// Campo que provee a la propiedad 'ID'.
        /// </summary>
        private int _id;

        /// <summary>
        /// Obtiene o establece la descripción de la falla.
        /// </summary>
        public String Description {
            get => _description;
            set {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Obtiene o establece el tipo de equipo que falla.
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
        /// Obtiene o establece el identificador unico de la falla.
        /// </summary>
        [Column(IsPrimaryKey = true, IsAutonumerical = true)]
        public int ID {
            get => _id;
            set {
                _id = value;
                OnPropertyChanged("ID");
            }
        }

        public override string ToString() => Description;
    }
}