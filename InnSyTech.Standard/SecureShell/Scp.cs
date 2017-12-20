using InnSyTech.Standard.Net;
using Renci.SshNet;
using System;
using System.Diagnostics;
using System.IO;

namespace InnSyTech.Standard.SecureShell
{
    public sealed class Scp : IDisposable
    {
        /// <summary>
        /// Obtiene o establece la clave del usuario para la autenticación en el equipo remoto.
        /// </summary>
        private String _password;

        /// <summary>
        /// Obtiene o establece la instancia de una conexión a el equipo remoto.
        /// </summary>
        private ScpClient _session;

        /// <summary>
        /// Crea una instancia de una conexión por SSH a un equipo remoto especificando
        /// la ruta de acceso y sus credenciales para la autenticación.
        /// </summary>
        /// <param name="host">Nombre o dirección IP del equipo remoto.</param>
        /// <param name="username">Nombre de usuario para la autenticación en el equipo remoto.</param>
        /// <param name="password">Contraseña correspondiente al usuario.</param>
        public Scp(String host, String username, String password)
        {
            try
            {
                // Tiempo inicial del establecimiento del enlace
                DateTime initTime = DateTime.Now;

                this.Host = host;
                this.Username = username;
                this._password = password;

                this._session = new ScpClient(host, username, password);

                if (!ConnectionTCP.IsAvaibleIP(this.Host))
                    throw new IOException(String.Format("No hay comunicación con el host {0}", this.Host));
                this._session.Connect();

                this._session.Downloading += (sender, args) => OnTransfer(args.Filename, args.Downloaded, args.Size);
                this._session.Uploading += (sender, args) => OnTransfer(args.Filename, args.Uploaded, args.Size);

                Trace.WriteLine(String.Format("Conectado al host {0}, Tiempo: {1}", Host, DateTime.Now - initTime), "INFO");
            }
            catch (Exception ex) { Trace.WriteLine(ex.Message, "ERROR"); }
        }

        /// <summary>
        /// Desctructor de la instancia.
        /// </summary>
        ~Scp() 
		{
			this.Dispose();
		}

        /// <summary>
        /// Evento que surge durante la transferencia de datos.
        /// </summary>
        public event EventHandler<ScpEventArgs> TransferEvent;

        /// <summary>
        /// Obtiene el nombre o la ip del equipo remoto.
        /// </summary>
        public String Host { get; private set; }

        /// <summary>
        /// Obtiene el nombre de usuario para autenticarse en el equipo remoto.
        /// </summary>
        public String Username { get; private set; }

        /// <summary>
        /// Libera la conexión al equipo remoto.
        /// </summary>
        public void Dispose()
        {
            if (this._session != null)
                this._session.Disconnect();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Permite copiar un archivo remoto al equipo local.
        /// </summary>
        /// <param name="filename">Nombre del archivo a copiar.</param>
        public Boolean Download(String filenameRemote, String filenameLocal)
        {
            if (String.IsNullOrEmpty(filenameLocal)
                || String.IsNullOrEmpty(filenameRemote)) return false;
            if (Directory.Exists(filenameLocal))
                _session.Download(filenameRemote, new DirectoryInfo(filenameLocal));
            else
                _session.Download(filenameRemote, new FileInfo(filenameLocal));
            return true;
        }

        /// <summary>
        /// Indica si se logró la comunicación al equipo remoto.
        /// </summary>
        /// <returns>Un valor verdadero si se estableció la comunicación.</returns>
        public Boolean IsConnected() => this._session.IsConnected;

        /// <summary>
        /// Permite copiar un archivo al equipo remoto.
        /// </summary>
        /// <param name="filename">Nombre del archivo a copiar.</param>
        public Boolean Upload(String filenameLocal, String filenameRemote)
        {
            if (String.IsNullOrEmpty(filenameLocal)
                || String.IsNullOrEmpty(filenameRemote)) return false;
            FileInfo file = new FileInfo(filenameLocal);
            _session.Upload(file, filenameRemote);
            return true;
        }

        /// <summary>
        /// Desencadena el evento de transferencia de datos.
        /// </summary>
        /// <param name="filename">Archivo que se encuentra en procesamiento.</param>
        /// <param name="transferredBytes">Bytes transferidos.</param>
        /// <param name="totalBytes">Total de bytes.</param>
        private void OnTransfer(string filename, long transferredBytes, long totalBytes) =>
            TransferEvent?.Invoke(this, new ScpEventArgs()
            {
                Filename = filename,
                TransferredBytes = transferredBytes,
                TotalBytes = totalBytes
            });

        /// <summary>
        /// Clase de argumentos para los eventos provocados por la transferencias de archivos
        /// por el protocolo Scp.
        /// </summary>
        public class ScpEventArgs : EventArgs
        {
            public String Filename { get; internal set; }
            public Int64 TotalBytes { get; internal set; }
            public Int64 TransferredBytes { get; internal set; }
        }
    }
}