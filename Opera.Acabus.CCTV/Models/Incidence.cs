using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using Opera.Acabus.Core.Models;
using Opera.Acabus.Core.Models.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Opera.Acabus.Cctv.Models
{
    /// <summary>
    /// Define la estructura de la incidencias de la operación de Acabus.
    /// </summary>
    [Entity(TableName = "Incidences")]
    public sealed class Incidence : AcabusEntityBase, IComparable, IComparable<Incidence>
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="Activity"/>
        /// </summary>
        private Activity _activity;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="AssignedStaff"/>.
        /// </summary>
        private AssignableStaff _assignedStaff;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Comments"/>
        /// </summary>
        private String _comments;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Device"/>
        /// </summary>
        private Device _device;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="FaultObservations"/>
        /// </summary>
        private String _faultObservations;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="FinishDate"/>
        /// </summary>
        private DateTime? _finishDate;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ID"/>
        /// </summary>
        private UInt64 _id;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="LockAssignation"/>.
        /// </summary>
        private bool _lockAssignation;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Priority"/>
        /// </summary>
        private Priority _priority;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="RefundsOfMoney"/>
        /// </summary>
        private ICollection<RefundOfMoney> _refundsOfMoney;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="StartDate"/>
        /// </summary>
        private DateTime _startDate;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Status"/>
        /// </summary>
        private IncidenceStatus _status;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="StaffThatResolve"/>
        /// </summary>
        private Staff _staffThatResolve;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="TrackIncidences"/>
        /// </summary>
        private ICollection<TrackIncidence> _trackIncidences;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="WhoReporting"/>
        /// </summary>
        private String whoReporting;

        /// <summary>
        /// Crea una instancia nueva de <see cref="Incidence"/> especificando el estado de la misma.
        /// </summary>
        /// <param name="id">Folio de la incidencia.</param>
        /// <param name="status">Estado de la incidencia.</param>
        public Incidence(UInt64 id, IncidenceStatus status)
        {
            _id = id;
            _status = status;
        }

        /// <summary>
        /// Crea una instancia básica de <see cref="Incidence"/>.
        /// </summary>
        public Incidence() : this(0, IncidenceStatus.OPEN) { }

        /// <summary>
        /// Obtiene o establece la descripción de la incidencia.
        /// </summary>
        [DbColumn(Name = "Fk_Activity_ID", IsForeignKey = true)]
        public Activity Activity {
            get => _activity;
            set {
                _activity = value;
                OnPropertyChanged(nameof(Activity));
            }
        }

        /// <summary>
        /// Obtiene o establece el personal asignado a esta incidencia.
        /// </summary>
        [DbColumn(IsForeignKey = true, Name = "Fk_AssignableStaff_ID")]
        public AssignableStaff AssignedStaff {
            get => _assignedStaff;
            set {
                _assignedStaff = value;
                OnPropertyChanged(nameof(AssignedStaff));
            }
        }

        /// <summary>
        /// Obtiene o establece el comentario de cierre de la incidencia.
        /// </summary>
        public String Comments {
            get => _comments;
            set {
                _comments = value;
                OnPropertyChanged(nameof(Comments));
            }
        }

        /// <summary>
        /// Obtiene o establece el equipo el cual presenta la incidencia.
        /// </summary>
        [DbColumn(IsForeignKey = true, Name = "Fk_Device_ID")]
        public Device Device {
            get => _device;
            set {
                _device = value;
                OnPropertyChanged(nameof(Device));
            }
        }

        /// <summary>
        /// Obtiene o establece las observaciones de la incidencia.
        /// </summary>
        public String FaultObservations {
            get => _faultObservations;
            set {
                _faultObservations = value;
                OnPropertyChanged(nameof(FaultObservations));
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
        [DbColumn(IsPrimaryKey = true, IsAutonumerical = true)]
        public override UInt64 ID {
            get => _id;
            protected set {
                _id = value;
                OnPropertyChanged(nameof(ID));
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
        /// Obtiene o establece la prioridad de la incidencia.
        /// </summary>
        [DbColumn(Converter = typeof(DbEnumConverter<Priority>))]
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
        [DbColumn(ForeignKeyName = "Fk_Incidence_ID")]
        public ICollection<RefundOfMoney> RefundsOfMoney
            => _refundsOfMoney ?? (_refundsOfMoney = new ObservableCollection<RefundOfMoney>());


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
        /// Obtiene o establece el estado de la incidencia especificadas en la enumeración <seealso cref="IncidenceStatus"/>.
        /// </summary>
        [DbColumn(Converter = typeof(DbEnumConverter<IncidenceStatus>))]
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
        [DbColumn(IsForeignKey = true, Name = "Fk_Staff_ID")]
        public Staff StaffThatResolve {
            get => _staffThatResolve;
            set {
                _staffThatResolve = value;
                OnPropertyChanged(nameof(StaffThatResolve));
            }
        }

        /// <summary>
        /// Obtiene el tiempo total de la solución.
        /// </summary>
        [DbColumn(IsIgnored = true)]
        public TimeSpan? TotalTime
            => FinishDate - StartDate;

        /// <summary>
        /// Obtiene el seguimiento de la incidencia.
        /// </summary>
        [DbColumn(ForeignKeyName = "Fk_Incidences_ID")]
        public ICollection<TrackIncidence> TrackIncidences
            => _trackIncidences ?? (_trackIncidences = new ObservableCollection<TrackIncidence>());

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

            return ID.CompareTo(other.ID);
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
        public Boolean HasRefundsOfMoney()
            => RefundsOfMoney.Count > 0;

        /// <summary>
        /// Representa la instancia de incidencia en una cadena utilizable para subir el reporte.
        /// </summary>
        /// <returns>Una cadena que representa la incidencia.</returns>
        public String ToStringForChat()
        {
            String template = "*F-{0:D5}* {1} {2}\n";
            String templateObs = "*Observaciones:* {0}\n";
            String templateAssign = "*Asignado a:* {0}";
            String templateLocation = "";

            StringBuilder builder = new StringBuilder();

            if (Device?.Bus != null)
                templateLocation = String.Format("{0} {1} {2}",
                    Device.Bus.Route.GetRouteCode(),
                    Device.Bus.EconomicNumber,
                    Device
                    );
            else
                templateLocation = Device.SerialNumber ?? "(No definido)";

            builder.AppendFormat(template, ID, templateLocation, Activity);

            if (Activity.Priority == Priority.HIGH || Priority == Priority.HIGH)
                if (Activity.Priority == Priority)
                    builder.Append("*Requiere atención inmediata* 🆘\n");
                else
                    builder.Append("Atención con prioridad ❗\n");

            if (FaultObservations != null)
                builder.AppendFormat(templateObs, FaultObservations);

            if (AssignedStaff != null)
                builder.AppendFormat(templateAssign, AssignedStaff.Staff);


            return builder.ToString().ToUpper();
        }

        /// <summary>
        /// Representa la instancia actual como una cadena.
        /// </summary>
        /// <returns>Una cadena que representa la instancia actual.</returns>
        public override string ToString()
            => ToStringForChat();
    }
}