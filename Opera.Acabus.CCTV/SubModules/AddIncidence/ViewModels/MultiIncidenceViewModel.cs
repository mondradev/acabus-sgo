using InnSyTech.Standard.Database.Linq;
using InnSyTech.Standard.Mvvm;
using InnSyTech.Standard.Utils;
using Opera.Acabus.Cctv.Helpers;
using Opera.Acabus.Cctv.Models;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Gui.Modules;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Input;

namespace Opera.Acabus.Cctv.SubModules.AddIncidence.ViewModels
{
    /// <summary>
    /// Define el modelo de la vista <see cref="Views.MultiIncidenceView" />.
    /// </summary>
    public sealed class MultiIncidenceViewModel : ModuleViewerBase
    {
        /// <summary>
        /// Todos los autobuses disponibles.
        /// </summary>
        private List<Bus> _allBuses;

        /// <summary>
        /// Todos los equipos disponibles.
        /// </summary>
        private List<Device> _allDevices;

        /// <summary>
        /// Todas las estaciones disponibles.
        /// </summary>
        private List<Station> _allStations;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="GlobalObservations" />.
        /// </summary>
        private String _globalObservations;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="IsBusIncidences" />.
        /// </summary>
        private bool _isBusIncidences;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedDescription" />.
        /// </summary>
        private String _selectedDescription;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedDevices" />.
        /// </summary>
        private ObservableCollection<MultiIncidenceItem> _selectedDevices;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedDeviceType" />.
        /// </summary>
        private DeviceType? _selectedDeviceType;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedDeviceTypes" />
        /// </summary>
        private ObservableCollection<DeviceType> _selectedDeviceTypes;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedLocation" />.
        /// </summary>
        private ILocation _selectedLocation;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedLocation" />
        /// </summary>
        private ObservableCollection<ILocation> _selectedLocations;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedRoute" />.
        /// </summary>
        private Route _selectedRoute;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedWhoReporting" />.
        /// </summary>
        private String _selectedWhoReporting;

        /// <summary>
        /// Crea una nueva instancia del modelo de la vista <see cref="Views.MultiIncidenceView" />.
        /// </summary>
        public MultiIncidenceViewModel()
        {
            _allStations = AcabusDataContext.AllStations.ToList();
            _allBuses = AcabusDataContext.AllBuses.ToList();
            _allDevices = AcabusDataContext.AllDevices.LoadReference(1).ToList();

            CreateIncidenceCommand = new Command(CreateIncidence, CanCreate);

            DiscardDeviceCommand = new Command(parameter =>
            {
                if (!(parameter is MultiIncidenceItem)) return;

                _selectedDevices.Remove(parameter as MultiIncidenceItem);

                OnPropertyChanged(nameof(SelectedDevices));
                OnPropertyChanged(nameof(DeviceFaults));
            });

            DiscardLocationCommand = new Command(parameter =>
            {
                if (!(parameter is ILocation)) return;

                _selectedLocations.Remove(parameter as ILocation);

                UpdateDeviceList();

                OnPropertyChanged(nameof(AllLocations));
            });

            DiscardTypeCommand = new Command(parameter =>
            {
                if (!(parameter is DeviceType)) return;

                _selectedDeviceTypes.Remove((DeviceType)parameter);

                UpdateDeviceList();

                OnPropertyChanged(nameof(AllDeviceTypes));
                OnPropertyChanged(nameof(SelectedDeviceTypes));
            });
        }

        /// <summary>
        /// Obtiene la lista de tipos de equipos disponibles.
        /// </summary>
        public IEnumerable<DeviceType> AllDeviceTypes
            => Enum.GetValues(typeof(DeviceType)).Cast<DeviceType>()
            .Where(dt => !SelectedDeviceTypes.Contains(dt)
                && !new[] {
                    DeviceType.APP,
                    DeviceType.DB,
                    DeviceType.PDE,
                    DeviceType.NONE
                }.Contains(dt))
            .OrderBy(dt => dt.TranslateToSpanish());

        /// <summary>
        /// Obtiene una lista de todas las ubicaciones disponibles.
        /// </summary>
        public IEnumerable<ILocation> AllLocations => IsBusIncidences
            ? SelectedRoute == null
                ? _allBuses.OrderBy(b => b.EconomicNumber)
                    .Cast<ILocation>().Where(l => !SelectedLocations.Contains(l))
                : SelectedRoute.Buses.OrderBy(b => b.EconomicNumber)
                    .Cast<ILocation>().Where(l => !SelectedLocations.Contains(l))
            : _allStations.OrderBy(s => s.StationNumber)
                .Cast<ILocation>().Where(l => !SelectedLocations.Contains(l));

        /// <summary>
        /// Obtiene una lista de las rutas disponibles.
        /// </summary>
        public ICollection<Route> AllRoutes {
            get {
                var routes = AcabusDataContext.AllRoutes.ToList();
                foreach (var route in routes)
                    AcabusDataContext.DbContext.LoadRefences(route, nameof(Route.Buses));

                return routes;
            }
        }

        /// <summary>
        /// Obtiene una lista de entidades que realizan el reporte de la incidencia.
        /// </summary>
        public IEnumerable<String> Business
            => AcabusDataContext.ConfigContext["Cctv"]?
                .GetSetting("business")?
                .GetSettings("business")?
                .Select(s => s.ToString("value"))
                .OrderBy(s => s);

        /// <summary>
        /// Comando que permite la creación de las incidencias.
        /// </summary>
        public ICommand CreateIncidenceCommand { get; }

        /// <summary>
        /// Obtiene el lista de las fallas o actividades que son aplicables a todos los equipos seleccionados.
        /// </summary>
        public IEnumerable<String> DeviceFaults
                            => FilterByType(AcabusDataContext.DbContext.Read<DeviceFault>())
                            .GroupBy(f => f.Description)
                            .Where(g => g.Count() == GetDevicesTypes().Count())
                            .Select(g => g.FirstOrDefault().Description)
                            .OrderBy(d => d);

        /// <summary>
        /// Comando para descartar el equipo seleccionado.
        /// </summary>
        public ICommand DiscardDeviceCommand { get; }

        /// <summary>
        /// Comando para descartar la ubicaciones seleccionada.
        /// </summary>
        public ICommand DiscardLocationCommand { get; }

        /// <summary>
        /// Comando para descartar el tipo de equipo seleccionado.
        /// </summary>
        public ICommand DiscardTypeCommand { get; }

        /// <summary>
        /// Obtiene o establece las observaciones globales de la incidencia.
        /// </summary>
        public String GlobalObservations {
            get => _globalObservations;
            set {
                _globalObservations = value;
                OnPropertyChanged(nameof(GlobalObservations));
                if (!String.IsNullOrEmpty(_globalObservations))
                    foreach (var device in SelectedDevices)
                        device.Observations = _globalObservations;

                OnPropertyChanged(nameof(SelectedDevices));
            }
        }

        /// <summary>
        /// Obtiene el encabezado del selector de ubicaciones.
        /// </summary>
        public String HeaderLocation => IsBusIncidences ? "Agregar autobus" : "Agregar estación";

        /// <summary>
        /// Obtiene o establece si son incidencias de autobuses.
        /// </summary>
        public bool IsBusIncidences {
            get => _isBusIncidences;
            set {
                _isBusIncidences = value;
                OnPropertyChanged(nameof(IsBusIncidences));
                OnPropertyChanged(nameof(AllLocations));
                OnPropertyChanged(nameof(HeaderLocation));
            }
        }

        /// <summary>
        /// Obtiene o establece si la lista de incidencias está activa.
        /// </summary>
        public bool IsEnabledFaultList => (_selectedDevices?.Count ?? 0) > 0;

        /// <summary>
        /// Obtiene o establece la descripción de la incidencia.
        /// </summary>
        public String SelectedDescription {
            get => _selectedDescription;
            set {
                _selectedDescription = value;
                OnPropertyChanged(nameof(SelectedDescription));
            }
        }

        /// <summary>
        /// Obtiene una lista de los equipos a aplicar la falla descrita.
        /// </summary>
        public IEnumerable<MultiIncidenceItem> SelectedDevices
            => (_selectedDevices ?? (_selectedDevices = new ObservableCollection<MultiIncidenceItem>()))
            .OrderBy(m => m.SelectedDevice.Type.TranslateToSpanish());

        /// <summary>
        /// Obtiene o establece el tipo de equipo seleccionado.
        /// </summary>
        public DeviceType? SelectedDeviceType {
            get => _selectedDeviceType;
            set {
                _selectedDeviceType = value;
                OnPropertyChanged(nameof(SelectedDeviceType));

                if (value != null && value != DeviceType.NONE && !SelectedDeviceTypes.Contains(value.Value))
                {
                    (_selectedDeviceTypes as ObservableCollection<DeviceType>).Add(value.Value);

                    UpdateDeviceList();

                    OnPropertyChanged(nameof(AllDeviceTypes));
                    OnPropertyChanged(nameof(SelectedDeviceTypes));
                }
            }
        }

        /// <summary>
        /// Obtiene la lista de los tipos de equipos seleccionados.
        /// </summary>
        public IEnumerable<DeviceType> SelectedDeviceTypes
                    => (_selectedDeviceTypes ?? (_selectedDeviceTypes = new ObservableCollection<DeviceType>()))
            .OrderBy(dt => dt.TranslateToSpanish());

        /// <summary>
        /// Obtiene o establece la ubicación seleccionada.
        /// </summary>
        public ILocation SelectedLocation {
            get => _selectedLocation;
            set {
                _selectedLocation = value;
                OnPropertyChanged(nameof(SelectedLocation));

                if (value != null)
                {
                    if (value is Bus)
                        AcabusDataContext.DbContext.LoadRefences(value, nameof(Bus.Devices), _allDevices);
                    else if (value is Station)
                        AcabusDataContext.DbContext.LoadRefences(value, nameof(Station.Devices), _allDevices);

                    SelectedLocations.Add(value);

                    UpdateDeviceList();

                    OnPropertyChanged(nameof(AllLocations));
                }
            }
        }

        /// <summary>
        /// Obtiene la lista de las ubicaciones seleccionadas.
        /// </summary>
        public ObservableCollection<ILocation> SelectedLocations
            => _selectedLocations ?? (_selectedLocations = new ObservableCollection<ILocation>());

        /// <summary>
        /// Obtiene o establece la ruta seleccionada.
        /// </summary>
        public Route SelectedRoute {
            get => _selectedRoute;
            set {
                _selectedRoute = value;
                OnPropertyChanged(nameof(SelectedRoute));
                OnPropertyChanged(nameof(AllLocations));
            }
        }

        /// <summary>
        /// Obtiene o establece quien reporta las incidencias.
        /// </summary>
        public String SelectedWhoReporting {
            get => _selectedWhoReporting;
            set {
                _selectedWhoReporting = value;
                OnPropertyChanged(nameof(SelectedWhoReporting));
            }
        }

        /// <summary>
        /// Función que es llamada durante la carga de la vista.
        /// </summary>
        /// <param name="parameter"> Parametro del comando. </param>
        protected override void OnLoad(object parameter)
        {
            _allStations = AcabusDataContext.AllStations.ToList();
            _allBuses = AcabusDataContext.AllBuses.ToList();
            _allDevices = AcabusDataContext.AllDevices.ToList();

            OnPropertyChanged(nameof(AllLocations));
            OnPropertyChanged(nameof(AllDeviceTypes));
            OnPropertyChanged(nameof(AllRoutes));
        }

        /// <summary>
        /// Valida la propiedad especificada. Ocurre cuando una propiedad cambia su valor.
        /// </summary>
        /// <param name="propertyName"> Nombre de la propiedad. </param>
        protected override void OnValidation(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(SelectedWhoReporting):
                    if (String.IsNullOrEmpty(SelectedWhoReporting))
                        AddError(nameof(SelectedWhoReporting), "Falta ingresar quién reporta.");
                    break;

                case nameof(SelectedDescription):
                    if (String.IsNullOrEmpty(SelectedDescription))
                        if (SelectedDevices.Count() == 0)
                            AddError(nameof(SelectedDescription), "Falta seleccionar los equipos a reportar.");
                        else
                            AddError(nameof(SelectedDescription), "Falta ingresar la descripción de la incidencia.");
                    break;

                case nameof(SelectedLocations):
                    if (SelectedLocations.Count == 0)
                        AddError(nameof(SelectedLocations), "Falta seleccionar alguna estación.");
                    break;

                case nameof(SelectedDeviceTypes):
                    if (SelectedDeviceTypes.Count() == 0)
                        AddError(nameof(SelectedDeviceTypes), "Falta seleccionar al menos un tipo de equipo.");
                    break;

                case nameof(SelectedDevices):
                    if (SelectedDevices.Count() == 0)
                        AddError(nameof(SelectedDevices), "No hay equipos agregados.");
                    break;
            }
        }

        /// <summary>
        /// Determina si se puede ejecutar el comando para crear las incidencias.
        /// </summary>
        /// <param name="parameter"> Parametro del comando. </param>
        /// <returns> Un valor true si se puede ejecutar el comando. </returns>
        private bool CanCreate(object parameter)
        {
            if (!Validate()) return false;

            AcabusDataContext.GetService("Cctv_Manager", out dynamic service);

            CctvModule cctvModule = service as CctvModule;

            var incidences = cctvModule.Incidences.Where(i => i.Status == IncidenceStatus.OPEN);

            incidences = incidences.Where(i => SelectedDevices.Any(d => d.SelectedDevice == i.Device) && i.Fault.Description == SelectedDescription);
            if (incidences.Any())
                AddError(nameof(SelectedDescription), String.Format("Ya existe una incidencia abierta igual para: F-{0:D5}", incidences.First().Folio));

            return !incidences.Any();
        }

        /// <summary>
        /// Crea las incidencias especificadas.
        /// </summary>
        /// <param name="parameter">Parametro del comando.</param>
        private void CreateIncidence(Object parameter)
        {
            AcabusDataContext.GetService("Cctv_Manager", out dynamic service);
            CctvModule cctvModule = service as CctvModule;

            foreach (var device in SelectedDevices)
            {
                var incidences = cctvModule.Incidences;
                var fault = AcabusDataContext.DbContext.Read<DeviceFault>()
                    .FirstOrDefault(f => f.Description == SelectedDescription && f.Category.DeviceType == device.SelectedDevice.Type);
                Incidence incidence = new Incidence
                {
                    Device = device.SelectedDevice,
                    Fault = fault,
                    Observations = device.Observations ?? GlobalObservations,
                    Priority = Priority.LOW,
                    StartDate = DateTime.Now,
                    Status = IncidenceStatus.OPEN,
                    WhoReporting = SelectedWhoReporting,
                };

                incidence.AssignStaff();

                AcabusDataContext.DbContext.Create(incidence);
            }

            Dispatcher.CloseDialog();
        }

        /// <summary>
        /// Filtra las incidencias aplicable a los equipos seleccionados.
        /// </summary>
        /// <param name="source"> Fuente de datos. </param>
        /// <returns> Una secuencia de fallas o actividades aplicables a los equipos. </returns>
        private IEnumerable<DeviceFault> FilterByType(IQueryable<DeviceFault> source)
        {
            var types = GetDevicesTypes();

            var parameter = Expression.Parameter(typeof(DeviceFault), "f");
            var categoryProp = Expression.MakeMemberAccess(parameter, typeof(DeviceFault).GetProperty("Category"));
            var typeProp = Expression.MakeMemberAccess(categoryProp, typeof(DeviceFaultCategory).GetProperty("DeviceType"));

            List<Expression> filters = new List<Expression>();

            foreach (var type in types)
            {
                var typeConst = Expression.Constant(type);
                var equals = Expression.Equal(typeProp, typeConst);
                filters.Add(equals);
            }

            Expression filterExp = null;

            if (filters.Count > 0)
                filterExp = GetExpression(filters.First(), filters.Skip(1));
            else
                filterExp = Expression.Equal(typeProp, Expression.Constant(DeviceType.NONE, typeof(DeviceType)));

            LambdaExpression lambda = Expression.Lambda<Func<DeviceFault, bool>>(filterExp, parameter);

            return source.Where((Expression<Func<DeviceFault, bool>>)lambda).ToList();
        }

        /// <summary>
        /// Obtiene todos los tipos de los equipos seleccionados.
        /// </summary>
        /// <returns> Una secuencia de tipos de equipo. </returns>
        private IEnumerable<DeviceType> GetDevicesTypes()
                                            => SelectedDevices.Select(m => m.SelectedDevice.Type).Distinct();

        /// <summary>
        /// Obtiene una expresion enlazada a otra a través de una expresión Or.
        /// </summary>
        /// <param name="expression"> Expresion a enlazar. </param>
        /// <param name="enumerable">
        /// Secuencia con expresiones. Si la secuencia es nula o sin elementos, se devuelve <paramref
        /// name="expression" />.
        /// </param>
        /// <returns>
        /// Una expresion que unifica las expresiones de la secuencia con la especificada en
        /// <paramref name="expression" />.
        /// </returns>
        private Expression GetExpression(Expression expression, IEnumerable<Expression> enumerable)
        {
            if (expression == null)
                return null;

            if (enumerable == null)
                return null;

            if (enumerable.Count() == 0)
                return expression;

            return Expression.OrElse(expression, GetExpression(enumerable.FirstOrDefault(), enumerable.Skip(1)));
        }

        /// <summary>
        /// Actualiza la lista de equipos seleccionados.
        /// </summary>
        private void UpdateDeviceList()
        {
            try
            {
                _selectedDevices.Clear();

                if (_selectedDeviceTypes.Count == 0) return;
                if (SelectedLocations.Count == 0) return;

                foreach (var item in _selectedLocations.Select(s => s.Devices).Merge()
                    .Where(d => SelectedDeviceTypes.Contains(d.Type)
                        || (SelectedDeviceTypes.Contains(DeviceType.TOR)
                        && new[] { DeviceType.TA, DeviceType.TD, DeviceType.TS, DeviceType.TSI }.Contains(d.Type))))
                    _selectedDevices.Add(new MultiIncidenceItem() { SelectedDevice = item, Observations = "" });
            }
            finally
            {
                OnPropertyChanged(nameof(SelectedDevices));
                OnPropertyChanged(nameof(DeviceFaults));
                OnPropertyChanged(nameof(IsEnabledFaultList));
            }
        }

        /// <summary>
        /// Provoca la validación de las propiedades.
        /// </summary>
        /// <returns> Un valor true si las propiedades son validas. </returns>
        private Boolean Validate()
        {
            ValidateProperty(nameof(SelectedWhoReporting));
            ValidateProperty(nameof(SelectedDescription));
            ValidateProperty(nameof(SelectedLocations));
            ValidateProperty(nameof(SelectedDeviceTypes));
            ValidateProperty(nameof(SelectedDevices));

            return !HasErrors;
        }

        /// <summary>
        /// Representa un elemento de la tabla de equipos seleccionados.
        /// </summary>
        public sealed class MultiIncidenceItem : NotifyPropertyChanged
        {
            /// <summary>
            /// Campo que provee a la propiedad <see cref="Observations" />.
            /// </summary>
            private String _observations;

            /// <summary>
            /// Campo que provee a la propiedad <see cref="SelectedDevice" />.
            /// </summary>
            private Device _selectedDevice;

            /// <summary>
            /// Obtiene o establece las observaciones de la incidencia.
            /// </summary>
            public String Observations {
                get => _observations;
                set {
                    _observations = value;
                    OnPropertyChanged(nameof(Observations));
                }
            }

            /// <summary>
            /// Obtiene o establece el valor de esta propiedad.
            /// </summary>
            public Device SelectedDevice {
                get => _selectedDevice;
                set {
                    _selectedDevice = value;
                    OnPropertyChanged(nameof(SelectedDevice));
                }
            }
        }
    }
}