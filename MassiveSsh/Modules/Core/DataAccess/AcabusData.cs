using Acabus.Models;
using Acabus.Modules.CctvReports.Models;
using Acabus.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Acabus.Modules.Core.DataAccess
{
    public static partial class AcabusData
    {
        /// <summary>
        /// Archivo donde se guarda la lista de unidades fuera de servicio.
        /// </summary>
        public static readonly String OFF_DUTY_VEHICLES_FILENAME = Path.Combine(Environment.CurrentDirectory, "Resources\\vehicles_{0:yyyyMMdd}.dat");

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
        /// Campo que provee a la propiedad 'AllTechnicians'.
        /// </summary>
        private static IEnumerable<Technician> _allTechnicians;

        /// <summary>
        /// Campo que provee a la propiedad 'CC'.
        /// </summary>
        private static Station _cc;

        /// <summary>
        /// Campo que provee a la propiedad 'OffDutyVehicles'.
        /// </summary>
        private static ObservableCollection<Vehicle> _offDutyVehicles;

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
        /// Obtiene la lista de todos los técnicos registrados.
        /// </summary>
        public static IEnumerable<Technician> AllTechnicians => _allTechnicians;

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
        /// Obtiene una lista de las unidades en taller o sin energía.
        /// </summary>
        public static ObservableCollection<Vehicle> OffDutyVehicles {
            get {
                if (_offDutyVehicles == null)
                    _offDutyVehicles = new ObservableCollection<Vehicle>();
                return _offDutyVehicles;
            }
        }

        /// <summary>
        /// Carga la lista de unidades fuera de servicio.
        /// </summary>
        public static void LoadOffDutyVehiclesSettings()
        {
            var filename = String.Format(OFF_DUTY_VEHICLES_FILENAME, DateTime.Now);

            OffDutyVehicles.Clear();
            try
            {
                if (!File.Exists(filename)) return;

                var lines = File.ReadAllLines(filename);

                foreach (var line in lines)
                {
                    var economicNumber = line.Split('|')?[0];
                    var status = line.Split('|')?[1];

                    var offDutyVehicle = AllVehicles.FirstOrDefault(vehicle => vehicle.EconomicNumber == economicNumber);
                    offDutyVehicle.Status = (VehicleStatus)Enum.Parse(typeof(VehicleStatus), status);

                    OffDutyVehicles.Add(offDutyVehicle);
                }
            }
            catch (IOException)
            {
                Trace.WriteLine("Ocurrió un error al intentar leer el archivo de la lista de vehículos.", "ERROR");
            }
        }

        /// <summary>
        /// Recarga los catálogos de la base de datos.
        /// </summary>
        public static void ReloadData() => LoadFromDatabase();

        /// <summary>
        /// Guarda toda la información de los vehículos en fuera de servicio.
        /// </summary>
        public static void SaveOffDutyVehiclesList()
        {
            var filename = String.Format(OFF_DUTY_VEHICLES_FILENAME, DateTime.Now);

            File.Delete(filename);
            try
            {
                foreach (Vehicle vehicle in OffDutyVehicles)
                    File.AppendAllText(filename, String.Format("{0}|{1}\n", vehicle.EconomicNumber, vehicle.Status));
            }
            catch (IOException)
            {
                Trace.WriteLine("Ocurrió un problema al intentar guardar la lista de vehículos.", "ERROR");
            }
        }

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

            _cc = AllStations.FirstOrDefault(station => station.Name.Contains("CENTRO DE CONTROL"));

            LoadOffDutyVehiclesSettings();
        }
    }
}