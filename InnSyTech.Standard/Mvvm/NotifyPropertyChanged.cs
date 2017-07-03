using System;
using System.ComponentModel;

namespace InnSyTech.Standard.Mvvm
{
    /// <summary>
    /// Implementación de la interfaz <see cref="INotifyPropertyChanged"/>.
    /// </summary>
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        /// <summary>
        /// Evento que surge cuando una propiedad cambia.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Función que es llamada en el momento del cambio de la propiedad.
        /// </summary>
        /// <param name="name">Nombre de la propiedad que cambia.</param>
        protected virtual void OnPropertyChanged(String name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
