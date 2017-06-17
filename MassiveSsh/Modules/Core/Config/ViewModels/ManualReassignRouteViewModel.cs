using Acabus.DataAccess;
using Acabus.Models;
using Acabus.Utils.Mvvm;
using Acabus.Window;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Acabus.Modules.Core.Config.ViewModels
{
    public sealed class ManualReassignRouteViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad 'EconomicNumbers'.
        /// </summary>
        private String _economicNumbers;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedRoute'.
        /// </summary>
        private Route _selectedRoute;

        /// <summary>
        /// Campo que provee a la propiedad 'SelectedVehicle'.
        /// </summary>
        private Vehicle _selectedVehicle;

        public ManualReassignRouteViewModel()
        {
            ReassignRouteCommand = new CommandBase(ReassignRoute, CanReassignRoute);
            DoneCommand = DialogHost.CloseDialogCommand;
            DiscartCommand = new CommandBase(parameter =>
            {
                SelectedRoute = null;
                EconomicNumbers = String.Empty;
            });
        }

        public ICommand DiscartCommand { get; }

        public ICommand DoneCommand { get; }

        /// <summary>
        /// Obtiene o establece los números económicos a reasignar ruta.
        /// </summary>
        public String EconomicNumbers {
            get => _economicNumbers;
            set {
                _economicNumbers = value;
                OnPropertyChanged("EconomicNumbers");
            }
        }

        public ICommand ReassignRouteCommand { get; }

        /// <summary>
        /// Obtiene una lista de las rutas disponibles para reasignación.
        /// </summary>
        public IEnumerable<Route> Routes => DataAccess.AcabusData.AllRoutes;

        /// <summary>
        /// Obtiene o establece la nueva ruta a reasignar.
        /// </summary>
        public Route SelectedRoute {
            get => _selectedRoute;
            set {
                _selectedRoute = value;
                OnPropertyChanged("SelectedRoute");
            }
        }

        /// <summary>
        /// Obtiene o establece vehículo seleccionado en la tabla.
        /// </summary>
        public Vehicle SelectedVehicle {
            get => _selectedVehicle;
            set {
                _selectedVehicle = value;
                OnPropertyChanged("SelectedVehicle");
                if (value != null)
                    EconomicNumbers = value.EconomicNumber;
            }
        }

        /// <summary>
        /// Obtiene una lista de todos los vehículos registrados.
        /// </summary>
        public IEnumerable<Vehicle> Vehicles => DataAccess.AcabusData.AllVehicles;

        protected override void OnValidation(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(SelectedRoute):
                    if (SelectedRoute == null)
                        AddError("SelectedRoute", "Seleccione una ruta valida.");
                    break;

                case nameof(EconomicNumbers):
                    if (String.IsNullOrEmpty(EconomicNumbers) || Regex.Matches(EconomicNumbers.ToUpper(), "A[APC]{1}-[0-9]{3}").Count == 0)
                        AddError("EconomicNumbers", "Ingrese uno o más números económicos por cada linea.");
                    break;
            }
        }

        private bool CanReassignRoute(object arg)
        {
            ValidateProperty(nameof(SelectedRoute));
            ValidateProperty(nameof(EconomicNumbers));

            return !HasErrors;
        }

        private void ReassignRoute(object obj)
        {
            var economicNumbers = Regex.Matches(EconomicNumbers.ToUpper(), "A[APC]{1}-[0-9]{3}");
            foreach (var item in economicNumbers)
            {
                Vehicle vehi = Vehicles.FirstOrDefault(vehicle => vehicle.EconomicNumber == item.ToString());
                vehi.Route = SelectedRoute;
                if (!AcabusData.Session.Update(vehi))
                {
                    AcabusControlCenterViewModel.ShowDialog($"Error al reasignar la unidad {vehi}");
                    return;
                }
            }
            SelectedRoute = null;
            EconomicNumbers = String.Empty;
            OnPropertyChanged("Vehicles");
        }
    }
}