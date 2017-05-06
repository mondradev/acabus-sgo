using Acabus.Utils;
using Acabus.Utils.MVVM;
using System;

namespace Acabus.Models
{
    /// <summary>
    /// Esta clase define la estructura básica de un equipo
    /// en ruta troncal.
    /// </summary>
    public class Device : NotifyPropertyChanged
    {
        /// <summary>
        /// Campo que provee a la propiedad 'ID'.
        /// </summary>
        private UInt16 _id;

        /// <summary>
        /// Campo que provee a la propiedad 'Type'
        /// </summary>
        private DeviceType _type;

        /// <summary>
        /// Campo que provee a la propiedad 'Station'.
        /// </summary>
        private Station _station;

        /// <summary>
        /// Campo que provee a la propiedad 'IP'
        /// </summary>
        private String _ip;

        /// <summary>
        /// Obtiene el identificador del equipo.
        /// </summary>
        public UInt16 ID => _id;

        /// <summary>
        /// Obtiene el tipo de equipo
        /// </summary>
        public DeviceType Type => _type;

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
        /// Campo que provee a la propiedad 'Enabled'.
        /// </summary>
        private Boolean _enabled;

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
        /// Campo que provee a la propiedad 'HasDataBase'.
        /// </summary>
        private Boolean _hasDataBase;

        /// <summary>
        /// Obtiene o establece si el equipo tiene una base de datos.
        /// </summary>  
        public Boolean HasDataBase {
            get => _hasDataBase;
            set {
                _hasDataBase = value;
                OnPropertyChanged("HasDataBase");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'SshEnabled'.
        /// </summary>
        private Boolean _sshEnabled;

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
        /// Obtiene la estación a la que pertenece el 
        /// equipo.
        /// </summary>
        [XmlAnnotation(Ignore = true)] public Station Station => _station;

        /// <summary>
        /// Campo que provee a la propiedad 'MemFree'.
        /// </summary>
        private UInt16 _memFree;

        /// <summary>
        /// Obtiene o establece la memoria (RAM) libre del equipo.
        /// </summary>  
        [XmlAnnotation(Ignore = true)]
        public UInt16 MemFree {
            get => _memFree;
            set {
                _memFree = value;
                OnPropertyChanged("MemFree");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'MemTotal'.
        /// </summary>
        private UInt16 _memTotal;

        /// <summary>
        /// Obtiene o establece la memoria (RAM) total del equipo.
        /// </summary>
        [XmlAnnotation(Ignore = true)]
        public UInt16 MemTotal {
            get => _memTotal;
            set {
                _memTotal = value;
                OnPropertyChanged("MemTotal");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'SpaceFree'.
        /// </summary>
        private UInt16 _spaceFree;

        /// <summary>
        /// Obtiene o establece el espacio libre en disco del equipo.
        /// </summary>
        [XmlAnnotation(Ignore = true)]
        public UInt16 SpaceFree {
            get => _spaceFree;
            set {
                _spaceFree = value;
                OnPropertyChanged("SpaceFree");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'SpaceTotal'.
        /// </summary>
        private UInt16 _spaceTotal;

        /// <summary>
        /// Obtiene o establece el espacio total en disco del equipo.
        /// </summary>
        [XmlAnnotation(Ignore = true)]
        public UInt16 SpaceTotal {
            get => _spaceTotal;
            set {
                _spaceTotal = value;
                OnPropertyChanged("SpaceTotal");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'TransactionsPending'.
        /// </summary>
        private UInt32 _transactionsPending;

        /// <summary>
        /// Obtiene o establece el número total de transacciones pendientes por enviar.
        /// </summary>
        [XmlAnnotation(Ignore = true)]
        public UInt32 TransactionsPending {
            get => _transactionsPending;
            set {
                _transactionsPending = value;
                OnPropertyChanged("TransactionsPending");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'LastSentTransaction'.
        /// </summary>
        private DateTime _lastSentTransaction;

        /// <summary>
        /// Obtiene o establece la fecha de la última transacción envíada.
        /// </summary>
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
        /// Campo que provee a la propiedad 'LastTransactionGenerated'.
        /// </summary>
        private DateTime _lastTransactionGenerated;

        /// <summary>
        /// Obtiene o establece la fecha de la última transacción generada en el equipo.
        /// </summary>
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
        /// Obtiene el tiempo sin replicar.
        /// </summary>
        [XmlAnnotation(Ignore = true)] public TimeSpan TimeWithoutReplicate => LastTransactionGenerated - LastSentTransaction;

        /// <summary>
        /// Campo que provee a la propiedad 'DataBaseName'.
        /// </summary>
        private String _databaseName;

        /// <summary>
        /// Obtiene o establece el nombre de la base de datos cuando el equipo está sin conexión.
        /// </summary>
        public String DataBaseName {
            get => _databaseName;
            set {
                _databaseName = value;
                OnPropertyChanged("DataBaseName");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'CanReplicate'.
        /// </summary>
        private Boolean _canReplicate;

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
        /// Obtiene el número de serie del equipo.
        /// </summary>
        public String NumeSeri => GetNumeSeri();

        /// <summary>
        /// Campo que provee a la propiedad 'State'.
        /// </summary>
        private StateValue _state;

        /// <summary>
        /// Obtiene o establece el estado de la conexión a la red.
        /// </summary>
        public StateValue State {
            get => _state;
            set {
                _state = value;
                OnPropertyChanged("State");
            }
        }

        /// <summary>
        /// Campo que provee a la propiedad 'Ping'.
        /// </summary>
        private Int16 _ping;

        /// <summary>
        /// Obtiene o establece la latencia de la comunicación con el equipo.
        /// </summary>
        public Int16 Ping {
            get => _ping;
            set {
                _ping = value;
                OnPropertyChanged("Ping");
            }
        }

        /// <summary>
        /// Crea una instancia nueva de un equipo.
        /// </summary>
        /// <param name="station">Estación a la que pertence
        /// el equipo.</param>
        public Device(UInt16 id, DeviceType type, Station station)
        {
            _id = id;
            _type = type;
            _station = station;
            SshEnabled = false;
        }

        /// <summary>
        /// Una cadena que representa a este equipo.
        /// </summary>
        /// <returns>Un número de serie que identifica al equipo.</returns>
        public override String ToString() => GetNumeSeri();

        /// <summary>
        /// Obtiene el número de serie del equipo.
        /// </summary>
        /// <returns>El número de serie del equipo.</returns>
        private String GetNumeSeri()
        {
            var type = Type.ToString();
            var trunkID = this.Station.Trunk.ID.ToString("D2");
            var stationID = this.Station.ID.ToString("D2");
            var deviceID = ID.ToString("D2");
            return String.Format("{0}{1}{2}{3}", type, trunkID, stationID, deviceID);
        }

        /// <summary>
        /// Crea una instancia de dispositivo especificando el ID, el tipo y la estación a la que pertenece.
        /// </summary>
        /// <param name="station">Estación a la que pertenece el dispositivo.</param>
        /// <param name="id">Identificador del dispositivo.</param>
        /// <param name="type">Tipo de dispositivo.</param>
        /// <returns>Una instancia de dispositivo.</returns>
        public static Device CreateDevice(Station station, UInt16 id, DeviceType type, Boolean canReplicate = true)
            => new Device(id, type, station)
            {
                CanReplicate = canReplicate
            };
    }
}
