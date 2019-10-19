using Acabus.DataAccess;
using Acabus.Models;
using Acabus.Utils.Mvvm;
using System;

namespace Acabus.Modules.CctvReports.Models
{
    public sealed class BusDisconnectedAlarm : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'EconomicNumber'.
        /// </summary>
        private String _economicNumber;

        /// <summary>
        /// Campo que provee a la propiedad 'LastSentLocation'.
        /// </summary>
        private DateTime _lastSentLocation;

        /// <summary>
        /// Campo que provee a la propiedad 'Priority'.
        /// </summary>
        private Priority _priority;

        /// <summary>
        /// Obtiene el tiempo máximo sin conexión para establecer prioridad baja.
        /// </summary>
        private TimeSpan LOW_PRIORITY_TIME = AcabusData.TimeMaxLowPriorityBus;

        /// <summary>
        /// Obtiene el tiempo máximo sin conexión para establecer prioridad media.
        /// </summary>
        private TimeSpan MEDIUM_PRIORITY_TIME = AcabusData.TimeMaxMediumPriorityBus;

        /// <summary>
        /// Crea una nueva instancia de alerta de vehículo sin conexión.
        /// </summary>
        public BusDisconnectedAlarm(String economicNumber)
        {
            _economicNumber = economicNumber;
        }

        /// <summary>
        /// Obtiene el número económico de la unidad.
        /// </summary>
        public String EconomicNumber => _economicNumber;

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
        /// Obtiene la prioridad de atención.
        /// </summary>
        public Priority Priority => _priority;

        /// <summary>
        /// Crea una nueva alarma de vehículo sin conexión indicando la fecha y hora de la última ubicación enviada.
        /// </summary>
        /// <param name="economicNumber">Número económico del vehículo.</param>
        /// <param name="lastSentLocation">Fecha y hora de la última ubicación enviada.</param>
        /// <returns>Una nueva alerta de vehículo sin conexión.</returns>
        public static BusDisconnectedAlarm CreateBusAlert(String economicNumber, DateTime lastSentLocation)
        {
            return new BusDisconnectedAlarm(economicNumber) { LastSentLocation = lastSentLocation };
        }

        /// <summary>
        /// Operador lógico de desigualdad, determina si dos instancias <see cref="BusDisconnectedAlarm"/> son diferentes.
        /// </summary>
        /// <param name="busAlarm">Una instancia.</param>
        /// <param name="otherBusAlarm">Otra instancia.</param>
        /// <returns>Un valor <code>true</code> si el EconomicNumber es diferente en ambas instancias <see cref="BusDisconnectedAlarm"/>.</returns>
        public static bool operator !=(BusDisconnectedAlarm busAlarm, BusDisconnectedAlarm otherBusAlarm)
        {
            if (otherBusAlarm.EconomicNumber != busAlarm.EconomicNumber)
                return true;

            return false;
        }

        /// <summary>
        /// Operador lógico de igualdad, determina si dos instancias <see cref="BusDisconnectedAlarm"/> son iguales.
        /// </summary>
        /// <param name="busAlarm">Una instancia.</param>
        /// <param name="otherBusAlarm">Otra instancia.</param>
        /// <returns>Un valor <code>true</code> si el EconomicNumber es igual en ambas instancias <see cref="BusDisconnectedAlarm"/>.</returns>
        public static bool operator ==(BusDisconnectedAlarm busAlarm, BusDisconnectedAlarm otherBusAlarm)
        {
            if (otherBusAlarm.EconomicNumber == busAlarm.EconomicNumber)
                return true;

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            return (this.LastSentLocation == (obj as BusDisconnectedAlarm).LastSentLocation
                && this.EconomicNumber == (obj as BusDisconnectedAlarm).EconomicNumber);
        }

        public override int GetHashCode() => base.GetHashCode();

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
    }
}