using System;
using System.IO;
using System.Text;
using System.Threading;
using Tamir.SharpSsh;

namespace ACABUS_Control_de_operacion
{
    /// <summary>
    /// Esta clase permite el envío de comandos a un equipo remoto que tenga 
    /// el servicio de SSH.
    /// </summary>
    public class Ssh : IDisposable
    {
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
        /// Obtiene o establece la instancia de una conexión a el equipo remoto por SSH.
        /// </summary>
        private SshShell _session;

        /// <summary>
        /// Indica si se estableció la conexión al equipo remoto.
        /// </summary>
        private bool _connected;

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
                this.Host = host;
                this.Username = username;
                this._password = password;
                this._session = new SshShell(this.Host, this.Username);
                this._session.Password = this._password;
                this._session.Connect();
                this._session.RemoveTerminalEmulationCharacters = true;
                this._connected = this._session.Connected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Indica si se logró la comunicación al equipo remoto.
        /// </summary>
        /// <returns>Un valor verdadero si se estableció la comunicación.</returns>
        public Boolean IsConnected()
        {
            return this._connected;
        }

        /// <summary>
        /// Envía y ejecuta un comando en la terminal del equipo remoto.
        /// </summary>
        /// <param name="command">Comando a ejecutar en el equipo remoto.</param>
        /// <returns>Respuesta de la terminal del equipo remoto al de ejecutar el comando.</returns>
        public String SendCommand(String command)
        {
            Stream sshStream = this._session.GetStream();
            Byte[] buffer = new Byte[256];
            int nbytes = 0;
            String response = "";
            Boolean receive = false;
            Boolean ready = false;
            try
            {
                while (this._session.ShellOpened && this._session.ShellConnected)
                {
                    nbytes = sshStream.Read(buffer, 0, buffer.Length);
                    if (nbytes < 0) throw new Exception("Error al leer desde buffer");
                    if (nbytes > 0) receive = true;
                    if (!ready && receive && nbytes == 0)
                    {
                        ready = true;
                        break;
                    }
                    Thread.Sleep(50);
                }

                command.Insert(0, "echo -n '$I$'; ");
                command = command + " ; echo -n '$F$' \n";
                nbytes = command.Length;

                if (nbytes > 0 && ready)
                {
                    this._session.WriteLine(command);
                }
                while (this._session.ShellOpened && this._session.ShellConnected)
                {
                    nbytes = sshStream.Read(buffer, 0, buffer.Length);
                    if (nbytes < 0) throw new Exception("Error al leer desde buffer");
                    if (nbytes > 0)
                    {
                        response = response + Encoding.UTF8.GetString(buffer);
                        if (response.IndexOf("$F$") >= 0) break;
                    }
                    Thread.Sleep(50);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        /// <summary>
        /// Libera la conexión al equipo remoto.
        /// </summary>
        public void Dispose()
        {
            this._session.Close();
        }
    }
}
