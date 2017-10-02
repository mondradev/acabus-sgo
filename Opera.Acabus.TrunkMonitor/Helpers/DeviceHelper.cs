using InnSyTech.Standard.Database;
using InnSyTech.Standard.Net;
using InnSyTech.Standard.Utils;
using Opera.Acabus.Core.DataAccess;
using Opera.Acabus.Core.Models;
using Opera.Acabus.TrunkMonitor.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Opera.Acabus.TrunkMonitor.Helpers
{
    /// <summary>
    /// Provee a las instancias de <see cref="Device"/> el manejo de enlaces de comunicación <see
    /// cref="Link"/> y sus propiedades adheridas.
    /// </summary>
    public static class DeviceHelper
    { ///<summary>
      /// Lleva el control de todas las instancias <see cref="Device"/> y su estado de enlace.
      ///</summary>
        private static Dictionary<Device, DeviceStateInfo> _deviceStateInfo
            = new Dictionary<Device, DeviceStateInfo>();

        /// <summary>
        /// Calcula <see cref="LinkState"/> a partir de la latencia obtenida durante <see cref="DoPingLinkDevice(Station)"/>.
        /// </summary>
        /// <param name="device">Estación a obtener su estado de enlace.</param>
        /// <returns>Estado de enlace de la estación.</returns>
        public static LinkState CalculateLinkState(this Device device)
            => LinkStateHelper.CalculateLinkState(
                device.GetPing(),
                device.Station.GetMaximunPing(),
                device.Station.GetMaximunAcceptablePing());

        /// <summary>
        /// Realiza un eco al dispositivo obteniendo el tiempo de este.
        /// </summary>
        /// <param name="device">Dispositivo a realizar eco.</param>
        /// <param name="eco">Especifica el número de ecos al dispositivo.</param>
        /// <returns>Duración del eco.</returns>
        public static Int16 DoPing(this Device device, Int16 eco = 3)
        {
            var info = device.GetStateInfo();
            var ping = ConnectionTCP.SendToPing(device.IPAddress.ToString(), eco);
            var linkState = device.CalculateLinkState();

            info.Ping = ping;
            info.State = linkState;

            return device.GetPing();
        }

        /// <summary>
        /// Obtiene la latencia obtenida durante <see cref="DoPing(Device)"/>.
        /// </summary>
        /// <param name="device">Dispositivo a obtener su propiedad.</param>
        /// <return>Latencia producida por <see cref="DoPing(Device)"/>.</return>
        public static Int16 GetPing(this Device device)
            => device.GetStateInfo().Ping;

        /// <summary>
        /// Obtiene el estado del enlace obtenida durante <see cref="DoPing(Device)"/>.
        /// </summary>
        /// <param name="device">Dispositivo a obtener su propiedad.</param>
        /// <return>El estado del enlace producida por <see cref="DoPing(Device)"/>.</return>
        public static LinkState GetState(this Device device)
            => device.GetStateInfo().State;

        /// <summary>
        /// Obtiene la información de estado del dispositivo.
        /// </summary>
        /// <param name="device">
        /// Instancia de <see cref="Device"/> a obtener su información de estado.
        /// </param>
        /// <returns>La información del estado del dispositivo.</returns>
        public static DeviceStateInfo GetStateInfo(this Device device)
        {
            lock (_deviceStateInfo)
                if (!_deviceStateInfo.ContainsKey(device))
                    _deviceStateInfo.Add(device, new DeviceStateInfo(device));

            return _deviceStateInfo[device];
        }

        /// <summary>
        /// Verifica si el dispositivo tiene transacciones por replicar.
        /// </summary>
        /// <param name="device">Dispositivo a verificar.</param>
        /// <returns>Un valor true si el equipo tiene información pendiente por replicar.</returns>
        public static bool PendingReplica(this Device device)
        {
            var query = AcabusDataContext.ConfigContext["queryCheckReplica"].GetSetting(device.Type.ToString())?.ToString("value");

            if (String.IsNullOrEmpty(query))
            {
                Trace.WriteLine("No hay una consulta SQL para verificar la replica de dispositivos tipo: " + device.Type, "NOTIFY");
                return false;
            }
            try
            {
                var dbType = TypeHelper.LoadFromDll("Libraries/Npgsql.dll", "Npgsql.NpgsqlConnection");
                var session = DbFactory.CreateSession(dbType, new PsqlDialect(String.Format("User ID=postgres;Password=4c4t3k;Host={0};Port=5432;Database=SITM;", device.IPAddress.ToString())));
                var row = session.Batch(query).ElementAt(0);

                if (!row.ContainsKey("pending") || !row.ContainsKey("fch_min"))
                    throw new InvalidOperationException("La consulta de registros pendientes debe devolver dos campos {pending: int, fch_min: datetime}");

                var count = Convert.ToInt32((row["pending"] ?? 0));
                var lastSend = (DateTime)(row["fch_min"] ?? DateTime.Now);

                Trace.WriteLine($"Replica {device} --> reg: {count}, fecha: {lastSend}", "DEBUG");

                return count > 0 && (DateTime.Now - lastSend) > TimeSpan.FromMinutes(10);
            }
            catch (InvalidOperationException ex)
            {
                Trace.WriteLine(ex.PrintMessage().JoinLines(), "ERROR");
                return false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{device} --> " + ex.PrintMessage().JoinLines(), "DEBUG");
                return false;
            }
        }
    }

    /// <summary>
    /// Dialecto utilizado para la comunicación de solo lectura de los equipos conectados.
    /// </summary>
    internal class PsqlDialect : DbDialectBase
    {
        /// <summary>
        /// Crea una instancia nueva de <see cref="PsqlDialect"/>.
        /// </summary>
        /// <param name="connection">Cadena de conexión a la base de datos de los equipos.</param>
        public PsqlDialect(String connection) : base(connection) { }

        /// <summary>
        /// Obtiene el convertidor de fecha de la base de datos.
        /// </summary>
        public override IDbConverter DateTimeConverter => null;

        /// <summary>
        /// Obtiene la función que permite obtener el ultimo insertado.
        /// </summary>
        public override string LastInsertFunctionName => "";

        /// <summary>
        /// Obtiene el número de transacciones por conexión permitidas.
        /// </summary>
        public override int TransactionPerConnection => 10;
    }
}