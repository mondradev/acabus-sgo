using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ACABUS_Control_de_operacion {
    /// <summary>
    /// Esta clase define la estructura básica de un equipo
    /// en ruta troncal.
    /// </summary>
    public class Device {
        /// <summary>
        /// Define los tipos de equipos disponibles.
        /// </summary>
        public enum DeviceType {
            /// <summary>
            /// Kiosko de venta y recarga.
            /// </summary>
            KVR,
            /// <summary>
            /// Torniquete de E/S.
            /// </summary>
            TOR,
            /// <summary>
            /// Puerta para personas de movilidad reducida.
            /// </summary>
            PMR
        }

        /// <summary>
        /// Hace la conversión de una cadena valida a una 
        /// instancia tipo de equipo.
        /// </summary>
        /// <param name="value">Cadena a convertir</param>
        /// <returns>Una instancia de tipo</returns>
        private static DeviceType? ParseType(String value) {
            switch (value) {
                case "KVR":
                    return DeviceType.KVR;
                case "PMR":
                    return DeviceType.PMR;
                case "TOR":
                    return DeviceType.TOR;
            }
            return null;
        }

        /// <summary>
        /// Convierte un nodo XML con una estructura correspondiente
        /// a un equipo de estación a una instancia Device.
        /// </summary>
        /// <param name="device">Nodo XML que representa un equipo.</param>
        /// <param name="station">Estación a la que pertenece el equipo.</param>
        /// <returns>Una instancia de un equipo de estación.</returns>
        public static Device ToDevice(XmlNode device, Station station) {
            if (!device.Name.Equals("Equip"))
                return null;
            var deviceTemp = new Device(station);
            deviceTemp.ID = Int32.Parse(device.Attributes["id"].Value);
            deviceTemp.IP = device.Attributes["ip"].Value;
            deviceTemp.Type = ParseType(device.Attributes["type"].Value);
            deviceTemp.Status = device.Attributes["status"] != null ? Boolean.Parse(device.Attributes["status"].Value) : true;
            if (deviceTemp.Type == DeviceType.KVR) {
                var kvr = KVR.ToKVR(deviceTemp);
                String maxCardStr = device.Attributes["maxCard"].Value;
                maxCardStr = String.IsNullOrEmpty(maxCardStr) ? "0" : maxCardStr;
                String minCardStr = device.Attributes["minCard"].Value;
                minCardStr = String.IsNullOrEmpty(minCardStr) ? "0" : minCardStr;
                kvr.MaxCard = Int32.Parse(maxCardStr);
                kvr.MinCard = Int32.Parse(minCardStr);
                kvr.Status = Boolean.Parse(device.Attributes["status"].Value);
                return kvr;
            }
            return deviceTemp;
        }

        /// <summary>
        /// Obtiene el identificador del equipo en la 
        /// estación.
        /// </summary>
        public int ID { get; protected set; }

        /// <summary>
        /// Obtiene el tipo de equipo
        /// </summary>
        public DeviceType? Type { get; protected set; }

        /// <summary>
        /// Obtiene la dirección IP del equipo
        /// </summary>
        public String IP { get; protected set; }

        /// <summary>
        /// Obtiene el estado del dispositivo.
        /// </summary>
        public Boolean Status { get; protected set; }

        /// <summary>
        /// Obtiene la estación a la que pertenece el 
        /// equipo.
        /// </summary>
        public Station Station { get; protected set; }

        /// <summary>
        /// Crea una instancia nueva de un equipo.
        /// </summary>
        /// <param name="station">Estación a la que pertence
        /// el equipo.</param>
        public Device(Station station) {
            this.Station = station;
        }

        /// <summary>
        /// Una cadena que representa a este equipo.
        /// </summary>
        /// <returns>Un número de serie que identifica al equipo.</returns>
        public new String ToString() {
            return GetNumeSeri();
        }

        /// <summary>
        /// Obtiene el número de serie del equipo.
        /// </summary>
        /// <returns>El número de serie del equipo.</returns>
        public String GetNumeSeri() {
            var type = Type.ToString();
            var trunkID = this.Station.Trunk.ID.ToString("D2");
            var stationID = this.Station.ID.ToString("D2");
            var deviceID = ID.ToString("D2");
            return String.Format("{0}{1}{2}{3}", type, trunkID, stationID, deviceID);
        }


    }
}
