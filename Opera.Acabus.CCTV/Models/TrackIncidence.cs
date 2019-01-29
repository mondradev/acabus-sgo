using InnSyTech.Standard.Database;
using Opera.Acabus.Core.Models;
using Opera.Acabus.Core.Models.Base;
using System;

namespace Opera.Acabus.Cctv.Models
{
    /// <summary>
    /// Representa la historia de una incidencia que se ha re-abierto. Cuando una incidencia se
    /// re-abre se genera un seguimiento capturando datos importantes sobre la indencia en ese momento.
    /// </summary>
    [Entity(TableName = "TrackIncidences")]
    public sealed class TrackIncidence : AcabusEntityBase, IComparable, IComparable<TrackIncidence>
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="Comments"/>
        /// </summary>
        private String _comments;

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
        /// Campo que provee a la propiedad <see cref="Incidence"/>
        /// </summary>
        private Incidence _incidence;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="StartDate"/>
        /// </summary>
        private DateTime _startDate;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="StaffThatResolve"/>
        /// </summary>
        private Staff _technician;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="WhoReporting"/>
        /// </summary>
        private String _whoReporting;

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
            }
        }

        /// <summary>
        /// Obtiene el identificador único del seguimiento de la incidencia.
        /// </summary>
        public override ulong ID {
            get => _id;
            protected set {
                _id = value;
                OnPropertyChanged(nameof(ID));
            }
        }

        /// <summary>
        /// Obtiene la incidencia adjunta.
        /// </summary>
        [DbColumn(IsForeignKey = true, Name = "Fk_Incidence_ID")]
        public Incidence Incidence {
            get => _incidence;
            private set {
                _incidence = value;
                OnPropertyChanged(nameof(Incidence));
            }
        }

        /// <summary>
        /// Obtiene o establece el técnico que resolvió la incidencia.
        /// </summary>
        [DbColumn(IsForeignKey = true, Name = "Fk_Staff_ID")]
        public Staff StaffThatResolve {
            get => _technician;
            set {
                _technician = value;
                OnPropertyChanged(nameof(StaffThatResolve));
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
            }
        }

        /// <summary>
        /// Obtiene o establece quien realiza el reporte.
        /// </summary>
        public String WhoReporting {
            get => _whoReporting;
            set {
                _whoReporting = value;
                OnPropertyChanged(nameof(WhoReporting));
            }
        }

        /// <summary>
        /// Compara la instancia actual con otra y devuelve un entero que indica si la posición de la
        /// instancia actual es anterior, posterior o igual que la del otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="other">Objeto que se va a comparar con esta instancia.</param>
        /// <returns>Un valor que indica el orden relativo de los objetos que se están comparando.</returns>
        public int CompareTo(TrackIncidence other)
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

            if (obj.GetType() != typeof(TrackIncidence)) return -1;

            return CompareTo(obj as TrackIncidence);
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

            TrackIncidence anotherInstance = (TrackIncidence)obj;

            return CompareTo(anotherInstance) == 0;
        }

        /// <summary>
        /// Obtiene el código hash del objeto actual.
        /// </summary>
        /// <returns>Código hash de la instancia.</returns>
        public override int GetHashCode()
            => Tuple.Create(
                Incidence,
                StaffThatResolve,
                StartDate,
                FinishDate,
                Comments,
                FaultObservations
            ).GetHashCode();

        /// <summary>
        /// Representa la instancia actual como una cadena.
        /// </summary>
        /// <returns>Una cadena que representa la instancia actual.</returns>
        public override string ToString()
            => String.Format("TrackIncidence={{Folio=F-{0}, FinishDate={1},  Technician={2}, Comments={3}}}",
                ID, FinishDate, StaffThatResolve, Comments);

        /// <summary>
        /// Crea una nueva instancia de seguimiento.
        /// </summary>
        /// <param name="id">Identificador de la instancia.</param>
        public TrackIncidence(UInt64 id, Incidence incidence)
        {
            ID = id;
            Incidence = incidence;
        }
    }
}