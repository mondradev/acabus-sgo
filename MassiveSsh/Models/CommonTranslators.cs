using Acabus.Converters;
using Acabus.Utils;
using System.Collections.Generic;

namespace Acabus.Models
{
    #region TranslateConverters

    public sealed class DeviceTypeConverter : TranslateEnumConverter<DeviceType>
    {
        public DeviceTypeConverter() : base(DeviceTypeTranslator.GetTranslator())
        {
        }
    }

    public sealed class RouteTypeConverter : TranslateEnumConverter<RouteType>
    {
        public RouteTypeConverter() : base(new Dictionary<RouteType, string>() {
            { RouteType.ALIM, "ALIMENTADORA" },
            { RouteType.TRUNK, "TRONCAL" }
        })
        {
        }
    }

    public sealed class VehicleTypeConverter : TranslateEnumConverter<VehicleType>
    {
        public VehicleTypeConverter() : base(new Dictionary<VehicleType, string>() {
            { VehicleType.ARTICULATED, "ARTICULADO" },
            { VehicleType.STANDARD, "PADRÓN" },
            { VehicleType.CONVENTIONAL, "CONVENCIONAL" },
            { VehicleType.UNKNOWN, "(TIPO DESCONOCIDO)" }
        })
        {
        }
    }

    #endregion TranslateConverters

    #region Translators

    public static class DeviceTypeTranslator
    {
        private static EnumTranslator<DeviceType> _translator = new Translator();

        public static string Translate(this DeviceType deviceType) =>
            _translator.Translate(deviceType);

        internal static EnumTranslator<DeviceType> GetTranslator() => _translator;

        private class Translator : EnumTranslator<DeviceType>
        {
            public Translator() : base(new Dictionary<DeviceType, string>() {
                { DeviceType.TA, "TORNIQUETE ABORDO" },
                { DeviceType.MRV, "GRABADOR DE VIDEO MÓVIL" },
                { DeviceType.PCA, "PC ABORDO" },
                { DeviceType.MON, "MONITOR" },
                { DeviceType.CONT, "CONTADOR DE PASAJEROS" },
                { DeviceType.DSPB, "DISPLAY BUS" },
                { DeviceType.CDE, "CONCENTRADOR DE ESTACIÓN" },
                { DeviceType.DSPL, "DISPLAY ESTACIÓN" },
                { DeviceType.KVR, "KIOSKO DE VENTA Y RECARGA" },
                { DeviceType.NVR, "GRABADOR DE VIDEO EN RED" },
                { DeviceType.PMR, "PASO DE MOVILIDAD REDUCIDA" },
                { DeviceType.SW, "SWITCH DE ESTACIÓN" },
                { DeviceType.TD, "TORNIQUETE DOBLE E/S" },
                { DeviceType.TS, "TORNIQUETE DE SALIDA" },
                { DeviceType.TSI, "TORNIQUETE SIMPLE E/S" },
                { DeviceType.PGE, "PLANTA ELECTRÓGENA" },
                { DeviceType.RACK, "RACK DE ESTACIÓN" },
                { DeviceType.LIGHT, "LUMINARIAS" },
                { DeviceType.SPEAKER, "BOCINA" },
                { DeviceType.TOR, "TORNIQUETE DE ESTACIÓN" },
                { DeviceType.DSP, "DISPLAY SITI" },
                { DeviceType.UNKNOWN, "(TIPO DESCONOCIDO)" }
            })
            {
            }
        }
    }

    #endregion Translators
}