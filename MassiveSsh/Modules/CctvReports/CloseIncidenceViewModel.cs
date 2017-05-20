using Acabus.DataAccess;
using Acabus.Modules.CctvReports.Models;
using Acabus.Modules.CctvReports.Services;
using Acabus.Utils.Mvvm;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Acabus.Modules.CctvReports
{
    public sealed class CloseIncidenceViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad 'EndDate'.
        /// </summary>
        private DateTime _finishDate;

        /// <summary>
        /// Obtiene o establece la fecha de solución de la incidencia a cerrar.
        /// </summary>
        public DateTime FinishDate {

            get => _finishDate;
            set {
                _finishDate = value;
                OnPropertyChanged("FinishDate");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'EndTime'.
        /// </summary>
        private TimeSpan _finishTime;

        /// <summary>
        /// Obtiene o establece la hora de solución de la incidencia a cerrar.
        /// </summary>
        public TimeSpan FinishTime {
            get => _finishTime;
            set {
                _finishTime = value;
                OnPropertyChanged("FinishTime");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'Observations'.
        /// </summary>
        private String _observationes;

        /// <summary>
        /// Obtiene o establece las observaciones con respecto a la incidencia a cerrar.
        /// </summary>
        public String Observations {
            get => _observationes;
            set {
                _observationes = value;
                OnPropertyChanged("Observations");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedTechnician'.
        /// </summary>
        private String _selectedTechnician;


        /// <summary>
        /// Obtiene o establece el nombre de la persona que solucionó la incidencia.
        /// </summary>
        public String SelectedTechnician {
            get => _selectedTechnician;
            set {
                _selectedTechnician = value;
                OnPropertyChanged("SelectedTechnician");
            }
        }

        /// <summary>
        /// Obtiene la incidencia actualmente seleccionada en la tabla principal.
        /// </summary>
        public Incidence SelectedIncidence => ViewModelService.GetViewModel<CctvReportsViewModel>().SelectedIncidence;

        /// <summary>
        /// Obtiene una lista de los técnicos seleccionables.
        /// </summary>
        public ObservableCollection<String> Technicians => AcabusData.Technicians;

        /// <summary>
        /// 
        /// </summary>
        public ICommand CloseIncidenceCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public ICommand UpdateTableCommand => ViewModelService.GetViewModel<CctvReportsViewModel>().UpdateDataCommand;


        /// <summary>
        /// 
        /// </summary>
        public CloseIncidenceViewModel()
        {
            _finishDate = DateTime.Now;
            _finishTime = DateTime.Now.TimeOfDay;

            CloseIncidenceCommand = new CommandBase(CloseIncidenceCommandExecute, CloseIncidenceCommandCanExec);

        }

        private void CloseIncidenceCommandExecute(object parameter)
        {

            SelectedIncidence.Status = IncidenceStatus.CLOSE;
            SelectedIncidence.Technician = SelectedTechnician;
            SelectedIncidence.Observations = Observations;
            SelectedIncidence.FinishDate = FinishDate.AddTicks(FinishTime.Ticks);
            SelectedIncidence.SaveInDataBase();

            DialogHost.CloseDialogCommand.Execute(parameter, null);
        }

        private bool CloseIncidenceCommandCanExec(object parameter)
        {
            if (!Validate()) return false;

            return true;
        }

        protected override void OnValidation(string propertyName)
        {
            switch (propertyName)
            {
                case "SelectedTechnician":
                    if (String.IsNullOrEmpty(SelectedTechnician))
                        AddError("SelectedTechnician", "Falta seleccionar un técnico");
                    break;
                case "FinishDate":
                    if (SelectedIncidence.StartDate.Date > FinishDate.Date)
                        AddError("FinishDate", "La fecha de finalización no puede ser menor que la de reporte");
                    break;
                case "FinishTime":
                    if (SelectedIncidence.StartDate.Date == FinishDate.Date && SelectedIncidence.StartDate.TimeOfDay > FinishTime)
                        AddError("FinishTime", "La hora de finalización no puede ser menor a la de reporte");
                    break;
            }
        }

        private bool Validate()
        {
            ValidateProperty("SelectedTechnician");
            ValidateProperty("FinishDate");
            ValidateProperty("FinishTime");

            if (HasErrors) return false;

            return true;
        }
    }
}
