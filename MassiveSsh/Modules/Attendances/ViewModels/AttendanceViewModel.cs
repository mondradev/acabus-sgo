using Acabus.Models;
using Acabus.Modules.Attendances.Models;
using Acabus.Modules.Attendances.Services;
using Acabus.Modules.Attendances.Views;
using Acabus.Modules.CctvReports.Models;
using Acabus.Modules.CctvReports.Services;
using Acabus.Utils;
using Acabus.Utils.Mvvm;
using Acabus.Window;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using static Acabus.Modules.Attendances.Models.Attendance;

namespace Acabus.Modules.Attendances.ViewModels
{
    public sealed class AttendanceViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Attendances'.
        /// </summary>
        private ObservableCollection<Attendance> _attendances;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedAttendance'.
        /// </summary>
        private Attendance _selectedAttendance;

        public AttendanceViewModel()
        {
            ViewModelService.Register(this);

            Attendances.LoadFromDataBase();

            RegisterDepartureCommand = new CommandBase(parameter =>
            {
                if (SelectedAttendance is null) return;
                if (SelectedAttendance.DateTimeDeparture is null)
                    DialogHost.OpenDialogCommand.Execute(new AttendanceDepartureView(), null);
            });
            ModifyAttendanceCommand = new CommandBase(parameter =>
            {
                if (SelectedAttendance is null) return;
                if (SelectedAttendance.DateTimeDeparture != null) return;
                DialogHost.OpenDialogCommand.Execute(new AttendanceModifyView(), null);
            });
            RegisterEntryCommand = new CommandBase(parameter =>
            {
                DialogHost.OpenDialogCommand.Execute(new AttendanceEntryView(), null);
            });
            OpenedIncidenceToClipboardCommand = new CommandBase(parameter =>
            {
                if (SelectedAttendance is null) return;
                if (SelectedAttendance.CountOpenedIncidences == 0)
                {
                    AcabusControlCenterViewModel.ShowDialog("No hay incidencias asignadas.");
                    return;
                }
                StringBuilder openedIncidence = new StringBuilder();
                foreach (Incidence incidence in SelectedAttendance.OpenedIncidences)
                    openedIncidence.AppendLine(incidence.ToReportString().Split('\n')?[0]);
                openedIncidence.AppendFormat("*ASIGNADO:* {0}", SelectedAttendance.Technician);

                try
                {
                    System.Windows.Forms.Clipboard.Clear();
                    System.Windows.Forms.Clipboard.SetDataObject(openedIncidence.ToString());
                }
                catch { }
            });
        }

        ~AttendanceViewModel()
        {
            ViewModelService.UnRegister(this);
        }

        /// <summary>
        /// Obtiene una lista de la asistencia de los técnicos.
        /// </summary>
        public ObservableCollection<Attendance> Attendances {
            get {
                if (_attendances == null)
                    _attendances = new ObservableCollection<Attendance>();
                return _attendances;
            }
        }

        public ICommand ModifyAttendanceCommand { get; }

        public ICommand RegisterDepartureCommand { get; }

        public ICommand RegisterEntryCommand { get; }

        public ICommand OpenedIncidenceToClipboardCommand { get; }


        /// <summary>
        /// Obtiene o establece la asistencia seleccionada en la tabla.
        /// </summary>
        public Attendance SelectedAttendance {
            get => _selectedAttendance;
            set {
                _selectedAttendance = value;
                OnPropertyChanged("SelectedAttendance");
            }
        }

        /// <summary>
        /// Reasigna tecnicos a las incidencias abiertas.
        /// </summary>
        public void ReassignTechnician()
        {
            IEnumerable<Incidence> openedIncidences
                   = ViewModelService.GetViewModel<CctvReports.CctvReportsViewModel>()?.IncidencesOpened;

            foreach (Incidence incidence in openedIncidences)
            {
                if (incidence.Status != IncidenceStatus.OPEN) continue;

                incidence.AssignedAttendance = ViewModelService.GetViewModel<AttendanceViewModel>()?
                    .GetTechnicianAssigned(incidence.Device, incidence.StartDate);
                incidence.Update();
            }
            UpdateCounters();
        }

        /// <summary>
        /// Reasigna tecnicos a las incidencias abiertas.
        /// </summary>
        public void AssignTechnicianIncoming()
        {
            IEnumerable<Incidence> openedIncidences
                   = ViewModelService.GetViewModel<CctvReports.CctvReportsViewModel>()?.IncidencesOpened;

            foreach (Incidence incidence in openedIncidences)
            {
                if (incidence.Status != IncidenceStatus.OPEN) continue;
                if (AttendanceService.GetTurn(incidence.StartDate)
                    != AttendanceService.GetTurn(DateTime.Now)) continue;

                incidence.AssignedAttendance = ViewModelService.GetViewModel<AttendanceViewModel>()?
                    .GetTechnicianAssigned(incidence.Device, incidence.StartDate);
                incidence.Update();
            }
            UpdateCounters();
        }

        /// <summary>
        /// Obtiene una lista de el valor de esta propiedad.
        /// </summary>
        public IEnumerable<WorkShift> Turns => Enum.GetValues(typeof(WorkShift)).Cast<WorkShift>();

        /// <summary>
        /// Obtiene la asistencia para la asignación de la incidencia.
        /// </summary>
        public Attendance GetTechnicianAssigned(Device device, DateTime startTime)
        {
            AssignableSection location = (AssignableSection)device?.Station
                ?? (device is DeviceBus ? (device as DeviceBus)?.Vehicle?.Route : null);

            /// Asignación cuando al menos hay un tecnico en turno.
            var attendances = (IEnumerable<Attendance>)Attendances
                .Where(attendance => attendance.DateTimeDeparture is null
                && AttendanceService.GetTurn(DateTime.Now) == attendance.Turn);
            var attendancesPrevious = attendances;

            if (attendances.Count() == 1)
                return attendances.First();
            else if (attendances.Count() < 1)
                return null;
            else
                attendancesPrevious = attendances;

            if (AttendanceService.GetTurn(DateTime.Now) != WorkShift.NIGHT_SHIFT)
            {
                /// Asignación por tramo.
                attendances = attendances.Where(attendance
                   => attendance.DateTimeDeparture is null
                       && attendance.Section == location.Section);

                if (attendances.Count() == 1)
                    return attendances.First();
                else if (attendances.Count() < 1)
                    attendances = attendancesPrevious;
                else
                    attendancesPrevious = attendances;

                /// Asignación por turno.
                attendances = attendances.Where(attendance
                  => attendance.Turn == AttendanceService.GetTurn(startTime));

                if (attendances.Count() == 1)
                    return attendances.First();
                else if (attendances.Count() < 1)
                    attendances = attendancesPrevious;
                else
                    attendancesPrevious = attendances;

                /// Asignación por llave.
                if (location is Vehicle)
                    attendances = attendances.Where(attendance
                          => attendance.HasNemaKey || attendance.Section.Contains("AUTOBUS"));
                else
                    attendances = attendances.Where(attendance
                          => attendance.HasKvrKey || attendance.Section.Contains("ESTACI"));

                if (attendances.Count() == 1)
                    return attendances.First();
                else if (attendances.Count() < 1)
                    attendances = attendancesPrevious;
                else
                    attendancesPrevious = attendances;
            }
            else
            {
                if (location.GetType() != typeof(Vehicle) && !location.Name.Contains("RENA"))
                    return null;

                //if ((location as Vehicle).BusType == VehicleType.ARTICULATED
                //       || (location as Vehicle).BusType == VehicleType.STANDARD)
                //    attendances = attendances.Where(attendance
                //       => attendance.DateTimeDeparture is null
                //           && attendance.Section.Contains("PATIO"));
                //else if ((location as Vehicle).BusType == VehicleType.CONVENTIONAL)
                //    attendances = attendances.Where(attendance
                //       => attendance.DateTimeDeparture is null
                //           && attendance.Section.Contains("RENA"));

                if (attendances.Count() == 1)
                    return attendances.First();
                else if (attendances.Count() < 1)
                    attendances = attendancesPrevious;
                else
                    attendancesPrevious = attendances;
            }

            /// Asignación por carga.
            return attendances.Aggregate((attendance1, attendance2)
                =>
            {
                if (attendance1.CountOpenedIncidences > attendance2.CountOpenedIncidences)
                    return attendance2;
                else
                    return attendance1;
            });
        }

        public void UpdateCounters()
        {
            foreach (Attendance attendance in Attendances)
                attendance.CountAssignedIncidences();
        }
    }
}