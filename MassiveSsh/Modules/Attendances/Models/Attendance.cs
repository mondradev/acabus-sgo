using Acabus.Converters;
using Acabus.Modules.CctvReports;
using Acabus.Modules.CctvReports.Models;
using Acabus.Utils;
using Acabus.Utils.Mvvm;
using System;
using System.Collections.Generic;

namespace Acabus.Modules.Attendances.Models
{
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
        /// Campo que provee a la propiedad 'Section'.
        /// </summary>
        private String _section;

        /// <summary>
        /// Campo que provee a la propiedad 'Technician'.
        /// </summary>
        private String _technician;

        /// <summary>
        /// Campo que provee a la propiedad 'Turn'.
        /// </summary>
        private WorkShift _turn;

        public enum WorkShift
        {
            MONING_SHIFT,
            AFTERNOON_SHIFT,
            NIGHT_SHIFT
        }

        /// <summary>
        /// Obtiene el número de incidencias asiganadas cerradas.
        /// </summary>
        public int CountClosedIncidences => Incidences.Where(incidence
            => incidence.Status == IncidenceStatus.CLOSE).Count;

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
        /// Obtiene una lista de las incidencias asignadas.
        /// </summary>
        public ICollection<Incidence> Incidences {
            get {
                return ((ICollection<Incidence>)ViewModelService.GetViewModel<CctvReportsViewModel>().Incidences)
                    .Where(incidence => incidence.AssignedAttendance == Technician
                                       && DateTimeDeparture is null);
            }
        }

        public ICollection<Incidence> OpenedIncidences
            => Incidences.Where(incidence => incidence.Status != IncidenceStatus.CLOSE);

        /// <summary>
        /// Obtiene el número de incidencias asiganadas abiertas.
        /// </summary>
        public int CountOpenedIncidences => Incidences.Where(incidence
            => incidence.Status != IncidenceStatus.CLOSE).Count;

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
        public String Technician {
            get => _technician;
            set {
                _technician = value;
                OnPropertyChanged("Technician");
            }
        }

        /// <summary>
        /// Obtiene o establece el turno asignado.
        /// </summary>
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
    }

    public sealed class TurnsConverter : TranslateEnumConverter<Attendance.WorkShift>
    {
        public TurnsConverter() : base(new Dictionary<Attendance.WorkShift, string>()
        {
            {Attendance.WorkShift.MONING_SHIFT, "MATUTINO" },
            {Attendance.WorkShift.AFTERNOON_SHIFT, "VESPERTINO" },
            {Attendance.WorkShift.NIGHT_SHIFT, "NOCTURNO" }
        })
        {
        }
    }
}