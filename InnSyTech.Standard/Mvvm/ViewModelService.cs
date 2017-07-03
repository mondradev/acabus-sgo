using System;
using System.Collections.Generic;

namespace InnSyTech.Standard.Mvvm
{
    /// <summary>
    /// Mantiene conectado los diferentes modelos de la vista, permitiendo realizar llamadas a
    /// funciones o propiedades de las instancias actualmente en ejecución.
    /// </summary>
    public static class ViewModelService
    {
        /// <summary>
        /// Lista que lleva el control de las instancias de los diferentes modelos de las vistas.
        /// </summary>
        private static List<ViewModelBase> _viewModelRegisted = new List<ViewModelBase>();

        /// <summary>
        /// Obtiene el modelo de la vista especificada.
        /// </summary>
        /// <param name="nameClass">Nombre completo de la clase del modelo de la vista.</param>
        /// <returns>Una instancia <see cref="ViewModelBase"/> que corresponde al nombre especificado.</returns>
        public static dynamic GetViewModel(String nameClass)
        {
            dynamic instance = null;
            _viewModelRegisted.ForEach(viewModel =>
            {
                if (viewModel.GetType().FullName.Equals(nameClass) && instance is null)
                    instance = viewModel;
            });
            return instance;
        }

        /// <summary>
        /// Registra un modelo de vista nuevo.
        /// </summary>
        /// <param name="instance">Instancia del modelo de la vista.</param>
        public static void Register(ViewModelBase instance)
        {
            if (!IsRegistered(instance))
                _viewModelRegisted.Add(instance);
        }

        /// <summary>
        /// Elimina el registro de una instancia de modelo de la vista.
        /// </summary>
        /// <param name="instance">Instancia a eliminar del registro.</param>
        public static void UnRegister(ViewModelBase instance)
        {
            _viewModelRegisted.Remove(instance);
        }

        /// <summary>
        /// Indica si el modelo de la vista está actualmente registrado.
        /// </summary>
        /// <param name="instance">Instancia a buscar.</param>
        /// <returns>Un valor <see cref="true"/> si el modelo de la vista está previamente registrado.</returns>
        private static bool IsRegistered(ViewModelBase instance)
        {
            bool exists = false;
            _viewModelRegisted.ForEach(viewModel =>
            {
                if (!exists)
                    exists = viewModel.GetType() == instance.GetType();
            });
            return exists;
        }
    }
}