using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACABUS_Control_de_operacion
{
    public class KVR : Device
    {

        public int MaxCard { get; set; }
        public int MinCard { get; set; }
        public Boolean Status { get; set; }

        public KVR(Station station) : base(station)
        {

        }

        public static KVR ToKVR(Device device)
        {
            KVR kvrTemp = new KVR(device.Station);
            kvrTemp.ID = device.ID;
            kvrTemp.IP = device.IP;
            kvrTemp.Type = DeviceType.KVR;
            return kvrTemp;
        }
    }
}
