using InnSyTech.Standard.Net.Messenger.Iso8583;
using Opera.Acabus.Core.DataAccess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Opera.Acabus.Core.Services
{
    /// <summary>
    /// Esta clase estática sumistra los servicio a través del protocolo TCP/IP por medio de
    /// mensajería, permitiendo así tener acceso a la base de datos centralizada y al resto de los
    /// componentes que integran todo el sistema.
    /// </summary>
    public static class AppServer
    {
        /// <summary>
        /// Define el puerto TCP utilizado por el servidor para escuchar peticiones.
        /// </summary>
        private static readonly Int32 _serverPort;

        /// <summary>
        /// Es la dirección IP del servidor.
        /// </summary>
        private static IPAddress _ipAddress;

        /// <summary>
        /// En lista todas las sesiones abiertas.
        /// </summary>
        private static List<AppSession> _sessions;

        /// <summary>
        /// Instancia que permite la escucha por TCP.
        /// </summary>
        private static TcpListener _tcpListener;

        /// <summary>
        /// Token utilizado para la cancelación de todas las tareas.
        /// </summary>
        private static CancellationTokenSource _tokenSource;

        /// <summary>
        /// Crea una instancia estática del servidor.
        /// </summary>
        static AppServer()
        {
            IPHostEntry iPHostEntry
                = Dns.GetHostEntry(AcabusDataContext.ConfigContext["Server"]?.ToString("Hostname") ?? "localhost");

            _serverPort = (int)(AcabusDataContext.ConfigContext["Server"]?.ToInteger("Hort") ?? 9000);
            _ipAddress = iPHostEntry.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();
            _tokenSource = new CancellationTokenSource();
            _sessions = new List<AppSession>();
        }

        /// <summary>
        /// Representa un método utilizado para la interpretación de las peticiones del servidor de aplicación SGO.
        /// </summary>
        /// <param name="message">Mensaje de petición.</param>
        /// <returns>Mensaje de respuesta.</returns>
        public delegate AppMessage CallProcessingRequest(AppMessage message);

        /// <summary>
        /// Método utilizado para la interpretación de las peticiones al servidor.
        /// </summary>
        public static CallProcessingRequest ProcessingRequest { get; set; }

        /// <summary>
        /// Cierra las sesiones abiertas y deja de recibir las peticiones de conexión.
        /// </summary>
        public static void CloseAllSessions()
        {
            if (_sessions == null)
                return;

            _tokenSource.Cancel();

            try
            {
                Task.WaitAll(_sessions.Select(session => session.Task).ToArray());
            }
            catch (AggregateException ex)
            {
                foreach (var ie in ex.InnerExceptions)
                {
                    if (ie is TaskCanceledException)
                        Trace.WriteLine($"Tarea detenida --> {ie.Message}", "INFO");
                }
            }
            finally
            {
                _tokenSource.Dispose();
            }
        }

        /// <summary>
        /// Inicializa el servidor del nucleo de Acabus SGO.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                _tcpListener = new TcpListener(_ipAddress, _serverPort);
                _tcpListener.Start();

                Task.Run((Action)InitMonitor, _tokenSource.Token)
                    .Wait();
            }
            catch (SocketException ex)
            {
                Trace.WriteLine($"Existe un error al iniciar el servidor del nucleo --> {ex.Message}", "ERROR");
            }
            catch (TaskCanceledException ex)
            {
                if (_tokenSource.IsCancellationRequested)
                    Trace.WriteLine($"Se detiene el servidor, no se aceptan más solicitudes de conexión --> {ex.Message}", "INFO");
                else
                    Trace.WriteLine($"Se ha detenido el servidor sin solicitarlo --> {ex.Message}", "ERROR");
            }
        }

        /// <summary>
        /// Procesa la petición del cliente y responde de la manera adecuada según sus parametros.
        /// </summary>
        /// <param name="client">Cliente TCP remoto.</param>
        /// <param name="buffer">Buffer de lectura.</param>
        internal static void MessageProcessing(AppSession client, AppMessage request)
        {
            var message = ProcessingRequest?.Invoke(request);
            if (message != null)
                message = new AppMessage() { { 64, "El servidor no tiene funciones disponibles." } };
            client.SendMessage(message);
        }

        /// <summary>
        /// Remueve una sesión cerrada de la lista de sesiones del servidor.
        /// </summary>
        /// <param name="session">Sesión a eliminar del servidor.</param>
        internal static void RemoveTask(AppSession session)
            => _sessions?.Remove(session);

        /// <summary>
        /// Crea una instancia que gestiona la sesión del cliente.
        /// </summary>
        /// <param name="client">Cliente TCP del cual sea creará la sesión.</param>
        /// <param name="token">Controla las peticiones de cancelación global.</param>
        private static void CreateSession(TcpClient client, CancellationToken token)
        {
            try
            {
                var session = new AppSession(client)
                {
                    GlobalCancellationToken = token
                };
                lock (_sessions)
                    if (!_tokenSource.IsCancellationRequested)
                        _sessions?.Add(session);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Ocurrió un error al momento de crear la sesión del cliente remoto ---> {ex.Message}", "ERROR");
            }
        }

        /// <summary>
        /// Inicializa el monitor de sesiones del servidor.
        /// </summary>
        private static void InitMonitor()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    Thread.Sleep(10);

                    if (!_tcpListener.Pending())
                        continue;

                    TcpClient client = _tcpListener.AcceptTcpClient();
                    CreateSession(client, _tokenSource.Token);
                }
                catch (SocketException ex)
                {
                    Trace.WriteLine($"Ocurrió un error en la comunicación con el cliente remoto ---> {ex.Message}", "ERROR");
                }
                catch (InvalidOperationException ex)
                {
                    Trace.WriteLine($"Operación invalida ---> {ex.Message}", "ERROR");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Ocurrió un error no identificado ---> {ex.Message}", "ERROR");
                }
            }
        }
    }
}