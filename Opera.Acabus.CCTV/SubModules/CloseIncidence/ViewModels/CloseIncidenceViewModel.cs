﻿using InnSyTech.Standard.Database.Linq;
using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Cctv.Models;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Gui.Modules;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Opera.Acabus.Cctv.SubModules.CloseIncidences.ViewModels
{
    /// <summary>
    /// Define el modelo de la vista <see cref="Views.CloseIncidenceView"/>.
    /// </summary>
    public sealed class CloseIncidenceViewModel : ModuleViewerBase
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="FinishDate" />.
        /// </summary>
        private DateTime _finishDate;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="IsCommit" />.
        /// </summary>
        private bool _isCommit;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="IsIncidenceFromKvr" />.
        /// </summary>
        private Boolean _isIncidenceFromKvr;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="IsMoney" />.
        /// </summary>
        private Boolean _isMoney;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Observations" />.
        /// </summary>
        private String _observationes;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Quantity" />.
        /// </summary>
        private String _quantity;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="IsRefundOfMoney" />.
        /// </summary>
        private Boolean _returnCash;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedCashDestiny" />.
        /// </summary>
        private CashDestiny _selectedCashDestiny;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedIncidence'
        /// </summary>
        private Incidence _selectedIncidence;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedTab" />.
        /// </summary>
        private int _selectedTab;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedTechnician" />.
        /// </summary>
        private Staff _selectedTechnician;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="UseParts" />.
        /// </summary>
        private Boolean _useParts;

        /// <summary>
        /// Crea una nueva instancia del modelo de la vista <see cref="Views.CloseIncidenceView"/>.
        /// </summary>
        public CloseIncidenceViewModel()
        {
            CloseIncidenceCommand = new Command(CloseIncidenceCommandExecute, CloseIncidenceCommandCanExec);
        }

        /// <summary>
        /// Obtiene el titulo del tipo de dinero.
        /// </summary>
        public String CashTypeName => IsMoney ? "Monedas" : "Billetes";

        /// <summary>
        /// Comando que cierra la incidencias.
        /// </summary>
        public ICommand CloseIncidenceCommand { get; }

        /// <summary>
        /// Obtiene una lista de destinos posibles para devolución de dinero.
        /// </summary>
        public IEnumerable<CashDestiny> Destinies => AcabusDataContext.DbContext.Read<CashDestiny>()
            .Where(cD => (IsMoney && cD.CashType == CashType.MONEY) || (!IsMoney && cD.CashType == CashType.BILL));

        /// <summary>
        /// Obtiene o establece la fecha de solución de la incidencia a cerrar.
        /// </summary>
        public DateTime FinishDate {
            get => _finishDate;
            set {
                _finishDate = value;
                OnPropertyChanged(nameof(FinishDate));
            }
        }

        /// <summary>
        /// Obtiene o establece si esta es una confirmación de incidencia.
        /// </summary>
        public bool IsCommit {
            get => _isCommit;
            set {
                _isCommit = value;
                OnPropertyChanged(nameof(IsCommit));
            }
        }

        /// <summary>
        /// Obtiene si la incidencia es de KVR.
        /// </summary>
        public Boolean IsIncidenceFromKvr => _isIncidenceFromKvr;

        /// <summary>
        /// Obtiene o establece si la devolución de dinero son monedas.
        /// </summary>
        public Boolean IsMoney {
            get => _isMoney;
            set {
                _isMoney = value;
                OnPropertyChanged(nameof(IsMoney));
                OnPropertyChanged(nameof(Destinies));
                OnPropertyChanged(nameof(CashTypeName));
            }
        }

        /// <summary>
        /// Obtiene o establece si la incidencia devuelve dinero.
        /// </summary>
        public Boolean IsRefundOfMoney {
            get => _returnCash;
            set {
                _returnCash = value;
                OnPropertyChanged(nameof(IsRefundOfMoney));
                if (_returnCash) SelectedTab = 0;
                if (!_returnCash) ClearReturnCashData();
            }
        }

        /// <summary>
        /// Obtiene o establece las observaciones con respecto a la incidencia a cerrar.
        /// </summary>
        public String Observations {
            get => _observationes;
            set {
                _observationes = value;
                OnPropertyChanged(nameof(Observations));
            }
        }

        /// <summary>
        /// Obtiene o establece la cantidad del dinero por trasladar de los kvrs.
        /// </summary>
        public String Quantity {
            get => _quantity;
            set {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        /// <summary>
        /// Obtiene o establece el destino del dinero a devolver.
        /// </summary>
        public CashDestiny SelectedCashDestiny {
            get => _selectedCashDestiny;
            set {
                _selectedCashDestiny = value;
                OnPropertyChanged(nameof(SelectedCashDestiny));
            }
        }

        /// <summary>
        /// Obtiene la incidencia actualmente seleccionada en la tabla principal.
        /// </summary>
        public Incidence SelectedIncidence {
            get => _selectedIncidence;
            set {
                _selectedIncidence = value;
                OnPropertyChanged(nameof(SelectedIncidence));
            }
        }

        /// <summary>
        /// Obtiene o establece el index de la pestaña seleccionada.
        /// </summary>
        public int SelectedTab {
            get => _selectedTab;
            set {
                _selectedTab = value;
                OnPropertyChanged(nameof(SelectedTab));
            }
        }

        /// <summary>
        /// Obtiene o establece el nombre de la persona que solucionó la incidencia.
        /// </summary>
        public Staff SelectedTechnician {
            get => _selectedTechnician;
            set {
                _selectedTechnician = value;
                OnPropertyChanged(nameof(SelectedTechnician));
            }
        }

        /// <summary>
        /// Obtiene una lista de los técnicos seleccionables.
        /// </summary>
        public IEnumerable<Staff> Technicians => AcabusDataContext.AllStaff
            .Where(technicia => technicia.Name != "SISTEMA" && technicia.Active);

        /// <summary>
        /// Obtiene o establece si la incidencia se resolvió usando refacciones.
        /// </summary>
        public Boolean UseParts {
            get => _useParts;
            set {
                _useParts = value;
                OnPropertyChanged(nameof(UseParts));
                if (_useParts) SelectedTab = 1;
            }
        }

        /// <summary>
        /// Inicializa todo los valores de las propiedades al momento de la carga de la vista.
        /// </summary>
        /// <param name="parameter">Parametro del comando.</param>
        protected override void OnLoad(object parameter)
        {
            _finishDate = DateTime.Now;
            _isIncidenceFromKvr = _selectedIncidence.Device.Type == DeviceType.KVR;
            _observationes = _selectedIncidence.Observations;
            if (_selectedIncidence.Status == IncidenceStatus.UNCOMMIT)
            {
                _selectedTechnician = _selectedIncidence.Technician;
                _isCommit = _isIncidenceFromKvr;
                _isIncidenceFromKvr = false;
            }

            OnPropertyChanged(nameof(FinishDate));
            OnPropertyChanged(nameof(IsIncidenceFromKvr));
            OnPropertyChanged(nameof(Observations));
            OnPropertyChanged(nameof(SelectedTechnician));
            OnPropertyChanged(nameof(IsCommit));
        }

        /// <summary>
        /// Valida la propiedad que ha cambiado su valor.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad.</param>
        protected override void OnValidation(string propertyName)
        {
            if (SelectedIncidence == null)
                return;

            switch (propertyName)
            {
                case nameof(SelectedTechnician):
                    if (SelectedTechnician is null || SelectedTechnician?.Name == "SISTEMA")
                        AddError(nameof(SelectedTechnician), "Falta seleccionar un técnico");
                    break;

                case nameof(FinishDate):
                    if (SelectedIncidence.StartDate.Date > FinishDate.Date)
                        AddError(nameof(FinishDate), "La fecha de finalización no puede ser menor que la de reporte");
                    if (SelectedIncidence.StartDate.Date == FinishDate.Date && SelectedIncidence.StartDate.TimeOfDay > FinishDate.TimeOfDay)
                        AddError(nameof(FinishDate), "La hora de finalización no puede ser menor a la de reporte");
                    break;

                case nameof(SelectedCashDestiny):
                    if (IsRefundOfMoney && String.IsNullOrEmpty(SelectedCashDestiny?.ToString()))
                        AddError(nameof(SelectedCashDestiny), "No ha seleccionado el destino del dinero.");
                    break;

                case nameof(Quantity):
                    if (IsRefundOfMoney && !Single.TryParse(Quantity, out Single result))
                        AddError(nameof(Quantity), "Ingrese una cantidad valida.");
                    break;

                case nameof(SelectedIncidence):
                    if (SelectedIncidence.Status == IncidenceStatus.CLOSE)
                        AddError(nameof(SelectedIncidence), "Folio cerrado anteriormente.");
                    break;
            }
        }

        /// <summary>
        /// Limpia los campos de la información referente a una devolución.
        /// </summary>
        private void ClearReturnCashData()
        {
            SelectedCashDestiny = null;
            Quantity = String.Empty;
        }

        /// <summary>
        /// Determina si es posible cerrar la incidencia.
        /// </summary>
        /// <param name="parameter">Parametro del comando.</param>
        /// <returns>Un valor true si es posible cerrar la incidencia.</returns>
        private bool CloseIncidenceCommandCanExec(object parameter)
            => Validate();

        /// <summary>
        /// Cierra la incidencia actualmente modificada por la vista.
        /// </summary>
        /// <param name="parameter">Parametro del comando.</param>
        private void CloseIncidenceCommandExecute(object parameter)
        {
            var previousStatus = SelectedIncidence.Status;
            SelectedIncidence.Technician = SelectedTechnician;
            SelectedIncidence.Observations = Observations;
            SelectedIncidence.Status = IncidenceStatus.CLOSE;
            SelectedIncidence.FinishDate = FinishDate;

            if (SelectedIncidence.Device.Type == DeviceType.KVR && previousStatus == IncidenceStatus.UNCOMMIT)
            {
                Models.RefundOfMoney refundOfMoney = AcabusDataContext.DbContext.Read<Models.RefundOfMoney>().LoadReference(2)
                    .FirstOrDefault(r => r.Incidence.Folio == SelectedIncidence.Folio);

                if (refundOfMoney != null)
                {
                    refundOfMoney.RefundDate = FinishDate;
                    refundOfMoney.Incidence.FinishDate = FinishDate;
                    refundOfMoney.Incidence.Technician = SelectedTechnician;
                    refundOfMoney.Incidence.Observations = Observations;
                    refundOfMoney.Incidence.Status = IncidenceStatus.CLOSE;

                    if (AcabusDataContext.DbContext.Update(refundOfMoney, 1))
                    {
                        Dispatcher.CloseDialog();
                        ShowMessage(String.Format("Devolución correcta: F-{0:D5}", SelectedIncidence.Folio));
                    }
                    else
                    {
                        Dispatcher.CloseDialog();
                        ShowMessage(String.Format("No se confirmó la devolución de dinero F-{0:D5}", SelectedIncidence.Folio));
                    }
                }
                else
                {
                    Dispatcher.CloseDialog();
                    ShowMessage("Existe un problema con la devolución, contacte a soporte técnico.");
                }
            }
            else if (IsRefundOfMoney)
            {
                var refundOfMoney = new Models.RefundOfMoney(SelectedIncidence)
                {
                    CashDestiny = SelectedCashDestiny,
                    Quantity = Single.Parse(Quantity)
                };

                if (SelectedCashDestiny.RequiresMoving)
                {
                    SelectedIncidence.Status = IncidenceStatus.UNCOMMIT;
                    SelectedIncidence.Priority = Priority.NONE;
                    refundOfMoney.Status = RefundStatus.FOR_DELIVERY;
                }
                else
                {
                    refundOfMoney.Status = RefundStatus.DELIVERED;
                    refundOfMoney.RefundDate = FinishDate;
                }

                if (String.IsNullOrEmpty(Observations))
                    SelectedIncidence.Observations = String.Format("DEVOLUCIÓN DE {0} {1:C} A {2}",
                        CashTypeName, refundOfMoney.Quantity, refundOfMoney.CashDestiny.Description);

                if (AcabusDataContext.DbContext.Create(refundOfMoney))
                    if (AcabusDataContext.DbContext.Update(SelectedIncidence))
                    {
                        Dispatcher.CloseDialog();
                        ShowMessage(String.Format("Incidencia cerrada correctamente: F-{0:D5}", SelectedIncidence.Folio));
                    }
                    else
                    {
                        AcabusDataContext.DbContext.Delete(refundOfMoney);
                        Dispatcher.CloseDialog();
                        ShowMessage("Error al cerrar la incidencia, intentelo de nuevo.");
                    }
            }
            else if (AcabusDataContext.DbContext.Update(SelectedIncidence))
            {
                Dispatcher.CloseDialog();
                ShowMessage(String.Format("Incidencia cerrada correctamente: F-{0:D5}", SelectedIncidence.Folio));
            }
            else
            {
                Dispatcher.CloseDialog();
                ShowMessage("Error al cerrar la incidencia, intentelo de nuevo.");
            }
        }

        /// <summary>
        /// Valida los campos requeridos del formulario de la vista.
        /// </summary>
        /// <returns>Un valor true en caso de ser todos validos.</returns>
        private bool Validate()
        {
            ValidateProperty(nameof(SelectedTechnician));
            ValidateProperty(nameof(FinishDate));
            ValidateProperty(nameof(SelectedCashDestiny));
            ValidateProperty(nameof(Quantity));
            ValidateProperty(nameof(SelectedIncidence));

            return !HasErrors;
        }
    }
}