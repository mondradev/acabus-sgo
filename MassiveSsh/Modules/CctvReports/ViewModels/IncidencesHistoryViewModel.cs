using Acabus.Models;
using Acabus.Modules.CctvReports.Models;
using Acabus.Utils;
using Acabus.Utils.Mvvm;
using InnSyTech.Standard.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Acabus.Modules.CctvReports.ViewModels
{
    public sealed class IncidencesHistoryViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Descriptión'.
        /// </summary>
        private DeviceFault _description;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedDevice'.
        /// </summary>
        private Device _device;

        /// <summary>
        /// Campo que provee a la propiedad 'FinishDate'.
        /// </summary>
        private DateTime? _finishDate;

        /// <summary>
        /// Campo que provee a la propiedad 'Folio'.
        /// </summary>
        private String _Folio;

        /// <summary>
        /// Campo que provee a la propiedad 'HasRefundOfMoney'.
        /// </summary>
        private Boolean _hasRefundOfMoney;

        /// <summary>
        /// Campo que provee a la propiedad 'IsBusIncidences'.
        /// </summary>
        private Boolean _isBusIncidence;

        /// <summary>
        /// Campo que provee a la propiedad 'IsEnabled'.
        /// </summary>
        private Boolean _isEnabled;

        /// <summary>
        /// Campo que provee a la propiedad 'Expanded'.
        /// </summary>
        private Boolean _isExpanded;

        /// <summary>
        /// Campo que provee a la propiedad 'IsStartDate'.
        /// </summary>
        private Boolean _isStartDate;

        /// <summary>
        /// Campo que provee a la propiedad 'Result'.
        /// </summary>
        private IEnumerable<Incidence> _result;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedLocation'.
        /// </summary>
        private AssignableSection _selectedLocation;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedStatus'.
        /// </summary>
        private IncidenceStatus? _selectedStatus;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedTechnician'.
        /// </summary>
        private Technician _selectedTechnician;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedVehicle'.
        /// </summary>
        private Vehicle _selectedVehicle;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedWhoReporting'.
        /// </summary>
        private String _selectedWhoReporting;

        /// <summary>
        /// Campo que provee a la propiedad 'StartDate'.
        /// </summary>
        private DateTime? _startDate;

        public IncidencesHistoryViewModel()
        {
            _result = null;
            _isEnabled = false;
            _isExpanded = false;
            _hasRefundOfMoney = false;
            _isStartDate = true;

            SearchCommand = new CommandBase(SeachIncidences);
            ClearCommand = new CommandBase(ClearFields);
        }

        public IEnumerable<String> AllCompanies => DataAccess.AcabusData.Companies;

        public IEnumerable<Device> AllDevices => SelectedLocation is null && !IsBusIncidences
                    ? Core.DataAccess.AcabusData.AllDevices.Where(device => device.Station != null)
                    : SelectedLocation is Station
                        ? (SelectedLocation as Station).Devices
                        : SelectedVehicle is null
                            ? null
                            : SelectedVehicle.Devices;

        public IEnumerable<DeviceFault> AllFaults => (SelectedDevice is null
                    ? Core.DataAccess.AcabusData.AllFaults
                        .GroupBy(fault => fault.Description)
                        .Select(group => group.FirstOrDefault())
                    : Core.DataAccess.AcabusData.AllFaults
                        .Where(fault => fault.Category.DeviceType == SelectedDevice.Type))
            .OrderBy(fault => fault.Description);

        public IEnumerable<AssignableSection> AllLocations => IsBusIncidences
                    ? Core.DataAccess.AcabusData.AllRoutes.Cast<AssignableSection>()
                    : Core.DataAccess.AcabusData.AllStations.Cast<AssignableSection>();

        public IEnumerable<IncidenceStatus> AllStatus => Enum.GetValues(typeof(IncidenceStatus)).Cast<IncidenceStatus>();

        public IEnumerable<Technician> AllTechnician => Core.DataAccess.AcabusData.AllTechnicians;

        public IEnumerable<Vehicle> AllVehicles => SelectedLocation is Route
                    ? (SelectedLocation as Route).Vehicles
                    : Core.DataAccess.AcabusData.AllVehicles;

        public CommandBase ClearCommand { get; }

        /// <summary>
        /// Obtiene o establece la fecha final de la búsqueda.
        /// </summary>
        public DateTime? FinishDate {
            get => _finishDate;
            set {
                _finishDate = value;
                OnPropertyChanged(nameof(FinishDate));
            }
        }

        /// <summary>
        /// Obtiene o establece el folio.
        /// </summary>
        public String Folio {
            get => _Folio;
            set {
                _Folio = value;
                OnPropertyChanged(nameof(Folio));
            }
        }

        /// <summary>
        /// Obtiene o establece si las incidencias tienen devolución de dinero.
        /// </summary>
        public Boolean HasRefundOfMoney {
            get => _hasRefundOfMoney;
            set {
                _hasRefundOfMoney = value;
                OnPropertyChanged(nameof(HasRefundOfMoney));
            }
        }

        public String HeadingDate => IsStartDate ? "Por fecha de inicio de incidencia" : "Por fecha de solución de incidencia";

        public String HeadingIncidenceType => IsBusIncidences ? "Incidencia de autobus" : "Incidencia de estación";

        /// <summary>
        /// Obtiene o establece si la incidencia es de autobus o no.
        /// </summary>
        public Boolean IsBusIncidences {
            get => _isBusIncidence;
            set {
                _isBusIncidence = value;
                OnPropertyChanged(nameof(IsBusIncidences));
                OnPropertyChanged(nameof(AllLocations));
                OnPropertyChanged(nameof(HeadingIncidenceType));
            }
        }

        /// <summary>
        /// Obtiene o establece si las opciones estan activas.
        /// </summary>
        public Boolean IsEnabled {
            get => _isEnabled;
            set {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        /// <summary>
        /// Obtiene o establece si las opciones de busqueda estan visibles o no.
        /// </summary>
        public Boolean IsExpanded {
            get => _isExpanded;
            set {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        /// <summary>
        /// Obtiene o establece si la busqueda por fecha es por fecha de incidencia o por fecha de solución.
        /// </summary>
        public Boolean IsStartDate {
            get => _isStartDate;
            set {
                _isStartDate = value;
                OnPropertyChanged(nameof(IsStartDate));
                OnPropertyChanged(nameof(HeadingDate));
            }
        }

        /// <summary>
        /// Obtiene la lista de incidencias que resultan de la búsqueda.
        /// </summary>
        public IEnumerable<Incidence> Result => _result;

        public ICommand SearchCommand { get; }

        /// <summary>
        /// Obtiene o establece la descripción de la falla para la búsqueda.
        /// </summary>
        public DeviceFault SelectedDescription {
            get => _description;
            set {
                _description = value;
                OnPropertyChanged(nameof(SelectedDescription));
            }
        }

        /// <summary>
        /// Obtiene o establece el equipo seleccionado para la búsqueda.
        /// </summary>
        public Device SelectedDevice {
            get => _device;
            set {
                _device = value;
                OnPropertyChanged(nameof(SelectedDevice));
                OnPropertyChanged(nameof(AllFaults));
            }
        }

        /// <summary>
        /// Obtiene o establece la ubicación para la búsqueda.
        /// </summary>
        public AssignableSection SelectedLocation {
            get => _selectedLocation;
            set {
                _selectedLocation = value;
                OnPropertyChanged(nameof(SelectedLocation));
                OnPropertyChanged(nameof(AllVehicles));
                OnPropertyChanged(nameof(AllDevices));
            }
        }

        /// <summary>
        /// Obtiene o establece el estado de incidencia.
        /// </summary>
        public IncidenceStatus? SelectedStatus {
            get => _selectedStatus;
            set {
                _selectedStatus = value;
                OnPropertyChanged(nameof(SelectedStatus));
            }
        }

        /// <summary>
        /// Obtiene o establece el técnico seleccionado.
        /// </summary>
        public Technician SelectedTechnician {
            get => _selectedTechnician;
            set {
                _selectedTechnician = value;
                OnPropertyChanged(nameof(SelectedTechnician));
            }
        }

        /// <summary>
        /// Obtiene o establece el vehículo seleccionado para la búsqueda.
        /// </summary>
        public Vehicle SelectedVehicle {
            get => _selectedVehicle;
            set {
                _selectedVehicle = value;
                OnPropertyChanged(nameof(SelectedVehicle));
                OnPropertyChanged(nameof(AllDevices));
            }
        }

        /// <summary>
        /// Obtiene o establece quien reporta.
        /// </summary>
        public String SelectedWhoReporting {
            get => _selectedWhoReporting;
            set {
                _selectedWhoReporting = value;
                OnPropertyChanged(nameof(SelectedWhoReporting));
            }
        }

        /// <summary>
        /// Obtiene o establece la fecha de incial de la búsqueda.
        /// </summary>
        public DateTime? StartDate {
            get => _startDate;
            set {
                _startDate = value;
                if (value != null && (_finishDate == null || _finishDate <= value))
                    _finishDate = value?.AddDays(1);
                OnPropertyChanged(nameof(StartDate));
                OnPropertyChanged(nameof(FinishDate));
            }
        }

        protected override void OnLoad(object arg)
        {
            ClearFields();
            IsExpanded = true;
            IsEnabled = true;
        }

        private void ClearFields(object obj = null)
        {
            Folio = null;
            SelectedDescription = null;
            SelectedDevice = null;
            SelectedLocation = null;
            SelectedStatus = null;
            SelectedTechnician = null;
            SelectedVehicle = null;
            SelectedWhoReporting = null;
            StartDate = null;
            FinishDate = null;
            HasRefundOfMoney = false;
            _result = null;
        }

        private void SeachIncidences(object obj = null)
        {
            IsEnabled = false;

            var filter = new DbFilter();

            if (!String.IsNullOrEmpty(Folio))
                filter.AddWhere(new DbFilterExpression(nameof(Incidence.Folio), String.Format("%{0}%", Folio), WhereOperator.LIKE));

            if (SelectedLocation != null && SelectedLocation is Station)
                filter.AddWhere(new DbFilterExpression("T4.ID", (SelectedLocation as Station).ID, WhereOperator.EQUALS));

            if (IsBusIncidences && SelectedVehicle != null)
                filter.AddWhere(new DbFilterExpression("T6.ID", SelectedVehicle.ID, WhereOperator.EQUALS));

            if (IsStartDate && (StartDate != null && FinishDate != null))
                filter.AddWhere(new DbFilterExpression(nameof(Incidence.StartDate), String.Format("{0:yyyy-MM-dd}", StartDate), WhereOperator.GREAT_AND_EQUALS))
                    .AddWhere(new DbFilterExpression(nameof(Incidence.StartDate), String.Format("{0:yyyy-MM-dd}", FinishDate), WhereOperator.LESS_AND_EQUALS));
            else if (!IsStartDate && (StartDate != null && FinishDate != null))
                filter.AddWhere(new DbFilterExpression(nameof(Incidence.FinishDate), String.Format("{0:yyyy-MM-dd}", StartDate), WhereOperator.GREAT_AND_EQUALS))
                    .AddWhere(new DbFilterExpression(nameof(Incidence.FinishDate), String.Format("{0:yyyy-MM-dd}", FinishDate), WhereOperator.LESS_AND_EQUALS));

            if (SelectedDevice != null)
                filter.AddWhere(new DbFilterExpression("T3.ID", SelectedDevice.ID, WhereOperator.EQUALS));

            if (SelectedTechnician != null)
                filter.AddWhere(new DbFilterExpression("T10.ID", SelectedTechnician, WhereOperator.EQUALS));

            if (SelectedDescription != null)
                filter.AddWhere(new DbFilterExpression("T8.Description", String.Format("%{0}%", SelectedDescription.Description), WhereOperator.LIKE));

            if (SelectedStatus != null)
                filter.AddWhere(new DbFilterExpression(nameof(Incidence.Status), (int)SelectedStatus, WhereOperator.EQUALS));

            if (!String.IsNullOrEmpty(SelectedWhoReporting))
                filter.AddWhere(new DbFilterExpression(nameof(Incidence.WhoReporting), SelectedWhoReporting, WhereOperator.EQUALS));

            if (HasRefundOfMoney)
                filter.AddWhere(new DbFilterExpression(nameof(Incidence.Folio), "(SELECT Fk_Folio FROM RefundOfMoney)", WhereOperator.IN));
            Task.Run(() =>
            {
                _result = DataAccess.AcabusData.Session.GetObjects<Incidence>(filter);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnPropertyChanged(nameof(Result));
                    IsExpanded = false;
                    IsEnabled = true;
                });
            });
        }
    }
}