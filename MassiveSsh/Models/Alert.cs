using Acabus.Utils.MVVM;
using System;

namespace Acabus.Models
{
    /// <summary>
    /// Define la estructura de una alarma de dispositivo.
    /// </summary>
    public sealed class Alert : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Description'.
        /// </summary>
        private String _description;

        /// <summary>
        /// Obtiene o establece la descripción de la alarma.
        /// </summary>
        public String Description {
            get => _description;
            set {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'DateTime'.
        /// </summary>
        private DateTime _dateTime;

        /// <summary>
        /// Obtiene o establece la fecha y hora de la alarma.
        /// </summary>
        public DateTime DateTime {
            get => _dateTime;
            set {
                _dateTime = value;
                OnPropertyChanged("DateTime");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'Device'.
        /// </summary>
        private Device _device;

        /// <summary>
        /// Obtiene o establece el equipo que produce la alarma.
        /// </summary>
        public Device Device {
            get => _device;
            set {
                _device = value;
                OnPropertyChanged("Device");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'ID'.
        /// </summary>
        private UInt32 _id;

        /// <summary>
        /// Obtiene el identificador de la alarma.
        /// </summary>
        public UInt32 ID => _id;

        /// <summary>
        /// Campo que provee a la propiedad 'State'.
        /// </summary>
        private AlertState _state;

        /// <summary>
        /// Obtiene o establece el estado de lectura de la alarma.
        /// </summary>
        public AlertState State {
            get => _state;
            set {
                _state = value;
                OnPropertyChanged("State");
            }
        }

        /// <summary>
        /// Crea una instancia de alarma de dispositivo.
        /// </summary>
        /// <param name="id">Identificador de alarma.</param>
        public Alert(UInt32 id) => _id = id;

        /// <summary>
        /// Crea una instancia nueva de alarma de dispositivo.
        /// </summary>
        /// <param name="id">Identificador de alarma.</param>
        /// <param name="numeSeri">Número de serie que produce la alarma.</param>
        /// <param name="description">Descripción de la alarma.</param>
        /// <param name="dateTime">Fecha y hora de la alarma.</param>
        /// <returns>Una alarma de dispositivo.</returns>
        public static Alert CreateAlert(UInt32 id, String numeSeri, String description, DateTime dateTime)
        {
            return new Alert(id)
            {
                Description = description,
                DateTime = dateTime,
                Device = DataAccess.AcabusData.FindDevice((device)
                            => device.NumeSeri.Equals(numeSeri)),
                State = AlertState.UNREAD
            };
        }

        /// <summary>
        /// Representa la instancia actual con una cadena.
        /// </summary>
        /// <returns>Una cadena que representa una alarma.</returns>
        public override string ToString()
        {
            return String.Format("{0} {1} {2}: {3} - {4}", ID, Device, DateTime, Description, State);
        }
    }
}
