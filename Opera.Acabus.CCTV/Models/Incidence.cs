using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using InnSyTech.Standard.Mvvm;
using InnSyTech.Standard.Mvvm.Converters;
using InnSyTech.Standard.Translates;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;

namespace Opera.Acabus.Cctv.Models
{
    /// <summary>
    /// Estados posibles de una incidencia.
    /// </summary>
    public enum IncidenceStatus
    {
        /// <summary>
        /// Incidencia abierta.
        /// </summary>
        OPEN = 0,

        /// <summary>
        /// Incidencia cerrada.
        /// </summary>
        CLOSE = 1,

        /// <summary>
        /// Por confirmar.
        /// </summary>
        UNCOMMIT = 2,

        /// <summary>
        /// Pendiente.
        /// </summary>
        PENDING = 4
    }

    /// <summary>
    /// Provee de funciones a la enumeración <see cref="IncidenceStatus"/>.
    /// </summary>
    public static class IncidenceStatusExtension
    {
        /// <summary>
        /// Traduce al idioma español el valor de la enumeración especificada.
        /// </summary>
        /// <param name="incidenceStatus">Valor de la enumeración a traducir.</param>
        /// <returns>Una cadena que representa en idioma español el valor de la enumeración.</returns>
        public static String Translate(this IncidenceStatus incidenceStatus)
            => new IncidenceStatusTranslator().Translate(incidenceStatus);
    }

    /// <summary>
    /// Define la estructura de la incidencias de la operación de Acabus.
    /// </summary>
    [Entity(TableName = "Incidences")]
    public sealed class Incidence : NotifyPropertyChanged, IComparable, IComparable<Incidence>
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="AssignedStaff"/>.
        /// </summary>
        private AssignableStaff _assignedStaff;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Device"/>
        /// </summary>
        private Device _device;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Activity"/>
        /// </summary>
        private Activity _activity;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="FinishDate"/>
        /// </summary>
        private DateTime? _finishDate;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Folio"/>
        /// </summary>
        private UInt64 _folio;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="LockAssignation"/>.
        /// </summary>
        private bool _lockAssignation;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Observations"/>
        /// </summary>
        private String _observations;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Priority"/>
        /// </summary>
        private Priority _priority;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="RefundOfMoney"/>
        /// </summary>
        private RefundOfMoney _refundOfMoney;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="StartDate"/>
        /// </summary>
        private DateTime _startDate;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Status"/>
        /// </summary>
        private IncidenceStatus _status;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Technician"/>
        /// </summary>
        private Staff _technician;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="WhoReporting"/>
        /// </summary>
        private String whoReporting;

        /// <summary>
        /// Crea una instancia nueva de <see cref="Incidence"/> especificando el estado de la misma.
        /// </summary>
        /// <param name="folio">Folio de la incidencia.</param>
        /// <param name="status">Estado de la incidencia.</param>
        public Incidence(UInt64 folio, IncidenceStatus status)
        {
            _folio = folio;
            _status = status;
        }

        /// <summary>
        /// Crea una instancia básica de <see cref="Incidence"/>.
        /// </summary>
        public Incidence() : this(0, IncidenceStatus.OPEN) { }

        /// <summary>
        /// Obtiene o establece el personal asignado a esta incidencia.
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_AssignableStaff_ID")]
        public AssignableStaff AssignedStaff {
            get => _assignedStaff;
            set {
                _assignedStaff = value;
                OnPropertyChanged(nameof(AssignedStaff));
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
                OnPropertyChanged(nameof(Device));
            }
        }

        /// <summary>
        /// Obtiene o establece la descripción de la incidencia.
        /// </summary>
        [Column(Name = "Fk_Activity_ID", IsForeignKey = true)]
        public Activity Activity {
            get => _activity;
            set {
                _activity = value;
                OnPropertyChanged(nameof(Activity));
            }
        }

        /// <summary>
        /// Obtiene o establece la fecha de finalización de la incidencia.
        /// </summary>
        public DateTime? FinishDate {
            get => _finishDate;
            set {
                _finishDate = value;
                OnPropertyChanged(nameof(FinishDate));
                OnPropertyChanged(nameof(TotalTime));
            }
        }

        /// <summary>
        /// Obtiene el folio de la incidencia.
        /// </summary>
        [Column(IsPrimaryKey = true, IsAutonumerical = true)]
        public UInt64 Folio {
            get => _folio;
            private set {
                _folio = value;
                OnPropertyChanged(nameof(Folio));
            }
        }

        /// <summary>
        /// Obtiene o establece si la asignación está bloqueada.
        /// </summary>
        public bool LockAssignation {
            get => _lockAssignation;
            set {
                _lockAssignation = value;
                OnPropertyChanged(nameof(LockAssignation));
            }
        }

        /// <summary>
        /// Obtiene o establece las observaciones de la incidencia.
        /// </summary>
        public String Observations {
            get => _observations;
            set {
                _observations = value;
                OnPropertyChanged(nameof(Observations));
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
                OnPropertyChanged(nameof(Priority));
            }
        }

        /// <summary>
        /// Obtiene o establece la devolución de dinero
        /// </summary>
        [Column(ForeignKeyName = "Fk_Incidence_Folio")]
        public RefundOfMoney RefundOfMoney {
            get => _refundOfMoney;
            set {
                _refundOfMoney = value;
                OnPropertyChanged(nameof(RefundOfMoney));
            }
        }

        /// <summary>
        /// Obtiene o establece la fecha y hora de inicio de la incidencia.
        /// </summary>
        public DateTime StartDate {
            get => _startDate;
            set {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
                OnPropertyChanged(nameof(TotalTime));
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
                OnPropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        /// Obtiene o establece el técnico que resolvió la incidencia.
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_Staff_ID")]
        public Staff Technician {
            get => _technician;
            set {
                _technician = value;
                OnPropertyChanged(nameof(Technician));
            }
        }

        /// <summary>
        /// Obtiene el tiempo total de la solución.
        /// </summary>
        [Column(IsIgnored = true)]
        public TimeSpan? TotalTime
            => FinishDate - StartDate;

        /// <summary>
        /// Obtiene o establece quien realiza el reporte.
        /// </summary>
        public String WhoReporting {
            get => whoReporting;
            set {
                whoReporting = value;
                OnPropertyChanged(nameof(WhoReporting));
            }
        }

        /// <summary>
        /// Compara dos instancias de <see cref="Incidence"/> y determina si son diferentes.
        /// </summary>
        /// <param name="left">Una incidencia a comparar.</param>
        /// <param name="right">Otra incidencia a comparar.</param>
        /// <returns>Un valor true si las incidencias son diferentes.</returns>
        public static bool operator !=(Incidence left, Incidence right)
        {
            if (left is null && right is null) return false;
            if (left is null || right is null) return true;

            return !left.Equals(right);
        }

        /// <summary>
        /// Compara dos instancias de <see cref="Incidence"/> y determina si son iguales.
        /// </summary>
        /// <param name="left">Una incidencia a comparar.</param>
        /// <param name="right">Otra incidencia a comparar.</param>
        /// <returns>Un valor true si las incidencias son iguales.</returns>
        public static bool operator ==(Incidence left, Incidence right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Compara la instancia actual con otra y devuelve un entero que indica si la posición de la
        /// instancia actual es anterior, posterior o igual que la del otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="other">Objeto que se va a comparar con esta instancia.</param>
        /// <returns>Un valor que indica el orden relativo de los objetos que se están comparando.</returns>

        public int CompareTo(Incidence other)
        {
            if (other == null) return -1;

            if (Device == other.Device)
                if (Activity == other.Activity)
                    return StartDate.CompareTo(other.StartDate);
                else
                    return Activity.CompareTo(other.Activity);

            return Device.CompareTo(other.Device);
        }

        /// <summary>
        /// Compara la instancia actual con otro objeto del mismo tipo y devuelve un entero que
        /// indica si la posición de la instancia actual es anterior, posterior o igual que la del
        /// otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="obj">Objeto que se va a comparar con esta instancia.</param>
        /// <returns>Un valor que indica el orden relativo de los objetos que se están comparando.</returns>

        public int CompareTo(object obj)
        {
            if (obj == null) return -1;

            if (obj.GetType() != typeof(Incidence)) return -1;

            return CompareTo(obj as Incidence);
        }

        /// <summary>
        /// Determina si la instancia actual es igual al objeto especificado.
        /// </summary>
        /// <param name="obj">Otro objeto a comparar.</param>
        /// <returns>Un valor true si los objetos son iguales.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            Incidence anotherInstance = (Incidence)obj;

            return CompareTo(anotherInstance) == 0;
        }

        /// <summary>
        /// Obtiene el código hash del objeto actual.
        /// </summary>
        /// <returns>Código hash de la instancia.</returns>
        public override int GetHashCode()
            => Tuple.Create(Device, Activity, StartDate).GetHashCode();

        /// <summary>
        /// Indica si la incidencia tiene una devolución de dinero.
        /// </summary>
        /// <returns>Un valor true en caso de presentar una devolución.</returns>
        public Boolean HasRefundOfMoney()
            => RefundOfMoney != null;

        /// <summary>
        /// Representa la instancia de incidencia en una cadena utilizable para subir el reporte.
        /// </summary>
        /// <returns>Una cadena que representa la incidencia.</returns>
        public String ToReportString()
        {
            return String.Format("*{0}* {1} {2} {3} {4}",
                Folio == 0 ? "(No definido)" : String.Format("F-{0:D5}", Folio),
                Device?.Bus != null
                    ? String.Format("{0} {1}",
                        Device?.Bus.EconomicNumber,
                        Device)
                    : Device?.SerialNumber ?? "(No definido)",
                String.Format("*{0}*, {1}", Activity?.Category?.Name, Activity),
                AssignedStaff is null
                ? String.Empty
                : String.Format("\n*Asignado:* {0}", AssignedStaff),
                String.IsNullOrEmpty(Observations?.Trim())
                ? String.Empty
                : String.Format("\n*Observaciones:* {0}", Observations?.Trim().ToUpper()));
        }

        /// <summary>
        /// Representa la instancia actual como una cadena.
        /// </summary>
        /// <returns>Una cadena que representa la instancia actual.</returns>
        public override string ToString()
            => ToReportString();
    }

    /// <summary>
    /// Convertidor para la traducción de la enumeración <see cref="IncidenceStatus"/>.
    /// </summary>
    public sealed class IncidenceStatusConverter : TranslateEnumConverter<IncidenceStatus>
    {
        /// <summary>
        /// Crea una instancia del traductor de la enumaración <see cref="IncidenceStatus"/>.
        /// </summary>
        public IncidenceStatusConverter() : base(new IncidenceStatusTranslator()) { }
    }

    /// <summary>
    /// Representa un traductor al idioma español de la enumeración <see cref="IncidenceStatus"/>.
    /// </summary>
    public sealed class IncidenceStatusTranslator : EnumTranslator<IncidenceStatus>
    {
        /// <summary>
        /// Crea una nueva instancia del traductor.
        /// </summary>
        public IncidenceStatusTranslator() : base(new Dictionary<IncidenceStatus, string>()
        {
            { IncidenceStatus.OPEN, "ABIERTA" },
            { IncidenceStatus.CLOSE, "CERRADA" },
            { IncidenceStatus.UNCOMMIT, "POR CONFIRMAR" },
            { IncidenceStatus.PENDING, "PENDIENTE" }
        })
        { }
    }
}