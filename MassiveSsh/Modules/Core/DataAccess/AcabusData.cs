using Acabus.Models;
using Acabus.Modules.CctvReports.Models;
using Acabus.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Acabus.Modules.Core.DataAccess
{
    public static partial class AcabusData
    {
        /// <summary>
        /// Campo que provee a la propiedad 'AllCashDestinies'.
        /// </summary>
        private static IEnumerable<CashDestiny> _allCashDestinies;

        /// <summary>
        /// Campo que provee a la propiedad 'AllFaults'.
        /// </summary>
        private static IEnumerable<DeviceFault> _allFaults;

        /// <summary>
        /// Campo que provee a la propiedad 'Routes'.
        /// </summary>
        private static IEnumerable<Route> _allRoutes;

        /// <summary>
        /// Campo que provee a la propiedad 'CC'.
        /// </summary>
        private static Station _cc;

        static AcabusData()
        {
            LoadFromDatabase();
        }

        /// <summary>
        /// Obtiene el valor de esta propiedad.
        /// </summary>
        public static IEnumerable<CashDestiny> AllCashDestinies => _allCashDestinies;

        /// <summary>
        /// Obtiene una lista de todos los dispositivos registrados en la base de datos.
        /// </summary>
        public static IEnumerable<Device> AllDevices => Util.Combine(new[]
        {
            AllStations.Select(station=> station.Devices).Combine(),
            AllVehicles.Select(vehicle=>vehicle.Devices).Combine()
        });

        /// <summary>
        /// Obtiene una lista de las fallas para los diferentes equipos.
        /// </summary>
        public static IEnumerable<DeviceFault> AllFaults => _allFaults;

        /// <summary>
        /// Obtiene una lista de las rutas registradas en la base de datos.
        /// </summary>
        public static IEnumerable<Route> AllRoutes => _allRoutes;

        /// <summary>
        /// Obtiene una lista de todos las estaciones registrados en la base de datos.
        /// </summary>
        public static IEnumerable<Station> AllStations => AllRoutes.Select(route => route.Stations).Combine();

        /// <summary>
        /// Obtiene una lista de todos los vehículos registrados en la base de datos.
        /// </summary>
        public static IEnumerable<Vehicle> AllVehicles => AllRoutes.Select(route => route.Vehicles)
            .Combine().OrderBy(vehicle => vehicle.EconomicNumber);

        /// <summary>
        /// Obtiene o establece una instancia de estación que representa al centro de control.
        /// </summary>
        public static Station CC => _cc;

        /// <summary>
        /// Recarga los catálogos de la base de datos.
        /// </summary>
        public static void ReloadData() => LoadFromDatabase();

        /// <summary>
        /// Campo que provee a la propiedad 'AllTechnicians'.
        /// </summary>
        private static IEnumerable<Technician> _allTechnicians;

        /// <summary>
        /// Obtiene la lista de todos los técnicos registrados.
        /// </summary>
        public static IEnumerable<Technician> AllTechnicians => _allTechnicians;

        /// <summary>
        /// Realiza una consulta a la base de datos de la aplicación y obtiene los catálogos.
        /// </summary>
        private static void LoadFromDatabase()
        {
            _allRoutes = Acabus.DataAccess.AcabusData.Session.GetObjects<Route>();
            _allFaults = Acabus.DataAccess.AcabusData.Session.GetObjects<DeviceFault>();
            _allCashDestinies = Acabus.DataAccess.AcabusData.Session.GetObjects<CashDestiny>();
            _allTechnicians = Acabus.DataAccess.AcabusData.Session.GetObjects<Technician>()
                                .OrderBy(technician => technician.Name);

            _cc = AllStations.FirstOrDefault(station =>
                station.Devices.FirstOrDefault(device =>
                    device.Type == DeviceType.DB) != null);
        }
    }
}