using Acabus.DataAccess;
using Acabus.Models;
using Acabus.Modules.CctvReports.Models;
using Acabus.Modules.CctvReports.Services;
using Acabus.Utils;
using Acabus.Utils.Mvvm;
using Acabus.Window;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
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
        /// Campo que provee a la propiedad 'SelectedIncidence'
        /// </summary>
        private Incidence _selectedIncidence;


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
        public Incidence SelectedIncidence => _selectedIncidence;

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
        /// Campo que provee a la propiedad 'UseParts'.
        /// </summary>
        private Boolean _useParts;

        /// <summary>
        /// Obtiene o establece si la incidencia se resolvió usando refacciones.
        /// </summary>
        public Boolean UseParts {
            get => _useParts;
            set {
                _useParts = value;
                OnPropertyChanged("UseParts");
                if (_useParts) SelectedTab = 1;
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'RefundOfMoney'.
        /// </summary>
        private Boolean _returnCash;

        /// <summary>
        /// Obtiene o establece si la incidencia devuelve dinero.
        /// </summary>
        public Boolean RefundOfMoney {
            get => _returnCash;
            set {
                _returnCash = value;
                OnPropertyChanged("RefundOfMoney");
                if (_returnCash) SelectedTab = 0;
                if (!_returnCash) ClearReturnCashData();
            }
        }

        private void ClearReturnCashData()
        {
            SelectedCashDestiny = null;
            Quantity = String.Empty;
        }

        /// <summary>
        /// Campo que provee a la propiedad 'IsIncidenceFromKvr'.
        /// </summary>
        private Boolean _isIncidenceFromKvr;

        /// <summary>
        /// Obtiene si la incidencia es de KVR.
        /// </summary>
        public Boolean IsIncidenceFromKvr => _isIncidenceFromKvr;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedTab'.
        /// </summary>
        private int _selectedTab;

        /// <summary>
        /// Obtiene o establece el index de la pestaña seleccionada.
        /// </summary>
        public int SelectedTab {
            get => _selectedTab;
            set {
                _selectedTab = value;
                OnPropertyChanged("SelectedTab");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'Quantity'.
        /// </summary>
        private String _quantity;

        /// <summary>
        /// Obtiene o establece la cantidad del dinero por trasladar de los kvrs.
        /// </summary>
        public String Quantity {
            get => _quantity;
            set {
                _quantity = value;
                OnPropertyChanged("Quantity");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'IsMoney'.
        /// </summary>
        private Boolean _isMoney;

        /// <summary>
        /// Obtiene o establece si la devolución de dinero son monedas.
        /// </summary>
        public Boolean IsMoney {
            get => _isMoney;
            set {
                _isMoney = value;
                OnPropertyChanged("IsMoney");
                OnPropertyChanged("Destinies");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedCashDestiny'.
        /// </summary>
        private CashDestiny? _selectedCashDestiny;

        /// <summary>
        /// Obtiene o establece el destino del dinero a devolver.
        /// </summary>
        public CashDestiny? SelectedCashDestiny {
            get => _selectedCashDestiny;
            set {
                _selectedCashDestiny = value;
                OnPropertyChanged("SelectedCashDestiny");
            }
        }

        /// <summary>
        /// Obtiene una lista de destinos posibles para devolución de dinero.
        /// </summary>
        public ICollection<CashDestiny> Destinies => AcabusData.CashDestiny.SelectFromList(cashDestiny =>
        {
            if (IsMoney && cashDestiny.Type == CashType.MONEY)
                return true;
            if (!IsMoney && cashDestiny.Type == CashType.BILL)
                return true;
            if (cashDestiny.Type == CashType.GENERAL)
                return true;
            return false;
        });


        /// <summary>
        /// 
        /// </summary>
        public CloseIncidenceViewModel()
        {
            _finishDate = DateTime.Now;
            _finishTime = DateTime.Now.TimeOfDay;
            _selectedIncidence = ViewModelService.GetViewModel<CctvReportsViewModel>().SelectedIncidence;
            _isIncidenceFromKvr = _selectedIncidence.Device is Kvr;
            _observationes = _selectedIncidence.Observations;
            if (_selectedIncidence.Status == IncidenceStatus.UNCOMMIT)
            {
                _selectedTechnician = _selectedIncidence.Technician;
                _isIncidenceFromKvr = false;
            }

            CloseIncidenceCommand = new CommandBase(CloseIncidenceCommandExecute, CloseIncidenceCommandCanExec);

        }

        private void CloseIncidenceCommandExecute(object parameter)
        {
            var previousStatus = SelectedIncidence.Status;
            SelectedIncidence.Technician = SelectedTechnician;
            SelectedIncidence.Observations = Observations;
            if (SelectedIncidence.Status != IncidenceStatus.UNCOMMIT)
                SelectedIncidence.FinishDate = FinishDate.AddTicks(FinishTime.Ticks);
            else if (SelectedIncidence.Device is Kvr)
                if (!SelectedIncidence.CommitRefund(FinishDate.AddTicks(FinishTime.Ticks)))
                {
                    AcabusControlCenterViewModel.ShowDialog("No se confirmó la incidencia con devolución");
                    ViewModelService.GetViewModel<CctvReportsViewModel>().ReloadData();
                    return;
                }
            SelectedIncidence.Status = IncidenceStatus.CLOSE;

            bool isDone = false;

            if (RefundOfMoney)
            {
                var refund = new RefundOfMoney(SelectedIncidence)
                {
                    CashDestiny = SelectedCashDestiny.Value,
                    Quantity = Single.Parse(Quantity)
                };
                if (SelectedCashDestiny?.Description == "CAU")
                {
                    SelectedIncidence.Status = IncidenceStatus.UNCOMMIT;
                    SelectedIncidence.Priority = Priority.NONE;
                }
                if (String.IsNullOrEmpty(SelectedIncidence.Observations))
                    SelectedIncidence.Observations = String.Format("DEVOLUCIÓN DE {0} (${1:F2}) A {2}",
                        IsMoney ? "MONEDAS" : "BILLETE",
                        Single.Parse(Quantity),
                        SelectedCashDestiny?.Description);
                isDone = refund.Save();
            }
            else
                isDone = SelectedIncidence.Update();

            if (!isDone)
            {
                AcabusControlCenterViewModel.ShowDialog("No se cerró la incidencia");
                ViewModelService.GetViewModel<CctvReportsViewModel>().ReloadData();
            }
            else DialogHost.CloseDialogCommand.Execute(parameter, null);
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
                    if (String.IsNullOrEmpty(SelectedTechnician) || SelectedTechnician == "SISTEMA")
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
                case "SelectedCashDestiny":
                    if (RefundOfMoney && String.IsNullOrEmpty(SelectedCashDestiny?.ToString()))
                        AddError("SelectedCashDestiny", "No ha seleccionado el destino del dinero.");
                    break;
                case "Quantity":
                    if (RefundOfMoney && !Single.TryParse(Quantity, out Single result))
                        AddError("Quantity", "Ingrese una cantidad valida.");
                    break;
                case "Folio":
                    if (SelectedIncidence.Status == IncidenceStatus.CLOSE)
                        AddError("Folio", "Folio cerrado anteriormente.");
                    break;
            }
        }

        private bool Validate()
        {
            ValidateProperty("SelectedTechnician");
            ValidateProperty("FinishDate");
            ValidateProperty("FinishTime");
            ValidateProperty("SelectedCashDestiny");
            ValidateProperty("Quantity");
            ValidateProperty("Folio");

            if (HasErrors) return false;

            return true;
        }
    }
}
