using Acabus.Modules.Attendances.Models;
using Acabus.Modules.Attendances.Services;
using Acabus.Modules.CctvReports.Models;
using Acabus.Modules.CctvReports.Services;
using Acabus.Utils.Mvvm;
using Acabus.Window;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Acabus.Modules.Attendances.ViewModels
{
    public sealed class AttendanceDepartureViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Attendance'.
        /// </summary>
        private Attendance _attendance;

        /// <summary>
        /// Campo que provee a la propiedad 'Observations'.
        /// </summary>
        private String _observations;

        /// <summary>
        /// Campo que provee a la propiedad 'TimeDeparture'.
        /// </summary>
        private TimeSpan _timeDeparture;

        public AttendanceDepartureViewModel()
        {
            _attendance = ViewModelService.GetViewModel<AttendanceViewModel>().SelectedAttendance;
            _timeDeparture = DateTime.Now.TimeOfDay;
            RegisterDepartureCommand = new CommandBase(RegisterDepartureExecute, RegisterDepartureCanExecute);
        }

        /// <summary>
        /// Obtiene o establece la asistencia a registrar salida.
        /// </summary>
        public Attendance Attendance {
            get => _attendance;
            set {
                _attendance = value;
                OnPropertyChanged("Attendance");
            }
        }

        /// <summary>
        /// Obtiene o establece las observaciones.
        /// </summary>
        public String Observations {
            get => _observations;
            set {
                _observations = value;
                OnPropertyChanged("Observations");
            }
        }

        public ICommand RegisterDepartureCommand { get; }

        /// <summary>
        /// Obtiene o establece la hora de salida del técnico.
        /// </summary>
        public TimeSpan TimeDeparture {
            get => _timeDeparture;
            set {
                _timeDeparture = value;
                OnPropertyChanged("TimeDeparture");
            }
        }

        protected override void OnValidation(string propertyName)
        {
            switch (propertyName)
            {
                case "TimeDeparture":
                    if (Attendance.DateTimeEntry > DateTime.Now.Date.AddTicks(TimeDeparture.Ticks))
                        AddError("TimeDeparture", "La hora de salida no es valida.");
                    break;
            }
        }

        private bool RegisterDepartureCanExecute(object parameter)
        {
            ValidateProperty("TimeDeparture");

            return !HasErrors && Attendance.DateTimeDeparture is null;
        }

        private void RegisterDepartureExecute(object parameter)
        {
            IEnumerable<Incidence> openedIncidences = Attendance.OpenedIncidences.ToArray();
            Attendance.DateTimeDeparture = DateTime.Now.Date.AddTicks(TimeDeparture.Ticks);
            Attendance.Observations = Observations;
            if (Attendance.Update())
            {

                foreach (Incidence incidence in openedIncidences)
                {
                    incidence.AssignedAttendance = ViewModelService.GetViewModel<AttendanceViewModel>()?
                        .GetTechnicianAssigned(incidence.Device, incidence.StartDate, incidence.Description);

                    incidence.Update();
                }

                ViewModelService.GetViewModel<AttendanceViewModel>()?.UpdateCounters();

                DialogHost.CloseDialogCommand.Execute(parameter, null);
            }
            else
                AcabusControlCenterViewModel.ShowDialog("Error al guardar la asistencia, intentelo de nuevo.");
        }
    }
}