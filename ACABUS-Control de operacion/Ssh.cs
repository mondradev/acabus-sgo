using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
        /// Indica si el equipo remoto maneja algun sistema con una terminal basada en UNIX.
        /// </summary>
        public Boolean IsUnix { get; private set; }

        /// <summary>
        /// Obtiene o establece la clave del usuario para la autenticación en el equipo remoto.
        /// </summary>
        private String _password;

        /// <summary>
        /// Obtiene o establece la instancia de una conexión a el equipo remoto por SSH.
        /// </summary>
        private SshShell _session;

        /// <summary>
        /// Obtiene el tamaño del buffer de datos.
        /// </summary>
        private const int _BUFFER_SIZE = 4096;

        /// <summary>
        /// Patrón de inicio de la respuesta.
        /// </summary>
        private const String _BEGIN_RESPONSE_PATTERN = "\\<i\\>";

        /// <summary>
        /// Patrón del final de la respuesta.
        /// </summary>
        private const String _END_RESPONSE_PATTERN = "\\<f\\>";

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
                this._session = new SshShell(this.Host, this.Username)
                {
                    Password = this._password
                };
                this._session.Connect();
                this._session.RemoveTerminalEmulationCharacters = true;
                Trace.WriteLine(String.Format("Conectado al host {0}, Tiempo: {1}", Host, DateTime.Now - initTime), "INFO");
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
            return this._session.Connected && this._session.ShellConnected && this._session.ShellOpened;
        }

        /// <summary>
        /// Envía y ejecuta un comando en la terminal del equipo remoto.
        /// </summary>
        /// <param name="command">Comando a ejecutar en el equipo remoto.</param>
        /// <returns>Respuesta de la terminal del equipo remoto al de ejecutar el comando.</returns>
        public String SendCommand(String command, Boolean clearHistory = true)
        {
            // Tiempo inicial del envío de comando
            DateTime initTime = DateTime.Now;

            // Variable local que indica el tamaño de la respuesta
            int responseSize = 0;

            // Ejecutamos un simple eco para poder leer el resto de los caracteres devueltos por remoto.
            this._session.Write(PrepareCommand("echo ''"));

            // Intentamos leer los datos que tenga el flujo actualmente para descartarlos
            responseSize = ReadResponse(out String response);

            Trace.WriteLine(String.Format("Enviando el comando al host {0}", Host), "INFO");

            // Preparamos el comando para ejecutar una salida limpia
            command = PrepareCommand(command);

            // Escribimos el comando a ejecutar en el equipo remoto
            this._session.WriteLine(command);

            // Intentamos leer
            responseSize = ReadResponse(out response);

            // Removemos el comando escrito en el buffer de ser necesario
            response = ProcessReponse(response);

            Trace.WriteLine(String.Format("Tiempo de espera de la respuesta: {0}", DateTime.Now - initTime), "DEBUG");

            if (IsUnix && clearHistory)
            {
                Trace.WriteLine(String.Format("Limpiando historial de Unix: {0}: ", Host), "DEBUG");
                SendCommand("history -a;  history -r; cat .bash_history | grep -v '\\<i\\>' >> .bash_history.bkp; mv .bash_history.bkp .bash_history ; history -c", false);
            }

            // Devolvemos la respuesta del comando pasado por argumento a esta función
            return response;
        }

        /// <summary>
        /// Prepara el comando de forma que se puede interpretar posteriormente para identificar el
        /// inicio y final de la respuesta.
        /// </summary>
        /// <param name="command">Comando a preparar.</param>
        /// <returns>Un comando complementado de patrones.</returns>
        private String PrepareCommand(String command)
        {
            String beginResponse = _BEGIN_RESPONSE_PATTERN.Replace("\\", "");
            String endResponse = _END_RESPONSE_PATTERN.Replace("\\", "");
            String tmpCommand = command.Insert(0, String.Format("echo -n '{0}' ; ", beginResponse));
            tmpCommand = String.Format("{0}; echo -n '{1}'\n", tmpCommand, endResponse);
            return tmpCommand;
        }

        /// <summary>
        /// Procesa la salida del programa para obtener el resultado real correspondiente al comando
        /// ejecutado en el host remoto.
        /// </summary>
        /// <param name="result">Resultado a procesar</param>
        /// <returns>Resultado procesado</returns>
        private string ProcessReponse(string result)
        {
            // Intentamos identificar si la terminal es linux
            IsUnix = DetectUNIXShell(result);

            // Removemos el comando enviado y nos quedamos con la respuesta a tratar
            result = result.Substring(result.LastIndexOf(_BEGIN_RESPONSE_PATTERN.Replace("\\", "")));

            // Preparamos el patrón utilizado para extraer solo la cadena de respuesta
            String regex = String.Format("{0}([^'])(.|\r\n|\n){{0,}}([^']){1}", _BEGIN_RESPONSE_PATTERN, _END_RESPONSE_PATTERN);

            // Extraemos la cadena de respuesta
            result = Regex.Match(result, regex).Value;

            Trace.WriteLine(String.Format("El host {0} respondió: {1}", Host, result), "DEBUG");

            // Eliminamos los patrones de la cadena de respuesta y poder obtener el valor real
            return new Regex(String.Format("{0}|{1}", _BEGIN_RESPONSE_PATTERN, _END_RESPONSE_PATTERN)).Replace(result, "");
        }

        /// <summary>
        /// Determina si la teminal del equipo remoto es basada en UNIX.
        /// </summary>
        /// <param name="result">Respuesta el equipo remoto a evaluar.</param>
        /// <returns>Un valor verdadero si el equipo remoto es basado en UNIX.</returns>
        private Boolean DetectUNIXShell(string result)
        {
            return !String.IsNullOrEmpty(Regex.Match(result, ".*\\@.*(\\#|\\~)").Value);
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

            // Flujo de datos del SSH
            Stream sshStream = this._session.IO;

            // Buffer bytes
            byte[] buffer = new byte[_BUFFER_SIZE];

            // Tamaño de la respuesta en bytes
            int responseSize = 0;

            // Comienza el intento de lectura del flujo de datos
            while (this.IsConnected())
            {
                responseSize = sshStream.Read(buffer, 0, _BUFFER_SIZE); // Intentamos leer

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
                String regex = String.Format("{0}([^'])(.|\r\n|\n){{0,}}([^']){1}", _BEGIN_RESPONSE_PATTERN, _END_RESPONSE_PATTERN);
                if (Regex.IsMatch(responseBuilder.ToString(), regex) || Regex.IsMatch(responseBuilder.ToString(), "\\<i\\>\\<f\\>"))
                    break; // Terminamos los intentos de leer más datos
            }

            // Si el tamaño de la respuesta es menor a cero, lanzamos una excepción.
            if (isError)
                throw new IOException(String.Format("Error: Al leer el flujo de datos SSH, Host: {0}", this.Host)); // Lanzamos una excepcion de E/S

            // Elegimos como respuesta todo lo que se recuperó del flujo de datos
            response = responseBuilder.ToString();

            // Devolvemos la respuesta
            return response.Length;
        }

        private void ClearHistoryInLinux()
        {

        }

        /// <summary>
        /// Desctructor de la instancia.
        /// </summary>
        ~Ssh()
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
