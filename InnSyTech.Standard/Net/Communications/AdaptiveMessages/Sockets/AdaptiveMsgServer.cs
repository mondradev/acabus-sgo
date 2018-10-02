﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed class AdaptiveMsgServer : IDisposable
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
        /// <param name="rules">Ubicación de las reglas de composición para los mensajes.</param>
        public AdaptiveMsgServer(String rulesPath)
        {
            _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _tasks = new List<Task>();
            Rules = MessageRules.Load(rulesPath);
        }

        /// <summary>
        /// Evento que se desencadena cuando se acepta un cliente.
        /// </summary>
        public event EventHandler<IAdaptiveMsgClientArgs> Accepted;


        /// <summary>
        /// Evento que se desencadena cuando se termina la conexión con un cliente.
        /// </summary>
        public event EventHandler<IAdaptiveMsgClientArgs> Disconnected;

        /// <summary>
        /// Evento que se desencadena cuando se recibe una petición de algún cliente.
        /// </summary>
        public event EventHandler<IAdaptiveMsgArgs> Received;

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
        internal MessageRules Rules { get; set; }

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

            if (Started)
                return;

            _server.Bind(new IPEndPoint(IPAddress, Port));
            _server.Listen(MaxConnections);

            Started = true;

            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    if (CancellationTokenSource.IsCancellationRequested)
                        break;

                    Socket client = _server.Accept();

                    Accepted?.Invoke(this, new AdaptiveMsgClientArgs(client));

                    if (CancellationTokenSource.IsCancellationRequested)
                        break;

                    ListenerRequest(client);
                }
                catch (SocketException) { break; }
            }

            Started = false;
        }

        /// <summary>
        /// Genera un hilo para aislar la gestión de las peticiones realizadas por los clientes.
        /// </summary>
        /// <param name="connection">Cliente a gestionar.</param>
        private void ListenerRequest(Socket connection)
        {
            Task requestTask = Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        try
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

                            Received?.Invoke(this, new AdaptiveMsgArgs(connection, Rules, buffer));
                        }
                        catch (AdaptiveMsgException ex)
                        {
                            Trace.WriteLine(ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }

                Disconnected?.Invoke(this, new AdaptiveMsgClientArgs(connection));
            }, CancellationTokenSource.Token);

            _tasks.Add(requestTask);
        }
    }
}