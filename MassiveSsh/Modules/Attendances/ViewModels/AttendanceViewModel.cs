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

                CctvService.ToClipboard(SelectedAttendance.OpenedIncidences);
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

        public ICommand OpenedIncidenceToClipboardCommand { get; }

        public ICommand RegisterDepartureCommand { get; }

        public ICommand RegisterEntryCommand { get; }

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
        /// Obtiene una lista de el valor de esta propiedad.
        /// </summary>
        public IEnumerable<WorkShift> Turns => Enum.GetValues(typeof(WorkShift)).Cast<WorkShift>();

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

                if (incidence.AssignedAttendance != null && incidence.AssignedAttendance.Turn
                    != AttendanceService.GetWorkShift(DateTime.Now) && incidence.AssignedAttendance.Turn != WorkShift.OPERATION_SHIT) continue;

                incidence.AssignedAttendance = ViewModelService.GetViewModel<AttendanceViewModel>()?
                    .GetTechnicianAssigned(incidence);
                incidence.Update();
            }
            UpdateCounters();
        }

        /// <summary>
        /// Obtiene la asistencia para la asignación de la incidencia.
        /// </summary>
        public Attendance GetTechnicianAssigned(Incidence incidence)
        {
            if (incidence == null) return null;

            var device = incidence.Device;
            var fault = incidence.Description;
            var startTime = incidence.StartDate;

            if (DateTime.Now.GetWorkShift() != WorkShift.NIGHT_SHIFT
               && device.Type == DeviceType.CONT)
                return null;

            if (DateTime.Now.GetWorkShift() != WorkShift.NIGHT_SHIFT
               && device.Type == DeviceType.MRV)
                return null;

            if (device.Type == DeviceType.PCA && DateTime.Now.GetWorkShift() != WorkShift.NIGHT_SHIFT
                && fault.ID == Core.DataAccess.AcabusData.AllFaults
                    .FirstOrDefault(f => f.Description.ToUpper().Contains("INFORMACIÓN") && f.Category?.DeviceType == DeviceType.PCA)?.ID
                && DateTime.Now.DayOfWeek.Between(DayOfWeek.Monday, DayOfWeek.Friday))
                return null;

            AssignableSection location = (AssignableSection)device?.Station
                ?? device?.Vehicle?.Route ?? null;

            var attendances = Attendances.Where(attendance => attendance.InWorkShift());
            var attendancesPrevious = attendances;

            /// Asignación por area
            attendances = attendances.Where(attendance
               => (fault.Assignable | attendance.Technician.Area) == fault.Assignable);

            if (attendances.Count() == 1)
                return attendances.First();
            else if (attendances.Count() < 1)
                attendances = attendancesPrevious;
            else
                attendancesPrevious = attendances;

            if (AttendanceService.GetWorkShift(DateTime.Now) != WorkShift.NIGHT_SHIFT)
            {
                /// Asignación cuando al menos hay un tecnico en turno.
                WorkShift currenteWorkShift = DateTime.Now.GetWorkShift();
                attendances = attendances
                    .Where(attendance => currenteWorkShift == attendance.Turn
                    || (DateTime.Now.IsOperationWorkShift() && attendance.Turn == WorkShift.OPERATION_SHIT));

                if (attendances.Count() == 1)
                    return attendances.First();
                else if (attendances.Count() < 1)
                    return null;
                else
                    attendancesPrevious = attendances;
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
                  => attendance.Turn == AttendanceService.GetWorkShift(startTime) || startTime.Date < DateTime.Now.Date);

                if (attendances.Count() == 1)
                    return attendances.First();
                else if (attendances.Count() < 1)
                    attendances = attendancesPrevious;
                else
                    attendancesPrevious = attendances;

                /// Asignación por llave.
                if (location is Route)
                    attendances = attendances.Where(attendance
                          => attendance.HasNemaKey || attendance.Section.Contains("AUTOBUSES"));
                else
                    attendances = attendances.Where(attendance
                          => attendance.HasKvrKey || attendance.Section.Contains("ESTACIONES"));

                if (attendances.Count() == 1)
                    return attendances.First();
                else if (attendances.Count() < 1)
                    attendances = attendancesPrevious;
                else
                    attendancesPrevious = attendances;
            }
            else
            {
                if (device.Station != null && !device.Station.Name.Contains("RENACIMIENTO"))
                    return null;

                if (device?.Vehicle?.BusType == VehicleType.ARTICULATED
                       || device?.Vehicle?.BusType == VehicleType.STANDARD)
                    attendances = attendances.Where(attendance
                       => attendance.DateTimeDeparture is null
                           && attendance.Section.Contains("PATIO"));
                else if (device?.Vehicle?.BusType == VehicleType.CONVENTIONAL || device.Station.Name.Contains("RENACIMIENTO"))
                    attendances = attendances.Where(attendance
                       => attendance.DateTimeDeparture is null
                           && attendance.Section.Contains("TERMINAL DE TRANSFERENCIA"));

                if (attendances.Count() == 1)
                    return attendances.First();
                else if (attendances.Count() < 1)
                    return null;
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
                    .GetTechnicianAssigned(incidence);
                incidence.Update();
            }
            UpdateCounters();
        }

        public void UpdateCounters()
        {
            foreach (Attendance attendance in Attendances)
                attendance.CountAssignedIncidences();
        }
    }
}