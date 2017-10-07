using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Utils;
using InnSyTech.Standard.Mvvm;
using InnSyTech.Standard.Translates;
using Opera.Acabus.Core.DataAccess.DbConverters;
using System;
using System.Collections.Generic;
using System.Net;

namespace Opera.Acabus.Core.Models
{
    /// <summary>
    /// Define todos los tipos de dispositivos disponibles.
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// Sin tipo definido.
        /// </summary>
        NONE,

        /// <summary>
        /// Kiosko de venta y recarga.
        /// </summary>
        KVR,

        /// <summary>
        /// Torniquete de E/S.
        /// </summary>
        TOR,

        /// <summary>
        /// Torniquete Doble E/S.
        /// </summary>
        TD,

        /// <summary>
        /// Torniquete de Salida
        /// </summary>
        TS,

        /// <summary>
        /// Torniquete Simple de E/S.
        /// </summary>
        TSI,

        /// <summary>
        /// Paso de movilidad reducida.
        /// </summary>
        PMR,

        /// <summary>
        /// Grabador de video en red.
        /// </summary>
        NVR,

        /// <summary>
        /// Switch de estación.
        /// </summary>
        SW,

        /// <summary>
        /// Concentrador de estación.
        /// </summary>
        CDE,

        /// <summary>
        /// Servidor de aplicación.
        /// </summary>
        APP,

        /// <summary>
        /// Servidor de patio de encierro.
        /// </summary>
        PDE,

        /// <summary>
        /// Servidor base de datos.
        /// </summary>
        DB,

        /// <summary>
        /// Display de autobus.
        /// </summary>
        DSPB,

        /// <summary>
        /// Display de estación.
        /// </summary>
        DSPL,

        /// <summary>
        /// Contador de pasajeros.
        /// </summary>
        CONT,

        /// <summary>
        /// Grabador de video móvil.
        /// </summary>
        MRV,

        /// <summary>
        /// Torniquete abordo.
        /// </summary>
        TA,

        /// <summary>
        /// Camara.
        /// </summary>
        CAM,

        /// <summary>
        /// Monitor de autobus.
        /// </summary>
        MON,

        /// <summary>
        /// PC Abordo.
        /// </summary>
        PCA,

        /// <summary>
        /// Planta electrógena de estación.
        /// </summary>
        PGE,

        /// <summary>
        /// Rack de estación.
        /// </summary>
        RACK,

        /// <summary>
        /// Luminarias
        /// </summary>
        LIGHT
    }

    /// <summary>
    /// Provee de funciones extra a la enumeración <see cref="DeviceType"/>.
    /// </summary>
    public static class DeviceTypeExtension
    {
        ///<summary>
        /// Traductor al español de la enumeración <see cref="DeviceType"/>.
        ///</summary>
        private static DeviceTypeSpanishTranslate _translate;

        /// <summary>
        /// Constructo estático de la clase <see cref="DeviceTypeExtension"/>.
        /// </summary>
        static DeviceTypeExtension()
        {
            _translate = new DeviceTypeSpanishTranslate();
        }

        /// <summary>
        /// Traduce al español la enumeración <see cref="DeviceType"/>.
        /// </summary>
        /// <param name="type">Valor de la enumeración <see cref="DeviceType"/> a traducir.</param>
        /// <returns>Una cadena en español que representa la enumeración <see cref="DeviceType"/>.</returns>
        public static String TranslateToSpanish(this DeviceType type)
        => _translate.Translate(type);
    }

    /// <summary>
    /// Define la estructura de todos los dispositivos que se encuentran en operación del BRT.
    /// </summary>
    [Entity(TableName = "Devices")]
    public class Device : NotifyPropertyChanged, IComparable, IComparable<Device>
    {
        /// <summary>
        /// Campo que provee a la propiedad <see cref="Bus" />.
        /// </summary>
        private Bus _bus;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ID" />.
        /// </summary>
        private UInt64 _id;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="IPAddress" />.
        /// </summary>
        private IPAddress _ipAddress;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="SerialNumber" />.
        /// </summary>
        private String _serialNumber;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Station" />.
        /// </summary>
        private Station _station;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Type" />.
        /// </summary>
        private DeviceType _type;

        /// <summary>
        /// Crea una instancia persistente de <see cref="Device"/>.
        /// </summary>
        /// <param name="id">Identificador único del equipo.</param>
        /// <param name="serialNumber">Número de serie del equipo.</param>
        /// <param name="type">Tipo de equipo.</param>
        public Device(ulong id, string serialNumber, DeviceType type)
        {
            _id = id;
            _serialNumber = serialNumber;
            _type = type;
        }

        /// <summary>
        /// Crea una instancia de <see cref="Device"/>.
        /// </summary>
        /// <param name="serialNumber">El número de serie del equipo.</param>
        /// <param name="type">El tipo de equipo.</param>
        public Device(string serialNumber, DeviceType type)
        {
            _serialNumber = serialNumber;
            _type = type;
        }

        /// <summary>
        /// Crea una instancia de <see cref="Device"/>.
        /// </summary>
        public Device() { }

        /// <summary>
        /// Obtiene o establece el autobus al que se encuentra asignado este equipo.
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_Bus_ID")]
        public Bus Bus {
            get => _bus;
            set {
                _bus = value;
                OnPropertyChanged(nameof(Bus));
                OnPropertyChanged(nameof(Location));
            }
        }

        /// <summary>
        /// Obtiene o establece el identificador único del equipo.
        /// </summary>
        [Column(IsPrimaryKey = true, IsAutonumerical = true)]
        public UInt64 ID {
            get => _id;
            private set {
                _id = value;
                OnPropertyChanged(nameof(ID));
            }
        }

        /// <summary>
        /// Obtiene o establece la dirección IP del equipo.
        /// </summary>
        [Column(Converter = typeof(DbIPAddressConverter))]
        public IPAddress IPAddress {
            get => _ipAddress ?? (_ipAddress = new IPAddress(new Byte[] { 0, 0, 0, 0 }));
            set {
                _ipAddress = value;
                OnPropertyChanged(nameof(IPAddress));
            }
        }

        /// <summary>
        /// Obtiene o establece la ubicación actual del dispositivo.
        /// </summary>
        [Column(IsIgnored = true)]
        public ILocation Location
             => (ILocation)Station ?? Bus;

        /// <summary>
        /// Obtiene o establece el número de seria del equipo.
        /// </summary>
        public String SerialNumber {
            get => _serialNumber;
            private set {
                _serialNumber = value;
                OnPropertyChanged(nameof(SerialNumber));
            }
        }

        /// <summary>
        /// Obtiene o establece la estación a la que se encuentra asignado este equipo.
        /// </summary>
        [Column(IsForeignKey = true, Name = "Fk_Station_ID")]
        public Station Station {
            get => _station;
            set {
                _station = value;
                OnPropertyChanged(nameof(Station));
                OnPropertyChanged(nameof(Location));
            }
        }

        /// <summary>
        /// Obtiene o establece el tipo de equipo.
        /// </summary>
        [Column(Converter = typeof(DbEnumConverter<DeviceType>))]
        public DeviceType Type {
            get => _type;
            private set {
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        /// <summary>
        /// Compara dos instancias de <see cref="Device"/> y determina si son diferentes.
        /// </summary>
        /// <param name="device">Un equipo a comparar.</param>
        /// <param name="anotherDevice">Otro equipo a comparar.</param>
        /// <returns>Un valor true si los equipos son diferentes.</returns>
        public static bool operator !=(Device device, Device anotherDevice)
        {
            if (device is null && anotherDevice is null) return false;
            if (device is null || anotherDevice is null) return true;

            return device.CompareTo(anotherDevice) != 0;
        }

        /// <summary>
        /// Compara dos instancias de <see cref="Device"/> y determina si son iguales.
        /// </summary>
        /// <param name="device">Un equipo a comparar.</param>
        /// <param name="anotherDevice">Otro equipo a comparar.</param>
        /// <returns>Un valor true si los equipos son iguales.</returns>
        public static bool operator ==(Device device, Device anotherDevice)
        {
            if (device is null && anotherDevice is null) return true;
            if (device is null || anotherDevice is null) return false;

            return device.Equals(anotherDevice);
        }

        /// <summary>
        /// Compara la instancia <see cref="Device"/> actual con otra instancia <see cref="Device"/> y
        /// devuelve un entero que indica si la posición de la instancia actual es anterior,
        /// posterior o igual que la del otro objeto en el criterio de ordenación.
        /// </summary>
        /// <param name="other">Otra instancia <see cref="Device"/>.</param>
        /// <returns>
        /// Un valor 0 si las instancias son iguales, 1 si la instancia es mayor que la otra y -1 si
        /// la instancia menor que la otra.
        /// </returns>
        public int CompareTo(Device other)
        {
            if (other is null) return 1;

            if (String.IsNullOrEmpty(SerialNumber) && Location != null)
                return Location is Station
                    ? Station.CompareTo(other.Station)
                    : Bus.CompareTo(other.Bus);

            if (String.IsNullOrEmpty(SerialNumber))
                return Type.CompareTo(other.Type);

            return SerialNumber.CompareTo(other.SerialNumber);
        }

        /// <summary>
        /// Compara la instancia <see cref="Device"/> actual con otra instancia y
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
            return CompareTo(other as Device);
        }

        /// <summary>
        /// Determina si la instancia actual es igual a la pasada por argumento de la función.
        /// </summary>
        /// <param name="obj">Instancia a comparar con la actual.</param>
        /// <returns>Un valor true si la instancia es igual a la actual.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != GetType()) return false;

            var anotherDevice = obj as Device;

            return CompareTo(anotherDevice) == 0;
        }

        /// <summary>
        /// Devuelve el código hash de la instancia actual.
        /// </summary>
        /// <returns>Un código hash que representa la instancia actual.</returns>
        public override int GetHashCode()
            => Tuple.Create(SerialNumber, Location, Type).GetHashCode();

        /// <summary>
        /// Representa en una cadena la instancia de <see cref="Device"/> actual.
        /// </summary>
        /// <returns>Una cadena que representa una instancia <see cref="Device"/>.</returns>
        public override string ToString()
            => String.IsNullOrEmpty(SerialNumber) ? Type.TranslateToSpanish() : SerialNumber;
    }

    /// <summary>
    /// Esta clase permite la traducción de la enumeración <see cref="DeviceType"/> a texto en
    /// "Español" que puede leer un ser humano.
    /// </summary>
    public sealed class DeviceTypeSpanishTranslate : EnumTranslator<DeviceType>
    {
        /// <summary>
        /// Crea una instancia del traductor.
        /// </summary>
        public DeviceTypeSpanishTranslate()
            : base(new Dictionary<DeviceType, string>() {
                { DeviceType.TA, "TORNIQUETE ABORDO" },
                { DeviceType.MRV, "GRABADOR DE VIDEO MÓVIL" },
                { DeviceType.PCA, "PC ABORDO" },
                { DeviceType.CAM, "CAMARA" },
                { DeviceType.MON, "MONITOR" },
                { DeviceType.CONT, "CONTADOR DE PASAJEROS" },
                { DeviceType.DSPB, "DISPLAY DE AUTOBUS" },
                { DeviceType.CDE, "CONCENTRADOR DE ESTACIÓN" },
                { DeviceType.DSPL, "DISPLAY DE ESTACIÓN" },
                { DeviceType.KVR, "KIOSKO DE VENTA Y RECARGA" },
                { DeviceType.NVR, "GRABADOR DE VIDEO EN RED" },
                { DeviceType.PMR, "PASO DE MOVILIDAD REDUCIDA" },
                { DeviceType.SW, "SWITCH DE ESTACIÓN" },
                { DeviceType.TD, "TORNIQUETE DOBLE E/S" },
                { DeviceType.TS, "TORNIQUETE DE SALIDA" },
                { DeviceType.TSI, "TORNIQUETE SIMPLE E/S" },
                { DeviceType.TOR, "TORNIQUETE GENÉRICO E/S" },
                { DeviceType.APP, "SERVIDOR DE APLICACIÓN" },
                { DeviceType.DB, "SERVIDOR BASE DE DATOS" },
                { DeviceType.PDE, "SERVIDOR PATIO DE ENCIERRO" },
                { DeviceType.PGE, "PLANTA ELECTRÓGENA" },
                { DeviceType.RACK, "RACK DE ESTACIÓN" },
                { DeviceType.LIGHT, "LUMINARIA DE ESTACIÓN" },
                { DeviceType.NONE, "NINGUNO" }
            })
        {
        }
    }
}