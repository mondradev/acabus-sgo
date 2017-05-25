using Acabus.Models;
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
        /// Campo que provee a la propiedad 'Description'.
        /// </summary>
        private String _description;

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
        /// Campo que provee a la propiedad 'StartDate'.
        /// </summary>
        private DateTime _startDate;

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
        /// Campo que provee a la propiedad 'FinishDate'.
        /// </summary>
        private DateTime? _finishDate;

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
        /// Campo que provee a la propiedad 'Bussines'.
        /// </summary>
        private String whoReporting;

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
        /// Campo que provee a la propiedad 'Location'.
        /// </summary>
        private Location _location;

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
        /// Campo que provee a la propiedad 'Device'.
        /// </summary>
        private Device _device;

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
        /// Campo que provee a la propiedad 'Technician'.
        /// </summary>
        private String _technician;

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
        /// Campo que provee a la propiedad 'Folio'.
        /// </summary>
        private String _folio;

        /// <summary>
        /// Obtiene el folio de la incidencia.
        /// </summary>
        public String Folio => _folio;

        /// <summary>
        /// Campo que provee a la propiedad 'Observations'.
        /// </summary>
        private String _observations;

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
        /// Campo que provee a la propiedad 'Status'.
        /// </summary>
        private IncidenceStatus _status;

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
        /// Crea una instancia nueva de una incidencia.
        /// </summary>
        public Incidence(String folio)
        {
            this._folio = folio;
            this._status = IncidenceStatus.OPEN;
        }

        /// <summary>
        /// Representa la instancia de incidencia en una cadena utilizable para subir el reporte.
        /// </summary>
        /// <returns></returns>
        public String ToReportString()
        {
            return String.Format("*{0}* {1} {2}",
                Folio,
                Location is Route
                    ? String.Format("{0} {1}",
                        (Location as Route).GetCodeRoute(),
                        (Device as Vehicle).EconomicNumber)
                    : Device is null
                        ? Location.Name
                        : Device.NumeSeri,
                Description);
        }
    }
}
