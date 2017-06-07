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
        private UInt64 _id;

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
        public UInt64 ID {
            get => _id;
            set {
                _id = value;
                OnPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Crea una instancia básica de <see cref="DeviceFault"/>
        /// </summary>
        public DeviceFault()
        {
        }

        /// <summary>
        /// Crea una instancia de <see cref="DeviceFault"/> definiendo el ID, descripción y el tipo de dispositivo.
        /// </summary>
        /// <param name="id">ID de la falla.</param>
        /// <param name="description">Descripción de la falla.</param>
        /// <param name="type">Tipo de dispositivo al que afecta.</param>
        public DeviceFault(UInt64 id, String description, DeviceType type)
        {
            _id = id;
            _description = description;
            _deviceType = type;
        }

        /// <summary>
        /// Representa en una cadena la falla actual.
        /// </summary>
        /// <returns>Una cadena que representa la instancia.</returns>
        public override string ToString() => Description;
    }
}