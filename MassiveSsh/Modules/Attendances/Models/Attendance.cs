using Acabus.Converters;
using Acabus.Models;
using Acabus.Modules.CctvReports;
using Acabus.Modules.CctvReports.Models;
using Acabus.Utils.Mvvm;
using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Acabus.Modules.Attendances.Models
{
    [Entity(TableName = "Attendances")]
    public sealed class Attendance : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'DateTimeDeparture'.
        /// </summary>
        private DateTime? _dateTimeDeparture;

        /// <summary>
        /// Campo que provee a la propiedad 'DateTimeEntry'.
        /// </summary>
        private DateTime? _dateTimeEntry;

        /// <summary>
        /// Campo que provee a la propiedad 'HasKvrKey'.
        /// </summary>
        private Boolean _hasKvrKey;

        /// <summary>
        /// Campo que provee a la propiedad 'HasNemaKey'.
        /// </summary>
        private Boolean _hasNemaKey;

        /// <summary>
        /// Campo que provee a la propiedad 'ID'.
        /// </summary>
        private UInt64 _id;

        /// <summary>
        /// Campo que provee a la propiedad 'Observations'.
        /// </summary>
        private String _observations;

        /// <summary>
        /// Campo que provee a la propiedad 'Section'.
        /// </summary>
        private String _section;

        /// <summary>
        /// Campo que provee a la propiedad 'Technician'.
        /// </summary>
        private Technician _technician;

        /// <summary>
        /// Campo que provee a la propiedad 'Turn'.
        /// </summary>
        private WorkShift _turn;

        /// <summary>
        /// Define los turnos posibles.
        /// </summary>
        public enum WorkShift
        {
            /// <summary>
            /// Matutino
            /// </summary>
            MONING_SHIFT,

            /// <summary>
            /// Vespertino
            /// </summary>
            AFTERNOON_SHIFT,

            /// <summary>
            /// Nocturno
            /// </summary>
            NIGHT_SHIFT,

            /// <summary>
            /// Vespertino
            /// </summary>
            OPERATION_SHIT
        }

        /// <summary>
        /// Obtiene el número de incidencias asiganadas cerradas.
        /// </summary>
        [Column(IsIgnored = true)]
        public int CountClosedIncidences {
            get => Incidences is null ? 0 : Incidences.Where(incidence => incidence.Status == IncidenceStatus.CLOSE).Count();
            private set { }
        }

        /// <summary>
        /// Obtiene el número de incidencias asiganadas abiertas.
        /// </summary>
        [Column(Name = "OpenedIncidences")]
        public int CountOpenedIncidences {
            get => Incidences is null ? 0 : Incidences.Where(incidence => incidence.Status != IncidenceStatus.CLOSE).Count();
            private set { }
        }

        /// <summary>
        /// Obtiene o establece la fecha/hora de salida de la asistencia.
        /// </summary>
        public DateTime? DateTimeDeparture {
            get => _dateTimeDeparture;
            set {
                _dateTimeDeparture = value;
                OnPropertyChanged("DateTimeDeparture");
            }
        }

        /// <summary>
        /// Obtiene o establece la fecha/hora de entrada de la asistencia.
        /// </summary>
        public DateTime? DateTimeEntry {
            get => _dateTimeEntry;
            set {
                _dateTimeEntry = value;
                OnPropertyChanged("DateTimeEntry");
            }
        }

        /// <summary>
        /// Obtiene o establece si la asistencia presenta una llave de Kvr.
        /// </summary>
        public Boolean HasKvrKey {
            get => _hasKvrKey;
            set {
                _hasKvrKey = value;
                OnPropertyChanged("HasKvrKey");
            }
        }

        /// <summary>
        /// Obtiene o establece si la asistencia presenta una llave de caja nema.
        /// </summary>
        public Boolean HasNemaKey {
            get => _hasNemaKey;
            set {
                _hasNemaKey = value;
                OnPropertyChanged("HasNemaKey");
            }
        }

        /// <summary>
        /// Obtiene o establece el idenficador unico de la instancia.
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
        /// Obtiene una lista de las incidencias asignadas.
        /// </summary>
        [Column(IsIgnored = true)]
        public IEnumerable<Incidence> Incidences {
            get => ViewModelService.GetViewModel<CctvReportsViewModel>()?.Incidences?
                    .Where(incidence => incidence.AssignedAttendance?.Technician == Technician
                                       && DateTimeDeparture is null)
                .Cast<Incidence>();
        }

        /// <summary>
        /// Obtiene o establece las observaciones de la asistencia.
        /// </summary>
        public String Observations {
            get => _observations;
            set {
                _observations = value;
                OnPropertyChanged("Observations");
            }
        }

        [Column(IsIgnored = true)]
        public IEnumerable<Incidence> OpenedIncidences
                    => Incidences?.Where(incidence => incidence.Status != IncidenceStatus.CLOSE);

        /// <summary>
        /// Obtiene o establece el tramo asignado.
        /// </summary>
        public String Section {
            get => _section?.ToUpper();
            set {
                _section = value;
                OnPropertyChanged("Section");
            }
        }

        /// <summary>
        /// Obtiene o establece el técnico de la asistencia.
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_technician_ID")]
        public Technician Technician {
            get => _technician;
            set {
                _technician = value;
                OnPropertyChanged("Technician");
            }
        }

        /// <summary>
        /// Obtiene o establece el turno asignado.
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<WorkShift>))]
        public WorkShift Turn {
            get => _turn;
            set {
                _turn = value;
                OnPropertyChanged("Turn");
            }
        }

        /// <summary>
        /// Realiza el conteo de las incidencias asignadas.
        /// </summary>
        public void CountAssignedIncidences()
        {
            OnPropertyChanged("Incidences");
            OnPropertyChanged("OpenedIncidences");
            OnPropertyChanged("CountOpenedIncidences");
            OnPropertyChanged("CountClosedIncidences");
        }

        public override string ToString() => Technician.Name;
    }

    public sealed class TurnsConverter : TranslateEnumConverter<Attendance.WorkShift>
    {
        public TurnsConverter() : base(new Dictionary<Attendance.WorkShift, string>()
        {
            {Attendance.WorkShift.MONING_SHIFT, "MATUTINO" },
            {Attendance.WorkShift.AFTERNOON_SHIFT, "VESPERTINO" },
            {Attendance.WorkShift.NIGHT_SHIFT, "NOCTURNO" },
            {Attendance.WorkShift.OPERATION_SHIT, "OPERACIÓN" }
        })
        {
        }
    }
}