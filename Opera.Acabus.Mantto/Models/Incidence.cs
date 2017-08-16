using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.Models;
using System;

namespace Opera.Acabus.Mantto.Models
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
    /// Define la estructura de la incidencias de la operación de Acabus.
    /// </summary>
    [Entity(TableName = "Incidences")]
    public class Incidence : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'AssignedAttendance'.
        /// </summary>
        private Attendance _assignedAttendance;

        /// <summary>
        /// Campo que provee a la propiedad 'Description'.
        /// </summary>
        private DeviceFault _description;

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
        private ITStaff _technician;

        /// <summary>
        /// Campo que provee a la propiedad 'Bussines'.
        /// </summary>
        private String whoReporting;

        /// <summary>
        /// Crea una instancia nueva de una incidencia.
        /// </summary>
        public Incidence(String folio)
        {
            _folio = folio;
            _status = IncidenceStatus.OPEN;
        }

        /// <summary>
        /// Crea una instancia básica de <see cref="Incidence"/>.
        /// </summary>
        public Incidence()
        {
            _status = IncidenceStatus.OPEN;
        }

        /// <summary>
        /// Obtiene o establece la asistencia correspondiente.
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_Attendance_ID")]
        public Attendance AssignedAttendance {
            get => _assignedAttendance;
            set {
                _assignedAttendance = value;
                OnPropertyChanged("AssignedAttendance");
            }
        }

        /// <summary>
        /// Obtiene o establece la descripción de la incidencia.
        /// </summary>
        [Column(Name = "Fk_Fault_ID", IsForeignKey = true)]
        public DeviceFault Description {
            get => _description;
            set {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Obtiene o establece el equipo el cual presenta la incidencia.
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_Device_ID")]
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
        /// Campo que provee a la propiedad 'RefundOfMoney'.
        /// </summary>
        private RefundOfMoney _refundOfMoney;

        /// <summary>
        /// Obtiene o establece la devolución de dinero
        /// </summary>
        [Column(ForeignKeyName = "Fk_Folio")]
        public RefundOfMoney RefundOfMoney {
            get => _refundOfMoney;
            set {
                _refundOfMoney = value;
                OnPropertyChanged(nameof(RefundOfMoney));
            }
        }

        /// <summary>
        /// Indica si la incidencia tiene una devolución de dinero.
        /// </summary>
        /// <returns></returns>
        public Boolean HasRefundOfMoney() => RefundOfMoney != null;

        /// <summary>
        /// Obtiene el folio de la incidencia.
        /// </summary>
        [Column(IsPrimaryKey = true)]
        public String Folio {
            get => _folio;
            private set {
                _folio = value;
                OnPropertyChanged("Folio");
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
        [Column(Converter = typeof(DbEnumConverter<Priority>))]
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
        [Column(Converter = typeof(DbEnumConverter<IncidenceStatus>))]
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
        [Column(IsForeignKey = true, Name = "Fk_ITStaff_ID")]
        public ITStaff Technician {
            get => _technician;
            set {
                _technician = value;
                OnPropertyChanged("Technician");
            }
        }

        /// <summary>
        /// Obtiene el tiempo total de la solución.
        /// </summary>
        [Column(IsIgnored = true)]
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
        /// <returns>Una cadena que representa la incidencia.</returns>
        public String ToReportString()
        {
            return String.Format("*{0}* {1} {2} {3} {4}",
                Folio,
                Device?.Bus != null
                    ? String.Format("{0} {1}",
                        Device.Bus.GetRouteAssignedAndEconomic(),
                        Device)
                    : Device?.SerialNumber,
                String.Format("*{0}*, {1}", Description?.Category?.Description, Description),
                AssignedAttendance is null
                ? String.Empty
                : String.Format("\n*Asignado:* {0}", AssignedAttendance),
                String.IsNullOrEmpty(Observations?.Trim())
                ? String.Empty
                : String.Format("\n*Observaciones:* {0}", Observations?.Trim().ToUpper()));
        }

        /// <summary>
        /// Representa la instancia actual como una cadena.
        /// </summary>
        /// <returns>Una cadena que representa la instancia actual.</returns>
        public override string ToString() => ToReportString();
    }
}