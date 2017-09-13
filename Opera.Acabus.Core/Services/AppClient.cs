using InnSyTech.Standard.Net.Messenger.Iso8583;
using Opera.Acabus.Core.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Opera.Acabus.Core.Services
{
    /// <summary>
    /// Gestiona las conexiones al servidor de aplicación y provee de toda la funcionalidad que su
    /// identificador de aplicación le permite.
    /// </summary>
    public sealed class AppClient
    {
        /// <summary>
        /// La longitud de tamaño del buffer de lectura.
        /// </summary>
        private readonly int BUFFER_SIZE = 1024;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="AppID" />.
        /// </summary>
        private Int64 _appID;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ClientIP" />.
        /// </summary>
        private IPAddress _clientIP;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ServerID" />.
        /// </summary>
        private Int64 _serverID;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ServerIP" />.
        /// </summary>
        private IPAddress _serverIP;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="ServerPort" />.
        /// </summary>
        private Int32 _serverPort;

        /// <summary>
        /// Señala el token de cancelación.
        /// </summary>
        private CancellationTokenSource _tokenSource;

        /// <summary>
        /// Crea una instancia nueva de <see cref="AppClient"/>.
        /// </summary>
        public AppClient()
        {
            _appID = AcabusDataContext.ConfigContext.Read("App")?.ToInteger("ID") ?? 0;
            _serverID = AcabusDataContext.ConfigContext.Read("Server")?.ToInteger("ID") ?? 0;
            _serverPort = (Int32)(AcabusDataContext.ConfigContext.Read("Server")?.ToInteger("Port") ?? 9000);
            _serverIP = IPAddress.Parse(AcabusDataContext.ConfigContext.Read("Server")?.ToString("IP") ?? "127.0.0.1");
            _clientIP = IPAddress.Any;
        }

        /// <summary>
        /// Obtiene el identificador de aplicación especificado en la configuración.
        /// </summary>
        public Int64 AppID => _appID;

        /// <summary>
        /// Obtiene la dirección IP del cliente.
        /// </summary>
        public IPAddress ClientIP => _clientIP;

        /// <summary>
        /// Obtiene el identificador del servidor a conectarse.
        /// </summary>
        public Int64 ServerID => _serverID;

        /// <summary>
        /// Obtiene la dirección IP del servidor.
        /// </summary>
        public IPAddress ServerIP => _serverIP;

        /// <summary>
        /// Obtiene el puerto TCP con el cual se realiza la conexión al servidor.
        /// </summary>
        public Int32 ServerPort => _serverPort;

        /// <summary>
        /// Obiente o establece el token de cancelación de la instancia.
        /// </summary>
        public CancellationTokenSource TokenSource {
            get => _tokenSource ?? (_tokenSource = new CancellationTokenSource());
            set => _tokenSource = value;
        }

        /// <summary>
        /// Envía un mensaje al servidor y espera una respuesta.
        /// </summary>
        /// <param name="message">Mensaje a envíar.</param>
        /// <returns>Mensaje con el cual ha respondido el servidor.</returns>
        public AppMessage SendRequest(AppMessage message)
        {
            TcpClient client = new TcpClient(ServerIP.ToString(), ServerPort);

            _clientIP = (client.Client.LocalEndPoint as IPEndPoint).Address;

            Byte[] buffer = message.ToBytes();

            NetworkStream stream = client.GetStream();

            stream.WriteAsync(buffer, 0, buffer.Length, TokenSource.Token);

            if (_tokenSource.IsCancellationRequested)
                return null;

            buffer = new byte[BUFFER_SIZE];

            List<Byte> readBytes = new List<byte>();

            bool responsing = false;

            while (!_tokenSource.IsCancellationRequested)
            {
                Thread.Sleep(10);

                if (_tokenSource.IsCancellationRequested)
                    return null;

                if (!stream.DataAvailable && responsing)
                    break;

                var task = stream.ReadAsync(buffer, 0, BUFFER_SIZE, _tokenSource.Token);

                int countReadBytes = task.Result;

                if (countReadBytes == 0)
                    break;

                if (countReadBytes < 0)
                    throw new IOException("Error al leer desde el socket de conexión.", countReadBytes);

                readBytes.AddRange(buffer.Take(countReadBytes));

                responsing = true;
            }

            AppMessage response = readBytes.Count > 0 ? AppMessage.FromBytes(readBytes.ToArray()) : null;

            client.Close();

            return response;
        }
    }
}