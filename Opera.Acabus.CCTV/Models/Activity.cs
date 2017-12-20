using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using InnSyTech.Standard.Mvvm;
using Opera.Acabus.Core.Models;
using System;

namespace Opera.Acabus.Cctv.Models
{
    /// <summary>
    /// Representa una actividad aplicada a un equipo del BRT.
    /// </summary>
    [Entity(TableName = "Activities")]
    public sealed class Activity : NotifyPropertyChanged, IComparable<Activity>, IComparable
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="Assignable"/>.
        /// </summary>
        private AssignableArea _assignable;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Category"/>.
        /// </summary>
        private ActivityCategory _category;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Descripcion"/>.
        /// </summary>
        private String _description;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ID"/>.
        /// </summary>
        private UInt64 _id;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Priority" />.
        /// </summary>
        private Priority _priority;

        /// <summary>
        /// Crea una instancia básica de <see cref="Activity"/>
        /// </summary>
        public Activity() : this(0, String.Empty) { }

        /// <summary>
        /// Crea una instancia de <see cref="Activity"/> definiendo el ID, descripción y el tipo
        /// de dispositivo.
        /// </summary>
        /// <param name="id">ID de la falla.</param>
        /// <param name="fault">Descripción de la falla.</param>
        /// <param name="type">Tipo de dispositivo al que afecta.</param>
        public Activity(UInt64 id, String fault)
        {
            _id = id;
            _description = fault;
        }

        /// <summary>
        /// Obtiene o establece el area a la que se puede asignar la falla.
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<AssignableArea>))]
        public AssignableArea Assignable {
            get => _assignable;
            set {
                _assignable = value;
                OnPropertyChanged(nameof(Assignable));
            }
        }

        /// <summary>
        /// Obtiene o establece la categoría de fallas.
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_ActivityCategories_ID")]
        public ActivityCategory Category {
            get => _category;
            set {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        /// <summary>
        /// Obtiene o establece la descripción de la falla.
        /// </summary>
        public String Description {
            get => _description;
            set {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        /// <summary>
        /// Obtiene o establece el identificador unico de la falla.
        /// </summary>
        [Column(IsPrimaryKey = true, IsAutonumerical = true)]
        public UInt64 ID {
            get => _id;
            set {
                _id = value;
                OnPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Obtiene o establece la prioridad de atención.
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<Priority>))]
        public Priority Priority {
            get => _priority;
            set {
                _priority = value;
                OnPropertyChanged(nameof(Priority));
            }
        }

        /// <summary>
        /// Compara dos instancias de <see cref="Activity"/> y determina si son diferentes.
        /// </summary>
        /// <param name="left">Una falla a comparar.</param>
        /// <param name="right">Otra falla a comparar.</param>
        /// <returns>Un valor true si las fallas son diferentes.</returns>
        public static bool operator !=(Activity left, Activity right)
        {
            if (left is null && right is null) return false;
            if (left is null || right is null) return true;

            return left.CompareTo(right) != 0;
        }

        /// <summary>
        /// Compara dos instancias de <see cref="Activity"/> y determina si son iguales.
        /// </summary>
        /// <param name="left">Una falla a comparar.</param>
        /// <param name="right">Otra falla a comparar.</param>
        /// <returns>Un valor true si las fallas son iguales.</returns>
        public static bool operator ==(Activity left, Activity right)
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

            if (obj.GetType() != typeof(Activity)) return -1;

            return CompareTo(obj as Activity);
        }

        /// <summary>
        /// Compara la instancia actual con otra y devuelve un entero que indica si la posición de la
        /// instancia actual es anterior, posterior o igual que la del otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="other">Objeto que se va a comparar con esta instancia.</param>
        /// <returns>Un valor que indica el orden relativo de los objetos que se están comparando.</returns>
        public int CompareTo(Activity other)
        {
            if (other == null) return -1;

            if (Category == other.Category)
                if (Description == other.Description)
                    return Priority.CompareTo(other.Priority);
                else
                    return Description.CompareTo(other.Description);

            return Category.CompareTo(other.Category);
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

            Activity anotherInstance = (Activity)obj;

            return CompareTo(anotherInstance) == 0;
        }

        /// <summary>
        /// Obtiene el código hash del objeto actual.
        /// </summary>
        /// <returns>Código hash de la instancia.</returns>
        public override int GetHashCode()
            => Tuple.Create(Description, Category, Priority).GetHashCode();

        /// <summary>
        /// Representa en una cadena la falla actual.
        /// </summary>
        /// <returns>Una cadena que representa la instancia.</returns>
        public override string ToString()
            => Description;
    }
}