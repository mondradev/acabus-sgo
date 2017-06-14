using Acabus.Utils;
using InnSyTech.Standard.Database;
using System;

namespace Acabus.Models
{
    /// <summary>
    /// Esta clase define la estructura básica de un Kiosko de venta y recarga.
    /// </summary>
    [XmlAnnotation(Name = "Device")]
    public sealed class Kvr : Device
    {
        /// <summary>
        /// Campo que provee a la propiedad 'CardStock'.
        /// </summary>
        private UInt16 _cardStock;

        /// <summary>
        /// Campo que provee a la propiedad 'MaxCard'.
        /// </summary>
        private UInt16 _maxCard;

        /// <summary>
        /// Campo que provee a la propiedad 'MinCard'.
        /// </summary>
        private UInt16 _minCard;

        /// <summary>
        /// Crea una instancia de un Kiosko de Venta y Recarga.
        /// </summary>
        /// <param name="id">Identificador del kiosko.</param>
        /// <param name="station">Estación a la que pertenece.</param>
        public Kvr(UInt64 id, Station station, String numeSeri) : base(id, DeviceType.KVR, station, numeSeri) { }

        /// <summary>
        /// Crea una instancia básica de KVR.
        /// </summary>
        public Kvr() { }

        /// <summary>
        /// Obtiene o establece el inventario actual de tarjetas en el Kiosko.
        /// </summary>
        [XmlAnnotation(Ignore = true)]
        [Column(IsIgnored = true)]
        public UInt16 CardStock {
            get => _cardStock;
            set {
                _cardStock = value;
                OnPropertyChanged("CardStock");
            }
        }

        /// <summary>
        /// Obtiene o establece el número máximo de tarjetas para el Kiosko.
        /// </summary>
        public UInt16 MaxCard {
            get => _maxCard;
            set {
                _maxCard = value;
                OnPropertyChanged("MaxCard");
            }
        }

        /// <summary>
        /// Obtiene o establece el número mínimo de tarjetas para el Kiosko.
        /// </summary>
        public UInt16 MinCard {
            get => _minCard;
            set {
                _minCard = value;
                OnPropertyChanged("MinCard");
            }
        }

        /// <summary>
        /// Obtiene una instancia de Kiosko de venta y recarga a partir de un dispositivo.
        /// </summary>
        /// <param name="device">Dispositivo a convertir.</param>
        /// <returns>Instancia de Kiosko de venta y recarga</returns>
        public static Kvr FromDeviceToKvr(ref Device device)
        {
            Kvr kvrTemp = new Kvr(device.ID, device.Station, device.NumeSeri)
            {
                IP = device.IP,
                HasDatabase = device.HasDatabase,
                SshEnabled = device.SshEnabled,
                Enabled = device.Enabled,
                CanReplicate = device.CanReplicate
            };
            device = kvrTemp;
            return kvrTemp;
        }
    }
}