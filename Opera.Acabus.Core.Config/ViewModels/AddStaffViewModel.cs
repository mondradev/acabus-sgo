using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Gui;
using Opera.Acabus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Opera.Acabus.Core.Config.ViewModels
{
    /// <summary>
    /// Define la estructura del modelo de la vista <see cref="Views.AddStaffView"/>.
    /// </summary>
    public sealed class AddStaffViewModel : ViewModelBase
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="FullName"/>.
        /// </summary>
        private String _fullName;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SelectedArea"/>.
        /// </summary>
        private AssignableArea? _selectedArea;

        /// <summary>
        /// Crea una nueva instancia del modelo de la vista.
        /// </summary>
        public AddStaffViewModel()
            => AddStaffCommand = new Command(AddStaffExecute, AddStaffCanExecute);

        /// <summary>
        /// Obtiene el comando que agrega el personal al sistema.
        /// </summary>
        public ICommand AddStaffCommand { get; }

        /// <summary>
        /// Obtiene una lista de areas válidas.
        /// </summary>
        public IEnumerable<AssignableArea> Areas
            => Enum.GetValues(typeof(AssignableArea)).Cast<AssignableArea>().Where(x => x != AssignableArea.EVERYBODY);

        /// <summary>
        /// Obtiene o establece un nombre para el personal.
        /// </summary>
        public String FullName {
            get => _fullName;
            set {
                _fullName = value;
                OnPropertyChanged(nameof(FullName));
            }
        }

        /// <summary>
        /// Obtiene o establece el puesto.
        /// </summary>
        public AssignableArea? SelectedArea {
            get => _selectedArea;
            set {
                _selectedArea = value;
                OnPropertyChanged(nameof(SelectedArea));
            }
        }

        /// <summary>
        /// Función que es llamada durante la validación del valor de las propiedades.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad a validar.</param>
        protected override void OnValidation(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(FullName):
                    if (String.IsNullOrEmpty(FullName))
                        AddError(nameof(FullName), "Especifique un nombre válido.");
                    else if (AcabusDataContext.AllStaff.Where(s => s.Name == FullName).Count() > 0)
                        AddError(nameof(FullName), "Existe una persona con el mismo nombre");
                    break;

                case nameof(SelectedArea):
                    if (SelectedArea == null)
                        AddError(nameof(SelectedArea), "Especifique un puesto válido.");
                    break;
            }
        }

        /// <summary>
        /// Determina si es posible ejecutar el comando <see cref="AddStaffCommand" />.
        /// </summary>
        /// <param name="arg">Parametro del comando.</param>
        /// <returns>Un valor <see cref="true"/> si es posible ejecutar el comando.</returns>
        private bool AddStaffCanExecute(object arg)
            => Validate();

        /// <summary>
        /// Crea una instancia <see cref="Staff"/> a partir de la información de la instancia actual
        /// <see cref="AddStaffViewModel"/> y la guarda en la base de datos. Acción que realiza el
        /// comando <see cref="AddStaffCommand"/>.
        /// </summary>
        /// <param name="obj">Parametro del comando.</param>
        private void AddStaffExecute(object parameter)
        {
            try
            {
                object staff = new Staff()
                {
                    Area = SelectedArea.Value,
                    Name = FullName
                };

                if (ServerContext.GetLocalSync("Staff").Create(ref staff))
                    Dispatcher.SendMessageToGUI($"Empleado {staff} agregado correctamente.");
            }
            catch (Exception reason)
            {
                Dispatcher.SendMessageToGUI("Fallo al guardar el empleado, razón: " + reason.Message);
            }

            Dispatcher.CloseDialog();
        }

        /// <summary>
        /// Determina si los campos del formulario está correctamente llenos.
        /// </summary>
        private bool Validate()
        {
            ValidateProperty(nameof(FullName));
            ValidateProperty(nameof(SelectedArea));

            return !HasErrors;
        }
    }
}