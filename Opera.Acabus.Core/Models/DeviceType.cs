using System;

namespace Opera.Acabus.Core.Models
{
    /// <summary>
    /// Define todos los tipos de dispositivos disponibles.
    /// </summary>
    [Flags]
    public enum DeviceType
    {
        /// <summary>
        /// Sin tipo definido.
        /// </summary>
        NONE,

        /// <summary>
        /// Kiosko de venta y recarga.
        /// </summary>
        KVR = 1,

        /// <summary>
        /// Torniquete Doble E/S.
        /// </summary>
        TD = 2,

        /// <summary>
        /// Torniquete de Salida
        /// </summary>
        TS = 4,

        /// <summary>
        /// Torniquete Simple de E/S.
        /// </summary>
        TSI = 8,

        /// <summary>
        /// Torniquete de E/S.
        /// </summary>
        TOR = TD | TS | TSI,

        /// <summary>
        /// Paso de movilidad reducida.
        /// </summary>
        PMR = 16,

        /// <summary>
        /// Grabador de video en red.
        /// </summary>
        NVR = 32,

        /// <summary>
        /// Switch de estación.
        /// </summary>
        SW = 64,

        /// <summary>
        /// Concentrador de estación.
        /// </summary>
        CDE = 128,

        /// <summary>
        /// Servidor de aplicación.
        /// </summary>
        APP = 256,

        /// <summary>
        /// Servidor de patio de encierro.
        /// </summary>
        PDE = 512,

        /// <summary>
        /// Servidor base de datos.
        /// </summary>
        DB = 1024,

        /// <summary>
        /// Display de autobus.
        /// </summary>
        DSPB = 2048,

        /// <summary>
        /// Display de estación.
        /// </summary>
        DSPL = 4096,

        /// <summary>
        /// Contador de pasajeros.
        /// </summary>
        CONT = 8192,

        /// <summary>
        /// Grabador de video móvil.
        /// </summary>
        MRV = 16384,

        /// <summary>
        /// Torniquete abordo.
        /// </summary>
        TA = 32768,

        /// <summary>
        /// Camara.
        /// </summary>
        CAM = 65536,

        /// <summary>
        /// Monitor de autobus.
        /// </summary>
        MON = 131072,

        /// <summary>
        /// PC Abordo.
        /// </summary>
        PCA = 262144,

        /// <summary>
        /// Planta electrógena de estación.
        /// </summary>
        PGE = 524288,

        /// <summary>
        /// Rack de estación.
        /// </summary>
        RACK = 1048576,

        /// <summary>
        /// Luminarias
        /// </summary>
        LIGHT = 2097152,

        /// <summary>
        /// Almacén
        /// </summary>
        STOCK = 4194304
    }
}