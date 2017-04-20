using System;
using System.Diagnostics;
using System.IO;

namespace MassiveSsh.Utils.SecureShell
{
    public class Scp : IDisposable
    {
        /// <summary>
        /// Indica el estado de la transferencia de los archivos.
        /// </summary>
        public enum ScpStatus
        {
            /// <summary>
            /// Comenzando la transferencia.
            /// </summary>
            START,

            /// <summary>
            /// En progreso de la transferencia.
            /// </summary>
            PROGRESS,

            /// <summary>
            /// Finalizando la transferencia.
            /// </summary>
            END
        }

        /// <summary>
        /// Clase de argumentos para los eventos provocados por la transferencias de archivos
        /// por el protocolo Scp.
        /// </summary>
        public class ScpEventArgs : EventArgs
        {
            public String SourceData { get; internal set; }
            public String DestinationData { get; internal set; }
            public Int32 TransferredBytes { get; internal set; }
            public Int32 TotalBytes { get; internal set; }
            public String Message { get; internal set; }
            public ScpStatus Status { get; internal set; }
        }

        /// <summary>
        /// Obtiene el nombre o la ip del equipo remoto.
        /// </summary>
        public String Host { get; private set; }

        /// <summary>
        /// Obtiene el nombre de usuario para autenticarse en el equipo remoto.
        /// </summary>
        public String Username { get; private set; }

        /// <summary>
        /// Obtiene o establece la clave del usuario para la autenticación en el equipo remoto.
        /// </summary>
        private String _password;

        /// <summary>
        /// Obtiene o establece la instancia de una conexión a el equipo remoto.
        /// </summary>
        private Tamir.SharpSsh.Scp _session;

        /// <summary>
        /// Evento que surge durante la transferencia de datos.
        /// </summary>
        public event EventHandler<ScpEventArgs> TransferEvent;

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

                this._session = new Tamir.SharpSsh.Scp(host, username)
                {
                    Password = password
                };
                if (!ConnectionTCP.IsAvaibleIP(this.Host))
                    throw new IOException(String.Format("No hay comunicación con el host {0}", this.Host));
                this._session.Connect();

                this._session.OnTransferStart += (string src, string dst, int transferredBytes, int totalBytes, string message) =>
                {
                    OnTransfer(src, dst, transferredBytes, totalBytes, message, ScpStatus.START);
                };
                this._session.OnTransferProgress += (string src, string dst, int transferredBytes, int totalBytes, string message) =>
                {
                    OnTransfer(src, dst, transferredBytes, totalBytes, message, ScpStatus.PROGRESS);
                };
                this._session.OnTransferEnd += (string src, string dst, int transferredBytes, int totalBytes, string message) =>
                {
                    OnTransfer(src, dst, transferredBytes, totalBytes, message, ScpStatus.END);
                };

                Trace.WriteLine(String.Format("Conectado al host {0}, Tiempo: {1}", Host, DateTime.Now - initTime), "INFO");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message, "ERROR");
            }
        }

        /// <summary>
        /// Indica si se logró la comunicación al equipo remoto.
        /// </summary>
        /// <returns>Un valor verdadero si se estableció la comunicación.</returns>
        public Boolean IsConnected()
        {
            return this._session.Connected;
        }

        /// <summary>
        /// Desencadena el evento de transferencia de datos.
        /// </summary>
        /// <param name="src">Origen de datos.</param>
        /// <param name="dst">Destino de datos.</param>
        /// <param name="transferredBytes">Bytes transferidos.</param>
        /// <param name="totalBytes">Total de bytes.</param>
        /// <param name="message">Mensajes de evento.</param>
        /// <param name="status">Estado de la transferencia.</param>
        private void OnTransfer(string src, string dst, int transferredBytes, int totalBytes, string message, ScpStatus status)
        {
            TransferEvent?.Invoke(this, new ScpEventArgs()
            {
                DestinationData = dst,
                SourceData = src,
                Message = message,
                TransferredBytes = transferredBytes,
                TotalBytes = totalBytes,
                Status = status
            });
        }

        /// <summary>
        /// Permite copiar un archivo al equipo remoto.
        /// </summary>
        /// <param name="filename">Nombre del archivo a copiar.</param>
        public Boolean Upload(String filenameLocal, String filenameRemote)
        {
            if (String.IsNullOrEmpty(filenameLocal)
                || String.IsNullOrEmpty(filenameRemote)) return false;
            _session.Put(filenameLocal, filenameRemote);
            return true;
        }

        /// <summary>
        /// Permite copiar un archivo remoto al equipo local.
        /// </summary>
        /// <param name="filename">Nombre del archivo a copiar.</param>
        public Boolean Download(String filenameRemote, String filenameLocal)
        {
            if (String.IsNullOrEmpty(filenameLocal)
                || String.IsNullOrEmpty(filenameRemote)) return false;
            _session.Get(filenameRemote, filenameLocal);
            return true;
        }

        /// <summary>
        /// Desctructor de la instancia.
        /// </summary>
        ~Scp()
        {
            this.Dispose();
        }

        /// <summary>
        /// Libera la conexión al equipo remoto.
        /// </summary>
        public void Dispose()
        {
            if (this._session != null)
                this._session.Close();
        }
    }
}
