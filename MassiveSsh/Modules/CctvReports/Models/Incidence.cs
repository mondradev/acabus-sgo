using Acabus.Models;
using Acabus.Modules.Attendances.Models;
using Acabus.Modules.Attendances.Services;
using Acabus.Utils.Mvvm;
using System;

namespace Acabus.Modules.CctvReports.Models
{
    /// <summary>
    /// Estados posibles de una incidencia.
    /// </summary>
    public enum IncidenceStatus
    {
        /// <summary>
        /// Incidencia abierta.
        /// </summary>
        OPEN,

        /// <summary>
        /// Incidencia cerrada.
        /// </summary>
        CLOSE,

        /// <summary>
        /// Por confirmar.
        /// </summary>
        UNCOMMIT
    }

    /// <summary>
    ///
    /// </summary>
    public class Incidence : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'AssignedAttendance'.
        /// </summary>
        private String _assignedAttendance;

        /// <summary>
        /// Campo que provee a la propiedad 'Description'.
        /// </summary>
        private String _description;

        /// <summary>
        /// Campo que provee a la propiedad 'Device'.
        /// </summary>
        private Device _device;

        /// <summary>
        /// Campo que provee a la propiedad 'FinishDate'.
        /// </summary>
        private DateTime? _finishDate;

        /// <summary>
        /// Campo que provee a la propiedad 'Folio'.
        /// </summary>
        private String _folio;

        /// <summary>
        /// Campo que provee a la propiedad 'Location'.
        /// </summary>
        private Location _location;

        /// <summary>
        /// Campo que provee a la propiedad 'Observations'.
        /// </summary>
        private String _observations;

        /// <summary>
        /// Campo que provee a la propiedad 'Priority'.
        /// </summary>
        private Priority _priority;

        /// <summary>
        /// Campo que provee a la propiedad 'StartDate'.
        /// </summary>
        private DateTime _startDate;

        /// <summary>
        /// Campo que provee a la propiedad 'Status'.
        /// </summary>
        private IncidenceStatus _status;

        /// <summary>
        /// Campo que provee a la propiedad 'Technician'.
        /// </summary>
        private String _technician;

        /// <summary>
        /// Campo que provee a la propiedad 'Bussines'.
        /// </summary>
        private String whoReporting;

        /// <summary>
        /// Crea una instancia nueva de una incidencia.
        /// </summary>
        public Incidence(String folio)
        {
            this._folio = folio;
            this._status = IncidenceStatus.OPEN;
        }

        /// <summary>
        /// Obtiene o establece la asistencia correspondiente.
        /// </summary>
        public String AssignedAttendance {
            get => _assignedAttendance;
            set {
                _assignedAttendance = value;
                OnPropertyChanged("AssignedAttendance");
                AttendanceService.CountIncidences(value);
            }
        }

        /// <summary>
        /// Obtiene o establece la descripción de la incidencia.
        /// </summary>
        public String Description {
            get => _description?.ToUpper();
            set {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Obtiene o establece el equipo el cual presenta la incidencia.
        /// </summary>
        public Device Device {
            get => _device;
            set {
                _device = value;
                OnPropertyChanged("Device");
            }
        }

        /// <summary>
        /// Obtiene o establece la fecha de finalización de la incidencia.
        /// </summary>
        public DateTime? FinishDate {
            get => _finishDate;
            set {
                _finishDate = value;
                OnPropertyChanged("FinishDate");
                OnPropertyChanged("TotalTime");
            }
        }

        /// <summary>
        /// Obtiene el folio de la incidencia.
        /// </summary>
        public String Folio => _folio;

        /// <summary>
        /// Obtiene o establece la ubicación de la inicidencia.
        /// </summary>
        public Location Location {
            get => _location;
            set {
                _location = value;
                OnPropertyChanged("Location");
            }
        }

        /// <summary>
        /// Obtiene o establece las observaciones de la incidencia.
        /// </summary>
        public String Observations {
            get => _observations;
            set {
                _observations = value;
                OnPropertyChanged("Observations");
            }
        }

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
        /// Obtiene o establece la fecha y hora de inicio de la incidencia.
        /// </summary>
        public DateTime StartDate {
            get => _startDate;
            set {
                _startDate = value;
                OnPropertyChanged("StartDate");
                OnPropertyChanged("TotalTime");
            }
        }

        /// <summary>
        /// Obtiene o establece el estado de la incidencia (Abierta|Cerrada).
        /// </summary>
        public IncidenceStatus Status {
            get => _status;
            set {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        /// <summary>
        /// Obtiene o establece el técnico que resolvió la incidencia.
        /// </summary>
        public String Technician {
            get => _technician;
            set {
                _technician = value;
                OnPropertyChanged("Technician");
            }
        }

        /// <summary>
        /// Obtiene el tiempo total de la solución.
        /// </summary>
        public TimeSpan? TotalTime => FinishDate - StartDate;

        /// <summary>
        /// Obtiene o establece quien realiza el reporte.
        /// </summary>
        public String WhoReporting {
            get => whoReporting;
            set {
                whoReporting = value;
                OnPropertyChanged("WhoReporting");
            }
        }

        /// <summary>
        /// Representa la instancia de incidencia en una cadena utilizable para subir el reporte.
        /// </summary>
        /// <returns></returns>
        public String ToReportString()
        {
            return String.Format("*{0}* {1} {2} {3}",
                Folio,
                Location is Vehicle
                    ? String.Format("{0} {1}",
                        (Location as Vehicle),
                        Device)
                    : Device.NumeSeri,
                Description,
                String.IsNullOrEmpty(AssignedAttendance)
                ? String.Empty
                : String.Format("\n*Asignado:* {0}", AssignedAttendance));
        }
    }
}