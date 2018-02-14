using System;

namespace Acabus.Models
{
    public enum AreaAssignable
    {
        /// <summary>
        ///
        /// </summary>
        MANTTO = 1,

        /// <summary>
        ///
        /// </summary>
        SUPERVISOR = 2,

        /// <summary>
        ///
        /// </summary>
        SUPPORT = 4,

        /// <summary>
        ///
        /// </summary>
        DATABASE = 8,

        /// <summary>
        ///
        /// </summary>
        IT_MANAGER = 16
    }

    /// <summary>
    /// Define los tipos de equipos disponibles.
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// Sin tipo definido.
        /// </summary>
        UNKNOWN,

        /// <summary>
        /// Kiosko de venta y recarga.
        /// </summary>
        KVR = 1,

        /// <summary>
        /// Torniquete Doble E/S.
        /// </summary>
        TD = 2,

        /// <summary>
        /// Torniquete de salida.
        /// </summary>
        TS = 4,

        /// <summary>
        /// Torniquete simple.
        /// </summary>
        TSI = 8,

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
        /// Display Bus
        /// </summary>
        DSPB = 256,

        /// <summary>
        /// Display
        /// </summary>
        DSPL = 512,

        /// <summary>
        /// Contador de pasajeros.
        /// </summary>
        CONT = 1024,

        /// <summary>
        /// Grabador de video móvil.
        /// </summary>
        MRV = 2048,

        /// <summary>
        /// Torniquete abordo.
        /// </summary>
        TA = 4096,

        /// <summary>
        /// Monitor.
        /// </summary>
        MON = 8192,

        /// <summary>
        /// PC Abordo.
        /// </summary>
        PCA = 16384,

        /// <summary>
        /// Planta electrógena de estación.
        /// </summary>
        PGE = 32768,

        /// <summary>
        /// Rack de estación.
        /// </summary>
        RACK = 65536,

        /// <summary>
        /// Luminarias
        /// </summary>
        LIGHT = 131072,

        /// <summary>
        /// Bocina SITI
        /// </summary>
        SPEAKER = 262144,

        /// <summary>
        /// Torniquetes de estación
        /// </summary>
        TOR = TD | TS | TSI,

        /// <summary>
        /// Display SITI
        /// </summary>
        DSP = DSPB | DSPL
    }

    /// <summary>
    /// Define las prioridades que establece una tarea.
    /// </summary>
    public enum Priority
    {
        /// <summary>
        /// Prioridad baja.
        /// </summary>
        LOW,

        /// <summary>
        /// Prioridad media.
        /// </summary>
        MEDIUM,

        /// <summary>
        /// Prioridad alta.
        /// </summary>
        HIGH,

        /// <summary>
        /// Sin prioridad.
        /// </summary>
        NONE
    }

    /// <summary>
    /// Define los tipos de rutas.
    /// </summary>
    public enum RouteType
    {
        /// <summary>
        /// Ruta alimentadora.
        /// </summary>
        ALIM,

        /// <summary>
        /// Ruta troncal.
        /// </summary>
        TRUNK
    }

    /// <summary>
    /// Enumeración que provee de los estados de conexión para los enlaces y dispositivos.
    /// </summary>
    public enum StateValue
    {
        /// <summary>
        /// Equipo sin conexión.
        /// </summary>
        DISCONNECTED,

        /// <summary>
        /// Equipo con mala conexión.
        /// </summary>
        BAD,

        /// <summary>
        /// Equipo con conexión media.
        /// </summary>
        MEDIUM,

        /// <summary>
        /// Equipo con buena conexión.
        /// </summary>
        GOOD
    }

    /// <summary>
    /// Define los posibles estados de un vehículo.
    /// </summary>
    public enum VehicleStatus
    {
        /// <summary>
        /// En reparación.
        /// </summary>
        IN_REPAIR,

        /// <summary>
        /// Sin energía en baterías
        /// </summary>
        WITHOUT_ENERGY,

        /// <summary>
        /// Desconocido.
        /// </summary>
        UNKNOWN
    }

    /// <summary>
    /// Define los tipos de vehículos disponibles.
    /// </summary>
    public enum VehicleType
    {
        /// <summary>
        /// Sin tipo.
        /// </summary>
        UNKNOWN,

        /// <summary>
        /// Autobús convencional.
        /// </summary>
        CONVENTIONAL,

        /// <summary>
        /// Autobús padrón.
        /// </summary>
        STANDARD,

        /// <summary>
        /// Autobús articulado.
        /// </summary>
        ARTICULATED
    }

    /// <summary>
    /// Provee de funciones para el manejo de estados de conexión.
    /// </summary>
    public static class StateValueExtension
    {
        /// <summary>
        /// Obtiene un estado evaluando ambos estados de conexión devolviendo el de menor valor.
        /// </summary>
        /// <param name="state">Un estado a evaluar.</param>
        /// <param name="anotherState">Otro estado a evaluar.</param>
        /// <returns>El estado de menor valor.</returns>
        public static StateValue And(this StateValue state, StateValue anotherState)
        {
            if (state == anotherState)
                return state;
            if (state < anotherState)
                return state;
            return anotherState;
        }

        /// <summary>
        /// Obtiene el estado de conexión determinado por la latencia mínima y máxima aceptable.
        /// </summary>
        /// <param name="ping">Latencia de la conexión.</param>
        /// <param name="pingMin">Latencia mínima.</param>
        /// <param name="pingMax">Latencia máxima.</param>
        /// <returns>Si la latencia revasa el máximo devuelve un 'BAD',
        ///          si revasa el mínimo devuelve un 'MEDIUM',
        ///          si es menor a cero devuelve un 'DISCONNECTED',
        ///          en otro caso devuelve un 'GOOD'.</returns>
        public static StateValue GetConnectionState(Int16 ping, UInt16 pingMin, UInt16 pingMax)
        {
            if (ping > (Int16)pingMax)
                return StateValue.BAD;
            if (ping > (Int16)pingMin)
                return StateValue.MEDIUM;
            if (ping < 0)
                return StateValue.DISCONNECTED;
            return StateValue.GOOD;
        }
    }

    /// <summary>
    /// Define una estructura que almacena las credenciales de acceso a un dispositivo.
    /// </summary>
    public sealed class Credential
    {
        /// <summary>
        /// Campo que provee a la propiedad 'IsRoot'.
        /// </summary>
        private Boolean _isRoot;

        /// <summary>
        /// Campo que provee a la propiedad 'Password'.
        /// </summary>
        private String _password;

        /// <summary>
        /// Campo que provee a la propiedad 'Type'.
        /// </summary>
        private String _type;

        /// <summary>
        /// Campo que provee a la propiedad 'Username'.
        /// </summary>
        private String _username;

        /// <summary>
        /// Crea una instancia nueva de una credencial de acceso.
        /// </summary>
        /// <param name="username">Nombre de usuario.</param>
        /// <param name="password">Clave de acceso.</param>
        /// <param name="type">El tipo de credencial.</param>
        /// <param name="isRoot">Indica si el usuario tiene permisos especiales.</param>
        public Credential(String username, String password, String type, Boolean isRoot = false)
        {
            this._username = username;
            this._password = password;
            this._type = type;
            this._isRoot = isRoot;
        }

        /// <summary>
        /// Obtiene si el usuario actual tiene permisos especiales.
        /// </summary>
        public Boolean IsRoot => _isRoot;

        /// <summary>
        /// Obtiene la clave de acceso.
        /// </summary>
        public String Password => _password;

        /// <summary>
        /// Obtiene el tipo de credencial.
        /// </summary>
        public String Type => _type;

        /// <summary>
        /// Obtiene el nombre de usuario.
        /// </summary>
        public String Username => _username;
    }
}