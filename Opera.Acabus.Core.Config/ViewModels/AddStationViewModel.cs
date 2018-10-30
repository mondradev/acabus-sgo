using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Opera.Acabus.Core.Config.ViewModels
{
    /// <summary>
    /// Define la estructura del modelo de la vista <see cref="Views.AddStationView"/>.
    /// </summary>
    public sealed class AddStationViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="AssignedSection"/>.
        /// </summary>
        private string _assignedSection;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="IsExternal"/>.
        /// </summary>
        private Boolean _isExternal = false;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Name"/>.
        /// </summary>
        private String _name;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedRoute" />.
        /// </summary>
        private Route _selectedRoute;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="StationNumber"/>.
        /// </summary>
        private String _stationNumber;

        /// <summary>
        /// Crea una instancia nueva de <see cref="AddStationViewModel"/>.
        /// </summary>
        public AddStationViewModel()
            => AddStationCommand = new Command(AddStationExecute, AddStationCanExecute);

        /// <summary>
        /// Obtiene el comando que agrega la estación al sistema.
        /// </summary>
        public ICommand AddStationCommand { get; }

        /// <summary>
        /// Obtiene una lista de las secciones que pueden ser asignados para dar el mantenimiento.
        /// </summary>
        public List<String> AssignableSections
            => AcabusDataContext.AssignableSection.ToList();

        /// <summary>
        /// Obtiene o establece la sección asignada para el mantenimiento.
        /// </summary>
        public String AssignedSection {
            get => _assignedSection;
            set {
                _assignedSection = value;
                OnPropertyChanged(nameof(AssignedSection));
            }
        }

        /// <summary>
        /// Obtiene o establece un valor que indica si la estación es externa.
        /// </summary>
        public Boolean IsExternal {
            get => _isExternal;
            set {
                _isExternal = value;
                OnPropertyChanged(nameof(IsExternal));
            }
        }

        /// <summary>
        /// Obtiene o establece el nombre de la estación.
        /// </summary>
        public String Name {
            get => _name;
            set {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Obtiene una lista de las rutas disponibles.
        /// </summary>
        public IEnumerable<Route> Routes => AcabusDataContext.AllRoutes
            .Where(x => x.Type == RouteType.TRUNK)
            .OrderBy(x => x.RouteNumber);

        /// <summary>
        /// Obtiene o establece la ruta seleccionada.
        /// </summary>
        public Route SelectedRoute {
            get => _selectedRoute;
            set {
                _selectedRoute = value;
                OnPropertyChanged(nameof(SelectedRoute));
            }
        }

        /// <summary>
        /// Obtiene o establece el número de la estación.
        /// </summary>
        public String StationNumber {
            get => _stationNumber;
            set {
                _stationNumber = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Determina si los campos del formulario está correctamente llenos.
        /// </summary>
        public bool Validate()
        {
            ValidateProperty(nameof(StationNumber));
            ValidateProperty(nameof(Name));
            ValidateProperty(nameof(SelectedRoute));
            ValidateProperty(nameof(AssignedSection));

            return !HasErrors;
        }

        /// <summary>
        /// Función que es llamada durante la validación del valor de las propiedades.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad a validar.</param>
        protected override void OnValidation(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Name):
                    if (String.IsNullOrEmpty(Name))
                        AddError(nameof(Name), "Especifique una descripción para la estación.");
                    break;

                case nameof(SelectedRoute):
                    if (SelectedRoute == null)
                        AddError(nameof(SelectedRoute), "Especifique la ruta a la que pertenece la estación.");
                    else if (!Routes.ToList().Any(x => x.ID == SelectedRoute.ID))
                        AddError(nameof(SelectedRoute), "Especifique una ruta válida.");
                    break;

                case nameof(StationNumber):
                    if (!UInt16.TryParse(StationNumber, out ushort result) || result <= 0)
                        AddError(nameof(StationNumber), "Especifique un número de estación válido.");
                    break;

                case nameof(AssignedSection):
                    if (String.IsNullOrEmpty(AssignedSection) || !AssignableSections.Any(x => x == AssignedSection))
                        AddError(nameof(AssignedSection), "Especifique una sección válida.");
                    break;
            }
        }

        /// <summary>
        /// Determina si es posible ejecutar el comando <see cref="AddStationCommand" />.
        /// </summary>
        /// <param name="arg">Parametro del comando.</param>
        /// <returns>Un valor true si es posible ejecutar el comando.</returns>
        private bool AddStationCanExecute(object arg)
            => Validate();

        /// <summary>
        /// Crea una instancia <see cref="Station"/> a partir de la información de la instancia actual
        /// <see cref="AddStationViewModel"/> y la guarda en la base de datos. Acción que realiza el
        /// comando <see cref="AddStationCommand"/>.
        /// </summary>
        /// <param name="obj">Parametro del comando.</param>
        private void AddStationExecute(object obj)
        {
            Station station = new Station(0, UInt16.Parse(StationNumber))
            {
                Name = Name,
                IsExternal = IsExternal,
                AssignedSection = AssignedSection,
                Route = SelectedRoute
            };

            if (ServerContext.GetLocalSync("Station").Create(ref station))
                Dispatcher.SendMessageToGUI($"Estación: {station} agregado correctamente.");
            else
                Dispatcher.SendMessageToGUI("No se pudo guardar la estación nueva.");

            Dispatcher.CloseDialog();
        }
    }
}