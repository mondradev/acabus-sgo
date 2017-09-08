using InnSyTech.Standard.Net.Messenger.Iso8583;
using System.Net.Sockets;

namespace Opera.Acabus.Core.Services
{
    /// <summary>
    /// Esta clase estática gestiona las conexiones al servidor de aplicación de todo el sistema.
    /// Provee de toda la funcionalidad que su identificador de aplicación le permite.
    /// </summary>
    public static class Client
    {
        public static void SendMessage(Message message)
        {
            TcpClient client = new TcpClient("localhost", 9000);
            byte[] v = message.ToBytes();
            client.GetStream().Write(v, 0, v.Length);
            client.Close();
        }
    }
}