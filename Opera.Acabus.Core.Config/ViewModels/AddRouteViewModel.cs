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
    /// Define la estructura del modelo de la vista <see cref="Views.AddRouteView"/>
    /// </summary>
    public sealed class AddRouteViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="AssignedSection"/>.
        /// </summary>
        private String _assignedSection;

        /// <summary>
        /// Campo que provee a la propidad <see cref="IDLin"/>
        /// </summary>
        private UInt32 _idLin;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Name"/>.
        /// </summary>
        private String _name;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="RouteNumber"/>.
        /// </summary>
        private String _routeNumber;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Type"/>.
        /// </summary>
        private RouteType? _type;

        /// <summary>
        /// Crea una nueva instancia de <see cref="AddRouteViewModel"/>.
        /// </summary>
        public AddRouteViewModel()
            => AddRouteCommand = new Command(AddRouteExecute, AddRouteCanExecute);

        /// <summary>
        /// Obtiene el comando que agrega la ruta al sistema.
        /// </summary>
        public ICommand AddRouteCommand { get; }

        /// <summary>
        /// Obtiene una lista de las secciones que pueden ser asignados para dar el mantenimiento.
        /// </summary>
        public List<String> AssignableSections
            => AcabusDataContext.AssignableSection.ToList();

        /// <summary>
        /// Obtiene o establece la sección asignada para la atención de la ruta.
        /// </summary>
        public string AssignedSection {
            get => _assignedSection;
            set {
                _assignedSection = value;
                OnPropertyChanged(nameof(AssignedSection));
            }
        }

        /// <summary>
        /// Obtiene o establece el valor del identificador de la ruta.
        /// </summary>
        public UInt32 IDLin {
            get => _idLin;
            set {
                _idLin = value;
                OnPropertyChanged(nameof(IDLin));
            }
        }

        /// <summary>
        /// Obtiene o establece el nombre de la ruta.
        /// </summary>
        public String Name {
            get => _name;
            set {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Obtiene o estable el número de la ruta.
        /// </summary>
        public String RouteNumber {
            get { return _routeNumber; }
            set {
                _routeNumber = value;
                OnPropertyChanged(nameof(RouteNumber));
            }
        }

        /// <summary>
        /// Obtiene o establece el tipo de la ruta.
        /// </summary>
        public RouteType? Type {
            get => _type;
            set {
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        /// <summary>
        /// Obtiene una lista de los tipos de rutas válidas.
        /// </summary>
        public IEnumerable<RouteType> Types
            => Enum.GetValues(typeof(RouteType)).Cast<RouteType>().Where(x => x != RouteType.NONE);

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
                        AddError(nameof(Name), "Especifique la descripción de la ruta.");
                    break;

                case nameof(RouteNumber):
                    if (!UInt16.TryParse(RouteNumber, out ushort result) || result <= 0)
                        AddError(nameof(RouteNumber), "Especifique un número válido de ruta.");
                    break;

                case nameof(Type):
                    if (Type == null || !Types.Any(x => x == Type))
                        AddError(nameof(Type), "Especifique un tipo de válido de ruta.");
                    break;

                case nameof(AssignedSection):
                    if (!String.IsNullOrEmpty(AssignedSection) && !AssignableSections.Any(x => x == AssignedSection))
                        AddError(nameof(AssignedSection), "Especifique una sección de válida.");
                    break;
            }
        }

        /// <summary>
        /// Determina si es posible ejecutar el comando <see cref="AddRouteCommand"/>.
        /// </summary>
        /// <param name="arg">Parametro del comando.</param>
        /// <returns>Un valor true si es posible ejecutar el comando.</returns>
        private bool AddRouteCanExecute(object arg)
            => Validate();

        /// <summary>
        /// Crea una instancia <see cref="Route"/> a partir de la información de la instancia actual
        /// <see cref="AddRouteViewModel"/> y la guarda en la base de datos. Acción que realiza el
        /// comando <see cref="AddRouteCommand"/>.
        /// </summary>
        /// <param name="obj">Parametro del comando.</param>
        private void AddRouteExecute(object obj)
        {
            Route route = new Route(IDLin, UInt16.Parse(RouteNumber), Type.Value)
            {
                AssignedSection = AssignedSection,
                Name = Name
            };

            if (ServerContext.GetLocalSync("Route").Create(ref route))
                Dispatcher.SendMessageToGUI($"Ruta: {route} agregado correctamente.");
            else
                Dispatcher.SendMessageToGUI("No se pudo guardar la ruta nueva.");

            Dispatcher.CloseDialog();
        }

        /// <summary>
        /// Determina si los campos del formulario está correctamente llenos.
        /// </summary>
        private bool Validate()
        {
            ValidateProperty(nameof(Name));
            ValidateProperty(nameof(RouteNumber));
            ValidateProperty(nameof(AssignedSection));
            ValidateProperty(nameof(Type));

            return !HasErrors;
        }
    }
}