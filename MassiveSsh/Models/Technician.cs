using Acabus.Utils.Mvvm;
using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using System;

namespace Acabus.Models
{
    [Entity(TableName = "Technicians")]
    public class Technician : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'ID'.
        /// </summary>
        private UInt64 _id;

        /// <summary>
        /// Obtiene o establece el identificador unico del técnico.
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
        /// Campo que provee a la propiedad 'Area'.
        /// </summary>
        private AreaAssignable _area;

        /// <summary>
        /// Obtiene o establece el area asignada.
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<AreaAssignable>))]
        public AreaAssignable Area {
            get => _area;
            set {
                _area = value;
                OnPropertyChanged(nameof(Area));
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
