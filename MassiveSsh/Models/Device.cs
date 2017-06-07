using Acabus.Utils;
using Acabus.Utils.Mvvm;
using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using System;

namespace Acabus.Models
{
    /// <summary>
    /// Esta clase define la estructura básica de un equipo en ruta troncal.
    /// </summary>
    [Entity(TableName = "Devices")]
    public class Device : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'CanReplicate'.
        /// </summary>
        private Boolean _canReplicate;

        /// <summary>
        /// Campo que provee a la propiedad 'DataBaseName'.
        /// </summary>
        private String _databaseName;

        /// <summary>
        /// Campo que provee a la propiedad 'Enabled'.
        /// </summary>
        private Boolean _enabled;

        /// <summary>
        /// Campo que provee a la propiedad 'HasDataBase'.
        /// </summary>
        private Boolean _hasDatabase;

        /// <summary>
        /// Campo que provee a la propiedad 'ID'.
        /// </summary>
        private UInt16 _id;

        /// <summary>
        /// Campo que provee a la propiedad 'IP'
        /// </summary>
        private String _ip;

        /// <summary>
        /// Campo que provee a la propiedad 'LastSentTransaction'.
        /// </summary>
        private DateTime _lastSentTransaction;

        /// <summary>
        /// Campo que provee a la propiedad 'LastTransactionGenerated'.
        /// </summary>
        private DateTime _lastTransactionGenerated;

        /// <summary>
        /// Campo que provee a la propiedad 'MemFree'.
        /// </summary>
        private UInt16 _memFree;

        /// <summary>
        /// Campo que provee a la propiedad 'MemTotal'.
        /// </summary>
        private UInt16 _memTotal;

        /// <summary>
        /// Campo que provee a la propiedad 'NumeSeri'.
        /// </summary>
        private String _numeSeri;

        /// <summary>
        /// Campo que provee a la propiedad 'Ping'.
        /// </summary>
        private Int16 _ping;

        /// <summary>
        /// Campo que provee a la propiedad 'SpaceFree'.
        /// </summary>
        private UInt16 _spaceFree;

        /// <summary>
        /// Campo que provee a la propiedad 'SpaceTotal'.
        /// </summary>
        private UInt16 _spaceTotal;

        /// <summary>
        /// Campo que provee a la propiedad 'SshEnabled'.
        /// </summary>
        private Boolean _sshEnabled;

        /// <summary>
        /// Campo que provee a la propiedad 'State'.
        /// </summary>
        private StateValue _state;

        /// <summary>
        /// Campo que provee a la propiedad 'Station'.
        /// </summary>
        private Station _station;

        /// <summary>
        /// Campo que provee a la propiedad 'TransactionsPending'.
        /// </summary>
        private UInt32 _transactionsPending;

        /// <summary>
        /// Campo que provee a la propiedad 'Type'
        /// </summary>
        private DeviceType _type;


        /// <summary>
        /// Crea una instancia nueva de un equipo.
        /// </summary>
        /// <param name="station">Estación a la que pertence
        /// el equipo.</param>
        public Device(UInt16 id, DeviceType type, Station station, String numeSeri)
        {
            _id = id;
            _type = type;
            _station = station;
            _numeSeri = numeSeri;
        }

        /// <summary>
        /// Crea una instancia básica de <see cref="Device"/>.
        /// </summary>
        public Device()
        {
            _type = DeviceType.NONE;
            _station = null;
        }

        /// <summary>
        /// Obtiene o establece si el equipo puede replicar información.
        /// </summary>
        public Boolean CanReplicate {
            get => _canReplicate;
            set {
                _canReplicate = value;
                OnPropertyChanged("CanReplicate");
            }
        }

        /// <summary>
        /// Obtiene o establece el nombre de la base de datos cuando el equipo está sin conexión.
        /// </summary>
        public String DatabaseName {
            get => _databaseName;
            set {
                _databaseName = value;
                OnPropertyChanged("DatabaseName");
            }
        }

        /// <summary>
        /// Obtiene o establece si el equipo está activo.
        /// </summary>
        public Boolean Enabled {
            get => _enabled;
            set {
                _enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        /// <summary>
        /// Obtiene o establece si el equipo tiene una base de datos.
        /// </summary>
        public Boolean HasDatabase {
            get => _hasDatabase;
            set {
                _hasDatabase = value;
                OnPropertyChanged("HasDatabase");
            }
        }

        /// <summary>
        /// Obtiene el identificador del equipo.
        /// </summary>
        [Column(IsPrimaryKey = true, IsAutonumerical = true)]
        public UInt16 ID {
            get => _id;
            protected set {
                _id = value;
                OnPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Obtiene o establece la dirección IP del equipo
        /// </summary>
        public String IP {
            get => _ip;
            set {
                _ip = value;
                OnPropertyChanged("IP");
            }
        }

        /// <summary>
        /// Obtiene o establece la fecha de la última transacción envíada.
        /// </summary>
        [Column(IsIgnored = true)]
        [XmlAnnotation(Ignore = true)]
        public DateTime LastSentTransaction {
            get => _lastSentTransaction;
            set {
                _lastSentTransaction = value;
                OnPropertyChanged("LastSentTransaction");
                OnPropertyChanged("TimeWithoutReplicate");
            }
        }

        /// <summary>
        /// Obtiene o establece la fecha de la última transacción generada en el equipo.
        /// </summary>
        [Column(IsIgnored = true)]
        [XmlAnnotation(Ignore = true)]
        public DateTime LastTransactionGenerated {
            get => _lastTransactionGenerated;
            set {
                _lastTransactionGenerated = value;
                OnPropertyChanged("LastTransactionGenerated");
                OnPropertyChanged("TimeWithoutReplicate");
            }
        }

        /// <summary>
        /// Obtiene o establece la memoria (RAM) libre del equipo.
        /// </summary>
        [Column(IsIgnored = true)]
        [XmlAnnotation(Ignore = true)]
        public UInt16 MemFree {
            get => _memFree;
            set {
                _memFree = value;
                OnPropertyChanged("MemFree");
            }
        }

        /// <summary>
        /// Obtiene o establece la memoria (RAM) total del equipo.
        /// </summary>
        [Column(IsIgnored = true)]
        [XmlAnnotation(Ignore = true)]
        public UInt16 MemTotal {
            get => _memTotal;
            set {
                _memTotal = value;
                OnPropertyChanged("MemTotal");
            }
        }

        /// <summary>
        /// Obtiene o establece el número de serie del equipo.
        /// </summary>
        [Column(Name = "SerialNumber")]
        public String NumeSeri {
            get => _numeSeri;
            protected set {
                _numeSeri = value;
                OnPropertyChanged("NumeSeri");
            }
        }

        /// <summary>
        /// Obtiene o establece la latencia de la comunicación con el equipo.
        /// </summary>
        [Column(IsIgnored = true)]
        [XmlAnnotation(Ignore = true)]
        public Int16 Ping {
            get => _ping;
            set {
                _ping = value;
                OnPropertyChanged("Ping");
            }
        }

        /// <summary>
        /// Obtiene o establece el espacio libre en disco del equipo.
        /// </summary>
        [Column(IsIgnored = true)]
        [XmlAnnotation(Ignore = true)]
        public UInt16 SpaceFree {
            get => _spaceFree;
            set {
                _spaceFree = value;
                OnPropertyChanged("SpaceFree");
            }
        }

        /// <summary>
        /// Obtiene o establece el espacio total en disco del equipo.
        /// </summary>
        [Column(IsIgnored = true)]
        [XmlAnnotation(Ignore = true)]
        public UInt16 SpaceTotal {
            get => _spaceTotal;
            set {
                _spaceTotal = value;
                OnPropertyChanged("SpaceTotal");
            }
        }

        /// <summary>
        /// Obtiene o establece si la consola por socket seguro está disponible.
        /// </summary>
        public Boolean SshEnabled {
            get => _sshEnabled;
            set {
                _sshEnabled = value;
                OnPropertyChanged("SshEnabled");
            }
        }

        /// <summary>
        /// Obtiene o establece el estado de la conexión a la red.
        /// </summary>
        [Column(IsIgnored = true)]
        [XmlAnnotation(Ignore = true)]
        public StateValue State {
            get => _state;
            set {
                _state = value;
                OnPropertyChanged("State");
            }
        }

        /// <summary>
        /// Obtiene la estación a la que pertenece el
        /// equipo.
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_Station_ID")]
        [XmlAnnotation(Ignore = true)]
        public Station Station {
            get => _station;
            protected set {
                _station = value;
                OnPropertyChanged("Station");
            }
        }

        /// <summary>
        /// Obtiene el tiempo sin replicar.
        /// </summary>
        [Column(IsIgnored = true)]
        [XmlAnnotation(Ignore = true)]
        public TimeSpan TimeWithoutReplicate => LastTransactionGenerated - LastSentTransaction;

        /// <summary>
        /// Obtiene o establece el número total de transacciones pendientes por enviar.
        /// </summary>
        [Column(IsIgnored = true)]
        [XmlAnnotation(Ignore = true)]
        public UInt32 TransactionsPending {
            get => _transactionsPending;
            set {
                _transactionsPending = value;
                OnPropertyChanged("TransactionsPending");
            }
        }

        /// <summary>
        /// Obtiene el tipo de equipo
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<DeviceType>))]
        public DeviceType Type {
            get => _type;
            protected set {
                _type = value;
                OnPropertyChanged("Type");
            }
        }


        /// <summary>
        /// Operador lógico de desigualdad, determina si dos instancias <see cref="Device"/> son diferentes.
        /// </summary>
        /// <param name="device">Una instancia.</param>
        /// <param name="otherDevice">Otra instancia.</param>
        /// <returns>
        /// Un valor
        /// <code>
        /// true
        /// </code>
        /// si el NumeSeri es diferente en ambas instancias <see cref="Device"/>.
        /// </returns>
        public static bool operator !=(Device device, Device otherDevice)
        {
            if (otherDevice?.NumeSeri == device?.NumeSeri) return false;
            return true;
        }

        /// <summary>
        /// Operador lógico de igualdad, determina si dos instancias <see cref="Device"/> son iguales.
        /// </summary>
        /// <param name="device">Una instancia.</param>
        /// <param name="otherDevice">Otra instancia.</param>
        /// <returns>Un valor <code>true</code> si el NumeSeri es igual en ambas instancias <see cref="Device"/>.</returns>
        public static bool operator ==(Device device, Device otherDevice)
        {
            if (otherDevice?.NumeSeri == device?.NumeSeri) return true;

            return false;
        }

        /// <summary>
        /// Compara dos instancias y determina si estas son iguales.
        /// </summary>
        /// <param name="obj">Instancia a comparar con la actual.</param>
        /// <returns>Un valor <see cref="true"/> si las instancias son iguales.</returns>
        public override bool Equals(object obj)
        {
            if (GetType() != obj.GetType())
                return false;

            return this == (Device)obj;
        }

        /// <summary>
        /// Obtiene un código HASH de la instancia actual.
        /// </summary>
        /// <returns>Un código HASH de la instancia.</returns>
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Una cadena que representa a este equipo.
        /// </summary>
        /// <returns>Un número de serie que identifica al equipo.</returns>
        public override String ToString() => NumeSeri;

    }
}