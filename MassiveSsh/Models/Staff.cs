using Acabus.Utils.Mvvm;
using System;

namespace Acabus.Models
{
    public class Staff : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Name'.
        /// </summary>
        private String _name;

        /// <summary>
        /// Obtiene o establece el nombre de la persona.
        /// </summary>
        public String Name {
            get => _name;
            set {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Representa la instancia de Personal en una cadena.
        /// </summary>
        /// <returns>El nombre del personal actual.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
