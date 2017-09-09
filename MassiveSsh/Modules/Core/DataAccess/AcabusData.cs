using Acabus.Models;
using Acabus.Modules.CctvReports.Models;
using Acabus.Utils;
using InnSyTech.Standard.Database.Linq;
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

        private static IEnumerable<Device> _allDevices;

        private static IEnumerable<Vehicle> _allVehicles;

        private static IEnumerable<Station> _allStations;

        /// <summary>
        /// Obtiene el valor de esta propiedad.
        /// </summary>
        public static IEnumerable<CashDestiny> AllCashDestinies => _allCashDestinies;

        /// <summary>
        /// Obtiene una lista de todos los dispositivos registrados en la base de datos.
        /// </summary>
        public static IEnumerable<Device> AllDevices => _allDevices;

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
        public static IEnumerable<Station> AllStations => _allStations;

        /// <summary>
        /// Obtiene la lista de todos los técnicos registrados.
        /// </summary>
        public static IEnumerable<Technician> AllTechnicians => _allTechnicians;

        /// <summary>
        /// Obtiene una lista de todos los vehículos registrados en la base de datos.
        /// </summary>
        public static IEnumerable<Vehicle> AllVehicles => _allVehicles;

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
            _allRoutes = Acabus.DataAccess.AcabusData.Session.Read<Route>()
               .OrderBy(r => r.RouteNumber);
            _allDevices = Acabus.DataAccess.AcabusData.Session.Read<Device>()
                .OrderBy(d => d.NumeSeri);
            _allStations = Acabus.DataAccess.AcabusData.Session.Read<Station>()
                .OrderBy(s => s.StationNumber);
            _allVehicles = Acabus.DataAccess.AcabusData.Session.Read<Vehicle>()
                .OrderBy(v => v.EconomicNumber);
            _allFaults = Acabus.DataAccess.AcabusData.Session.Read<DeviceFault>()
                .LoadReference(1).OrderBy(f => f.Description);
            _allCashDestinies = Acabus.DataAccess.AcabusData.Session.Read<CashDestiny>();
            _allTechnicians = Acabus.DataAccess.AcabusData.Session.Read<Technician>()
                                .OrderBy(technician => technician.Name);

            _cc = AllStations.FirstOrDefault(station => station.Name.Contains("CENTRO DE CONTROL"));

            LoadOffDutyVehiclesSettings();
        }
    }
}