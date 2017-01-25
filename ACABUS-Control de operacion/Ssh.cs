using System;
using System.IO;
using System.Text;
using System.Threading;
using Tamir.SharpSsh;

namespace ACABUS_Control_de_operacion {
    /// <summary>
    /// Esta clase permite el envío de comandos a un equipo remoto que tenga 
    /// el servicio de SSH.
    /// </summary>
    public class Ssh : IDisposable {
        /// <summary>
        /// Obtiene el nombre o la ip del equipo remoto.
        /// </summary>
        public String Host { get; private set; }

        /// <summary>
        /// Obtiene el nombre de usuario para autenticarse en el equipo remoto.
        /// </summary>
        public String Username { get; private set; }

        /// <summary>
        /// Obtiene o establece el tiempo de espera de la lectura.
        /// </summary>
        public int TimeOut { get; set; }

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
        public Ssh(String host, String username, String password) {
            try {
                this.Host = host;
                this.Username = username;
                this._password = password;
                this.TimeOut = 2000;
                this._session = new SshShell(this.Host, this.Username);
                this._session.Password = this._password;
                this._session.Connect();
                this._session.RemoveTerminalEmulationCharacters = true;
                this._connected = this._session.Connected;
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// Indica si se logró la comunicación al equipo remoto.
        /// </summary>
        /// <returns>Un valor verdadero si se estableció la comunicación.</returns>
        public Boolean IsConnected() {
            return this._connected;
        }

        /// <summary>
        /// Envía y ejecuta un comando en la terminal del equipo remoto.
        /// </summary>
        /// <param name="command">Comando a ejecutar en el equipo remoto.</param>
        /// <returns>Respuesta de la terminal del equipo remoto al de ejecutar el comando.</returns>
        public String SendCommand(String command) {

            /// TODO Por corregir proceso de envío de comandos y lectura de respuesta

            String buffer;
            int nbytes = 0;
            StringBuilder responseBuilder = new StringBuilder();

            try {
                nbytes = readResponse(out buffer);
                if (nbytes < 0)
                    throw new Exception("Error al leer desde buffer");

                command = command.Insert(0, "echo -n '$I$'; ");
                command = command + " ; echo -n '$F$' \n";
                nbytes = command.Length;

                this._session.Write(command);

                while (!responseBuilder.ToString().Contains("$F$") && this._session.ShellConnected
                    && this._session.ShellOpened) {
                    nbytes = readResponse(out buffer);
                    if (nbytes < 0)
                        throw new Exception("Error al leer desde buffer");
                    if (nbytes > 0) {
                        buffer = buffer.Substring(buffer.IndexOf(command) + command.Length);
                        responseBuilder.Append(buffer);
                    }
                }
            }
            catch (Exception ex) {
                throw ex;
            }
            String response = responseBuilder.ToString();
            if (response.Length > 0) {
                response = response.Substring(response.IndexOf("$I$") + 3);
                response = response.Substring(0, response.IndexOf("\r\n$F$"));
            }
            Console.WriteLine(String.Format("{0}: Listo", this.Host));
            return response;
        }

        /// <summary>
        /// Realiza la lectura del enlace del SSH con un tiempo limite definido por la propiedad
        /// TimeOut. Cuando se agota el tiempo este devuelve lo obtenido.
        /// </summary>
        /// <param name="response">Varible donde se devolverá la respuesta.</param>
        /// <returns>El número de bytes leidos.</returns>
        private int readResponse(out String response) {

            /// Corregir logica de lectura de respuesta del flujo SSH

            StringBuilder responseBuilder = new StringBuilder();
            int timer = 0;
            Thread threadSshRead = new Thread(() => {
                bool receive = false;
                while (true) {
                    Byte[] buffer = new Byte[1024];
                    int nbytes = 0;
                    nbytes = this._session.IO.Read(buffer, 0, buffer.Length);
                    if (nbytes > 0) {
                        receive = true;
                        responseBuilder.Append(Encoding.UTF8.GetString(buffer, 0, nbytes));
                        timer = 0;
                    }
                    if (nbytes < 0 || (nbytes == 0 && receive)) {
                        timer = TimeOut;
                        return;
                    }
                }
            });
            threadSshRead.Start();
            while (timer < TimeOut) {
                Thread.Sleep(1);
                timer++;
            }
            threadSshRead.Abort();
            response = responseBuilder.ToString();
            return response.Length;
        }

        /// <summary>
        /// Libera la conexión al equipo remoto.
        /// </summary>
        public void Dispose() {
            this._session.Close();
        }
    }
}
