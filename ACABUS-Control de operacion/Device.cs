using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ACABUS_Control_de_operacion
{
    public class Device
    {
        public enum DeviceType
        {
            KVR, TOR, PMR
        }

        private static DeviceType? ParseType(String value)
        {
            switch (value)
            {
                case "KVR": return DeviceType.KVR;
                case "PMR": return DeviceType.PMR;
                case "TOR": return DeviceType.TOR;
            }
            return null;
        }

        public int ID { get; protected set; }
        public DeviceType? Type { get; protected set; }
        public String IP { get; protected set; }
        public Station Station { get; protected set; }

        public Device(Station station)
        {
            this.Station = station;
        }

        public static Device ToDevice(XmlNode device, Station station)
        {
            if (!device.Name.Equals("Equip")) return null;
            var deviceTemp = new Device(station);
            deviceTemp.ID = Int32.Parse(device.Attributes["id"].Value);
            deviceTemp.IP = device.Attributes["ip"].Value;
            deviceTemp.Type = ParseType(device.Attributes["type"].Value);
            if (deviceTemp.Type == DeviceType.KVR)
            {
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

        public new String ToString()
        {
            return GetNumeSeri();
        }

        public String GetNumeSeri() {
            var type = Type.ToString();
            var trunkID = this.Station.Trunk.ID.ToString("D2");
            var stationID = this.Station.ID.ToString("D2");
            var deviceID = ID.ToString("D2");
            return String.Format("{0}{1}{2}{3}", type, trunkID, stationID, deviceID);
        }

       
    }
}
