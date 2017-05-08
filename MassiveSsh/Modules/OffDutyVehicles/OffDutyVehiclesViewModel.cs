using Acabus.DataAccess;
using Acabus.Models;
using Acabus.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Acabus.Modules.OffDutyVehicles
{
    public sealed class OffDutyVehiclesViewModel : NotifyPropertyChanged
    {

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
        /// 
        /// </summary>
        public ICommand AddVehicleCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public ICommand ClearVehicleCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public ICommand SaveVehicleCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public ICommand ReloadVehicleCommand { get; }


        /// <summary>
        /// 
        /// </summary>
        public OffDutyVehiclesViewModel()
        {
            AddVehicleCommand = new CommandBase(delegate
            {
                if (String.IsNullOrEmpty(EconomicNumber)) return;

                var economicNumbers = EconomicNumber.Split('\n');

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
        }

    }
}
