using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using InnSyTech.Standard.Mvvm;
using System;

namespace Opera.Acabus.Core.Models
{
    /// <summary>
    /// Define las áreas asignables de actividades.
    /// </summary>
    public enum AssignableArea
    {
        /// <summary>
        /// Área de mantenimiento (Valor predeterminado).
        /// </summary>
        MANTTO,

        /// <summary>
        /// Área de mantenimiento y supervisión.
        /// </summary>
        MANTTO_SUPERVISOR,

        /// <summary>
        /// Área de supervisión de mantenimiento.
        /// </summary>
        SUPERVISOR,

        /// <summary>
        /// Área de supervisión y soporte técnico.
        /// </summary>
        SUPERVISOR_SUPPORT,

        /// <summary>
        /// Área de soporte técnico.
        /// </summary>
        SUPPORT,

        /// <summary>
        /// Área de soporte técnico y base de datos.
        /// </summary>
        SUPPORT_DATABASE,

        /// <summary>
        /// Área de base de datos.
        /// </summary>
        DATABASE,

        /// <summary>
        /// Aréa de base de datos y gerencia TI.
        /// </summary>
        DATABASE_IT_MANAGER,

        /// <summary>
        /// Área de gerencia TI.
        /// </summary>
        IT_MANAGER
    }

    /// <summary>
    /// Define la estructura del personal del área de TI.
    /// </summary>
    [Entity]
    public class ITStaff : NotifyPropertyChanged, IComparable, IComparable<ITStaff>
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Area'.
        /// </summary>
        private AssignableArea _area;

        /// <summary>
        /// Campo que provee a la propiedad 'ID'.
        /// </summary>
        private UInt64 _id;

        /// <summary>
        /// Campo que provee a la propiedad 'Name'.
        /// </summary>
        private String _name;

        /// <summary>
        /// Obtiene o establece el area asignada.
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<AssignableArea>))]
        public AssignableArea Area {
            get => _area;
            set {
                _area = value;
                OnPropertyChanged(nameof(Area));
            }
        }

        /// <summary>
        /// Obtiene o establece el identificador unico de este miembro del personal.
        /// </summary>
        [Column(IsPrimaryKey = true, IsAutonumerical = true)]
        public UInt64 ID {
            get => _id;
            set {
                _id = value;
                OnPropertyChanged(nameof(ID));
            }
        }

        /// <summary>
        /// Obtiene o establece el nombre del miembro del personal.
        /// </summary>
        public String Name {
            get => _name;
            set {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Compara dos instancias de <see cref="ITStaff"/> y determina si son diferentes.
        /// </summary>
        /// <param name="tiStaff">Un miembro del personal a comparar.</param>
        /// <param name="anotherTIStaff">Otro miembro del personal a comparar.</param>
        /// <returns>Un valor <see cref="true"/> si los miembros son diferentes.</returns>
        public static bool operator !=(ITStaff tiStaff, ITStaff anotherTIStaff)
        {
            if (tiStaff is null && anotherTIStaff is null) return false;
            if (tiStaff is null || anotherTIStaff is null) return true;

            if (tiStaff.Area != anotherTIStaff.Area) return true;
            if (tiStaff.Name != anotherTIStaff.Name) return true;

            return false;
        }

        /// <summary>
        /// Compara dos instancias de <see cref="ITStaff"/> y determina si son iguales.
        /// </summary>
        /// <param name="tiStaff">Un miembro del personal a comparar.</param>
        /// <param name="anotherTIStaff">Otro miembro del personal a comparar.</param>
        /// <returns>Un valor <see cref="true"/> si ambos miembros son iguales.</returns>
        public static bool operator ==(ITStaff tiStaff, ITStaff anotherTIStaff)
        {
            if (tiStaff is null && anotherTIStaff is null) return true;
            if (tiStaff is null || anotherTIStaff is null) return false;

            if (tiStaff.Area != anotherTIStaff.Area) return false;
            if (tiStaff.Name != anotherTIStaff.Name) return false;

            return true;
        }

        /// <summary>
        /// Compara la instancia <see cref="ITStaff"/> actual con otra instancia <see cref="ITStaff"/> y
        /// devuelve un entero que indica si la posición de la instancia actual es anterior,
        /// posterior o igual que la del otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="other">Otra instancia <see cref="ITStaff"/>.</param>
        /// <returns>
        /// Un valor 0 si las instancias son iguales, 1 si la instancia es mayor que la otra y -1 si
        /// la instancia menor que la otra.
        /// </returns>
        public int CompareTo(ITStaff other)
        {
            if (other is null) return 1;
            if (other.Area == Area)
                return Name.CompareTo(other.Name);
            return Area.CompareTo(other.Area);
        }

        /// <summary>
        /// Compara la instancia <see cref="ITStaff"/> actual con otra instancia y
        /// devuelve un entero que indica si la posición de la instancia actual es anterior,
        /// posterior o igual que la del otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="other">Otra instancia.</param>
        /// <returns>
        /// Un valor 0 si las instancias son iguales, 1 si la instancia es mayor que la otra y -1 si
        /// la instancia menor que la otra.
        /// </returns>
        public int CompareTo(object other)
        {
            if (other is null) return 1;
            if (other.GetType() != GetType()) return 1;
            return CompareTo(other as ITStaff);
        }

        /// <summary>
        /// Determina si la instancia actual es igual a la pasada por argumento de la función.
        /// </summary>
        /// <param name="obj">Instancia a comparar con la actual.</param>
        /// <returns>Un valor <see cref="true"/> si la instancia es igual a la actual.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != GetType()) return false;

            var anotherTechnician = obj as ITStaff;

            if (anotherTechnician.Area != Area) return false;
            if (anotherTechnician.Name != Name) return false;

            return true;
        }

        /// <summary>
        /// Devuelve el código hash de la instancia actual.
        /// </summary>
        /// <returns>Un código hash que representa la instancia actual.</returns>
        public override int GetHashCode()
            => Tuple.Create(Name, Area).GetHashCode();

        /// <summary>
        /// Representa la instancia de <see cref="ITStaff"/> en una cadena.
        /// </summary>
        /// <returns>El nombre del elemento del personal.</returns>
        public override string ToString()
            => Name;
    }
}