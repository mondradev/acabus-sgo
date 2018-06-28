using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace InnSyTech.Standard.Net.Communications.AdaptativeMessages.Sockets
{
    /// <summary>
    /// Representa un servidor que escucha todas las peticiones de los clientes compatibles a través
    /// del protocolo TCP/IP.
    /// </summary>
    public sealed class AdaptativeMsgServer : IDisposable
    {
        /// <summary>
        /// Socket del servidor que escucha las peticiones.
        /// </summary>
        private readonly Socket _server;

        /// <summary>
        /// Lista de las tareas que gestionan las peticiones de los clientes.
        /// </summary>
        private readonly List<Task> _tasks;

        /// <summary>
        /// Indica si el servidor a liberado los recursos.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Crea una nueva instancia especificando la composición de los mensajes.
        /// </summary>
        /// <param name="rules">Reglas de composición de los mensajes.</param>
        public AdaptativeMsgServer(MessageRules rules)
        {
            _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _tasks = new List<Task>();
            Rules = rules;
        }

        /// <summary>
        /// Evento que se desencadena cuando se recibe una petición de algún cliente.
        /// </summary>
        public event EventHandler<AdaptativeMsgArgs> Received;

        /// <summary>
        /// Obtiene un token de cancelación para detener las peticiones y finalizar el servidor.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();

        /// <summary>
        /// Obtiene o establece la dirección IP a la cual el servidor deberá escuchar.
        /// </summary>
        public IPAddress IPAddress { get; set; } = IPAddress.Any;

        /// <summary>
        /// Obtiene o establece el número máximo de las conexiones que el servidor debe aceptar.
        /// </summary>
        public int MaxConnections { get; set; } = 100;

        /// <summary>
        /// Obtiene o establece las reglas que permiten serializar y deserializar los mensajes.
        /// </summary>
        public MessageRules Rules { get; set; }

        /// <summary>
        /// Obtiene o establece el puerto TCP por el cual escucha el servidor.
        /// </summary>
        public int Port { get; set; } = 5500;

        /// <summary>
        /// Libera los recursos no administrador por el servidor.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            Shutdown();
        }

        /// <summary>
        /// Se finalizan las conexiones del servidor y se liberan los recursos utilizados.
        /// </summary>
        public void Shutdown()
        {
            CancellationTokenSource.Cancel();

            if (_server.Connected)
                _server.Shutdown(SocketShutdown.Both);

            _server.Close();

            Task.WaitAll(_tasks.ToArray());
        }

        /// <summary>
        /// Inicializa el servidor, bloqueando el hilo actual donde es llamado.
        /// </summary>
        public void Startup()
        {
            if (Rules == null)
                throw new InvalidOperationException("Se requiere establecer las reglas de mensaje para interpretar las peticiones y respuestas correctamente.");

            _server.Bind(new IPEndPoint(IPAddress, Port));
            _server.Listen(MaxConnections);

            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    if (CancellationTokenSource.IsCancellationRequested)
                        break;

                    Socket client = _server.Accept();

                    if (CancellationTokenSource.IsCancellationRequested)
                        break;
                    
                    ListenerRequest(client);
                }
                catch (SocketException) { break; }
            }
        }

        /// <summary>
        /// Genera un hilo para aislar la gestión de las peticiones realizadas por los clientes.
        /// </summary>
        /// <param name="connection">Cliente a gestionar.</param>
        private void ListenerRequest(Socket connection)
        {
            Task requestTask = Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(10);

                    if (CancellationTokenSource.IsCancellationRequested)
                        break;

                    if (connection.Available <= 0)
                        continue;

                    Byte[] buffer = new byte[connection.Available];

                    int bytesTransferred = connection.Receive(buffer);

                    if (bytesTransferred <= 0)
                        continue;

                    if (CancellationTokenSource.IsCancellationRequested)
                        break;

                    Received?.Invoke(this, new AdaptativeMsgArgs(connection, Rules, buffer));
                }
            }, CancellationTokenSource.Token);

            _tasks.Add(requestTask);
        }
    }
}