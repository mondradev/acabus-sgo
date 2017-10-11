using InnSyTech.Standard.Database.Linq;
using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Gui.Modules;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Opera.Acabus.Cctv.SubModules.OffDutyBus.ViewModel
{
    /// <summary>
    /// Defiene el modelo de la vista para el cuadro de diálogo utilizado para la adición de
    /// vehículos fuera de servicio.
    /// </summary>
    public sealed class OffDutyBusViewModel : ModuleViewerBase
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="AllBuses" />.
        /// </summary>
        private ICollection<Bus> _allBuses;

        /// <summary>
        /// Campo que provee a lapropiedad <see cref="EconomicNumber" />
        /// </summary>
        private String _economicNumber;

        /// <summary>
        /// Un listado de los autobuses que fueron removidos de la lista.
        /// </summary>
        private List<Bus> _removedBuses = new List<Bus>();

        /// <summary>
        /// Campo que provee a lapropiedad <see cref="SelectedBus" />
        /// </summary>
        private Bus _selectedBus;

        /// <summary>
        /// Campo que provee a lapropiedad <see cref="Status" />
        /// </summary>
        private BusStatus _selectedStatus;

        /// <summary>
        /// Crea una instancia del modelo de la vista de <see cref="OffDutyVehiclesView" />.
        /// </summary>
        public OffDutyBusViewModel()
        {
            AddBusCommand = new Command(delegate
            {
                if (string.IsNullOrEmpty(EconomicNumber)) return;

                var economicNumbers = EconomicNumber.Split(new String[] { "\n", "\r\n" },
                                                                StringSplitOptions.RemoveEmptyEntries);

                foreach (var economicNumber in economicNumbers)
                {
                    var matches = Regex.Match(economicNumber.ToUpper(), "A[ACP]{1}-[0-9]{3}").Groups;

                    if (matches.Count == 0) continue;

                    foreach (var match in matches)
                        if (!String.IsNullOrEmpty(match.ToString()))
                        {
                            if (!AllBuses.Any(b => b.EconomicNumber == match.ToString()))
                            {
                                Bus bus = AcabusDataContext.AllBuses.LoadReference(1)
                                                            .FirstOrDefault(b => b.EconomicNumber == match.ToString());
                                if (bus is null) continue;

                                bus.Status = SelectedStatus;

                                _removedBuses.Remove(bus);
                                AllBuses.Add(bus);
                            }
                        }
                }

                EconomicNumber = String.Empty;
            });

            ClearListCommand = new Command(p =>
            {
                _removedBuses.AddRange(AllBuses);
                AllBuses?.Clear();
            });

            SaveListCommand = new Command(p =>
            {
                foreach (Bus bus in AllBuses)
                    AcabusDataContext.DbContext.Update(bus);

                foreach (Bus removedBus in _removedBuses)
                {
                    removedBus.Status = Core.Models.BusStatus.OPERATIONAL;
                    AcabusDataContext.DbContext.Update(removedBus);
                }

                _removedBuses.Clear();

                Dispatcher.CloseDialog();
            });

            DiscardChangesCommand = new Command(p => Dispatcher.CloseDialog());

            RefreshListCommand = new Command(p =>
            {
                _removedBuses.Clear();
                LoadData();
            });

            RemoveBusCommand = new Command(p =>
            {
                AllBuses?.Remove(SelectedBus);
                _removedBuses.Add(SelectedBus);
                SelectedBus = null;
            });

            LoadData();
        }

        /// <summary>
        /// Comando para agregar vehículos al listado.
        /// </summary>
        public ICommand AddBusCommand { get; }

        /// <summary>
        /// Obtiene una lista de el valor de esta propiedad.
        /// </summary>
        public ICollection<Bus> AllBuses
            => _allBuses ?? (_allBuses = new ObservableCollection<Bus>());

        /// <summary>
        /// Obtiene una lista de los estados disponibles para los autobuses.
        /// </summary>
        public IEnumerable<BusStatus> BusStatus
            => Enum.GetValues(typeof(BusStatus)).Cast<BusStatus>().Where(bs => bs != Core.Models.BusStatus.OPERATIONAL);

        /// <summary>
        /// Comando para limpiar la lista de vehículos.
        /// </summary>
        public ICommand ClearListCommand { get; }

        /// <summary>
        /// Comando para salir de la ventana sin hacer ningún cambio.
        /// </summary>
        public ICommand DiscardChangesCommand { get; }

        /// <summary>
        /// Obtiene o establece el número económico del equipo a agregar.
        /// </summary>
        public String EconomicNumber {
            get => _economicNumber;
            set {
                _economicNumber = value;
                OnPropertyChanged(nameof(EconomicNumber));
            }
        }

        /// <summary>
        /// Comando para recargar la información guardada en la lista de vehículos.
        /// </summary>
        public ICommand RefreshListCommand { get; }

        /// <summary>
        /// Comando para remover un vehículo de la lista de vehículos.
        /// </summary>
        public ICommand RemoveBusCommand { get; }

        /// <summary>
        /// Comando para guardar la información de los vehículos.
        /// </summary>
        public ICommand SaveListCommand { get; }

        /// <summary>
        /// Obtiene o establece el vehículo actualmente seleccionado en la tabla.
        /// </summary>
        public Bus SelectedBus {
            get => _selectedBus;
            set {
                _selectedBus = value;
                OnPropertyChanged(nameof(SelectedBus));
            }
        }

        /// <summary>
        /// Obtiene o establece el estado seleccionado para el vehículo.
        /// </summary>
        public BusStatus SelectedStatus {
            get => _selectedStatus;
            set {
                _selectedStatus = value;
                OnPropertyChanged(nameof(SelectedStatus));
            }
        }

        /// <summary>
        /// Recarga la lista de los autobuses fuera de operación.
        /// </summary>
        private void LoadData()
        {
            _allBuses = new ObservableCollection<Bus>(AcabusDataContext.AllBuses
                .LoadReference(1).Where(b => b.Status != Core.Models.BusStatus.OPERATIONAL));
            OnPropertyChanged(nameof(AllBuses));
        }
    }
}