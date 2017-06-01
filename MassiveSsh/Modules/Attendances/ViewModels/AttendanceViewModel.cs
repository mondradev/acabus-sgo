using Acabus.DataAccess;
using Acabus.Models;
using Acabus.Modules.Attendances.Models;
using Acabus.Modules.Attendances.Services;
using Acabus.Modules.Attendances.Views;
using Acabus.Utils;
using Acabus.Utils.Mvvm;
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
                DialogHost.OpenDialogCommand.Execute(new AttendanceModifyView(), null);
            });
            RegisterEntryCommand = new CommandBase(parameter =>
            {
                DialogHost.OpenDialogCommand.Execute(new AttendanceEntryView(), null);
            });
        }

        public ICommand ModifyAttendanceCommand { get; }

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
        /// Obtiene la asistencia para la asignación de la incidencia.
        /// </summary>
        public String GetTechnicianAssigned(Location location, Device device, DateTime startTime)
        {

            /// Asignación cuando al menos hay un tecnico en turno.
            var attendances = (IEnumerable<Attendance>)Attendances
                .Where(attendance => attendance.DateTimeDeparture is null);
            var attendancesPrevious = attendances;

            if (attendances.Count() == 1)
                return attendances.First()?.Technician;
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
                    return attendances.First()?.Technician;
                else if (attendances.Count() < 1)
                    attendances = attendancesPrevious;
                else
                    attendancesPrevious = attendances;

                /// Asignación por turno.
                attendances = attendances.Where(attendance
                  => attendance.Turn == AttendanceService.GetTurn(startTime));

                if (attendances.Count() == 1)
                    return attendances.First()?.Technician;
                else if (attendances.Count() < 1)
                    attendances = attendancesPrevious;
                else
                    attendancesPrevious = attendances;

                /// Asignación por llave.
                if (device is Vehicle)
                    attendances = attendances.Where(attendance
                          => attendance.HasNemaKey);
                else
                    attendances = attendances.Where(attendance
                          => attendance.HasKvrKey);

                if (attendances.Count() == 1)
                    return attendances.First()?.Technician;
                else if (attendances.Count() < 1)
                    attendances = attendancesPrevious;
                else
                    attendancesPrevious = attendances;
            }
            else
            {
                if (device.GetType() != typeof(Vehicle) || !location.Name.Contains("RENA"))
                    return null;

                if ((device as Vehicle).BusType == VehicleType.ARTICULATED
                       || (device as Vehicle).BusType == VehicleType.STANDARD)
                    attendances = attendances.Where(attendance
                       => attendance.DateTimeDeparture is null
                           && attendance.Section.Contains("PATIO"));
                else if ((device as Vehicle).BusType == VehicleType.CONVENTIONAL)
                    attendances = attendances.Where(attendance
                       => attendance.DateTimeDeparture is null
                           && attendance.Section.Contains("RENA"));

                if (attendances.Count() == 1)
                    return attendances.First()?.Technician;
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
            })?.Technician;
        }

        public void UpdateCounters()
        {
            foreach (Attendance attendance in Attendances)
                attendance.CountAssignedIncidences();
        }
    }
}