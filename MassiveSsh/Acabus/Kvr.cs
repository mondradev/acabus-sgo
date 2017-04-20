using MassiveSsh.Utils;
using System;

namespace MassiveSsh.Acabus
{
    /// <summary>
    /// Esta clase define la estructura básica de un Kiosko de venta y recarga.
    /// </summary>
    [XmlAnnotation(Name = "Device")]
    public class Kvr : Device
    {
        /// <summary>
        /// Obtiene una instancia de Kiosko de venta y recarga a partir de un dispositivo.
        /// </summary>
        /// <param name="device">Dispositivo a convertir.</param>
        /// <returns>Instancia de Kiosko de venta y recarga</returns>
        public static Kvr ToKVR(Device device)
        {
            Kvr kvrTemp = new Kvr(device.Station)
            {
                ID = device.ID,
                IP = device.IP,
                Type = DeviceType.KVR,
                HasDataBase = device.HasDataBase,
                SshEnabled=device.SshEnabled,
                Status = device.Status
            };
            return kvrTemp;
        }

        /// <summary>
        /// Capacidad máxima de tarjetas.
        /// </summary>
        public Int16 MaxCard { get; set; }

        /// <summary>
        /// Capacidad minima de tarjetas.
        /// </summary>
        public Int16 MinCard { get; set; }

        /// <summary>
        /// Indica si el kiosko es externo.
        /// </summary>
        public Boolean IsExtern { get; set; }

        /// <summary>
        /// Es el nombre de la base de datos en caso de ser KVR externo
        /// </summary>
        public String DataBaseName { get; set; }

        /// <summary>
        /// Crea una instancia de Kiosko de venta y recarga.
        /// </summary>
        /// <param name="station">Estación a la que pertenece el equipo.</param>
        public Kvr(Station station) : base(station) { }
    }
}
