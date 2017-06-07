using Acabus.DataAccess;
using Acabus.Modules.Attendances.Models;
using Acabus.Modules.Attendances.Services;
using Acabus.Modules.CctvReports.Models;
using Acabus.Modules.CctvReports.Services;
using Acabus.Utils.Mvvm;
using Acabus.Window;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Acabus.Modules.Attendances.ViewModels
{
    public sealed class AttendanceModifyViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Attendance'.
        /// </summary>
        private Attendance _attendance;

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

        public AttendanceModifyViewModel()
        {
            _attendance = ViewModelService.GetViewModel<AttendanceViewModel>().SelectedAttendance;
            _section = _attendance?.Section;
            _hasKvrKey = _attendance is null ? false : _attendance.HasKvrKey;
            _hasNemaKey = _attendance is null ? false : _attendance.HasNemaKey;

            ModifyCommand = new CommandBase(ModifyExecute);
        }

        private void ModifyExecute(object obj)
        {
            Attendance.Section = Section;
            Attendance.HasKvrKey = HasKvrKey;
            Attendance.HasNemaKey = HasNemaKey;

            if (Attendance.Update())
            {
                IEnumerable<Incidence> openedIncidences
                    = ViewModelService.GetViewModel<CctvReports.CctvReportsViewModel>()?.IncidencesOpened;

                foreach (Incidence incidence in openedIncidences)
                {
                    incidence.AssignedAttendance = ViewModelService.GetViewModel<AttendanceViewModel>()?
                        .GetTechnicianAssigned(incidence.Device.Station, incidence.Device, incidence.StartDate);
                    incidence.Update();
                }

                ViewModelService.GetViewModel<AttendanceViewModel>()?.UpdateCounters();
            }
            else
                AcabusControlCenterViewModel.ShowDialog("Error al actualizar la asignación.");
            DialogHost.CloseDialogCommand.Execute(obj, null);
        }

        /// <summary>
        /// Obtiene o establece la asistencia a modificar.
        /// </summary>
        public Attendance Attendance {
            get => _attendance;
            set {
                _attendance = value;
                OnPropertyChanged("Attendance");
            }
        }

        /// <summary>
        /// Obtiene o establece si el tecnico tiene asignada una llave de Kvr.
        /// </summary>
        public Boolean HasKvrKey {
            get => _hasKvrKey;
            set {
                _hasKvrKey = value;
                OnPropertyChanged("HasKvrKey");
            }
        }

        /// <summary>
        /// Obtiene o establece si el tecnico tiene asignada una llave de nema.
        /// </summary>
        public Boolean HasNemaKey {
            get => _hasNemaKey;
            set {
                _hasNemaKey = value;
                OnPropertyChanged("HasNemaKey");
            }
        }

        /// <summary>
        /// Obtiene o establece el tramo asignado.
        /// </summary>
        public String Section {
            get => _section;
            set {
                _section = value;
                OnPropertyChanged("Section");
            }
        }

        public IEnumerable<String> Sections => AcabusData.Sections;

        public ICommand ModifyCommand { get; private set; }
    }
}