using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ACABUS_Control_de_operacion
{
    class ConnectionTCP
    {
        public bool sendToPing(string strIP, int it)
        {
            if (!isIPv4(strIP))
                return false;
            Ping ping = new Ping();
            try
            {
                PingReply pr = ping.Send(strIP);
                if (pr.Status != IPStatus.Success)
                {
                    for (int i = 1; i < it; i++)
                    {
                        pr = ping.Send(strIP);
                        if (pr.Status == IPStatus.Success)
                            return true;
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool isIPv4(string strIp)
        {
            if (strIp.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length == 4)
            {
                IPAddress ipAddr;
                if (IPAddress.TryParse(strIp, out ipAddr))
                    return true;
            }
            return false;
        }

    }
}
