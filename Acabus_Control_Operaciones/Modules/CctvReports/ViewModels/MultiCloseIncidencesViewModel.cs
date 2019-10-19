using Acabus.Models;
using Acabus.Modules.Attendances.ViewModels;
using Acabus.Modules.CctvReports.Models;
using Acabus.Modules.CctvReports.Services;
using Acabus.Modules.Core.DataAccess;
using Acabus.Utils.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Acabus.Modules.CctvReports.ViewModels
{
    public sealed class MultiCloseIncidencesViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="FinishDate" />.
        /// </summary>
        private DateTime _finishDate;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="FinishTime" />.
        /// </summary>
        private TimeSpan _finishTime;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedIncidences" />.
        /// </summary>
        private ICollection<Incidence> _selectedIncidences;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedTechnician" />.
        /// </summary>
        private Technician _technician;

        public MultiCloseIncidencesViewModel()
        {
            _selectedIncidences = new ObservableCollection<Incidence>();

            FinishTime = DateTime.Now.TimeOfDay;
            FinishDate = DateTime.Now;

            CloseIncidenceCommand = new CommandBase(parameter =>
            {
                try
                {
                    foreach (var incidence in _selectedIncidences)
                    {

                        var previousStatus = incidence.Status;
                        incidence.Technician = SelectedTechnician;
                        if (incidence.Status != IncidenceStatus.UNCOMMIT)
                            incidence.FinishDate = FinishDate.AddTicks(FinishTime.Ticks);
                        else if (incidence.Device.Type == DeviceType.KVR)
                            if (!incidence.CommitRefund(FinishDate.AddTicks(FinishTime.Ticks)))
                            {
                                ViewModelService.GetViewModel<CctvReportsViewModel>().ReloadData();
                                throw new Exception($"No se confirmó la incidencia con devolución: {incidence.Folio}");
                            }
                        incidence.Status = IncidenceStatus.CLOSE;

                        bool isDone = false;

                        isDone = incidence.Update();

                        if (!isDone)
                        {
                            ViewModelService.GetViewModel<CctvReportsViewModel>().ReloadData();
                            throw new Exception($"No se cerró la incidencia: {incidence.Folio}");
                        }


                    }
                    ViewModelService.GetViewModel<AttendanceViewModel>()?.UpdateCounters();
                    ViewModelService.GetViewModel<CctvReportsViewModel>()?.UpdateData();
                    Window.AcabusControlCenterViewModel.ShowDialog($"Incidencias cerradas\nTotal: {SelectedIncidences.Count}");
                }
                catch (Exception ex)
                {
                    Window.AcabusControlCenterViewModel.ShowDialog("No se cerraron todas las incidencias, verifique las faltantes.\n" + ex.Message);
                }
            }, parameter =>
            {
                Validate();
                return !HasErrors && SelectedIncidences.Count > 0;
            });

            DiscardIncidenceCommand = new CommandBase(parameter =>
            {
                var incidence = parameter as Incidence;
                _selectedIncidences?.Remove(incidence);
            });
        }

        public ICommand CloseIncidenceCommand { get; }

        public ICommand DiscardIncidenceCommand { get; }

        /// <summary>
        /// Obtiene o establece la fecha de solución.
        /// </summary>
        public DateTime FinishDate {
            get => _finishDate;
            set {
                _finishDate = value;
                OnPropertyChanged(nameof(FinishDate));
            }
        }

        /// <summary>
        /// Obtiene o establece la hora de solución.
        /// </summary>
        public TimeSpan FinishTime {
            get => _finishTime;
            set {
                _finishTime = value;
                OnPropertyChanged(nameof(FinishTime));
            }
        }

        /// <summary>
        /// Obtiene una lista de las incidencias seleccionadas para cerrar.
        /// </summary>
        public ICollection<Incidence> SelectedIncidences
            => _selectedIncidences;

        /// <summary>
        /// Obtiene o establece el técnico que soluciona la incidencia.
        /// </summary>
        public Technician SelectedTechnician {
            get => _technician;
            set {
                _technician = value;
                OnPropertyChanged(nameof(SelectedTechnician));
            }
        }

        /// <summary>
        /// Obtiene una lista de el valor de esta propiedad.
        /// </summary>
        public IEnumerable<Technician> Technicians
            => AcabusData.AllTechnicians.Where(t => !t.Name.Contains("SISTEMA") && t.Enabled);

        protected override void OnLoad(object arg)
        {
            SelectedIncidences.Clear();
            foreach (var incidence in ViewModelService.GetViewModel<CctvReportsViewModel>().SelectedIncidences)
                SelectedIncidences?.Add(incidence);
        }

        protected override void OnValidation(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(SelectedTechnician):
                    if (SelectedTechnician is null)
                        AddError(nameof(SelectedTechnician), "Seleccione el técnico que solucionó la incidencia.");
                    break;

                case nameof(FinishDate):
                    var badDate = false;
                    foreach (var incidence in SelectedIncidences)
                        badDate |= incidence.StartDate.Date > FinishDate.Date;
                    if (badDate)
                        AddError(nameof(FinishDate), "La fecha de solución no puede ser menor a la fecha de incidencia.");
                    break;

                case nameof(FinishTime):
                    var badTime = false;
                    foreach (var incidence in SelectedIncidences)
                        if (incidence.StartDate.Date == FinishDate)
                            badTime |= incidence.StartDate.TimeOfDay > FinishTime;
                    if (badTime)
                        AddError(nameof(FinishTime), "La hora de solución no puede ser menor a la hora de incidencia.");
                    break;
            }
        }

        private void Validate()
        {
            ValidateProperty(nameof(SelectedTechnician));
            ValidateProperty(nameof(FinishDate));
            ValidateProperty(nameof(FinishTime));
        }
    }
}