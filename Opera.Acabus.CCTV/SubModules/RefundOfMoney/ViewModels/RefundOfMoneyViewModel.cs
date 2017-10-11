using InnSyTech.Standard.Mvvm;
using InnSyTech.Standard.Utils;
using Opera.Acabus.Cctv.Models;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Gui.Modules;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace Opera.Acabus.Cctv.SubModules.RefundOfMoney.ViewModels
{
    /// <summary>
    /// Define el modelo de la vista <see cref=" Views.RefundOfMoneyView" />.
    /// </summary>
    public sealed class RefundOfMoneyViewModel : ModuleViewerBase
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="AllDestinies" />.
        /// </summary>
        private ICollection<CashDestiny> _allDestinies;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="AllKvrs" />.
        /// </summary>
        private ICollection<Device> _allKvrs;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="AllStations" />.
        /// </summary>
        private ICollection<Station> _allStations;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="AllTechnician" />.
        /// </summary>
        private ICollection<Staff> _allTechnician;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="IsMoney" />.
        /// </summary>
        private bool _isMoney;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Observations" />.
        /// </summary>
        private String _observations;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Quantity" />.
        /// </summary>
        private String _quantity;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="RefundDateTime" />.
        /// </summary>
        private DateTime _refundDateTime;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedDestiny" />.
        /// </summary>
        private CashDestiny _selectedDestiny;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedKvr" />.
        /// </summary>
        private Device _selectedKvr;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedStation" />.
        /// </summary>
        private Station _selectedStation;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedTechnician" />.
        /// </summary>
        private Staff _selectedTechnician;

        /// <summary>
        /// Crea una nueva instancia de <see cref="RefundOfMoneyViewModel" />.
        /// </summary>
        public RefundOfMoneyViewModel()
        {
            DiscardCommand = new Command(Dispatcher.CloseDialog);
            CreateRefundCommand = new Command(CreateRefund, CanCreate);
        }

        /// <summary>
        /// Obtiene una lista de todos los destino de dinero disponibles.
        /// </summary>
        public ICollection<CashDestiny> AllDestinies
            => _allDestinies ?? (_allDestinies = new ObservableCollection<CashDestiny>());

        /// <summary>
        /// Obtiene una lista de todos los KVRs.
        /// </summary>
        public ICollection<Device> AllKvrs
            => _allKvrs ?? (_allKvrs = new ObservableCollection<Device>());

        /// <summary>
        /// Obtiene una lista de todas las estaciones.
        /// </summary>
        public ICollection<Station> AllStations
            => _allStations ?? (_allStations = new ObservableCollection<Station>());

        /// <summary>
        /// Obtiene una lista de todos los técnicos.
        /// </summary>
        public ICollection<Staff> AllTechnician
            => _allTechnician ?? (_allTechnician = new ObservableCollection<Staff>());

        /// <summary>
        /// Comando que confirma los cambios y crea la devolución de dinero.
        /// </summary>
        public ICommand CreateRefundCommand { get; }

        /// <summary>
        /// Comando que anula los cambios y finaliza el cuadro de dialogo.
        /// </summary>
        public ICommand DiscardCommand { get; }

        /// <summary>
        /// Obtiene o establece si la devolución es de monedas.
        /// </summary>
        public bool IsMoney {
            get => _isMoney;
            set {
                _isMoney = value;
                OnPropertyChanged(nameof(IsMoney));

                var cashDestinies = AcabusDataContext.DbContext.Read<CashDestiny>();

                if (value)
                    _allDestinies = cashDestinies.Where(cd => cd.CashType == CashType.MONEY).ToList();
                else
                    _allDestinies = cashDestinies.Where(cd => cd.CashType == CashType.BILL).ToList();

                OnPropertyChanged(nameof(AllDestinies));
            }
        }

        /// <summary>
        /// Obtiene o establece las observaciones de la devolución.
        /// </summary>
        public String Observations {
            get => _observations;
            set {
                _observations = value;
                OnPropertyChanged(nameof(Observations));
            }
        }

        /// <summary>
        /// Obtiene o establece el monto a devolver.
        /// </summary>
        public String Quantity {
            get => _quantity;
            set {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        /// <summary>
        /// Obtiene o establece la fecha y hora de la devolución de dinero.
        /// </summary>
        public DateTime RefundDateTime {
            get => _refundDateTime;
            set {
                _refundDateTime = value;
                OnPropertyChanged(nameof(RefundDateTime));
            }
        }

        /// <summary>
        /// Obtiene o establece el destino del dinero.
        /// </summary>
        public CashDestiny SelectedDestiny {
            get => _selectedDestiny;
            set {
                _selectedDestiny = value;
                OnPropertyChanged(nameof(SelectedDestiny));
            }
        }

        /// <summary>
        /// Obtiene o establece el kiosko seleccionado.
        /// </summary>
        public Device SelectedKvr {
            get => _selectedKvr;
            set {
                _selectedKvr = value;
                if (_selectedStation == null && value != null)
                    SelectedStation = value.Station;
                OnPropertyChanged(nameof(SelectedKvr));
            }
        }

        /// <summary>
        /// Obtiene o establece la estación seleccionada.
        /// </summary>
        public Station SelectedStation {
            get => _selectedStation;
            set {
                _selectedStation = value;
                OnPropertyChanged(nameof(SelectedStation));

                if (value != null && _selectedKvr == null)
                {
                    _allKvrs = new ObservableCollection<Device>(AcabusDataContext
                        .AllDevices.Where(d => d.Station.ID == value.ID && d.Type == DeviceType.KVR));
                    OnPropertyChanged(nameof(AllKvrs));
                }
            }
        }

        /// <summary>
        /// Obtiene o establece el técnico seleccionado para la devolución.
        /// </summary>
        public Staff SelectedTechnician {
            get => _selectedTechnician;
            set {
                _selectedTechnician = value;
                OnPropertyChanged(nameof(SelectedTechnician));
            }
        }

        /// <summary>
        /// Método invocado cuando ocurre la carga del visor.
        /// </summary>
        /// <param name="parameter"> Parametro del comando </param>
        protected override void OnLoad(object parameter)
        {
            RefundDateTime = DateTime.Now;

            _allStations = new ObservableCollection<Station>(AcabusDataContext.AllStations);
            _allKvrs = new ObservableCollection<Device>(AcabusDataContext.AllDevices.Where(d => d.Type == DeviceType.KVR));
            _allTechnician = new ObservableCollection<Staff>(AcabusDataContext.AllStaff);
            _allDestinies = AcabusDataContext.DbContext.Read<CashDestiny>().Where(cd => cd.CashType == CashType.BILL).ToList();

            OnPropertyChanged(nameof(AllStations));
            OnPropertyChanged(nameof(AllKvrs));
            OnPropertyChanged(nameof(AllDestinies));
            OnPropertyChanged(nameof(AllTechnician));
        }

        /// <summary>
        /// Valida el valor de la propiedad, esta función es llamada cuando el valor de la propiedad cambia.
        /// </summary>
        /// <param name="propertyName"> Nombre de la propiedad. </param>
        protected override void OnValidation(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(SelectedStation):
                    if (SelectedStation == null)
                        AddError(nameof(SelectedStation), "Seleccione una estación.");
                    break;

                case nameof(SelectedKvr):
                    if (SelectedKvr == null)
                        AddError(nameof(SelectedKvr), "Seleccione un Kiosko.");
                    break;

                case nameof(SelectedTechnician):
                    if (SelectedTechnician == null)
                        AddError(nameof(SelectedTechnician), "Seleccione el técnico que realiza la devolución.");
                    break;

                case nameof(SelectedDestiny):
                    if (SelectedDestiny == null)
                        AddError(nameof(SelectedDestiny), "Seleccione el destino del dinero.");
                    break;

                case nameof(Quantity):
                    if (!Single.TryParse(Quantity, out Single result))
                        AddError(nameof(Quantity), "El formato no es valido.");
                    else if (result <= 0)
                        AddError(nameof(Quantity), "La cantidad no puede ser menor o igual que cero.");
                    break;
            }
        }

        /// <summary>
        /// Determina si es posible ejecutar el comando <see cref="CreateRefundCommand" />.
        /// </summary>
        /// <param name="arg"> Parametro del comando. </param>
        /// <returns> Un valor true si el comando se puede ejecutar. </returns>
        private bool CanCreate(object arg)
        {
            Validate();

            return !HasErrors;
        }

        /// <summary>
        /// Crea la devolución de dinero.
        /// </summary>
        /// <param name="obj"> Parametro del comando. </param>
        private void CreateRefund(object obj)
        {
            try
            {
                ulong idFault = (ulong)(AcabusDataContext.ConfigContext["RefundOfMoney"]?.ToInteger(IsMoney ? "Money" : "Bill") ?? 0);

                if (idFault == 0)
                    throw new InvalidOperationException("Se requiere que exista una configuración " +
                        "para el identificador de la actividad relacionada con la devolución del dinero. " +
                        "Añada la configuración --> <RefundOfMoney Money=\"id_fault\" Bill=\"id_fault\" />");

                IncidenceStatus status = SelectedDestiny.RequiresMoving ? IncidenceStatus.UNCOMMIT : IncidenceStatus.CLOSE;
                DeviceFault refundFault = AcabusDataContext.DbContext.Read<DeviceFault>().FirstOrDefault(df => df.ID == idFault);

                if (refundFault == null)
                    throw new ArgumentException("No existe la actividad de devolución especificada en la configuración.");

                Incidence incidence = new Incidence()
                {
                    Device = SelectedKvr,
                    FinishDate = RefundDateTime,
                    StartDate = RefundDateTime,
                    Technician = SelectedTechnician,
                    WhoReporting = AcabusDataContext.ConfigContext["Owner"]?.ToString("name"),
                    Observations = Observations ?? String.Format("DEVOLUCIÓN {0:C} A {1}", Quantity, SelectedDestiny),
                    Priority = Priority.NONE,
                    Fault = refundFault,
                    Status = status
                };

                Models.RefundOfMoney refundOfMoney = new Models.RefundOfMoney(incidence)
                {
                    CashDestiny = SelectedDestiny,
                    Quantity = Single.Parse(Quantity),
                    RefundDate = status == IncidenceStatus.CLOSE ? RefundDateTime : (DateTime?)null,
                    Status = status == IncidenceStatus.CLOSE ? RefundStatus.DELIVERED : RefundStatus.FOR_DELIVERY
                };

                if (AcabusDataContext.DbContext.Create(refundOfMoney, 1))
                    ShowMessage(String.Format("Devolución de dinero generada con folio: F-{0:D5}", incidence.Folio));
                else
                    throw new InvalidOperationException("No se logró generar la devolución de dinero.");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.PrintMessage().JoinLines(), "ERROR");
                ShowMessage("No se generó la devolución, intentelo nuevamente. Si el problema persiste contacte a soporte técnico.");
            }
        }

        /// <summary>
        /// Valida los campos del formulario.
        /// </summary>
        private void Validate()
        {
            ValidateProperty(nameof(SelectedStation));
            ValidateProperty(nameof(SelectedKvr));
            ValidateProperty(nameof(SelectedTechnician));
            ValidateProperty(nameof(SelectedDestiny));
            ValidateProperty(nameof(Quantity));
        }
    }
}