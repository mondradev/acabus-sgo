using InnSyTech.Standard.Database.Linq;
using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;
using Opera.Acabus.Server.Core.Utils;
using System;
using System.Linq;

namespace Opera.Acabus.Server.Gui
{
    /// <summary>
    /// Provee de funciones de consulta básica del sistema.
    /// </summary>
    public static class ServerCoreFunctions
    {
        /// <summary>
        /// Crea una ruta nueva.
        /// </summary>
        /// <param name="number">Numero de la ruta.</param>
        /// <param name="name">Nombre de la ruta.</param>
        /// <param name="type">Tipo de la ruta. ALIM = 1 o TRUNK = 2.</param>
        public static void CreateRoute([ParameterField(12)] UInt16 number,
            [ParameterField(17)] String name, [ParameterField(13)] RouteType type,
            [ParameterField(18)] String assignedSection, IMessage message)
        {
            Route route = new Route(0, number, type) { Name = name, AssignedSection = assignedSection };
            bool res = AcabusDataContext.DbContext.Create(route);

            message[22] = BitConverter.GetBytes(true);
        }

        /// <summary>
        /// Crea una estación nueva.
        /// </summary>
        /// <param name="number">Numero de la estación.</param>
        /// <param name="name">Nombre de la estación.</param>
        public static void CreateStation([ParameterField(12)] UInt16 number,
            [ParameterField(17)] String name, [ParameterField(18)] String assignedSection,
            [ParameterField(13)] UInt16 routeID, [ParameterField(23)] Boolean isExternal,
            IMessage message)
        {
            Station station = new Station(0, number)
            {
                Name = name,
                AssignedSection = assignedSection,
                IsExternal = isExternal,
                Route = AcabusDataContext.AllRoutes.FirstOrDefault(x => x.ID == routeID)
            };

            bool res = AcabusDataContext.DbContext.Create(station);

            message[22] = BitConverter.GetBytes(true);
        }

        /// <summary>
        /// Obtiene la ruta con el ID especificado.
        /// </summary>
        /// <param name="IDRoute">ID de la estación.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetRoute([ParameterField(12)] UInt16 IDRoute, IMessage message)
        {
            message[60] = AcabusDataContext.AllRoutes.FirstOrDefault(x => x.ID == IDRoute).Serialize();
        }

        /// <summary>
        /// Obtiene las rutas.
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetRoutes(IMessage message)
        {
            IQueryable<Route> routeQuery = AcabusDataContext.AllRoutes;

            Helpers.Enumerating(message, routeQuery.ToList().Count,
                  i => message[60] = routeQuery.ToList()[i].Serialize());
        }

        /// <summary>
        /// Obtiene la estación con el ID especificado.
        /// </summary>
        /// <param name="IDStation">ID de la estación.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetStation([ParameterField(12)] UInt16 IDStation, IMessage message)
        {
            message[60] = AcabusDataContext.AllStations.FirstOrDefault(x => x.ID == IDStation).Serialize();
        }


        /// <summary>
        /// Obtiene las estaciones.
        /// </summary>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetStations(IMessage message)
        {
            /***
                7, FieldType.Binary, 1, false, "Es enumerable"
                8, FieldType.Numeric, 100, true, "Registros totales del enumerable"
                9, FieldType.Numeric, 100, true, "Posición del enumerable"
                10, FieldType.Numeric, 1, false, "Operaciones del enumerable (Siguiente|Inicio)"
             */

            IQueryable<Station> stationsQuery = AcabusDataContext.AllStations;

            Helpers.Enumerating(message, stationsQuery.ToList().Count,
                i => message[60] = stationsQuery.ToList()[i].Serialize());
        }


        /// <summary>
        /// Obtiene las estaciones de la ruta especificada.
        /// </summary>
        /// <param name="IDRoute">ID de la ruta.</param>
        /// <param name="message">Mensaje de la petición.</param>
        public static void GetStations([ParameterField(12)] UInt16 IDRoute, IMessage message)
        {
            /***
                7, FieldType.Binary, 1, false, "Es enumerable"
                8, FieldType.Numeric, 100, true, "Registros totales del enumerable"
                9, FieldType.Numeric, 100, true, "Posición del enumerable"
                10, FieldType.Numeric, 1, false, "Operaciones del enumerable (Siguiente|Inicio)"
             */

            IQueryable<Station> stationsQuery = AcabusDataContext.AllStations
                .LoadReference(1).Where(x => x.Route.ID == IDRoute);

            Helpers.Enumerating(message, stationsQuery.ToList().Count,
                i => message[60] = stationsQuery.ToList()[i].Serialize());
        }
    }
}