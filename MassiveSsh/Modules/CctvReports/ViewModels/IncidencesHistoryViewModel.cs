using Acabus.Models;
using Acabus.Modules.CctvReports.Models;
using Acabus.Utils;
using Acabus.Utils.Mvvm;
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
        /// Campo que provee a la propiedad 'IsBusIncidences'.
        /// </summary>
        private Boolean _isBusIncidence;

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
        /// Campo que provee a la propiedad 'SelectedWhoReporting'.
        /// </summary>
        private String _selectedWhoReporting;

        /// <summary>
        /// Campo que provee a la propiedad 'StartDate'.
        /// </summary>
        private DateTime? _startDate;

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
                OnPropertyChanged(nameof(StartDate));
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedVehicle'.
        /// </summary>
        private Vehicle _selectedVehicle;
        private ICollection<Incidence> _incidences;

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

        public IEnumerable<AssignableSection> AllLocations => IsBusIncidences
            ? Core.DataAccess.AcabusData.AllRoutes.Cast<AssignableSection>()
            : Core.DataAccess.AcabusData.AllStations.Cast<AssignableSection>();

        public IEnumerable<Device> AllDevices => SelectedLocation is null && !IsBusIncidences
            ? Core.DataAccess.AcabusData.AllDevices
            : SelectedLocation is Station
                ? (SelectedLocation as Station).Devices
                : SelectedVehicle is null
                    ? null
                    : SelectedVehicle.Devices;

        public IEnumerable<DeviceFault> AllFaults => SelectedDevice is null
            ? Core.DataAccess.AcabusData.AllFaults
            : Core.DataAccess.AcabusData.AllFaults
                .Where(fault => fault.Category.DeviceType == SelectedDevice.Type);

        public IEnumerable<Vehicle> AllVehicles => SelectedLocation is Route
            ? (SelectedLocation as Route).Vehicles
            : Core.DataAccess.AcabusData.AllVehicles;

        public IEnumerable<String> AllCompanies => DataAccess.AcabusData.Companies;

        public IEnumerable<Technician> AllTechnician => Core.DataAccess.AcabusData.AllTechnicians;

        public IEnumerable<IncidenceStatus> AllStatus => Enum.GetValues(typeof(IncidenceStatus)).Cast<IncidenceStatus>();

        public ICommand SearchCommand { get; }

        public CommandBase ClearCommand { get;}

        public IncidencesHistoryViewModel()
        {
            _result = null;
            _isEnabled = false;
            _isExpanded = false;

            SearchCommand = new CommandBase(SeachIncidences);
            ClearCommand = new CommandBase(ClearFields);
        }

        private void ClearFields(object obj)
        {
            Folio = String.Empty;
            SelectedDescription = null;
            SelectedDevice = null;
            SelectedLocation = null;
            SelectedStatus = null;
            SelectedTechnician = null;
            SelectedVehicle = null;
            SelectedWhoReporting = string.Empty;
            StartDate = null;
            FinishDate = null;
        }

        /// <summary>
        /// Campo que provee a la propiedad 'Expanded'.
        /// </summary>
        private Boolean _isExpanded;

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
        /// Campo que provee a la propiedad 'IsEnabled'.
        /// </summary>
        private Boolean _isEnabled;

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

        protected override void OnLoad(object arg)
        {
            Task.Run(() =>
            {
                _incidences = DataAccess.AcabusData.Session.GetObjects<Incidence>();
                Application.Current.Dispatcher.Invoke(() => IsEnabled = true);
                Application.Current.Dispatcher.Invoke(() => IsExpanded = true);
            });

        }

        private void SeachIncidences(object obj)
        {
            IsEnabled = false;

            _result = _incidences.ToList();

            if (!String.IsNullOrEmpty(Folio))
                _result = _result.Where(incidence => incidence.Folio.Contains(Folio));

            if (SelectedLocation != null && SelectedLocation is Station)
                _result = _result.Where(incidence => incidence.Device?.Station == SelectedLocation as Station);

            if (IsBusIncidences && SelectedVehicle != null)
                _result = _result.Where(incidence => incidence.Device?.Vehicle == SelectedVehicle);

            if (IsStartDate && (StartDate != null || FinishDate != null))
                _result = _result.Where(incidence => incidence.StartDate.Between(StartDate, StartDate));
            else if (!IsStartDate && (StartDate != null || FinishDate != null))
                _result = _result.Where(incidence => incidence.FinishDate != null && incidence.FinishDate.Value.Between(StartDate, StartDate));

            if (SelectedDevice != null)
                _result = _result.Where(incidence => incidence.Device == SelectedDevice);

            if (SelectedTechnician != null)
                _result = _result.Where(incidence => incidence.Technician == SelectedTechnician);

            if (SelectedDescription != null)
                _result = _result.Where(incidence => incidence.Description != null
                    && incidence.Description.Equals(SelectedDescription));

            if (SelectedStatus != null)
                _result = _result.Where(incidence => incidence.Status == SelectedStatus);

            if (SelectedWhoReporting != null)
                _result = _result.Where(incidence => incidence.WhoReporting == SelectedWhoReporting);

            OnPropertyChanged(nameof(Result));
            IsExpanded = false;
            IsEnabled = true;
        }
    }
}