using System;
using System.Collections.Generic;
using System.Xml;

namespace ACABUS_Control_de_operacion {
    /// <summary>
    /// Esta es una estructura que define los atributos de una ruta troncal,
    /// además de gestionar la lista de troncales.
    /// </summary>
    public class Trunk {
        #region StaticFunctions
        /// <summary>
        /// Archivo de configuración de las rutas.
        /// </summary>
        private static String FILE_NAME_CONFIG_XML = "XMLConfig.xml";

        /// <summary>
        /// Instancia que pertenece al documento XML.
        /// </summary>
        private static XmlDocument xmlConfig = null;

        /// <summary>
        /// Lista de rutas troncales leidas desde el archivo XML.
        /// </summary>
        private static List<Trunk> trunks;

        /// <summary>
        /// Obtiene la lista de rutas troncales.
        /// </summary>
        public static List<Trunk> Trunks {
            get {
                if (trunks == null)
                    trunks = new List<Trunk>();
                return trunks;
            }
        }

        /// <summary>
        /// Carga la configuración del XML en Trunk.Trunks.
        /// </summary>
        public static void LoadConfiguration() {
            Trunks.Clear();
            xmlConfig = new XmlDocument();
            xmlConfig.Load(FILE_NAME_CONFIG_XML);
            foreach (XmlNode trunk in xmlConfig.SelectSingleNode("Trunks")) {
                if (!trunk.Name.Equals("Trunk"))
                    continue;
                var trunkTemp = Trunk.ToTrunk(trunk) as Trunk;
                trunkTemp.LoadStations(trunk.ChildNodes);
                Trunks.Add(trunkTemp);
            }
        }

        /// <summary>
        /// Convierte un nodo XML con la estructura correspondiente a un
        /// elemento de ruta troncal a una instancia Trunk.
        /// </summary>
        /// <param name="trunk">Nodo XML a parsear.</param>
        /// <returns>Una instancia de ruta troncal correspondiente al nodo
        /// pasado como argumento.</returns>
        public static Trunk ToTrunk(XmlNode trunk) {
            Trunk trunkTemp = new Trunk();
            trunkTemp.ID = Int32.Parse(trunk.Attributes["id"].Value);
            return trunkTemp;
        }
        #endregion

        /// <summary>
        /// Obtiene el ID de la ruta troncal.
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// Obtiene la lista de estaciones que pertenencen a la ruta.
        /// </summary>
        public List<Station> Stations { get; private set; }

        /// <summary>
        /// Crea una instancia de una ruta troncal.
        /// </summary>
        public Trunk() {
            Stations = new List<Station>();
        }

        /// <summary>
        /// Carga las estaciones a partir de una lista de nodos
        /// XML.
        /// </summary>
        /// <param name="childNodes">Lista de nodos XML que representan
        /// una estación cada uno.</param>
        public void LoadStations(XmlNodeList childNodes) {
            Stations.Clear();
            foreach (XmlNode station in childNodes) {
                if (!station.Name.Equals("Station"))
                    continue;
                var stationTemp = Station.ToStation(station, this) as Station;
                stationTemp.LoadDevices(station.ChildNodes);
                Stations.Add(stationTemp);
            }
        }

        /// <summary>
        /// Representa en una cadena la instancia actual.
        /// </summary>
        /// <returns>Una cadena que representa una instancia Trunk.</returns>
        public new String ToString() {
            return String.Format("Ruta T{0}", ID.ToString("D2"));
        }

        /// <summary>
        /// Obtiene el número de dispositivos en la ruta troncal
        /// </summary>
        /// <returns>Número de dispositivos</returns>
        public int CountDevices() {
            int count = 0;
            foreach (Station station in Stations)
                foreach (Device device in station.Devices)
                    count++;
            return count;
        }
    }
}
