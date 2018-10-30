using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Opera.Acabus.Core.Config.ViewModels
{
    /// <summary>
    /// Define la estructura del modelo de la vista <see cref="Views.AddBusView"/>.
    /// </summary>
    public sealed class AddBusViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="AssignedRoute" />.
        /// </summary>
        private Route _assignedRoute;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="EconomicNumber" />.
        /// </summary>
        private string _economicNumber;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Type" />.
        /// </summary>
        private BusType? _type;

        /// <summary>
        /// Crea una nueva instancia del modelo de la vista.
        /// </summary>
        public AddBusViewModel()
            => AddBusCommand = new Command(AddBusExecute, AddBusCanExecute);

        /// <summary>
        /// Obtiene el comando que agrega el autobús al sistema.
        /// </summary>
        public Command AddBusCommand { get; }

        /// <summary>
        /// Obtiene o establece la ruta a la cual se asigna el autobús.
        /// </summary>
        public Route AssignedRoute {
            get => _assignedRoute;
            set {
                _assignedRoute = value;
                OnPropertyChanged(nameof(AssignedRoute));
            }
        }

        /// <summary>
        /// Obtiene o establece el número económico del autobús.
        /// </summary>
        public string EconomicNumber {
            get => _economicNumber;
            set {
                _economicNumber = value;
                OnPropertyChanged(nameof(EconomicNumber));
            }
        }

        /// <summary>
        /// Obtiene una lista de las rutas para asignar.
        /// </summary>
        public IEnumerable<Route> Routes
            => AcabusDataContext.AllRoutes.ToList().OrderBy(x => x.Type).ThenBy(x => x.RouteNumber);

        /// <summary>
        /// Obtiene o establece el tipo de la unidad. <seealso cref="BusType"/>.
        /// </summary>
        public BusType? Type {
            get => _type;
            set {
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        /// <summary>
        /// Obtiene una lista de los tipos de autobuses.
        /// </summary>
        public IEnumerable<BusType> Types => Enum.GetValues(typeof(BusType))
            .Cast<BusType>().Where(x => x != BusType.NONE);

        /// <summary>
        /// Determina si los campos del formulario está correctamente llenos.
        /// </summary>
        public Boolean Validate()
        {
            ValidateProperty(nameof(EconomicNumber));
            ValidateProperty(nameof(Type));
            ValidateProperty(nameof(AssignedRoute));

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
                case nameof(EconomicNumber):
                    if (String.IsNullOrEmpty(EconomicNumber))
                        AddError(nameof(EconomicNumber), "Especifique el número económico del autobus.");
                    break;

                case nameof(Type):
                    if (Type == null || !Types.Any(x => x == Type))
                        AddError(nameof(Type), "Especifique el tipo del autobús.");
                    break;

                case nameof(AssignedRoute):
                    if (AssignedRoute == null)
                        AddError(nameof(AssignedRoute), "Especifique una ruta válida.");
                    break;
            }
        }

        /// <summary>
        /// Determina si es posible ejecutar el comando <see cref="AddBusCommand"/>.
        /// </summary>
        /// <param name="arg">Parametro del comando.</param>
        /// <returns>Un valor true si es posible ejecutar el comando.</returns>
        private bool AddBusCanExecute(object arg)
            => Validate();

        /// <summary>
        /// Crea una instancia <see cref="Bus"/> a partir de la información de la instancia actual
        /// <see cref="AddBusViewModel"/> y la guarda en la base de datos. Acción que realiza el
        /// comando <see cref="AddBusCommand"/>.
        /// </summary>
        /// <param name="obj">Parametro del comando.</param>
        private void AddBusExecute(object obj)
        {
            Bus bus = new Bus(0, EconomicNumber)
            {
                Status = BusStatus.OPERATIONAL,
                Type = Type.Value,
                Route = AssignedRoute
            };

            if (ServerContext.GetLocalSync("Bus").Create(ref bus))
                Dispatcher.SendMessageToGUI($"Autobús: {bus} agregado correctamente.");
            else
                Dispatcher.SendMessageToGUI("No se pudo guardar el autobús nuevo.");

            Dispatcher.CloseDialog();
        }
    }
}