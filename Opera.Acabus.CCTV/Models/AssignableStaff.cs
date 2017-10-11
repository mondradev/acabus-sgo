using InnSyTech.Standard.Database;
using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.Models;
using System;

namespace Opera.Acabus.Cctv.Models
{
    /// <summary>
    /// Representa un elemento del personal asignable a una actividad o incidencia de los equipos en
    /// el BRT.
    /// </summary>
    [Entity]
    public sealed class AssignableStaff : NotifyPropertyChanged, IComparable, IComparable<AssignableStaff>, IAssignableSection
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="AssignedSection"/>.
        /// </summary>
        private string _assignedSection;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="HasKvrKey"/>.
        /// </summary>
        private bool _hasKvrKey;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="HasNemaKey"/>.
        /// </summary>
        private bool _hasNemaKey;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ID"/>.
        /// </summary>
        private UInt64 _id;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Staff"/>.
        /// </summary>
        private Staff _staff;

        /// <summary>
        /// Obtiene o establece la sección asignada de atención.
        /// </summary>
        public string AssignedSection {
            get => _assignedSection;
            set {
                _assignedSection = value;
                OnPropertyChanged(nameof(AssignedSection));
            }
        }

        /// <summary>
        /// Obtiene o establece si la asignación tiene una llave de KVR.
        /// </summary>
        public bool HasKvrKey {
            get => _hasKvrKey;
            set {
                _hasKvrKey = value;
                OnPropertyChanged(nameof(HasKvrKey));
            }
        }

        /// <summary>
        /// Obtiene o establece si la asignación tiene una llave de caja nema.
        /// </summary>
        public bool HasNemaKey {
            get => _hasNemaKey;
            set {
                _hasNemaKey = value;
                OnPropertyChanged(nameof(HasNemaKey));
            }
        }

        /// <summary>
        /// Obtiene o establece el identificador único del personal asignable.
        /// </summary>
        [Column(IsPrimaryKey = true)]
        public UInt64 ID {
            get => _id;
            set {
                _id = value;
                OnPropertyChanged(nameof(ID));
            }
        }

        /// <summary>
        /// Obtiene o establece el elemento del personal de la asignación.
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_Staff_ID")]
        public Staff Staff {
            get => _staff;
            set {
                _staff = value;
                OnPropertyChanged(nameof(Staff));
            }
        }

        /// <summary>
        /// Compara dos instancias de <see cref="AssignableStaff"/> y determina si son diferentes.
        /// </summary>
        /// <param name="left">Un personal asignable a comparar.</param>
        /// <param name="right">Otro personal asignable a comparar.</param>
        /// <returns>Un valor true si son diferentes.</returns>
        public static bool operator !=(AssignableStaff left, AssignableStaff right)
        {
            if (left is null && right is null) return false;
            if (left is null || right is null) return true;

            return left.CompareTo(right) != 0;
        }

        /// <summary>
        /// Compara dos instancias de <see cref="AssignableStaff"/> y determina si son iguales.
        /// </summary>
        /// <param name="left">Un personal asignable a comparar.</param>
        /// <param name="right">Otro personal asignable a comparar.</param>
        /// <returns>Un valor true si son iguales.</returns>
        public static bool operator ==(AssignableStaff left, AssignableStaff right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Compara la instancia actual con otro objeto del mismo tipo y devuelve un entero que
        /// indica si la posición de la instancia actual es anterior, posterior o igual que la del
        /// otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="obj">Objeto que se va a comparar con esta instancia.</param>
        /// <returns>Un valor que indica el orden relativo de los objetos que se están comparando.</returns>
        public int CompareTo(object obj)
        {
            if (obj == null) return -1;

            if (obj.GetType() != typeof(AssignableStaff)) return -1;

            return CompareTo(obj as AssignableStaff);
        }

        /// <summary>
        /// Compara la instancia actual con otra y devuelve un entero que indica si la posición de la
        /// instancia actual es anterior, posterior o igual que la del otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="other">Objeto que se va a comparar con esta instancia.</param>
        /// <returns>Un valor que indica el orden relativo de los objetos que se están comparando.</returns>
        public int CompareTo(AssignableStaff other)
        {
            if (other == null) return -1;

            if (Staff == other.Staff)
                if (AssignedSection == other.AssignedSection)
                    if (HasKvrKey == other.HasKvrKey)
                        return HasNemaKey.CompareTo(other.HasNemaKey);
                    else
                        return HasKvrKey.CompareTo(other.HasKvrKey);
                else
                    return AssignedSection.CompareTo(other.AssignedSection);

            return Staff.CompareTo(other.Staff);
        }

        /// <summary>
        /// Determina si la instancia actual es igual al objeto especificado.
        /// </summary>
        /// <param name="obj">Otro objeto a comparar.</param>
        /// <returns>Un valor true si los objetos son iguales.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            AssignableStaff anotherInstance = (AssignableStaff)obj;

            return CompareTo(anotherInstance) == 0;
        }

        /// <summary>
        /// Obtiene el código hash del objeto actual.
        /// </summary>
        /// <returns>Código hash de la instancia.</returns>
        public override int GetHashCode()
            => Tuple.Create(Staff, AssignedSection, HasKvrKey, HasNemaKey).GetHashCode();

        /// <summary>
        /// Representa en una cadena la falla actual.
        /// </summary>
        /// <returns>Una cadena que representa la instancia.</returns>
        public override string ToString()
            => Staff.ToString();
    }
}