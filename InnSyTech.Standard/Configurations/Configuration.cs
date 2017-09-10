using InnSyTech.Standard.Utils;
using System;
using System.Diagnostics;
using System.Threading;
using System.Xml;

namespace InnSyTech.Standard.Configuration
{
    /// <summary>
    /// Representa un controlador de configuración que permite tener un archivo donde se especifiquen
    /// los cambios. Cuando se realiza un cambio en el archivo inmediatamente es reflejado en la
    /// aplicación, en caso no ser correcto el archivo será ignorado y se mantendrá la versión
    /// cargada en memoría. Si el archivo es incorrecto al momento de iniciar la aplicación, este
    /// será ignorado y no podrán ser aplicadas por lo cual la aplicación puede no funcionar.
    /// </summary>
    public sealed class Configuration
    {
        /// <summary>
        /// Especifica el estado de lectura del archivo de configuración.
        /// </summary>
        private ConfigState _state;

        /// <summary>
        /// Instancia de documento Xml que controla al archivo de configuración.
        /// </summary>
        private XmlDocument _xmlDoc;

        /// <summary>
        /// Especifica el estado de E/S del archivo de configuración.
        /// </summary>
        private enum ConfigState
        {
            /// <summary>
            /// Listo.
            /// </summary>
            DONE = 0,

            /// <summary>
            /// En lectura.
            /// </summary>
            IN_READ = 1,

            /// <summary>
            /// En escritura.
            /// </summary>
            IN_WRITE = 2
        }

        /// <summary>
        /// Obtiene o establece la ruta del archivo de configuración.
        /// </summary>
        public String Filename { get; set; }

        /// <summary>
        /// Obtiene la configuración especificada, esto es un acceso directo a <see cref=" Read(string)"/>.
        /// </summary>
        public ISetting this[String name] => Read(name);

        /// <summary>
        /// Convierte el nodo Xml en una instancia <see cref="ISetting"/>
        /// </summary>
        public ISetting Convert(XmlNode node)
            => new Setting(node);

        /// <summary>
        /// Crea una configuración en el archivo, indicando su nombre y su valor de configuración.
        /// </summary>
        /// <param name="name">Nombre de la configuración a crear.</param>
        /// <param name="setting">Instancia de configuración a crear.</param>
        /// <returns>Un valor true si se agrego la configuración correctamente.</returns>
        public bool Create(String name, ISetting setting)
        {
            lock (this)
            {
                try
                {
                    if (_state != ConfigState.DONE)
                        Monitor.Wait(this);

                    _state = ConfigState.IN_WRITE;

                    LoadFile();

                    var node = SearchNode(name);

                    if (node != null)
                        throw new ArgumentException($"Ya existe una configuración con el mismo nombre '{name}'", nameof(name));

                    node = ToNode(setting);

                    node = _xmlDoc?.SelectSingleNode("configuration").AppendChild(node);

                    if (node == null)
                        throw new InvalidOperationException($"No se puede agregar la configuración debido a un problema con el archivo: {Filename}");

                    LoadFile();

                    return true;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message.JoinLines(), "ERROR");
                    return false;
                }
                finally
                {
                    _state = ConfigState.DONE;
                    Monitor.Pulse(this);
                }
            }
        }

        /// <summary>
        /// Elimina una configuración del archivo según el nombre especificado.
        /// </summary>
        public void Delete(String name)
        {
            lock (this)
            {
                if (_state != ConfigState.DONE)
                    Monitor.Wait(this);

                _state = ConfigState.IN_WRITE;

                LoadFile();

                var node = SearchNode(name);

                _xmlDoc.RemoveChild(node);
                _xmlDoc.Save(Filename);

                LoadFile();

                _state = ConfigState.DONE;

                Monitor.Pulse(this);
            }
        }

        /// <summary>
        /// Carga el archivo xml según la ruta especificada.
        /// </summary>
        public void LoadFile()
        {
            try
            {
                var xmlDoc = new XmlDocument();

                xmlDoc.Load(Filename);

                _xmlDoc = xmlDoc;
            }
            catch { }
        }

        /// <summary>
        /// Lee una configuración del archivo de configuración según el nombre especificado.
        /// </summary>
        /// <param name="name">El nombre de la configuración a leer.</param>
        /// <returns>La configuración a leida del archivo.</returns>
        public ISetting Read(String name)
        {
            lock (this)
            {
                if (_state != ConfigState.DONE)
                    Monitor.Wait(this);

                _state = ConfigState.IN_READ;

                LoadFile();

                var node = SearchNode(name);
                var setting = Convert(node);

                _state = ConfigState.DONE;

                Monitor.Pulse(this);

                return setting;
            }
        }

        /// <summary>
        /// Busca el nodo que contiene el nombre especificado.
        /// </summary>
        /// <param name="name">Nombre del nodo a buscar.</param>
        /// <returns>Nodo que coincide con el nombre especificado.</returns>
        public XmlNode SearchNode(String name)
            => _xmlDoc?.SelectSingleNode("configuration")?.SelectSingleNode(name);

        /// <summary>
        /// Actualiza los valores de una configuración en el archivo.
        /// </summary>
        /// <param name="name">Nombre de la configuración.</param>
        /// <param name="setting">Configuración a actualizar en el archivo.</param>
        /// <returns>Un valor true si la configuración fue actualizada.</returns>
        public bool Update(String name, ISetting setting)
        {
            lock (this)
            {
                try
                {
                    if (_state != ConfigState.DONE)
                        Monitor.Wait(this);

                    _state = ConfigState.IN_WRITE;

                    LoadFile();

                    var node = SearchNode(name);

                    if (node == null)
                        throw new ArgumentException($"No existe una configuración con el mismo nombre '{name}'", nameof(name));

                    node = _xmlDoc?.SelectSingleNode("configuration").RemoveChild(node);

                    if (node == null)
                        throw new InvalidOperationException($"No se puede actualizar la configuración debido a un problema con el archivo: {Filename}");

                    node = ToNode(setting);

                    node = _xmlDoc?.SelectSingleNode("configuration").AppendChild(node);
                    
                    LoadFile();

                    return true;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message.JoinLines(), "ERROR");
                    return false;
                }
                finally
                {
                    _state = ConfigState.DONE;
                    Monitor.Pulse(this);
                }
            }
        }

        /// <summary>
        /// Define la estructura básica del archivo de configuración.
        /// </summary>
        /// <returns>obtiene la instancia de documento Xml.</returns>
        private static XmlDocument CreateConfBase()
        {
            var doc = new XmlDocument();
            doc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
                            <configuration>
                            </configuration>
            ");
            return doc;
        }

        /// <summary>
        /// Convierte una instancia de <see cref="ISetting"/> en un nodo Xml.
        /// </summary>
        /// <param name="setting">Configuración a convertir en nodo Xml.</param>
        /// <returns>El nodo que representa la configuración.</returns>
        private XmlNode ToNode(ISetting setting)
        {
            throw new NotImplementedException();
        }
    }
}