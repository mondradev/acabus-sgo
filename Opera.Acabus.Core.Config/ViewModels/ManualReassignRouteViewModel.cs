using InnSyTech.Standard.Database.Linq;
using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Opera.Acabus.Core.Config.ViewModels
{
    /// <summary>
    /// Define la estructura del modelo de la vista <see cref="Views.ManualReassignRouteView"/>.
    /// </summary>
    public sealed class ManualReassignRouteViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="EconomicNumbers"/>.
        /// </summary>
        private String _economicNumbers;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedBus"/>.
        /// </summary>
        private Bus _selectedBus;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedRoute"/>.
        /// </summary>
        private Route _selectedRoute;

        /// <summary>
        /// Crea una instancia nueva de <see cref="ManualReassignRouteViewModel"/>.
        /// </summary>
        public ManualReassignRouteViewModel()
        {
            ReassignRouteCommand = new Command(ReassignRoute, CanReassignRoute);
            DoneCommand = Dispatcher.CloseDialogCommand;

            DiscartCommand = new Command(parameter =>
            {
                SelectedRoute = null;
                EconomicNumbers = String.Empty;
            });

            UpdateSelectionCommand = new Command(param =>
            {
                if (!(param is IList selectedBuses)) return;

                _economicNumbers = String.Empty;

                foreach (Bus b in selectedBuses)
                    _economicNumbers += b.EconomicNumber + "\n";

                _economicNumbers= _economicNumbers.Remove(_economicNumbers.Length - 1);

                OnPropertyChanged(nameof(EconomicNumbers));

            });
        }

        /// <summary>
        /// Obtiene una lista de todos los autobuses registrados.
        /// </summary>
        public IEnumerable<Bus> Buses => AcabusDataContext.AllBuses.LoadReference(1);

        /// <summary>
        /// Obtiene el comando para descartar la información del formulario.
        /// </summary>
        public ICommand DiscartCommand { get; }

        /// <summary>
        /// Obtiene el comando para confirmar la información de la vista.
        /// </summary>
        public ICommand DoneCommand { get; }

        /// <summary>
        /// Obtiene o establece los números económicos a reasignar ruta.
        /// </summary>
        public String EconomicNumbers {
            get => _economicNumbers;
            set {
                _economicNumbers = value;
                OnPropertyChanged(nameof(EconomicNumbers));
            }
        }

        /// <summary>
        /// Obtiene el comando para reasignar la ruta a los autobuses especificados.
        /// </summary>
        public ICommand ReassignRouteCommand { get; }

        /// <summary>
        /// Actualiza la lista de selección de la tabla.
        /// </summary>
        public ICommand UpdateSelectionCommand { get; }


        /// <summary>
        /// Obtiene una lista de las rutas disponibles para reasignación.
        /// </summary>
        public IEnumerable<Route> Routes => AcabusDataContext.AllRoutes;

        /// <summary>
        /// Obtiene o establece autobus seleccionado en la tabla.
        /// </summary>
        public Bus SelectedBus {
            get => _selectedBus;
            set {
                _selectedBus = value;
                OnPropertyChanged(nameof(SelectedBus));
                if (value != null)
                    EconomicNumbers = value.EconomicNumber;
            }
        }

        /// <summary>
        /// Obtiene o establece la nueva ruta a reasignar.
        /// </summary>
        public Route SelectedRoute {
            get => _selectedRoute;
            set {
                _selectedRoute = value;
                OnPropertyChanged(nameof(SelectedRoute));
            }
        }

        /// <summary>
        /// Función que es llamada durante la validación del valor de las propiedades.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad a validar.</param>
        protected override void OnValidation(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(SelectedRoute):
                    if (SelectedRoute == null)
                        AddError(nameof(SelectedRoute), "Seleccione una ruta valida.");
                    break;

                case nameof(EconomicNumbers):
                    if (String.IsNullOrEmpty(EconomicNumbers)
                        || Regex.Matches(EconomicNumbers.ToUpper(), "A[APC]{1}-[0-9]{3}").Count == 0)
                        AddError(nameof(EconomicNumbers), "Ingrese uno o más números económicos por cada linea.");
                    break;
            }
        }

        /// <summary>
        /// Determina si el comando <see cref="ReassignRouteCommand"/> puede ser ejecutado.
        /// </summary>
        /// <param name="arg">Parametro del comando.</param>
        /// <returns>Un valor <see cref="true"/> si el comando puede ser ejecutado.</returns>
        private bool CanReassignRoute(object arg)
        {
            ValidateProperty(nameof(SelectedRoute));
            ValidateProperty(nameof(EconomicNumbers));

            return !HasErrors;
        }

        /// <summary>
        /// Acción que ejecuta el comando <see cref="ReassignRouteCommand"/>.
        /// </summary>
        /// <param name="obj">Parametro del comando.</param>
        private void ReassignRoute(object obj)
        {
            var economicNumbers = Regex.Matches(EconomicNumbers.ToUpper(), "A[APC]{1}-[0-9]{3}");
            foreach (var item in economicNumbers)
            {
                Bus bus = Buses.FirstOrDefault(vehicle => vehicle.EconomicNumber == item.ToString());
                bus.Route = SelectedRoute;
                if (!AcabusDataContext.DbContext.Update(bus))
                {
                    Dispatcher.SendMessageToGUI($"Error al reasignar la unidad {bus}");
                    return;
                }
            }
            SelectedRoute = null;
            EconomicNumbers = String.Empty;
            OnPropertyChanged(nameof(Buses));
        }
    }
}