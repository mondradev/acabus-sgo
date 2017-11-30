using InnSyTech.Standard.Database.Linq;
using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Cctv.Models;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Gui.Modules;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Opera.Acabus.Cctv.SubModules.CloseIncidences.ViewModels
{
    /// <summary>
    /// Define el modelo de la vista <see cref="Views.MultiCloseIncidencesView"/> , permitiendo cerrar multiples incidencias a la vez.
    /// </summary>
    public sealed class MultiCloseIncidencesViewModel : ModuleViewerBase
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="FinishDate" />.
        /// </summary>
        private DateTime _finishDate;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedIncidences" />.
        /// </summary>
        private ICollection<Incidence> _selectedIncidences;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedTechnician" />.
        /// </summary>
        private Staff _technician;

        /// <summary>
        /// Crea una nueva instancia.
        /// </summary>
        public MultiCloseIncidencesViewModel()
        {
            _selectedIncidences = new ObservableCollection<Incidence>();

            FinishDate = DateTime.Now;

            CloseIncidenceCommand = new Command(parameter =>
            {
                foreach (var incidence in _selectedIncidences)
                {
                    var previousStatus = incidence.Status;

                    incidence.Status = IncidenceStatus.CLOSE;
                    incidence.Technician = SelectedTechnician;
                    incidence.FinishDate = FinishDate;

                    if (previousStatus == IncidenceStatus.UNCOMMIT && incidence.Device.Type == DeviceType.KVR)
                    {
                        Models.RefundOfMoney refundOfMoney = AcabusDataContext.DbContext.Read<Models.RefundOfMoney>().LoadReference(2)
                            .FirstOrDefault(r => r.Incidence.Folio == incidence.Folio);

                        if (refundOfMoney != null)
                        {
                            refundOfMoney.RefundDate = FinishDate;
                            refundOfMoney.Incidence.FinishDate = FinishDate;
                            refundOfMoney.Incidence.Observations = incidence.Observations;
                            refundOfMoney.Incidence.Status = IncidenceStatus.CLOSE;

                            if (!AcabusDataContext.DbContext.Update(refundOfMoney, 1))
                            {
                                Dispatcher.CloseDialog();
                                ShowMessage(String.Format("No se confirmó la devolución de dinero F-{0:D5}", incidence.Folio));
                                return;
                            }
                        }
                        else
                        {
                            Dispatcher.CloseDialog();
                            ShowMessage("Existe un problema con la devolución, contacte a soporte técnico.");
                            return;
                        }
                    }
                    else
                    {
                        if (!AcabusDataContext.DbContext.Update(incidence))
                        {
                            Dispatcher.CloseDialog();
                            ShowMessage(String.Format("No se cerró la incidencia: F-{0:D5}", incidence.Folio));
                            return;
                        }
                    }
                }
                Dispatcher.CloseDialog();
                ShowMessage("Se cerraron las incidencias satisfactoriamente");
            }, parameter =>
            {
                Validate();
                return !HasErrors && SelectedIncidences.Count > 0;
            });

            DiscardIncidenceCommand = new Command(parameter =>
            {
                var incidence = parameter as Incidence;
                _selectedIncidences?.Remove(incidence);
            });

            DiscardChangeCommand = Dispatcher.CloseDialogCommand;
        }

        /// <summary>
        /// Comando para cerrar las incidencias.
        /// </summary>
        public ICommand CloseIncidenceCommand { get; }

        /// <summary>
        /// Comando para descartar los cambios.
        /// </summary>
        public ICommand DiscardChangeCommand { get; }

        /// <summary>
        /// Comando para descartar la incidencia.
        /// </summary>
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
        /// Obtiene una lista de las incidencias seleccionadas para cerrar.
        /// </summary>
        public ICollection<Incidence> SelectedIncidences {
            get => _selectedIncidences;
            set {
                _selectedIncidences = value;
                OnPropertyChanged(nameof(SelectedIncidences));
            }
        }

        /// <summary>
        /// Obtiene o establece el técnico que soluciona la incidencia.
        /// </summary>
        public Staff SelectedTechnician {
            get => _technician;
            set {
                _technician = value;
                OnPropertyChanged(nameof(SelectedTechnician));
            }
        }

        /// <summary>
        /// Obtiene una lista de el valor de esta propiedad.
        /// </summary>
        public IEnumerable<Staff> Technicians
            => AcabusDataContext.AllStaff.Where(t => t.Name != "SISTEMA" && t.Active == true);

        /// <summary>
        /// Valida el valor de la propiedad que ha cambiado.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad.</param>
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
            }
        }

        /// <summary>
        /// Provoca la validacione de las propiedades requeridas.
        /// </summary>
        private void Validate()
        {
            ValidateProperty(nameof(SelectedTechnician));
            ValidateProperty(nameof(FinishDate));
        }
    }
}