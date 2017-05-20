using Acabus.Models;
using Acabus.Utils.Mvvm;
using System;

namespace Acabus.Modules.CctvReports.Models
{
    /// <summary>
    /// Define la estructura de una alarma de dispositivo.
    /// </summary>
    public sealed class Alarm : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Description'.
        /// </summary>
        private String _description;

        /// <summary>
        /// Obtiene o establece la descripción de la alarma.
        /// </summary>
        public String Description {
            get => _description?.ToUpper();
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
        /// Campo que provee a la propiedad 'Priority'.
        /// </summary>
        private Priority _priority;

        /// <summary>
        /// Obtiene o establece la prioridad de la incidencia.
        /// </summary>
        public Priority Priority {
            get => _priority;
            set {
                _priority = value;
                OnPropertyChanged("Priority");
            }
        }

        /// <summary>
        /// Crea una instancia de alarma de dispositivo.
        /// </summary>
        /// <param name="id">Identificador de alarma.</param>
        public Alarm(UInt32 id) => _id = id;

        /// <summary>
        /// Crea una instancia nueva de alarma de dispositivo.
        /// </summary>
        /// <param name="id">Identificador de alarma.</param>
        /// <param name="numeSeri">Número de serie que produce la alarma.</param>
        /// <param name="description">Descripción de la alarma.</param>
        /// <param name="dateTime">Fecha y hora de la alarma.</param>
        /// <param name="priority">La prioridad que requiere de atención.</param>
        /// <returns>Una alarma de dispositivo.</returns>
        public static Alarm CreateAlarm(UInt32 id, String numeSeri, String description, DateTime dateTime, Priority priority)
        {
            return new Alarm(id)
            {
                Description = description,
                DateTime = dateTime,
                Device = DataAccess.AcabusData.FindDevice((device)
                            => device.NumeSeri.Equals(numeSeri)),
                Priority = priority
            };
        }

        /// <summary>
        /// Representa la instancia actual con una cadena.
        /// </summary>
        /// <returns>Una cadena que representa una alarma.</returns>
        public override string ToString()
        {
            return String.Format("{0} {1} {2}: {3}", ID, Device, DateTime, Description);
        }

        /// <summary>
        /// Operador lógico de igualdad, determina si dos instancias <see cref="Alarm"/> son iguales.
        /// </summary>
        /// <param name="alarm">Una instancia.</param>
        /// <param name="otherAlarm">Otra instancia.</param>
        /// <returns>Un valor <code>true</code> si el ID y el Device es igual en ambas instancias <see cref="Alarm"/>.</returns>
        public static bool operator ==(Alarm alarm, Alarm otherAlarm)
        {
            if (otherAlarm.ID == alarm.ID && otherAlarm.Device == alarm.Device)
                return true;

            return false;
        }

        /// <summary>
        /// Operador lógico de desigualdad, determina si dos instancias <see cref="Alarm"/> son diferentes.
        /// </summary>
        /// <param name="alarm">Una instancia.</param>
        /// <param name="otherAlarm">Otra instancia.</param>
        /// <returns>Un valor <code>true</code> si el ID o el Device es diferente en ambas instancias <see cref="Alarm"/>.</returns>
        public static bool operator !=(Alarm alarm, Alarm otherAlarm)
        {
            if (otherAlarm.ID != alarm.ID || otherAlarm.Device != alarm.Device)
                return true;

            return false;
        }

    }
}
