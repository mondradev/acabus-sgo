using InnSyTech.Standard.Translates;
using System.Collections.Generic;

namespace Opera.Acabus.Core.Models
{
    /// <summary>
    /// Esta clase permite la traducción de la enumeración <see cref="DeviceType"/> a texto en
    /// "Español" que puede leer un ser humano.
    /// </summary>
    public sealed class DeviceTypeSpanishTranslator : EnumTranslator<DeviceType>
    {
        /// <summary>
        /// Crea una instancia del traductor.
        /// </summary>
        public DeviceTypeSpanishTranslator()
            : base(new Dictionary<DeviceType, string>() {
                { DeviceType.TA, "TORNIQUETE ABORDO" },
                { DeviceType.MRV, "GRABADOR DE VIDEO MÓVIL" },
                { DeviceType.PCA, "PC ABORDO" },
                { DeviceType.CAM, "CAMARA" },
                { DeviceType.MON, "MONITOR" },
                { DeviceType.CONT, "CONTADOR DE PASAJEROS" },
                { DeviceType.DSPB, "DISPLAY DE AUTOBUS" },
                { DeviceType.CDE, "CONCENTRADOR DE ESTACIÓN" },
                { DeviceType.DSPL, "DISPLAY DE ESTACIÓN" },
                { DeviceType.KVR, "KIOSKO DE VENTA Y RECARGA" },
                { DeviceType.NVR, "GRABADOR DE VIDEO EN RED" },
                { DeviceType.PMR, "PASO DE MOVILIDAD REDUCIDA" },
                { DeviceType.SW, "SWITCH DE ESTACIÓN" },
                { DeviceType.TD, "TORNIQUETE DOBLE E/S" },
                { DeviceType.TS, "TORNIQUETE DE SALIDA" },
                { DeviceType.TSI, "TORNIQUETE SIMPLE E/S" },
                { DeviceType.TOR, "TORNIQUETE GENÉRICO E/S" },
                { DeviceType.APP, "SERVIDOR DE APLICACIÓN" },
                { DeviceType.DB, "SERVIDOR BASE DE DATOS" },
                { DeviceType.PDE, "SERVIDOR PATIO DE ENCIERRO" },
                { DeviceType.PGE, "PLANTA ELECTRÓGENA" },
                { DeviceType.RACK, "RACK DE ESTACIÓN" },
                { DeviceType.LIGHT, "LUMINARIA DE ESTACIÓN" },
                { DeviceType.STOCK, "ALMACÉN" },
                { DeviceType.NONE, "NINGUNO" }
            })
        {
        }
    }
}