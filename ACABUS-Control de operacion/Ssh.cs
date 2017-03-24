using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
        private const int _BUFFER_SIZE = 2048;

        /// <summary>
        /// Patrón de inicio de la respuesta.
        /// </summary>
        private const string _BEGIN_RESPONSE_PATTERN = "\\<\\<i\\<\\<";

        /// <summary>
        /// Patrón del final de la respuesta.
        /// </summary>
        private const string _END_RESPONSE_PATTERN = "\\>\\>f\\>\\>";

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
                this.TimeOut = 600;
                this._session = new SshShell(this.Host, this.Username)
                {
                    Password = this._password
                };
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
            // Variable local que indica el tamaño de la respuesta
            int responseSize = 0;

            string beginResponse = _BEGIN_RESPONSE_PATTERN.Replace("\\", "");
            string endResponse = _END_RESPONSE_PATTERN.Replace("\\", "");
            this._session.WriteLine(string.Format("\n\n\necho -n '{0}'; echo -n '{1}'\n", beginResponse, endResponse));


            // Intentamos leer los datos que tenga el flujo actualmente para descartarlos
            responseSize = ReadResponse(out String response);

            // Preparamos el comando para ejecutar una salida limpia
            command = command.Insert(0, string.Format("echo -n '{0}' ; ", beginResponse));
            command = string.Format("{0}; echo -n '{1}'; history -c", command, endResponse);

            // Escribimos el comando a ejecutar en el equipo remoto
            this._session.WriteLine(command);

            // Intentamos leer
            responseSize = ReadResponse(out response);
            
            // Removemos el comando escrito en el buffer de ser necesario
            response = ProcessReponse(response);            

            // Devolvemos la respuesta del comando pasado por argumento a esta función
            return response;
        }

        /// <summary>
        /// Procesa la salida del programa para obtener el resultado real correspondiente al comando
        /// ejecutado en el host remoto.
        /// </summary>
        /// <param name="result">Resultado a procesar</param>
        /// <returns>Resultado procesado</returns>
        private string ProcessReponse(string result)
        {
            // Preparamos el patrón utilizado para extraer solo la cadena de respuesta
            String regex = String.Format("{0}([^'])(.|\r\n|\n){{0,}}([^']){1}", _BEGIN_RESPONSE_PATTERN, _END_RESPONSE_PATTERN);

            // Extraemos la cadena de respuesta
            result = Regex.Match(result, regex).Value;

            // Eliminamos los patrones de la cadena de respuesta y poder obtener el valor real
            return new Regex(String.Format("{0}|{1}", _BEGIN_RESPONSE_PATTERN, _END_RESPONSE_PATTERN)).Replace(result, "");
        }

        /// <summary>
        /// Intenta la lectura del flujo de datos SSH antes que se agote el tiempo de espera
        /// definido por la propiedad TimeOut.
        /// </summary>
        /// <param name="response">Varible donde se devolverá la respuesta.</param>
        /// <returns>El número de bytes leidos.</returns>
        private int ReadResponse(out String response)
        {
            // Indica si ocurrió un error en la lectura
            bool isError = false;

            // Variable local donde construiremos la respuesta obtenida del flujo de datos SSH 
            StringBuilder responseBuilder = new StringBuilder();

            // Variable local que controla el temporizador de espera de lectura
            int timer = 0;

            // Declaración e implementación del hilo para la lectura del flujo de datos SSH
            Thread threadSshRead = new Thread(() =>
            {
                // Flujo de datos del SSH
                Stream sshStream = this._session.IO;

                // Buffer bytes
                byte[] buffer = new byte[_BUFFER_SIZE];

                // Tamaño de la respuesta en bytes
                int responseSize = 0;

                // Comienza el intento de lectura del flujo de datos
                while (this._session.ShellConnected && this._session.ShellOpened)
                {
                    timer = 0; // Iniciamos el temporizador en 0

                    try
                    {
                        responseSize = sshStream.Read(buffer, 0, _BUFFER_SIZE); // Intentamos leer
                    }
                    catch (ThreadInterruptedException)
                    {
                        Trace.WriteLine("Comunicación con host se detuvo por tiempo agotado");
                    }

                    if (responseSize < 0)
                    {
                        // Si es menor a cero el tamaño de la respuesta 
                        isError = true;
                        break; // Terminamos la lectura
                    }
                    if (responseSize > 0)
                    { // Si es mayor a cero
                        responseBuilder.Append(Encoding.UTF8.GetString(buffer, 0, responseSize)); // Añadimos a la respuesta
                    }
                    String regex = String.Format("echo\\s-n\\s'{0}'(.|\r\n|\n){{0,}}{0}", _END_RESPONSE_PATTERN);
                    if (Regex.IsMatch(responseBuilder.ToString(), regex))
                        break; // Terminamos los intentos de leer más datos
                }
            });

            // Ejecución del hilo de lectura del flujo de datos de SSH
            threadSshRead.Start();

            // Ejecutamos un temporizador que se reinicia cada vez que se lee de nuevo
            // Si la variable local se vuelve mayor que el tiempo de espera, termina el temporizador
            while (timer < TimeOut && threadSshRead.IsAlive)
            {
                Thread.Sleep(1); // Esperamos un milisegundo
                timer++; // Incrementamos el tiempo del temporizador en 1ms
            }

            if (threadSshRead.IsAlive)
            {
                // Al terminar el tiempo detenemos el hilo para evitar la espera infinita
                threadSshRead.Interrupt();
                threadSshRead.Abort();
            }

            // Si el tamaño de la respuesta es menor a cero, lanzamos una excepción.
            if (isError)
                throw new IOException(String.Format("Error: Al leer el flujo de datos SSH, Host: {0}", this.Host)); // Lanzamos una excepcion de E/S

            // Si el tiempo de espera de la respuesta se agota, lanzamos una excepción
            if (timer >= TimeOut)
                throw new IOException(String.Format("Error: Se agoto el tiempo de espera, Host: {0}", this.Host)); // Lanzamos una excepcion de E/S

            // Elegimos como respuesta todo lo que se recuperó del flujo de datos
            response = responseBuilder.ToString();

            // Devolvemos la respuesta
            return response.Length;
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
