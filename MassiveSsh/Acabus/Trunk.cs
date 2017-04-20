using MassiveSsh.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace MassiveSsh.Acabus
{
    /// <summary>
    /// Esta es una estructura que define los atributos de una ruta troncal,
    /// además de gestionar la lista de troncales.
    /// </summary>
    public class Trunk
    {
        #region StaticFunctions

        /// <summary>
        /// Convierte un nodo XML con la estructura correspondiente a un
        /// elemento de ruta troncal a una instancia Trunk.
        /// </summary>
        /// <param name="trunkNode">Nodo XML a parsear.</param>
        /// <returns>Una instancia de ruta troncal correspondiente al nodo
        /// pasado como argumento.</returns>
        public static Trunk ToTrunk(XmlNode trunkNode)
        {
            try
            {
                Trunk trunkTemp = new Trunk()
                {
                    ID = Int16.Parse(XmlUtils.GetAttribute(trunkNode, "ID")),
                    Name = XmlUtils.GetAttribute(trunkNode, "Name")
                };
                return trunkTemp;
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is ArgumentNullException)
                    Trace.WriteLine("Un nodo 'Trunk' debe tener un ID ", "ERROR");

            }
            return null;
        }
        #endregion

        /// <summary>
        /// Una lista de todas las estaciones asignadas a esta ruta
        /// </summary>
        public List<Station> Stations { get; set; }

        /// <summary>
        /// Obtiene el ID de la ruta troncal.
        /// </summary>
        public Int16 ID { get; set; }

        /// <summary>
        /// Nombre de la ruta
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// Crea una instancia de una ruta troncal.
        /// </summary>
        public Trunk()
        {
            Stations = new List<Station>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="station"></param>
        public void AddStation(Station station)
        {
            Stations.Add(station);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idStation"></param>
        /// <returns></returns>
        public Station GetStation(Int16 idStation)
        {
            foreach (var station in Stations)
                if (station.ID == idStation) return station;
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="station"></param>
        public void RemoveStation(Station station)
        {
            Stations.Remove(station);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Int16 StationCount()
        {
            return (Int16)(Stations.Count);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Station[] GetStations()
        {
            return Stations.ToArray();
        }

        /// <summary>
        /// Representa en una cadena la instancia actual.
        /// </summary>
        /// <returns>Una cadena que representa una instancia Trunk.</returns>
        public new String ToString()
        {
            return String.Format("Ruta T{0} - {1}", ID.ToString("D2"), Name);
        }

        /// <summary>
        /// Obtiene el número de dispositivos en la ruta troncal
        /// </summary>
        /// <returns>Número de dispositivos</returns>
        public int CountDevices()
        {
            int count = 0;
            foreach (Station station in GetStations())
                foreach (Device device in station.GetDevices())
                    count++;
            return count;
        }
    }
}
