using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using InnSyTech.Standard.Mvvm;
using InnSyTech.Standard.Mvvm.Utils;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;

namespace Opera.Acabus.Mantto.Models
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
        private ITStaff _technician;

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
        /// Obtiene o establece las observaciones de la asistencia.
        /// </summary>
        public String Observations {
            get => _observations;
            set {
                _observations = value;
                OnPropertyChanged("Observations");
            }
        }        

        /// <summary>
        /// Indica si la asistencia sigue en turno.
        /// </summary>
        /// <returns></returns>
        public bool InWorkShift() => DateTimeDeparture == null;

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
        [Column(IsForeignKey = true, Name = "Fk_ITStaff_ID")]
        public ITStaff Technician {
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