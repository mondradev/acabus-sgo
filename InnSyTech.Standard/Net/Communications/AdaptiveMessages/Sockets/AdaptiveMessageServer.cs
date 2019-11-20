using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace InnSyTech.Standard.Net.Communications.AdaptiveMessages.Sockets
{
    /// <summary>
    /// Representa un servidor que escucha todas las peticiones de los clientes compatibles a través
    /// del protocolo TCP/IP.
    /// </summary>
    public sealed class AdaptiveMessageServer : IDisposable
    {
        /// <summary>
        /// Bloqueo de conexiones entrantes.
        /// </summary>
        private static readonly ManualResetEvent _lock = new ManualResetEvent(false);

        /// <summary>
        /// Indica si el servidor a liberado los recursos.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Socket del servidor que escucha las peticiones.
        /// </summary>
        private Socket _server;

        /// <summary>
        /// Crea una nueva instancia especificando la composición de los mensajes.
        /// </summary>
        /// <param name="rules">Ubicación de las reglas de composición para los mensajes.</param>
        public AdaptiveMessageServer(String rulesPath)
        {
            Rules = AdaptiveMessageRules.Load(rulesPath);
        }

        /// <summary>
        /// Evento que se desencadena cuando se acepta un cliente.
        /// </summary>
        public event EventHandler<IAdaptiveMessageAcceptedArgs> Accepted;

        /// <summary>
        /// Evento que se desencadena cuando se termina la conexión con un cliente.
        /// </summary>
        public event EventHandler<IAdaptiveMessageAcceptedArgs> Disconnected;

        /// <summary>
        /// Evento que se desencadena cuando se recibe una petición de algún cliente.
        /// </summary>
        public event EventHandler<IAdaptiveMessageReceivedArgs> Received;

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
        /// Obtiene o establece el puerto TCP por el cual escucha el servidor.
        /// </summary>
        public int Port { get; set; } = 5500;

        /// <summary>
        /// Indica si el servidor actualmente está iniciado.
        /// </summary>
        public bool Started { get; private set; }

        /// <summary>
        /// Obtiene o establece las reglas que permiten serializar y deserializar los mensajes.
        /// </summary>
        internal AdaptiveMessageRules Rules { get; set; }

        /// <summary>
        /// Libera los recursos no administrador por el servidor.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            Shutdown();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Se finalizan las conexiones del servidor y se liberan los recursos utilizados.
        /// </summary>
        public void Shutdown()
        {
            if (!Started)
                return;

            _lock.Set();

            CancellationTokenSource.Cancel();

            if (_server.Connected)
                _server.Shutdown(SocketShutdown.Both);

            _server.Close();
        }

        /// <summary>
        /// Inicializa el servidor, bloqueando el hilo actual donde es llamado.
        /// </summary>
        public void Startup()
        {
            if (Rules == null)
                throw new InvalidOperationException("Se requiere establecer las reglas de mensaje para interpretar las peticiones y respuestas correctamente.");

            if (Started)
                return;

            _server = new Socket(IPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _server.Bind(new IPEndPoint(IPAddress, Port));
            _server.Listen(MaxConnections);

            Started = true;

            while (!CancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    _lock.Reset();

                    _server.BeginAccept(new AsyncCallback(RequestHandle), _server);

                    if (!CancellationTokenSource.IsCancellationRequested)
                        _lock.WaitOne();
                }
                catch (SocketException) { break; }
            }

            Started = false;
        }

        /// <summary>
        /// Acepta de forma asíncrona la conexión de un cliente remoto y procesa su petición.
        /// </summary>
        /// <param name="result">Resultado de la operación asíncrona.</param>
        private void RequestHandle(IAsyncResult result)
        {
            _lock.Set();

            Socket server = (Socket)result.AsyncState;
            Socket remoteEndPoint = server.EndAccept(result);

            Accepted?.Invoke(this, new AdaptiveMessageAcceptedArgs(remoteEndPoint));

            if (CancellationTokenSource.IsCancellationRequested)
                return;

            Received?.Invoke(this, new AdaptiveMessageReceivedArgs(remoteEndPoint, Rules, AdaptiveMessageSocketHelper.ReadBuffer(remoteEndPoint)));

            CheckStatus(remoteEndPoint);
        }

        /// <summary>
        /// Verifica el estado de la conexión del cliente remoto.
        /// </summary>
        /// <param name="remoteEndPoint">Cliente remoto.</param>
        private async void CheckStatus(Socket remoteEndPoint)
        {
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                if (!remoteEndPoint.Connected)
                {
                    Disconnected?.Invoke(this, new AdaptiveMessageAcceptedArgs(remoteEndPoint));
                    break;
                }

                bool canReadAndWrite = remoteEndPoint.Poll(1000, SelectMode.SelectRead) && remoteEndPoint.Poll(1000, SelectMode.SelectWrite);

                if (!canReadAndWrite || !remoteEndPoint.Connected)
                {
                    Disconnected?.Invoke(this, new AdaptiveMessageAcceptedArgs(remoteEndPoint));
                    break;
                }

                await Task.Delay(2000, CancellationTokenSource.Token);
            }
        }
    }
}