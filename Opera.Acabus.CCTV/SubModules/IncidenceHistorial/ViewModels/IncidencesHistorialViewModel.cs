using InnSyTech.Standard.Database.Linq;
using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Cctv.Models;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui.Modules;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Opera.Acabus.Cctv.SubModules.IncidenceHistorial.ViewModels
{
    /// <summary>
    /// Define el modelo de la vista <see cref="Views.IncidencesHistorialView" />, la cual permite la
    /// busqueda en el historial de incidencias.
    /// </summary>
    public sealed class IncidencesHistorialViewModel : ModuleViewerBase
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
        private IAssignableSection _selectedLocation;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedStatus'.
        /// </summary>
        private IncidenceStatus? _selectedStatus;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedTechnician'.
        /// </summary>
        private Staff _selectedTechnician;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedVehicle'.
        /// </summary>
        private Bus _selectedBus;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedWhoReporting'.
        /// </summary>
        private String _selectedWhoReporting;

        /// <summary>
        /// Campo que provee a la propiedad 'StartDate'.
        /// </summary>
        private DateTime? _startDate;

        /// <summary>
        /// Crea una instancia nueva.
        /// </summary>
        public IncidencesHistorialViewModel()
        {
            _result = null;
            _isEnabled = false;
            _isExpanded = false;
            _hasRefundOfMoney = false;
            _isStartDate = true;

            SearchCommand = new Command(FindIncidences);
            ClearCommand = new Command(ClearFields);
        }

        /// <summary>
        /// Obtiene el listado de todos los autobuses.
        /// </summary>
        public IEnumerable<Bus> AllBuses
            => SelectedLocation is Route
                    ? AcabusDataContext.AllBuses.Where(b => b.Route.ID == (SelectedLocation as Route).ID).ToList()
                    : AcabusDataContext.AllBuses.ToList();

        /// <summary>
        /// Obtiene el listado de las compañias que reprotan.
        /// </summary>
        public IEnumerable<String> AllCompanies
                    => AcabusDataContext.ConfigContext["Cctv"]?
                .GetSetting("business")?
                .GetSettings("business")?
                .Select(s => s.ToString("value"))
                .OrderBy(s => s);

        /// <summary>
        /// Obtiene el listado de los equipos.
        /// </summary>
        public IEnumerable<Device> AllDevices
            =>
                IsBusIncidences && SelectedBus != null
                    ? AcabusDataContext.AllDevices.Where(d => d.Bus.ID == SelectedBus.ID).ToList()
                    :
                !IsBusIncidences && SelectedLocation != null
                    ? AcabusDataContext.AllDevices.Where(d => d.Station.ID == (SelectedLocation as Station).ID).ToList()
                    : null;

        /// <summary>
        /// Obtiene el listado de las fallas disponibles.
        /// </summary>
        public IEnumerable<DeviceFault> AllFaults
            => (SelectedDevice != null
                ? AcabusDataContext.DbContext.Read<DeviceFault>().Where(df => df.Category.DeviceType == SelectedDevice.Type).ToList()
                : AcabusDataContext.DbContext.Read<DeviceFault>().ToList().GroupBy(df => df.Description).Select(g => g.FirstOrDefault()))
            .OrderBy(f => f.Description);

        /// <summary>
        /// Obtiene el listado de las ubicaciones disponibles.
        /// </summary>
        public IEnumerable<IAssignableSection> AllLocations => IsBusIncidences
                    ? AcabusDataContext.AllRoutes.ToList().Cast<IAssignableSection>()
                    : AcabusDataContext.AllStations.ToList().Cast<IAssignableSection>();

        /// <summary>
        /// Obtiene el listado de los estados de incidencias.
        /// </summary>
        public IEnumerable<IncidenceStatus> AllStatus
            => Enum.GetValues(typeof(IncidenceStatus)).Cast<IncidenceStatus>();

        /// <summary>
        /// Obtiene el listado de todos los ténicos registrados.
        /// </summary>
        public IEnumerable<Staff> AllTechnician => AcabusDataContext.AllStaff;

        /// <summary>
        /// Comando para la limpieza de los campos.
        /// </summary>
        public ICommand ClearCommand { get; }

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

        /// <summary>
        /// Obtiene el nombre de la opción para fecha.
        /// </summary>
        public String HeadingDate => IsStartDate ? "Por fecha de inicio de incidencia" : "Por fecha de solución de incidencia";

        /// <summary>
        /// Obtiene el nombre que indica si la incidencias es de autobus o estación.
        /// </summary>
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

        /// <summary>
        /// Comando para realizar la busqueda.
        /// </summary>
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
        public IAssignableSection SelectedLocation {
            get => _selectedLocation;
            set {
                _selectedLocation = value;
                OnPropertyChanged(nameof(SelectedLocation));
                OnPropertyChanged(nameof(AllBuses));
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
        public Staff SelectedTechnician {
            get => _selectedTechnician;
            set {
                _selectedTechnician = value;
                OnPropertyChanged(nameof(SelectedTechnician));
            }
        }

        /// <summary>
        /// Obtiene o establece el vehículo seleccionado para la búsqueda.
        /// </summary>
        public Bus SelectedBus {
            get => _selectedBus;
            set {
                _selectedBus = value;
                OnPropertyChanged(nameof(SelectedBus));
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

        /// <summary>
        /// Carga los valores predeterminados del formulario de busqueda.
        /// </summary>
        /// <param name="arg">Parametro de comando.</param>
        protected override void OnLoad(object arg)
        {
            ClearFields();
            IsExpanded = true;
            IsEnabled = true;
        }

        /// <summary>
        /// Limpia los campos del formulario.
        /// </summary>
        /// <param name="obj">Parametro de comando</param>
        private void ClearFields(object obj = null)
        {
            Folio = null;
            SelectedDescription = null;
            SelectedDevice = null;
            SelectedLocation = null;
            SelectedStatus = null;
            SelectedTechnician = null;
            SelectedBus = null;
            SelectedWhoReporting = null;
            StartDate = null;
            FinishDate = null;
            HasRefundOfMoney = false;
            _result = null;
        }

        /// <summary>
        /// Funcion de busqueda de las incidencias.
        /// </summary>
        /// <param name="obj">Parametro del comando.</param>
        private void FindIncidences(object obj = null)
        {
            IsEnabled = false;

            if (!HasRefundOfMoney)
            {
                var query = AcabusDataContext.DbContext.Read<Incidence>().LoadReference(3);

                if (UInt64.TryParse(Folio, out ulong folio))
                    query = query.Where(i => i.Folio == folio);

                if (SelectedLocation != null && SelectedLocation is Station)
                    query = query.Where(i => i.Device.Station.ID == (SelectedLocation as Station).ID);

                if (IsBusIncidences && SelectedBus != null)
                    query = query.Where(i => i.Device.Bus.ID == SelectedBus.ID);

                if (IsStartDate && (StartDate != null && FinishDate != null))
                    query = query.Where(i => i.StartDate >= StartDate && i.StartDate <= FinishDate);
                else if (!IsStartDate && (StartDate != null && FinishDate != null))
                    query = query.Where(i => i.FinishDate >= StartDate && i.FinishDate <= FinishDate);

                if (SelectedDevice != null)
                    query = query.Where(i => i.Device.ID == SelectedDevice.ID);

                if (SelectedTechnician != null)
                    query = query.Where(i => i.Technician.ID == SelectedTechnician.ID);

                if (SelectedDescription != null)
                    query = query.Where(i => i.Fault.Description == SelectedDescription.Description);

                if (SelectedStatus != null)
                    query = query.Where(i => i.Status == SelectedStatus);

                if (!String.IsNullOrEmpty(SelectedWhoReporting))
                    query = query.Where(i => i.WhoReporting == SelectedWhoReporting);

                Task.Run(() =>
                {
                    _result = query.ToList();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        OnPropertyChanged(nameof(Result));
                        IsExpanded = false;
                        IsEnabled = true;
                    });
                });
            }
            else
            {
                var query = AcabusDataContext.DbContext.Read<Models.RefundOfMoney>().LoadReference(4);

                if (UInt64.TryParse(Folio, out ulong folio))
                    query = query.Where(r => r.Incidence.Folio == folio);

                if (SelectedLocation != null && SelectedLocation is Station)
                    query = query.Where(r => r.Incidence.Device.Station.ID == (SelectedLocation as Station).ID);

                if (IsBusIncidences && SelectedBus != null)
                    query = query.Where(r => r.Incidence.Device.Bus.ID == SelectedBus.ID);

                if (IsStartDate && (StartDate != null && FinishDate != null))
                    query = query.Where(r => r.Incidence.StartDate >= StartDate && r.Incidence.StartDate <= FinishDate);
                else if (!IsStartDate && (StartDate != null && FinishDate != null))
                    query = query.Where(r => r.Incidence.FinishDate >= StartDate && r.Incidence.FinishDate <= FinishDate);

                if (SelectedDevice != null)
                    query = query.Where(r => r.Incidence.Device.ID == SelectedDevice.ID);

                if (SelectedTechnician != null)
                    query = query.Where(r => r.Incidence.Technician.ID == SelectedTechnician.ID);

                if (SelectedDescription != null)
                    query = query.Where(r => r.Incidence.Fault.Description == SelectedDescription.Description);

                if (SelectedStatus != null)
                    query = query.Where(r => r.Incidence.Status == SelectedStatus);

                if (!String.IsNullOrEmpty(SelectedWhoReporting))
                    query = query.Where(r => r.Incidence.WhoReporting == SelectedWhoReporting);

                Task.Run(() =>
                {
                    var executed = query.ToList();

                    foreach (var refund in executed)
                        refund.Incidence.RefundOfMoney = refund;

                    _result = executed.Select(r => r.Incidence);

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
}