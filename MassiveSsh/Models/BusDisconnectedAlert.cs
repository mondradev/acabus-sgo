using Acabus.Utils.MVVM;
using System;

namespace Acabus.Models
{
    public sealed class BusDisconnectedAlert : NotifyPropertyChanged
    {
        /// <summary>
        /// Obtiene el tiempo máximo sin conexión para establecer prioridad baja.
        /// </summary>
        private TimeSpan LOW_PRIORITY_TIME = TimeSpan.FromMinutes(60);

        /// <summary>
        /// Obtiene el tiempo máximo sin conexión para establecer prioridad media.
        /// </summary>
        private TimeSpan MEDIUM_PRIORITY_TIME = TimeSpan.FromMinutes(300);

        /// <summary>
        /// Campo que provee a la propiedad 'EconomicNumber'.
        /// </summary>
        private String _economicNumber;

        /// <summary>
        /// Obtiene el número económico de la unidad.
        /// </summary>
        public String EconomicNumber => _economicNumber;

        /// <summary>
        /// Campo que provee a la propiedad 'LastSentLocation'.
        /// </summary>
        private DateTime _lastSentLocation;

        /// <summary>
        /// Obtiene o establece fecha de la última ubicación enviada.
        /// </summary>
        public DateTime LastSentLocation {
            get => _lastSentLocation;
            set {
                _lastSentLocation = value;
                CalculatePriority(_lastSentLocation);
                OnPropertyChanged("LastSentLocation");
            }
        }

        /// <summary>
        /// Determina la prioridad en base al tiempo que lleva sin enviar su ubicación.
        /// </summary>
        /// <param name="lastSentLocation">Fecha y hora de la última ubicación enviada.</param>
        private void CalculatePriority(DateTime lastSentLocation)
        {
            if ((DateTime.Now - lastSentLocation) > MEDIUM_PRIORITY_TIME)
                _priority = Priority.HIGH;
            else if ((DateTime.Now - lastSentLocation) > LOW_PRIORITY_TIME)
                _priority = Priority.MEDIUM;
            else
                _priority = Priority.LOW;
            OnPropertyChanged("Priority");
        }

        /// <summary>
        /// Campo que provee a la propiedad 'Priority'.
        /// </summary>
        private Priority _priority;

        /// <summary>
        /// Obtiene la prioridad de atención.
        /// </summary>
        public Priority Priority => _priority;

        /// <summary>
        /// Crea una nueva instancia de alerta de vehículo sin conexión.
        /// </summary>
        public BusDisconnectedAlert(String economicNumber)
        {
            _economicNumber = economicNumber;
        }

        /// <summary>
        /// Crea una nueva alarma de vehículo sin conexión indicando la fecha y hora de la última ubicación enviada.
        /// </summary>
        /// <param name="economicNumber">Número económico del vehículo.</param>
        /// <param name="lastSentLocation">Fecha y hora de la última ubicación enviada.</param>
        /// <returns>Una nueva alerta de vehículo sin conexión.</returns>
        public static BusDisconnectedAlert CreateBusAlert(String economicNumber, DateTime lastSentLocation)
        {
            return new BusDisconnectedAlert(economicNumber) { LastSentLocation = lastSentLocation };
        }
    }
}
