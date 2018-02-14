using Acabus.Utils.Mvvm;
using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using System;

namespace Acabus.Models
{
    [Entity(TableName = "Faults")]
    public sealed class DeviceFault : NotifyPropertyChanged
    {
        /// Campo que provee a la propiedad 'Assignable'.
        /// </summary>
        private AreaAssignable _assignable;

        /// <summary>
        /// Campo que provee a la propiedad 'Category'.
        /// </summary>
        private DeviceFaultCategory _category;

        /// <summary>
        /// Campo que provee a la propiedad 'Descripcion'.
        /// </summary>
        private String _description;

        /// <summary>
        /// Campo que provee a la propiedad 'ID'.
        /// </summary>
        private UInt64 _id;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="IsMulti" />.
        /// </summary>
        private bool _isMulti;

        /// <summary>
        /// Crea una instancia básica de <see cref="DeviceFault"/>
        /// </summary>
        public DeviceFault()
        {
        }

        /// <summary>
        /// Crea una instancia de <see cref="DeviceFault"/> definiendo el ID, descripción y el tipo de dispositivo.
        /// </summary>
        /// <param name="id">ID de la falla.</param>
        /// <param name="description">Descripción de la falla.</param>
        /// <param name="type">Tipo de dispositivo al que afecta.</param>
        public DeviceFault(UInt64 id, String description)
        {
            _id = id;
            _description = description;
        }

        /// <summary>
        /// Obtiene o establece el area a la que se puede asignar la falla.
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<AreaAssignable>))]
        public AreaAssignable Assignable {
            get => _assignable;
            set {
                _assignable = value;
                OnPropertyChanged("Assignable");
            }
        }

        /// <summary>
        /// Obtiene o establece la categoría de fallas.
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_FaultCategories_ID")]
        public DeviceFaultCategory Category {
            get => _category;
            set {
                _category = value;
                OnPropertyChanged("Category");
            }
        }

        /// <summary>
        /// Obtiene o establece la descripción de la falla.
        /// </summary>
        public String Description {
            get => _description;
            set {
                _description = value;
                OnPropertyChanged("Description");
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
        /// Obtiene o establece si la falla se puede reportar varias veces.
        /// </summary>
        public bool IsMulti {
            get => _isMulti;
            set {
                _isMulti = value;
                OnPropertyChanged(nameof(IsMulti));
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            return Description == ((DeviceFault)obj).Description && Assignable == ((DeviceFault)obj).Assignable;
        }

        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Representa en una cadena la falla actual.
        /// </summary>
        /// <returns>Una cadena que representa la instancia.</returns>
        public override string ToString() => Description;
    }
}