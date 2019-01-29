using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using Opera.Acabus.Core.Models;
using Opera.Acabus.Core.Models.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Opera.Acabus.Cctv.Models
{
    /// <summary>
    /// Representa la categoría de las actividades se presentan en los equipos del BRT.
    /// </summary>
    [Entity(TableName = "ActivityCategories")]
    public sealed class ActivityCategory : AcabusEntityBase, IComparable, IComparable<ActivityCategory>
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="Activities" />.
        /// </summary>
        private ICollection<Activity> _activities;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="DeviceType"/>.
        /// </summary>
        private DeviceType _deviceType;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ID"/>.
        /// </summary>
        private UInt64 _id;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Name"/>.
        /// </summary>
        private String _name;

        /// <summary>
        /// Crea una nueva instancia.
        /// </summary>
        public ActivityCategory() : this(0, DeviceType.NONE) { }

        /// <summary>
        /// Crea una instancia especificando su ID.
        /// </summary>
        /// <param name="id">Identificador de la instancia.</param>
        /// <param name="assignableType">Tipo de dispositivo asignable.</param>
        public ActivityCategory(UInt64 id, DeviceType assignableType)
        {
            _id = id;
            _deviceType = assignableType;
        }

        /// <summary>
        /// Obtiene una lista de las fallas correspondientes a esta categoría.
        /// </summary>
        [DbColumn(ForeignKeyName = "Fk_ActivityCategories_ID")]
        public ICollection<Activity> Activities
            => _activities ?? (_activities = new ObservableCollection<Activity>());

        /// <summary>
        /// Obtiene o establece el tipo de dispositivo a la que corresponde esta categoría de fallas.
        /// </summary>
        [DbColumn(Converter = typeof(DbEnumConverter<DeviceType>))]
        public DeviceType DeviceType {
            get => _deviceType;
            private set {
                _deviceType = value;
                OnPropertyChanged(nameof(DeviceType));
            }
        }

        /// <summary>
        /// Obtiene el identificador de la categoría de fallas.
        /// </summary>
        [DbColumn(IsPrimaryKey = true, IsAutonumerical = true)]
        public override UInt64 ID {
            get => _id;
            protected set {
                _id = value;
                OnPropertyChanged(nameof(ID));
            }
        }

        /// <summary>
        /// Obtiene o establece el nombre de la categoría.
        /// </summary>
        public String Name {
            get => _name;
            set {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Compara dos instancias de <see cref="ActivityCategory"/> y determina si son diferentes.
        /// </summary>
        /// <param name="left">Una categoría de falla a comparar.</param>
        /// <param name="right">Otra categoría de falla a comparar.</param>
        /// <returns>Un valor true si las categoría de fallas son diferentes.</returns>
        public static bool operator !=(ActivityCategory left, ActivityCategory right)
        {
            if (left is null && right is null) return false;
            if (left is null || right is null) return true;

            return left.CompareTo(right) != 0;
        }

        /// <summary>
        /// Compara dos instancias de <see cref="ActivityCategory"/> y determina si son iguales.
        /// </summary>
        /// <param name="left">Una categoría de falla a comparar.</param>
        /// <param name="right">Otra categoría de falla a comparar.</param>
        /// <returns>Un valor true si las categoría de fallas son iguales.</returns>
        public static bool operator ==(ActivityCategory left, ActivityCategory right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Compara la instancia actual con otra y devuelve un entero que indica si la posición de la
        /// instancia actual es anterior, posterior o igual que la del otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="obj">Objeto que se va a comparar con esta instancia.</param>
        /// <returns>Un valor que indica el orden relativo de los objetos que se están comparando.</returns>
        public int CompareTo(ActivityCategory other)
        {
            if (other == null) return -1;

            if (Name == other.Name)
                return DeviceType.CompareTo(other.DeviceType);

            return Name.CompareTo(other.Name);
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

            if (obj.GetType() != typeof(ActivityCategory)) return -1;

            return CompareTo(obj as ActivityCategory);
        }

        /// <summary>
        /// Determina si la instancia actual es igual al objeto especificado.
        /// </summary>
        /// <param name="obj">Otro objeto a comparar.</param>
        /// <returns>Un valor true si los objetos son iguales.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj.GetType() != typeof(ActivityCategory)) return false;

            return CompareTo(obj as ActivityCategory) == 0;
        }

        /// <summary>
        /// Obtiene el código hash del objeto actual.
        /// </summary>
        /// <returns>Código hash de la instancia.</returns>
        public override int GetHashCode()
            => Tuple.Create(Name, DeviceType).GetHashCode();

        /// <summary>
        /// Representa en una cadena la categoría de fallas actual.
        /// </summary>
        /// <returns>Una cadena que representa la instancia.</returns>
        public override string ToString()
            => Name;
    }
}