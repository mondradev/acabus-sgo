using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace InnSyTech.Standard.Mvvm
{
    /// <summary>
    /// Define la base de una modelo de la vista implementando las interfaces <see
    /// cref="INotifyPropertyChanged"/> y <see cref="INotifyDataErrorInfo"/>.
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        /// <summary>
        /// Collección de los errores actualmente surgieron.
        /// </summary>
        private Dictionary<String, ICollection<String>> _errorsCollection
            = new Dictionary<string, ICollection<string>>();

        /// <summary>
        /// Crea una instancia de <see cref="ViewModelBase"/>.
        /// </summary>
        public ViewModelBase()
        {
            LoadCommand = new Command(OnLoad);
            UnloadCommand = new Command(OnUnload);
        }

        /// <summary>
        /// Evento que surge cuando una propiedad presenta cambio en su errores.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <summary>
        /// Evento que surge cuando una propieddad cambia su valor.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Indica si el modelo de la vista presenta un error.
        /// </summary>
        public bool HasErrors
            => !(_errorsCollection.Values.FirstOrDefault(error => error.Count > 0) is null);

        /// <summary>
        /// Comando de carga de la vista.
        /// </summary>
        public ICommand LoadCommand { get; }

        /// <summary>
        /// Comando de descarga de la vista.
        /// </summary>
        public ICommand UnloadCommand { get; }

        /// <summary>
        /// Obtiene los error de la propiedad especificada.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad.</param>
        /// <returns>Un <see cref="IEnumerable"/> que contiene todos los errores de la propiedad especificada.</returns>
        public IEnumerable GetErrors(string propertyName)
        {
            if (!_errorsCollection.ContainsKey(propertyName)
                || _errorsCollection[propertyName].Count < 1)
                return null;

            return _errorsCollection[propertyName];
        }

        /// <summary>
        /// Permite añadir un error a una propiedad.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad.</param>
        /// <param name="error">Descripción del error.</param>
        protected void AddError(String propertyName, String error)
        {
            if (!_errorsCollection.ContainsKey(propertyName))
                _errorsCollection.Add(propertyName, new List<String>());

            _errorsCollection[propertyName].Add(error);
            OnErrorChanged(propertyName);
        }

        /// <summary>
        /// Limpia el listado de todos los errores de la propiedad especificada.
        /// </summary>
        protected void ClearErrors(String propertyName)
        {
            if (!String.IsNullOrEmpty(propertyName)
                && _errorsCollection.ContainsKey(propertyName))
            {
                _errorsCollection[propertyName].Clear();
                OnErrorChanged(propertyName);
            }
        }

        /// <summary>
        /// Limpia el listado de todos los errores de cada una de las propiedades del modelo de la vista.
        /// </summary>
        protected void ClearErrors()
        {
            foreach (var propertyName in _errorsCollection.Keys)
                ClearErrors(propertyName);
        }

        /// <summary>
        /// Función que es ejecutada durante el cambio del error de la propiedad especificada.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad.</param>
        protected void OnErrorChanged(String propertyName) =>
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

        /// <summary>
        /// Función llamada por el comando <see cref="LoadCommand"/>.
        /// </summary>
        /// <param name="parameter">Parametro del comando.</param>
        protected virtual void OnLoad(object parameter)
        {
        }

        /// <summary>
        /// Función que es ejecutada durante el cambio del valor de la propiedad especificada. Aquí
        /// se valida el valor de la propiedad llamando la función <see cref="ValidateProperty(string)"/>.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            ValidateProperty(propertyName);
        }

        /// <summary>
        /// Función llamada por el comando <see cref="UnloadCommand"/>.
        /// </summary>
        /// <param name="parameter">Parametro del comando.</param>
        protected virtual void OnUnload(object parameter)
        {
        }

        /// <summary>
        /// Función ejecutada durante la validación de la propiedad especificada.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad.</param>
        protected virtual void OnValidation(string propertyName) { }

        /// <summary>
        /// Valida la propiedad especificada.
        /// </summary>
        /// <param name="propertyName"></param>
        protected void ValidateProperty(string propertyName)
        {
            ClearErrors(propertyName);
            OnValidation(propertyName);
            OnErrorChanged(propertyName);
        }
    }
}