using Acabus.Models;
using InnSyTech.Debug;
using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
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
    [Entity(TableName = "Incidences")]
    public class Incidence
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Description'.
        /// </summary>
        private String _description;

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
        ///
        /// </summary>
        public Incidence() : this(null) { }

        /// <summary>
        /// Obtiene o establece la descripción de la incidencia.
        /// </summary>
        public String Description {
            get => _description?.ToUpper();
            set {
                _description = value;
            }
        }

        /// <summary>
        /// Obtiene o establece la fecha de finalización de la incidencia.
        /// </summary>
        [Column(Converter = typeof(SQLiteDateTimeConverter))]
        public DateTime? FinishDate {
            get => _finishDate;
            set {
                _finishDate = value;
            }
        }

        /// <summary>
        /// Obtiene el folio de la incidencia.
        /// </summary>
        [Column(IsPrimaryKey = true)]
        public String Folio {
            get => _folio;
            private set => _folio = value;
        }

        /// <summary>
        /// Obtiene o establece las observaciones de la incidencia.
        /// </summary>
        public String Observations {
            get => _observations;
            set {
                _observations = value;
            }
        }

        /// <summary>
        /// Obtiene o establece la prioridad de la incidencia.
        /// </summary>
        [Column(Converter = typeof(PriorityDbConverter))]
        public Priority Priority {
            get => _priority;
            set {
                _priority = value;
            }
        }
        /// <summary>
        /// Obtiene o establece la fecha y hora de inicio de la incidencia.
        /// </summary>
        [Column(Converter = typeof(SQLiteDateTimeConverter))]
        public DateTime StartDate {
            get => _startDate;
            set {
                _startDate = value;
            }
        }
        /// <summary>
        /// Obtiene o establece el estado de la incidencia (Abierta|Cerrada).
        /// </summary>
        [Column(Converter = typeof(IncidenceStatusDbConverter))]
        public IncidenceStatus Status {
            get => _status;
            set {
                _status = value;
            }
        }

        /// <summary>
        /// Obtiene o establece el técnico que resolvió la incidencia.
        /// </summary>
        public String Technician {
            get => _technician;
            set {
                _technician = value;
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
            }
        }

        public override string ToString()
        {
            return $"{Folio} - {Description}";
        }
    }

    public class PriorityDbConverter : DbEnumConverter<Priority> { }

    public class IncidenceStatusDbConverter : DbEnumConverter<IncidenceStatus> { }

}