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
        /// Obtiene el tamaño del buffer de datos.
        /// </summary>
        private const int BUFFER_SIZE = 1024;

        /// <summary>
        /// Patrón de inicio de la respuesta.
        /// </summary>
        private const string PATTERN_I = "<<i<<";

        /// <summary>
        /// Patrón del final de la respuesta.
        /// </summary>
        private const string PATTERN_F = ">>f>>";

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
                this.TimeOut = 600;
                this._session = new SshShell(this.Host, this.Username)
                {
                    Password = this._password
                };
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

            // Variable local para obtener la respuesta del flujo
            String response;

            // Variable local que indica el tamaño de la respuesta
            int responseSize = 0;

            // Variable local donde construiremos la respuesta obtenida del comando
            StringBuilder responseBuilder = new StringBuilder();

            // Variable local donde llevaremos el numero de intentos
            int time = 0;

            // Número máximo de intentos
            int timeMax = 5;

            // Intentamos leer los datos que tenga el flujo actualmente para descartarlos
            responseSize = ReadResponse(out response);

            // Preparamos el comando para ejecutar una salida limpia
            command = command.Insert(0, String.Format("echo -n '{0}' ; ", PATTERN_I));
            command = String.Format("{0}; echo -n '{1}'", command, PATTERN_F);

            // Escribimos el comando a ejecutar en el equipo remoto
            this._session.WriteLine(command);

            while (time < timeMax && _session.ShellConnected && _session.ShellOpened) {

                // Intentamos leer
                responseSize = ReadResponse(out response);
                // Si el tamaño de la respuesta es cero, intentamos leer de nuevo
                if (responseSize == 0) {
                    time++;
                    continue;
                }
                // Si es mayor a cero,  
                if (responseSize > 0) {
                    // Construimos la respuesta
                    responseBuilder.Append(response);

                    // Si ya leimos el final de la respuesta, terminamos los intentos
                    if (response.Contains(PATTERN_F) && responseBuilder.Length > command.Length)
                        break;
                }
            }


            response = responseBuilder.ToString();

            // Removemos el comando escrito en el buffer de ser necesario
            response = response.Contains(String.Format("'{0}'", PATTERN_F))
                ? response.Substring(response.IndexOf(String.Format("'{0}'", PATTERN_F)) + PATTERN_F.Length + 2)
                : response;

            if (response.Contains(PATTERN_F)) {
                response = response.Substring(response.LastIndexOf(PATTERN_I) + PATTERN_I.Length);
                response = response.Contains(String.Format("\r\n{0}", PATTERN_F)) ?
                    response.Substring(0, response.LastIndexOf("\r\n" + PATTERN_F))
                    : response.Substring(0, response.LastIndexOf(PATTERN_F));
            }
            return response;
        }

        /// <summary>
        /// Intenta la lectura del flujo de datos SSH antes que se agote el tiempo de espera
        /// definido por la propiedad TimeOut.
        /// </summary>
        /// <param name="response">Varible donde se devolverá la respuesta.</param>
        /// <returns>El número de bytes leidos.</returns>
        private int ReadResponse(out String response) {
            // Indica si ocurrió un error en la lectura
            bool isError = false;

            // Variable local donde construiremos la respuesta obtenida del flujo de datos SSH 
            StringBuilder responseBuilder = new StringBuilder();

            // Variable local que controla el temporizador de espera de lectura
            int timer = 0;

            // Declaración e implementación del hilo para la lectura del flujo de datos SSH
            Thread threadSshRead = new Thread(() => {

                // Flujo de datos del SSH
                Stream sshStream = this._session.IO;

                // Buffer bytes
                byte[] buffer = new byte[BUFFER_SIZE];

                // Tamaño de la respuesta en bytes
                int responseSize = 0;

                // Indica si ya ha recibido datos
                bool receive = false;

                // Comienza el intento de lectura del flujo de datos
                while (true) {
                    timer = 0; // Iniciamos el temporizador en 0
                    responseSize = sshStream.Read(buffer, 0, BUFFER_SIZE); // Intentamos leer
                    if (responseSize < 0) { // Si es menor a cero el tamaño de la respuesta 
                        // timer = TimeOut;
                        isError = true;
                        return; // Terminamos la lectura
                    }
                    if (responseSize > 0) { // Si es mayor a cero
                        receive = true; // Marcamos que ya estamos recibiendo datos
                        responseBuilder.Append(Encoding.UTF8.GetString(buffer, 0, responseSize)); // Añadimos a la respuesta
                        continue; // Continuamos en el bucle para intentar leer más de nuevo
                    }
                    if (responseSize == 0 && receive) // Si ya recibimos datos anteriormente pero esta ocasion no
                        break; // Terminamos los intentos de leer más datos
                }

            });

            // Ejecución del hilo de lectura del flujo de datos de SSH
            threadSshRead.Start();

            // Ejecutamos un temporizador que se reinicia cada vez que se lee de nuevo
            // Si la variable local se vuelve mayor que el tiempo de espera, termina el temporizador
            while (timer < TimeOut && threadSshRead.IsAlive) {
                Thread.Sleep(1); // Esperamos un milisegundo
                timer++; // Incrementamos el tiempo del temporizador en 1ms
            }
            // Al terminar el tiempo detenemos el hilo para evitar la espera infinita
            threadSshRead.Abort();

            // Si el tamaño de la respuesta es menor a cero, lanzamos una excepción.
            if (isError)
                throw new IOException(String.Format("Error: Al leer el flujo de datos SSH, Host: {0}", this.Host)); // Lanzamos una excepcion de E/S

            // Elegimos como respuesta todo lo que se recuperó del flujo de datos
            response = responseBuilder.ToString();

            // Devolvemos la respuesta
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
