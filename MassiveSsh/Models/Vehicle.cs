using Acabus.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acabus.Models
{
    /// <summary>
    /// Provee de una estructura para el manejo de los vehiculos.
    /// </summary>
    public sealed class Vehicle : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'EconomicNumber'.
        /// </summary>
        private String _economicNumber;

        /// <summary>
        /// Obtiene el número económico de la unidad.
        /// </summary>
        public String EconomicNumber => _economicNumber;

        /// <summary>
        /// Campo que provee a la propiedad 'Status'.
        /// </summary>
        private VehicleStatus _status;

        /// <summary>
        /// Obtiene o establece el estado de la unidad.
        /// </summary>
        public VehicleStatus Status {
            get => _status;
            set {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        /// <summary>
        /// Crea una instancia nueva de 'Vehicle' indicando el número económico.
        /// </summary>
        /// <param name="economicNumber">Número Económico de la unidad.</param>
        /// <param name="status">Estado del funcionamiento de la unidad.</param>
        public Vehicle(String economicNumber, VehicleStatus status = VehicleStatus.OPERATING)
        {
            this._economicNumber = economicNumber;
            this._status = status;
        }
    }
}
