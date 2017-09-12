using InnSyTech.Standard.Net.Messenger.Iso8583;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Opera.Acabus.Core.Services
{
    /// <summary>
    /// Representa una sesión de una conexión al servidor de aplicación.
    /// </summary>
    internal sealed class AppSession
    {
        /// <summary>
        /// Determina el tamaño del buffer de datos.
        /// </summary>
        private const int BUFFER_SIZE = 1024;

        /// <summary>
        /// Cliente TCP remoto a la que pertenece esta sesión.
        /// </summary>
        private TcpClient _client;

        /// <summary>
        /// Es la tarea utilizada para controlar la lectura y escritura de la comunicación del cliente TCP de la sesión.
        /// </summary>
        private Task _task;

        /// <summary>
        /// Token utilizado para la cancelación del token actual.
        /// </summary>
        private CancellationTokenSource _tokenSource;

        /// <summary>
        /// Crea una nueva sesión de conexión con el cliente remoto.
        /// </summary>
        /// <param name="client">Cliente TCP remoto.</param>
        public AppSession(TcpClient client)
        {
            _client = client;
            _tokenSource = new CancellationTokenSource();

            InitializeSession();
        }

        /// <summary>
        /// Obtiene o establece el token de cancelación global de todo el servidor.
        /// </summary>
        public CancellationToken GlobalCancellationToken { get; set; }

        /// <summary>
        /// Obtiene la tarea que ejecuta la sesión.
        /// </summary>
        public Task Task => _task;

        /// <summary>
        /// Detiene y cierra la sesión del cliente actual.
        /// </summary>
        public void Close()
        {
            _tokenSource.Cancel();
            try
            {
                Task.Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var ie in ex.InnerExceptions)
                    if (ie is TaskCanceledException)
                        Trace.WriteLine($"Sesión terminada {_client.Client} ---> {ie.Message}", "INFO");
            }
            AppServer.RemoveTask(this);
        }

        /// <summary>
        /// Envía un mensaje al cliente remoto,
        /// </summary>
        /// <param name="message">Mensaje por envíar.</param>
        public void SendMessage(AppMessage message)
        {
            var stream = _client.GetStream();
            byte[] bytes = message.ToBytes();
            stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Inicializa la sesión a través de un hilo que monitorea su activadad.
        /// </summary>
        private void InitializeSession()
        {
            var stream = _client.GetStream();
            var buffer = new Byte[BUFFER_SIZE];

            _task = Task.Run(() =>
            {
                List<byte> builder = new List<byte>();
                Boolean endTask = false;
                while (!_tokenSource.IsCancellationRequested)
                {
                    if (GlobalCancellationToken != null && GlobalCancellationToken.IsCancellationRequested)
                    {
                        Close();
                        continue;
                    }

                    Thread.Sleep(10);

                    if (_tokenSource.IsCancellationRequested)
                        break;

                    stream.ReadTimeout = 600;

                    if ((endTask = !stream.DataAvailable))
                        break;

                    var task = stream.ReadAsync(buffer, 0, BUFFER_SIZE, _tokenSource.Token);
                    int bytesRead = task.Result;

                    builder.AddRange(buffer.Take(bytesRead));

                    if (_tokenSource.IsCancellationRequested)
                        break;

                    if ((endTask = bytesRead < BUFFER_SIZE))
                        break;
                }

                if (endTask)
                    AppServer.MessageProcessing(this, AppMessage.FromBytes(builder.ToArray()));
            }, _tokenSource.Token);
        }
    }
}