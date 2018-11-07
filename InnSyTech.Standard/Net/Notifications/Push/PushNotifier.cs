using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace InnSyTech.Standard.Net.Notifications.Push
{
    /// <summary>
    /// Representa un notificador de peticiones de descarga.
    /// </summary>
    /// <typeparam name="T">Tipo de dato para la información de la notificación.</typeparam>
    public class PushNotifier<T> : IDisposable where T : class, IPushData
    {
        /// <summary>
        /// Lista de clientes conectados.
        /// </summary>
        private readonly List<Socket> _clients;

        /// <summary>
        /// Socket del servidor.
        /// </summary>
        private readonly Socket _server;

        /// <summary>
        /// Indica si está liberada la instancia.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Crea una nueva instancia.
        /// </summary>
        public PushNotifier()
        {
            _clients = new List<Socket>();
            _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// Obtiene o establece la dirección IP de escucha.
        /// </summary>
        public IPAddress IPAddress { get; set; } = IPAddress.Any;

        /// <summary>
        /// Obtiene o establece el máximo de conexiones.
        /// </summary>
        public int MaxConnection { get; set; } = 200;

        /// <summary>
        /// Obtiene o establece el puerto UDP.
        /// </summary>
        public int Port { get; set; } = 5501;

        /// <summary>
        /// Obtiene un valor que indica si el notificador está iniciado.
        /// </summary>
        public bool Started { get; private set; }

        /// <summary>
        /// Libera los recursos del notificador.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (_server.Connected)
                _server.Shutdown(SocketShutdown.Both);

            _server.Close();
        }

        /// <summary>
        /// Envía una notificación a los clientes.
        /// </summary>
        /// <param name="data">Datos a notificar.</param>
        public void Notify(T data)
        {
            while (true)
            {
                try
                {
                    foreach (Socket client in _clients)
                        SendNotify(client, new PushNotification<T>(data));

                    break;
                }
                catch {

                }

                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Inicia el notificador.
        /// </summary>
        public void Start()
        {
            Task.Run(() =>
            {
                _server.Bind(new IPEndPoint(IPAddress, Port));
                _server.Listen(MaxConnection);

                Started = true;

                BeginListen();
            });
        }

        /// <summary>
        /// Comienza a escuchar las nuevas conexiones.
        /// </summary>
        private void BeginListen()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    Socket _client = _server.Accept();

                    _clients.Add(_client);
                }
                catch (SocketException) { break; }
            }

            Started = false;
        }

        /// <summary>
        /// Envía la notificación al cliente especificado.
        /// </summary>
        /// <param name="client">Cliente a notificar.</param>
        /// <param name="data">Notificación.</param>
        private void SendNotify(Socket client, PushNotification<T> data)
        {
            try
            {
                int bytesTransferred = client.Send(data.ToBytes());

                if (bytesTransferred <= 0)
                    throw new SocketException((int)SocketError.HostDown);
            }
            catch (SocketException)
            {
                if (client.Connected)
                    client.Shutdown(SocketShutdown.Both);

                client.Close();

                _clients.Remove(client);
            }
        }
    }
}