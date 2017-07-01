using Acabus.DataAccess;
using Acabus.Models;
using Acabus.Modules.Attendances.Models;
using Acabus.Modules.Attendances.Services;
using Acabus.Modules.CctvReports.Services;
using Acabus.Utils.Mvvm;
using Acabus.Window;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Acabus.Modules.Attendances.ViewModels
{
    public class AttendanceEntryViewModel : ViewModelBase
    {
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
        private Technician _technician;

        /// <summary>
        /// Campo que provee a la propiedad 'TimeEntry'.
        /// </summary>
        private TimeSpan _timeEntry;

        /// <summary>
        /// Campo que provee a la propiedad 'Turn'.
        /// </summary>
        private Models.Attendance.WorkShift? _turn;

        public AttendanceEntryViewModel()
        {
            RegisterEntryCommand = new CommandBase(RegisterEntryExecute, RegisterEntryCanExecute);
            _timeEntry = DateTime.Now.TimeOfDay;
        }

        /// <summary>
        /// Obtiene una lista de asistencia.
        /// </summary>
        public ObservableCollection<Attendance> Attendances
            => ViewModelService.GetViewModel<AttendanceViewModel>().Attendances;

        /// <summary>
        /// Obtiene o establece si el técnico tiene asiganda una llave de Kvr.
        /// </summary>
        public Boolean HasKvrKey {
            get => _hasKvrKey;
            set {
                _hasKvrKey = value;
                OnPropertyChanged("HasKvrKey");
            }
        }

        /// <summary>
        /// Obtiene o establece si el técnico tiene asignada una llave de caja nema.
        /// </summary>
        public Boolean HasNemaKey {
            get => _hasNemaKey;
            set {
                _hasNemaKey = value;
                OnPropertyChanged("HasNemaKey");
            }
        }

        public ICommand RegisterEntryCommand { get; }

        /// <summary>
        /// Obtiene o establece el tramo asignado
        /// </summary>
        public String Section {
            get => _section;
            set {
                _section = value;
                OnPropertyChanged("Section");
            }
        }

        public IEnumerable<String> Sections =>
            AcabusData.Sections
            .Where(section =>
            {
                if (section.Contains("SUPERVICIÓN"))
                    return true;
                if (Turn == Attendance.WorkShift.NIGHT_SHIFT)
                    return section.Contains("PATIO") || section.Contains("TERMINAL");
                else
                    return !section.Contains("PATIO") && !section.Contains("TERMINAL");
            });

        /// <summary>
        /// Obtiene o establece el nombre del técnico a entrar.
        /// </summary>
        public Technician Technician {
            get => _technician;
            set {
                _technician = value;
                OnPropertyChanged("Technician");
            }
        }

        public IEnumerable<Technician> Technicians
            => Core.DataAccess.AcabusData.AllTechnicians.Where(technicia => technicia.Name != "SISTEMA");

        /// <summary>
        /// Obtiene o establece la hora de entrada del técnico.
        /// </summary>
        public TimeSpan TimeEntry {
            get => _timeEntry;
            set {
                _timeEntry = value;
                OnPropertyChanged("TimeEntry");
            }
        }

        /// <summary>
        /// Obtiene o establece el turno de trabajo.
        /// </summary>
        public Models.Attendance.WorkShift? Turn {
            get => _turn;
            set {
                _turn = value;
                OnPropertyChanged("Turn");
                OnPropertyChanged("Sections");
                OnPropertyChanged("TimeEntry");
            }
        }

        /// <summary>
        /// Obtiene una lista de los turnos de trabajo disponibles.
        /// </summary>
        public IEnumerable<Attendance.WorkShift> Turns
            => Enum.GetValues(typeof(Attendance.WorkShift)).Cast<Attendance.WorkShift>();

        /// <summary>
        /// Determina si es posible registrar la asistencia validando los campos.
        /// </summary>
        public Boolean RegisterEntryCanExecute(object parameter)
        {
            return Validate();
        }

        /// <summary>
        /// Registra una asistencia en el sistema.
        /// </summary>
        public void RegisterEntryExecute(object parameter)
        {
            var attendance = new Attendance()
            {
                Technician = Technician,
                Turn = Turn.Value,
                DateTimeEntry = DateTime.Now.Date.AddTicks(TimeEntry.Ticks),
                HasKvrKey = HasKvrKey,
                HasNemaKey = HasNemaKey,
                Section = Section
            };

            if (attendance.Save())
            {
                Attendances.Add(attendance);
                ViewModelService.GetViewModel<AttendanceViewModel>()?.AssignTechnicianIncoming();
            }
            else
                AcabusControlCenterViewModel.ShowDialog("No se registro la asistencia, intentelo de nuevo");

            DialogHost.CloseDialogCommand.Execute(parameter, null);
        }

        /// <summary>
        /// Valida si los campos requeridos estan completos.
        /// </summary>
        public bool Validate()
        {
            ValidateProperty("Technician");
            ValidateProperty("Section");
            ValidateProperty("Turn");
            ValidateProperty("TimeEntry");

            return !HasErrors;
        }

        protected override void OnValidation(string propertyName)
        {
            switch (propertyName)
            {
                case "Technician":
                    if (Technician is null)
                        AddError("Technician", "Falta seleccionar el técnico que ingresa.");
                    if (Attendances.Where(attendance => attendance.DateTimeDeparture is null
                            && attendance.Technician?.Name == Technician?.Name).Count() > 0)
                        AddError("Technician", "Ya hay una asistencia en curso del técnico.");
                    break;

                case "Section":
                    if (String.IsNullOrEmpty(Section))
                        AddError("Section", "Falta seleccionar el tramo.");
                    break;

                case "Turn":
                    if (Turn is null)
                        AddError("Turn", "Falta seleccionar el turno al que ingresa.");
                    break;

                case "TimeEntry":
                    if (Turn != null)
                    {
                        Boolean error = false;
                        Attendance.WorkShift turn = AttendanceService.GetWorkShift(DateTime.Now.Date.AddTicks(TimeEntry.Ticks));

                        if (turn == Attendance.WorkShift.MONING_SHIFT && Turn == Attendance.WorkShift.NIGHT_SHIFT)
                            error = true;
                        if (turn == Attendance.WorkShift.AFTERNOON_SHIFT && Turn == Attendance.WorkShift.MONING_SHIFT)
                            error = true;
                        if (turn == Attendance.WorkShift.NIGHT_SHIFT && Turn == Attendance.WorkShift.AFTERNOON_SHIFT)
                            error = true;

                        if (error)
                            AddError("TimeEntry", "La hora de entrada no es valida.");
                    }
                    if (DateTime.Now < DateTime.Now.Date.AddTicks(TimeEntry.Ticks))
                        AddError("TimeEntry", "La hora de entrada no puede ser en el futuro.");
                    break;
            }
        }
    }
}