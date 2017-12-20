using InnSyTech.Standard.Database.Linq;
using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Cctv.Helpers;
using Opera.Acabus.Cctv.Models;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Gui.Modules;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Opera.Acabus.Cctv.SubModules.AddIncidence.ViewModels
{
    /// <summary>
    /// Define el modelo de la vista <see cref="Views.AddIncidencesView"/>.
    /// </summary>
    public sealed class AddIncidencesViewModel : ModuleViewerBase
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="IsBusIncidences" />
        /// </summary>
        private Boolean _isBusIncidences;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Observations" />.
        /// </summary>
        private String _observations;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedBus" />
        /// </summary>
        private Bus _selectedBus;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedDevice" />
        /// </summary>
        private Device _selectedDevice;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedFault" />
        /// </summary>
        private Activity _selectedFault;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedLocation" />
        /// </summary>
        private IAssignableSection _selectedLocation;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="StartTime" />
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="WhoReport" />
        /// </summary>
        private String _whoReporting;

        /// <summary>
        /// Crea una nueva instancia del cuadro de dialogo para crear incidencias.
        /// </summary>
        public AddIncidencesViewModel()
        {
            CreateIncidenceCommand = new Command(CreateIncidence, CanCreate);

            _startTime = DateTime.Now;
        }

        /// <summary>
        /// Obtiene o establece el listado de los autobuses.
        /// </summary>
        public IEnumerable<Bus> Buses {
            get {
                if (!IsBusIncidences)
                    return null;
                if (SelectedLocation is null)
                    return AcabusDataContext.AllBuses.LoadReference(1);
                else
                    return AcabusDataContext.AllBuses.LoadReference(1)
                        .Where(b => b.Route.ID == (SelectedLocation as Route).ID);
            }
        }

        /// <summary>
        /// Obtiene una lista de las entidades que realizan reportes.
        /// </summary>
        public IEnumerable<String> Business
            => AcabusDataContext.ConfigContext["Cctv"]?
                .GetSetting("business")?
                .GetSettings("business")?
                .Select(s => s.ToString("value"))
                .OrderBy(s => s);

        /// <summary>
        /// Comando que crea la incidencia nueva.
        /// </summary>
        public ICommand CreateIncidenceCommand { get; }

        /// <summary>
        /// Obtiene una lista de todas las fallas disponibles.
        /// </summary>
        public IEnumerable<Activity> DeviceFaults {
            get {
                if (SelectedDevice is null)
                    return null;

                return AcabusDataContext.DbContext.Read<Activity>()
                    .Where(f => f.Category.DeviceType == SelectedDevice.Type);
            }
        }

        /// <summary>
        /// Obtiene una lista de equipos disponibles en la estación.
        /// </summary>
        public IEnumerable<Device> Devices {
            get {
                if (IsBusIncidences)
                    return SelectedBus is null
                        ? null
                        : AcabusDataContext.AllDevices.LoadReference(1)
                                    .Where(d => d.Bus.EconomicNumber == SelectedBus.EconomicNumber);
                else
                    return SelectedLocation is null
                        ? null
                        : AcabusDataContext.AllDevices.LoadReference(1)
                                    .Where(d => d.Station.ID == (SelectedLocation as Station).ID);
            }
        }

        /// <summary>
        /// Obtiene el nombre del cuadro de texto para autobus o estación.
        /// </summary>
        public String HeaderDevice => IsBusIncidences ? "Equipo abordo" : "Equipo";

        /// <summary>
        /// Obtiene el nombre del cuadro de texto para ubicación.
        /// </summary>
        public String HeaderRouteOrStation => IsBusIncidences ? "Ruta" : "Estación";

        /// <summary>
        /// Obtiene o establece si la incidencias es originada en un autobus.
        /// </summary>
        public Boolean IsBusIncidences {
            get => _isBusIncidences;
            set {
                _isBusIncidences = value;

                OnPropertyChanged(nameof(IsBusIncidences));
                OnPropertyChanged(nameof(Locations));
                OnPropertyChanged(nameof(Devices));
                OnPropertyChanged(nameof(Buses));
                OnPropertyChanged(nameof(HeaderDevice));
                OnPropertyChanged(nameof(HeaderRouteOrStation));

                ClearErrors();
            }
        }

        /// <summary>
        /// Obtiene una lista de todas las ubicaciones disponibles.
        /// </summary>
        public IEnumerable<IAssignableSection> Locations {
            get {
                if (IsBusIncidences)
                    return AcabusDataContext.AllRoutes.ToList().Cast<IAssignableSection>();

                return AcabusDataContext.AllStations.ToList().Cast<IAssignableSection>();
            }
        }

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
        /// Obtiene o establece el autobus seleccionado.
        /// </summary>
        public Bus SelectedBus {
            get => _selectedBus;
            set {
                _selectedBus = value;

                OnPropertyChanged(nameof(SelectedBus));
                OnPropertyChanged(nameof(Devices));

                if (value != null)
                {
                    _selectedLocation = value.Route;
                    OnPropertyChanged(nameof(SelectedLocation));
                }
            }
        }

        /// <summary>
        /// Obtiene o establece el equipo que presenta la incidencia.
        /// </summary>
        public Device SelectedDevice {
            get => _selectedDevice;
            set {
                _selectedDevice = value;

                OnPropertyChanged(nameof(SelectedDevice));
                OnPropertyChanged(nameof(DeviceFaults));

                if (!IsBusIncidences && SelectedDevice != null && !SelectedDevice.Station.Equals(SelectedLocation))
                {
                    _selectedLocation = SelectedDevice.Station;
                    OnPropertyChanged(nameof(SelectedLocation));
                }
            }
        }

        /// <summary>
        /// Obtiene o establece la falla del equipo.
        /// </summary>
        public Activity SelectedFault {
            get => _selectedFault;
            set {
                _selectedFault = value;
                OnPropertyChanged(nameof(SelectedFault));
            }
        }

        /// <summary>
        /// Obtiene o establece la ubicación de la incidencia.
        /// </summary>
        public IAssignableSection SelectedLocation {
            get => _selectedLocation;
            set {
                _selectedLocation = value;
                OnPropertyChanged(nameof(SelectedLocation));
                OnPropertyChanged(nameof(Devices));
                OnPropertyChanged(nameof(Buses));
            }
        }

        /// <summary>
        /// Obtiene o establece el tiempo de inicio de la incidencia.
        /// </summary>
        public DateTime StartTime {
            get => _startTime;
            set {
                _startTime = value;
                OnPropertyChanged(nameof(StartTime));
            }
        }

        /// <summary>
        /// Obtiene o establece quien reporta la incidencia.
        /// </summary>
        public String WhoReporting {
            get => _whoReporting;
            set {
                _whoReporting = value;
                OnPropertyChanged(nameof(WhoReporting));
            }
        }

        /// <summary>
        /// Valida las propiedades despues de ser asigandas.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad a validar.</param>
        protected override void OnValidation(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(WhoReporting):
                    if (String.IsNullOrEmpty(WhoReporting))
                        AddError(nameof(WhoReporting), "Falta ingresar quién reporta");
                    break;

                case nameof(SelectedFault):
                    if (SelectedFault is null)
                        if (SelectedDevice is null)
                            AddError(nameof(SelectedFault), "Seleccione primero el equipo que presenta la incidencia.");
                        else
                            AddError(nameof(SelectedFault), "Falta ingresar la descripción de la incidencia.");
                    break;

                case nameof(SelectedLocation):
                    if (String.IsNullOrEmpty(SelectedLocation?.ToString()))
                        AddError(nameof(SelectedLocation), String.Format("Falta seleccionar {0}.", IsBusIncidences ? "la ruta" : "la estación"));
                    break;

                case nameof(SelectedBus):
                    if (IsBusIncidences && String.IsNullOrEmpty(SelectedBus?.ToString()))
                        AddError(nameof(SelectedBus), "Falta seleccionar la unidad.");
                    break;

                case nameof(SelectedDevice):
                    if (String.IsNullOrEmpty(SelectedDevice?.ToString()))
                        AddError(nameof(SelectedDevice), "Falta seleccionar el equipo.");
                    break;
            }
        }

        /// <summary>
        /// Determina si el comando <see cref="CreateIncidenceCommand"/> puede ser ejecutado.
        /// </summary>
        /// <param name="parameter">Parametro del comando.</param>
        /// <returns>Un valor true si es posible ejecutar el comando.</returns>
        private bool CanCreate(object parameter)
        {
            if (!Validate()) return false;

            AcabusDataContext.GetService("Cctv_Manager", out dynamic service);
            CctvModule cctvModule = service as CctvModule;

            var incidences = cctvModule.Incidences;

            if (incidences.Any(i => i.Device == SelectedDevice && i.Activity == SelectedFault && i.Status == IncidenceStatus.OPEN))
                AddError(nameof(SelectedFault), "Ya existe una incidencia abierta igual para el equipo");

            return !HasErrors;
        }

        /// <summary>
        /// Crea la incidencia nueva y finaliza el cuadro de dialogo.
        /// </summary>
        /// <param name="parameter">Parametro del comando.</param>
        private void CreateIncidence(Object parameter)
        {
            AcabusDataContext.GetService("Cctv_Manager", out dynamic service);
            CctvModule cctvModule = service as CctvModule;

            var incidences = cctvModule.Incidences;
            Incidence incidence = new Incidence
            {
                Device = SelectedDevice,
                Activity = SelectedFault,
                Observations = Observations,
                Priority = Priority.LOW,
                StartDate = StartTime,
                Status = IncidenceStatus.OPEN,
                WhoReporting = WhoReporting,
            };

            incidence.AssignStaff();

            AcabusDataContext.DbContext.Create(incidence);

            Dispatcher.CloseDialog();
        }

        /// <summary>
        /// Provoca la validación de las propiedades necesarias para la creacion de la incidencia.
        /// </summary>
        /// <returns> Un valor true si es valido crear la incidencia. </returns>
        private Boolean Validate()
        {
            ValidateProperty(nameof(WhoReporting));
            ValidateProperty(nameof(SelectedFault));
            ValidateProperty(nameof(SelectedLocation));
            ValidateProperty(nameof(SelectedDevice));

            if (IsBusIncidences)
                ValidateProperty(nameof(SelectedBus));

            if (HasErrors) return false;

            return true;
        }
    }
}