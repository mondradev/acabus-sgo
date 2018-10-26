using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace InnSyTech.Standard.Net.Notifications.Push
{
    /// <summary>
    /// Encapsula un métedo utlizado para crear los eventos de la clase <see cref="PushService{T}"/>.
    /// </summary>
    /// <param name="args">Datos del evento.</param>
    public delegate void PushEvent(PushArgs args);

    /// <summary>
    /// Clase que provee de un cliente para notificaciones Push.
    /// IP: localhost, Port: 5501
    /// </summary>
    /// <typeparam name="T">Tipo de dato manejado para la información de las notificaciones.</typeparam>
    public class PushService<T> where T : class, IPushData
    {
        /// <summary>
        /// Socket de conexión.
        /// </summary>
        private readonly Socket _socket;

        /// <summary>
        /// Crea una instancia con sus valores predeterminados.
        /// </summary>
        public PushService() : this(IPAddress.Loopback, 5501) { }

        /// <summary>
        /// Crea una instancia especificando la IP y el puerto.
        /// </summary>
        /// <param name="ip">Dirección IP del servidor Push.</param>
        /// <param name="port">Puerto.</param>
        public PushService(IPAddress ip, int port)
        {
            IPAddress = ip;
            Port = port;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Udp);
            _socket.Connect(IPAddress, Port);

            Task.Run((Action)ListenNotifications);
        }

        /// <summary>
        /// Evento que se desencadena cuando el servidor envía una notificación nueva.
        /// </summary>
        public event PushEvent Notified;

        /// <summary>
        /// Obtiene la dirección IP a la cual se conecta el cliente.
        /// </summary>
        public IPAddress IPAddress { get; }

        /// <summary>
        /// Obtiene el puerto por el cual se conecta el cliente.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Escucha todas las notificaciones del servidor.
        /// </summary>
        private void ListenNotifications()
        {
            while (true)
            {
                Thread.Sleep(10);

                if (_socket.Available <= 0)
                    continue;

                Byte[] buffer = new Byte[_socket.Available];

                int byteTransferred = _socket.Receive(buffer);

                if (byteTransferred <= 0)
                    continue;

                Notified?.Invoke(new PushArgs(PushNotification<T>.FromBytes<T>(buffer)));
            }
        }
    }
}