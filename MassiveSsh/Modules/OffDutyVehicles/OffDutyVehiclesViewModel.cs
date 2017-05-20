using Acabus.DataAccess;
using Acabus.Models;
using Acabus.Utils.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Acabus.Modules.OffDutyVehicles
{
    /// <summary>
    /// Defiene el modelo de la vista para el cuadro de diálogo utilizado para la
    /// adición de vehículos fuera de servicio.
    /// </summary>
    public sealed class OffDutyVehiclesViewModel : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'SelectedVehicle'.
        /// </summary>
        private Vehicle _selectedVehicle;

        /// <summary>
        /// Obtiene o establece el vehículo actualmente seleccionado en la tabla.
        /// </summary>
        public Vehicle SelectedVehicle {
            get => _selectedVehicle;
            set {
                _selectedVehicle = value;
                OnPropertyChanged("SelectedVehicle");
            }
        }

        /// <summary>
        /// Obtiene una lista de los vehículos con algún problema que impida enviar su localización.
        /// </summary>
        public ObservableCollection<Vehicle> Vehicles => AcabusData.OffDutyVehicles;

        /// <summary>
        /// Campo que provee a la propiedad 'EconomicNumber'.
        /// </summary>
        private String _economicNumber;

        /// <summary>
        /// Obtiene o establece el número económico del equipo a agregar.
        /// </summary>
        public String EconomicNumber {
            get => _economicNumber;
            set {
                _economicNumber = value;
                OnPropertyChanged("EconomicNumber");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'Status'.
        /// </summary>
        private VehicleStatus _selectedStatus;

        /// <summary>
        /// Obtiene o establece el estado seleccionado para el vehículo.
        /// </summary>
        public VehicleStatus SelectedStatus {
            get => _selectedStatus;
            set {
                _selectedStatus = value;
                OnPropertyChanged("SelectedStatus");
            }
        }

        /// <summary>
        /// Obtiene una lista de los estados del vehículo disponibles.
        /// </summary>
        public IEnumerable<VehicleStatus> VehicleStatus => Enum.GetValues(typeof(VehicleStatus)).Cast<VehicleStatus>();

        /// <summary>
        /// Comando para agregar vehículos al listado.
        /// </summary>
        public ICommand AddVehicleCommand { get; }

        /// <summary>
        /// Comando para limpiar la lista de vehículos.
        /// </summary>
        public ICommand ClearVehicleCommand { get; }

        /// <summary>
        /// Comando para guardar la información de los vehículos.
        /// </summary>
        public ICommand SaveVehicleCommand { get; }

        /// <summary>
        /// Comando para recargar la información guardada en la lista de vehículos.
        /// </summary>
        public ICommand ReloadVehicleCommand { get; }

        /// <summary>
        /// Comando para remover un vehículo de la lista de vehículos.
        /// </summary>
        public ICommand RemoveVehicleCommand { get; }

        /// <summary>
        /// Crea una instancia del modelo de la vista de <see cref="OffDutyVehiclesView"/>.
        /// </summary>
        public OffDutyVehiclesViewModel()
        {
            AddVehicleCommand = new CommandBase(delegate
            {
                if (String.IsNullOrEmpty(EconomicNumber)) return;

                var economicNumbers = EconomicNumber.Split(new String[] { "\n", "\r\n" },
                                                                StringSplitOptions.RemoveEmptyEntries);

                foreach (var economicNumber in economicNumbers)
                {
                    var matches = Regex.Match(economicNumber, "A[ACP]{1}-[0-9]{3}").Groups;
                    if (matches.Count < 1) continue;
                    foreach (var match in matches)
                        if (!String.IsNullOrEmpty(match.ToString()))
                        {
                            Boolean exists = false;
                            foreach (Vehicle vehi in Vehicles)
                                if (vehi.EconomicNumber == match.ToString())
                                {
                                    exists = true;
                                    break;
                                }
                            if (!exists)
                                Vehicles.Add(new Vehicle(match.ToString(), SelectedStatus));
                        }
                }

                EconomicNumber = String.Empty;
            });

            ClearVehicleCommand = new CommandBase((param) => Vehicles?.Clear());

            SaveVehicleCommand = new CommandBase((param) => AcabusData.SaveOffDutyVehiclesList());

            ReloadVehicleCommand = new CommandBase((param) => AcabusData.LoadOffDutyVehicles());

            RemoveVehicleCommand = new CommandBase((param) =>
            {
                Vehicles?.Remove(SelectedVehicle);
                SelectedVehicle = null;
            });
        }

    }
}
