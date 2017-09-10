using InnSyTech.Standard.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Opera.Acabus.Core.Services
{
    internal class Session
    {
        /// <summary>
        /// Determina el tamaño del buffer de datos.
        /// </summary>
        private readonly int BUFFER_SIZE = 4096;

        /// <summary>
        /// Cliente TCP remoto a la que pertenece esta sesión.
        /// </summary>
        private TcpClient _client;

        /// <summary>
        /// Credencial de la sesión actual.
        /// </summary>
        private Credential _credential;

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
        public Session(TcpClient client)
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
        /// Obtiene si la sesión está autenticada en el servidor.
        /// </summary>
        public Boolean IsAuthenticated => _credential != null;

        /// <summary>
        /// Obtiene la tarea que ejecuta la sesión.
        /// </summary>
        public Task Task => _task;

        /// <summary>
        /// Establece la credencial de sesión.
        /// </summary>
        public Credential Credential {
            get => _credential;
            set => _credential = value;
        }

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
            Server.RemoveTask(this);
        }

        /// <summary>
        /// Envía un mensaje al cliente remoto,
        /// </summary>
        /// <param name="message">Mensaje por envíar.</param>
        public void SendMessage(Messages message, Byte[] data = null)
        {
            var stream = _client.GetStream();

            if (message != Messages.SEND_RESPONSE)
            {
                stream.Write(BitConverter.GetBytes((int)message), 0, 4);
                return;
            }

            stream.Write(BitConverter.GetBytes((int)Messages.BEGIN_RESPONSE), 0, 4);
            stream.Write(data, 0, data.Length);
            stream.Write(BitConverter.GetBytes((int)Messages.END_RESPONSE), 0, 4);
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
                var requestSignal = false;
                while (!_tokenSource.IsCancellationRequested)
                {
                    if (GlobalCancellationToken != null && GlobalCancellationToken.IsCancellationRequested)
                    {
                        Close();
                        continue;
                    }

                    Thread.Sleep(10);

                    if (!stream.DataAvailable && !requestSignal)
                    {
                        if (_tokenSource.IsCancellationRequested)
                            break;

                        stream.Write(BitConverter.GetBytes((int)Messages.IS_ALIVE_SIGNAL), 0, 4);
                        requestSignal = true;
                        Thread.Sleep(10);
                        continue;
                    }
                    else if (!stream.DataAvailable)
                        Close();
                    else
                    {
                        int byteReceived = stream.Read(buffer, 0, BUFFER_SIZE);
                        Messages request = (Messages)BitConverter.ToInt64(buffer, 0);

                        if (requestSignal)
                            if (request == Messages.ALIVE_SIGNAL)
                            {
                                requestSignal = false;
                                continue;
                            }

                        if (_tokenSource.IsCancellationRequested)
                            break;

                        Server.ProcessMessage(this, request);
                    }
                }
            }, _tokenSource.Token);
        }

        /// <summary>
        /// Obtiene los datos de una respuesta.
        /// </summary>
        /// <returns>Una respuesta de datos de longitud dinámica.</returns>
        public String GetResponseData()
        {
            var stream = _client.GetStream();
            var buffer = new Byte[BUFFER_SIZE];

            int bytesReceived = stream.Read(buffer, 0, BUFFER_SIZE);
            Messages request = (Messages)BitConverter.ToInt64(buffer, 0);

            if (request != Messages.BEGIN_RESPONSE)
                return null;

            Queue<Byte> dataQueue = new Queue<Byte>();

            while (!_tokenSource.IsCancellationRequested || request != Messages.END_RESPONSE)
            {
                if (GlobalCancellationToken != null && GlobalCancellationToken.IsCancellationRequested)
                {
                    Close();
                    continue;
                }

                Thread.Sleep(10);

                if (!stream.DataAvailable)
                    continue;

                bytesReceived = stream.Read(buffer, 0, BUFFER_SIZE);
                request = (Messages)BitConverter.ToInt64(buffer, 0);

                if (request != Messages.END_RESPONSE)
                    Array.ForEach(buffer, byteReceived => dataQueue.Enqueue(byteReceived));

            }

            return Encoding.UTF8.GetString(dataQueue.ToArray());
        }
    }
}