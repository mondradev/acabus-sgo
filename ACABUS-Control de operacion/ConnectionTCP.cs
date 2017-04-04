using System;
using System.Net;
using System.Net.NetworkInformation;

namespace ACABUS_Control_de_operacion
{
    public class ConnectionTCP
    {
        public bool SendToPing(string strIP, int it)
        {
            if (!IsIPv4(strIP))
                return false;

            Ping ping = new Ping();
            try
            {
                PingReply pr = null;
                Int16 i = 0;
                while ((pr == null || pr.Status != IPStatus.Success) && i < it)
                {
                    pr = ping.Send(strIP);
                    i++;
                    if (i == it && pr.Status != IPStatus.Success)
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool IsIPv4(string strIp)
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
