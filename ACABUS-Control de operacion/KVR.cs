using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACABUS_Control_de_operacion {
    /// <summary>
    /// Esta clase define la estructura básica de un Kiosko de venta y recarga.
    /// </summary>
    public class KVR : Device {
        /// <summary>
        /// Obtiene una instancia de Kiosko de venta y recarga a partir de un dispositivo.
        /// </summary>
        /// <param name="device">Dispositivo a convertir.</param>
        /// <returns>Instancia de Kiosko de venta y recarga</returns>
        public static KVR ToKVR(Device device) {
            KVR kvrTemp = new KVR(device.Station);
            kvrTemp.ID = device.ID;
            kvrTemp.IP = device.IP;
            kvrTemp.Type = DeviceType.KVR;
            return kvrTemp;
        }

        /// <summary>
        /// Capacidad máxima de tarjetas.
        /// </summary>
        public int MaxCard { get; set; }

        /// <summary>
        /// Capacidad minima de tarjetas.
        /// </summary>
        public int MinCard { get; set; }

        /// <summary>
        /// Crea una instancia de Kiosko de venta y recarga.
        /// </summary>
        /// <param name="station">Estación a la que pertenece el equipo.</param>
        public KVR(Station station) : base(station) { }
    }
}
