using InnSyTech.Standard.Net;
using Renci.SshNet;
using System;
using System.Diagnostics;
using System.IO;

namespace InnSyTech.Standard.SecureShell
{
    /// <summary>
    /// Esta clase permite el envío de comandos a un equipo remoto que tenga
    /// el servicio de SSH.
    /// </summary>
    public class Ssh : IDisposable
    {
        /// <summary>
        /// Clave del usuario para la autenticación en el equipo remoto.
        /// </summary>
        private String _password;

        /// <summary>
        /// La instancia de una conexión a el equipo remoto por SSH.
        /// </summary>
        private SshClient _session;

        /// <summary>
        /// Crea una instancia de una conexión por SSH a un equipo remoto especificando
        /// la ruta de acceso y sus credenciales para la autenticación.
        /// </summary>
        /// <param name="host">Nombre o dirección IP del equipo remoto.</param>
        /// <param name="username">Nombre de usuario para la autenticación en el equipo remoto.</param>
        /// <param name="password">Contraseña correspondiente al usuario.</param>
        public Ssh(String host, String username, String password)
        {
            try
            {
                // Tiempo inicial del establecimiento del enlace
                DateTime initTime = DateTime.Now;

                this.Host = host;
                this.Username = username;
                this._password = password;
                this._session = new SshClient(this.Host, this.Username, this._password);

                if (!ConnectionTCP.IsAvaibleIP(this.Host))
                    throw new IOException(String.Format("No hay comunicación con el host {0}", this.Host));

                this._session.Connect();
                Trace.WriteLine(String.Format("Conectado al host {0}, Tiempo: {1}", Host, DateTime.Now - initTime), "INFO");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message, "ERROR");
            }
        }

        /// <summary>
        /// Desctructor de la instancia.
        /// </summary>
        ~Ssh() 
		{
            this.Dispose();
		}
		
        /// <summary>
        /// Obtiene el resultado de un error al ejecutar un comando.
        /// </summary>
        public String ErrorResult { get; private set; }

        /// <summary>
        /// Obtiene el nombre o la ip del equipo remoto.
        /// </summary>
        public String Host { get; private set; }

        /// <summary>
        /// Indica si hay error después de la ejecución del ultimo comando envíado.
        /// </summary>
        public Boolean ThereError { get; private set; }

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
        /// Indica si se logró la comunicación al equipo remoto.
        /// </summary>
        /// <returns>Un valor verdadero si se estableció la comunicación.</returns>
        public Boolean IsConnected() => this._session.IsConnected;

        /// <summary>
        /// Envía y ejecuta un comando en la terminal del equipo remoto.
        /// </summary>
        /// <param name="command">Comando a ejecutar en el equipo remoto.</param>
        /// <returns>Respuesta de la terminal del equipo remoto al de ejecutar el comando.</returns>
        public String SendCommand(String command, Boolean clearHistory = true)
        {
            // Tiempo inicial del envío de comando
            DateTime initTime = DateTime.Now;

            Trace.WriteLine(String.Format("Enviando el comando al host {0}", Host), "INFO");

            // Ejecución del comando en el equipo remoto
            var sshCommand = this._session.CreateCommand(command);
            sshCommand.Execute();

            Trace.WriteLine(String.Format("Tiempo de espera de la respuesta: {0}", DateTime.Now - initTime), "DEBUG");

            // Obtenemos todos los resultados de la ejecución del comando
            String response = sshCommand.Result;

            ThereError = sshCommand.ExitStatus != 0;

            ErrorResult = ThereError ? sshCommand.Error : String.Empty;

            // Devolvemos la respuesta del comando ejecutado
            return response;
        }
    }
}