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
    public static class Server
    {
        /// <summary>
        /// Define el puerto TCP utilizado por el servidor para escuchar peticiones.
        /// </summary>
        private const Int32 DEFAULT_PORT = 9000;

        /// <summary>
        /// Es la dirección IP del servidor.
        /// </summary>
        private static IPAddress _ipAddress;

        /// <summary>
        /// En lista todas las sesiones abiertas.
        /// </summary>
        private static List<Session> _sessions;

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
        static Server()
        {
            _ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
            _tokenSource = new CancellationTokenSource();
            _sessions = new List<Session>();
        }

        /// <summary>
        /// Cierra las sesiones abiertas y deja de recibir las peticiones de conexión.
        /// </summary>
        public static void CloseAllSessions()
        {
            if (_sessions == null)
                return;

            foreach (var session in _sessions)
                session.SendMessage(Messages.DISCONNECTING_SERVER);

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
        public static bool Initialize()
        {
            try
            {
                _tcpListener = new TcpListener(_ipAddress, DEFAULT_PORT);
                _tcpListener.Start();

                Task.Run((Action)InitMonitor, _tokenSource.Token);

                return true;
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

            return false;
        }

        /// <summary>
        /// Procesa la petición del cliente y responde de la manera adecuada según sus parametros.
        /// </summary>
        /// <param name="client">Cliente TCP remoto.</param>
        /// <param name="buffer">Buffer de lectura.</param>
        internal static void ProcessMessage(Session client, Messages request)
        {
            AuthenticateIfRequired(client, request);

            if (!client.IsAuthenticated) return;

            /// Solicitud de operaciones
            if ((int)request >= '\x20' && (int)request <= '\x2F')
            {
                RequestOperation(request, client);
                return;
            }

            /// Solicitud de operaciones de base de datos
            if ((int)request >= '\x40' && (int)request <= '\x4F')
            {
                RequestDbOperation(request, client);
                return;
            }

            /// Petición segura de desconexión.
            if (request == Messages.REQUEST_DISCONNECT)
            {
                client.SendMessage(Messages.DISCONNECTED);
                client.Close();
                return;
            }
        }

        /// <summary>
        /// Remueve una sesión cerrada de la lista de sesiones del servidor.
        /// </summary>
        /// <param name="session">Sesión a eliminar del servidor.</param>
        internal static void RemoveTask(Session session)
            => _sessions?.Remove(session);

        /// <summary>
        /// Autentica al usuario remoto de ser requerido.
        /// </summary>
        /// <param name="client">Usuario remoto.</param>
        /// <param name="request">Mensaje de petición.</param>
        private static void AuthenticateIfRequired(Session client, Messages request)
        {
            if (!client.IsAuthenticated || request == Messages.REQUEST_CONNECT)
            {
                client.SendMessage(Messages.NEED_AUTHENTICATE);
                return;
            }

            if (!client.IsAuthenticated || request == Messages.SEND_CREDENTIALS)
            {
                String data = client.GetResponseData();

                if (data is null)
                {
                    client.SendMessage(Messages.BAD_REQUEST);
                    client.Close();
                    return;
                }

                client.Credential = Extensions.ParseFromJson(data);

                if (client.Credential != null && DataAccess.AcabusData.Authenticate(client.Credential))
                    client.SendMessage(Messages.ACCEPT);
                else
                {
                    client.SendMessage(Messages.REJECT);
                    client.Close();
                }
                return;
            }
        }

        /// <summary>
        /// Crea una instancia que gestiona la sesión del cliente.
        /// </summary>
        /// <param name="client">Cliente TCP del cual sea creará la sesión.</param>
        /// <param name="token">Controla las peticiones de cancelación global.</param>
        private static void CreateSession(TcpClient client, CancellationToken token)
        {
            try
            {
                var session = new Session(client)
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

        /// <summary>
        ///
        /// </summary>
        /// <param name="request"></param>
        /// <param name="client"></param>
        private static void RequestDbOperation(Messages request, Session client)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="client"></param>
        private static void RequestOperation(Messages request, Session client)
        {
            throw new NotImplementedException();
        }
    }
}