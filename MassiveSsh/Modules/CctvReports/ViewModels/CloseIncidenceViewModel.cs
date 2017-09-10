using Acabus.Models;
using Acabus.Modules.Attendances.ViewModels;
using Acabus.Modules.CctvReports.Models;
using Acabus.Modules.CctvReports.Services;
using Acabus.Utils.Mvvm;
using Acabus.Window;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Campo que provee a la propiedad 'EndTime'.
        /// </summary>
        private TimeSpan _finishTime;

        /// <summary>
        /// Campo que provee a la propiedad 'IsIncidenceFromKvr'.
        /// </summary>
        private Boolean _isIncidenceFromKvr;

        /// <summary>
        /// Campo que provee a la propiedad 'IsMoney'.
        /// </summary>
        private Boolean _isMoney;

        /// <summary>
        /// Campo que provee a la propiedad 'Observations'.
        /// </summary>
        private String _observationes;

        /// <summary>
        /// Campo que provee a la propiedad 'Quantity'.
        /// </summary>
        private String _quantity;

        /// <summary>
        /// Campo que provee a la propiedad 'RefundOfMoney'.
        /// </summary>
        private Boolean _returnCash;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedCashDestiny'.
        /// </summary>
        private CashDestiny _selectedCashDestiny;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedIncidence'
        /// </summary>
        private Incidence _selectedIncidence;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedTab'.
        /// </summary>
        private int _selectedTab;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedTechnician'.
        /// </summary>
        private Technician _selectedTechnician;

        /// <summary>
        /// Campo que provee a la propiedad 'UseParts'.
        /// </summary>
        private Boolean _useParts;

        /// <summary>
        ///
        /// </summary>
        public CloseIncidenceViewModel()
        {
            _finishDate = DateTime.Now;
            _finishTime = DateTime.Now.TimeOfDay;
            _selectedIncidence = ViewModelService.GetViewModel<CctvReportsViewModel>().SelectedIncidence;
            _isIncidenceFromKvr = _selectedIncidence.Device.Type == DeviceType.KVR;
            _observationes = _selectedIncidence.Observations;
            if (_selectedIncidence.Status == IncidenceStatus.UNCOMMIT)
            {
                _selectedTechnician = _selectedIncidence.Technician;
                _isIncidenceFromKvr = false;
            }

            CloseIncidenceCommand = new CommandBase(CloseIncidenceCommandExecute, CloseIncidenceCommandCanExec);
        }

        /// <summary>
        /// Obtiene el titulo del tipo de dinero.
        /// </summary>
        public String CashTypeName => IsMoney ? "MONEDAS" : "BILLETES";

        /// <summary>
        ///
        /// </summary>
        public ICommand CloseIncidenceCommand { get; }

        /// <summary>
        /// Obtiene una lista de destinos posibles para devolución de dinero.
        /// </summary>
        public IEnumerable<CashDestiny> Destinies => Core.DataAccess.AcabusData.AllCashDestinies
            .Where(cashDestiny =>
            {
                if (IsMoney && (cashDestiny as CashDestiny).CashType == CashType.MONEY)
                    return true;
                if (!IsMoney && (cashDestiny as CashDestiny).CashType == CashType.BILL)
                    return true;
                return false;
            });

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
                OnPropertyChanged("IsMoney");
                OnPropertyChanged("Destinies");
                OnPropertyChanged("CashTypeName");
            }
        }

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

        /// <summary>
        /// Obtiene o establece el destino del dinero a devolver.
        /// </summary>
        public CashDestiny SelectedCashDestiny {
            get => _selectedCashDestiny;
            set {
                _selectedCashDestiny = value;
                OnPropertyChanged("SelectedCashDestiny");
            }
        }

        /// <summary>
        /// Obtiene la incidencia actualmente seleccionada en la tabla principal.
        /// </summary>
        public Incidence SelectedIncidence => _selectedIncidence;

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
        /// Obtiene o establece el nombre de la persona que solucionó la incidencia.
        /// </summary>
        public Technician SelectedTechnician {
            get => _selectedTechnician;
            set {
                _selectedTechnician = value;
                OnPropertyChanged("SelectedTechnician");
            }
        }

        /// <summary>
        /// Obtiene una lista de los técnicos seleccionables.
        /// </summary>
        public IEnumerable<Technician> Technicians => Core.DataAccess.AcabusData.AllTechnicians
            .Where(technicia => technicia.Name != "SISTEMA" && technicia.Enabled);

        /// <summary>
        ///
        /// </summary>
        public ICommand UpdateTableCommand => ViewModelService.GetViewModel<CctvReportsViewModel>().UpdateDataCommand;

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

        protected override void OnValidation(string propertyName)
        {
            switch (propertyName)
            {
                case "SelectedTechnician":
                    if (SelectedTechnician is null || SelectedTechnician?.Name == "SISTEMA")
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

        private void ClearReturnCashData()
        {
            SelectedCashDestiny = null;
            Quantity = String.Empty;
        }

        private bool CloseIncidenceCommandCanExec(object parameter)
        {
            if (!Validate()) return false;

            return true;
        }

        private void CloseIncidenceCommandExecute(object parameter)
        {
            var previousStatus = SelectedIncidence.Status;
            SelectedIncidence.Technician = SelectedTechnician;
            SelectedIncidence.Observations = Observations;
            if (SelectedIncidence.Status != IncidenceStatus.UNCOMMIT)
                SelectedIncidence.FinishDate = FinishDate.AddTicks(FinishTime.Ticks);
            else if (SelectedIncidence.Device.Type == DeviceType.KVR)
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
                    CashDestiny = SelectedCashDestiny,
                    Quantity = Single.Parse(Quantity)
                };
                if (SelectedCashDestiny?.Description == "CAU")
                {
                    SelectedIncidence.Status = IncidenceStatus.UNCOMMIT;
                    SelectedIncidence.Priority = Priority.NONE;
                }
                else
                    refund.RefundDate = SelectedIncidence.FinishDate;

                if (String.IsNullOrEmpty(SelectedIncidence.Observations))
                    SelectedIncidence.Observations = String.Format("DEVOLUCIÓN DE {0} (${1:F2}) A {2}",
                        IsMoney ? "MONEDAS" : "BILLETE",
                        Single.Parse(Quantity),
                        SelectedCashDestiny?.Description);
                isDone = refund.Save();
            }
            isDone = SelectedIncidence.Update();

            if (!isDone)
            {
                AcabusControlCenterViewModel.ShowDialog("No se cerró la incidencia");
                ViewModelService.GetViewModel<CctvReportsViewModel>().ReloadData();
            }
            else
                AcabusControlCenterViewModel.ShowDialog($"Incidencia '{SelectedIncidence.Folio}' cerrada correctamente");
            ViewModelService.GetViewModel<AttendanceViewModel>()?.UpdateCounters();
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