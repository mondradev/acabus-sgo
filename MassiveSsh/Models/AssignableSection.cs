using Acabus.Utils.Mvvm;
using System;

namespace Acabus.Models
{
    public class AssignableSection : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Name'.
        /// </summary>
        private String _name;

        /// <summary>
        /// Obtiene o establece el nombre de la ubicación.
        /// </summary>
        public String Name {
            get => _name;
            set {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Representa la instancia en una cadena.
        /// </summary>
        /// <returns>El nombre de la ubicación actual.</returns>
        public override string ToString() => Name;


        /// <summary>
        /// Campo que provee a la propiedad 'Section'.
        /// </summary>
        private String _section;

        /// <summary>
        /// Obtiene o establece la sección correspondiente.
        /// </summary>
        public String Section {
            get => _section?.ToUpper();
            set {
                _section = value;
                OnPropertyChanged("Section");
            }
        }
    }
}
