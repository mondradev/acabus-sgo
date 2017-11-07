using System;

namespace Acabus.Models
{
    public enum AreaAssignable
    {
        /// <summary>
        ///
        /// </summary>
        MANTTO,

        /// <summary>
        ///
        /// </summary>
        MANTTO_SUPERVISOR,

        /// <summary>
        ///
        /// </summary>
        SUPERVISOR,

        /// <summary>
        ///
        /// </summary>
        SUPERVISOR_SUPPORT,

        /// <summary>
        ///
        /// </summary>
        SUPPORT,

        /// <summary>
        ///
        /// </summary>
        SUPPORT_DATABASE,

        /// <summary>
        ///
        /// </summary>
        DATABASE,

        /// <summary>
        ///
        /// </summary>
        DATABASE_IT_MANAGER,

        /// <summary>
        ///
        /// </summary>
        IT_MANAGER
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
        KVR,

        /// <summary>
        /// Torniquete de E/S.
        /// </summary>
        TOR,

        /// <summary>
        /// Torniquete Doble E/S.
        /// </summary>
        TD,

        /// <summary>
        /// Torniquete Simple de E/S.
        /// </summary>
        TS,

        /// <summary>
        /// Torniquete de Salida.
        /// </summary>
        TSI,

        /// <summary>
        /// Paso de movilidad reducida.
        /// </summary>
        PMR,

        /// <summary>
        /// Grabador de video en red.
        /// </summary>
        NVR,

        /// <summary>
        /// Switch de estación.
        /// </summary>
        SW,

        /// <summary>
        /// Concentrador de estación.
        /// </summary>
        CDE,

        /// <summary>
        /// Servidor de aplicación.
        /// </summary>
        APP,

        /// <summary>
        /// Servidor de patio de encierro.
        /// </summary>
        PDE,

        /// <summary>
        /// Servidor base de datos.
        /// </summary>
        DB,

        /// <summary>
        /// Display Bus
        /// </summary>
        DSPB,

        /// <summary>
        /// Display
        /// </summary>
        DSPL,

        /// <summary>
        /// Contador de pasajeros.
        /// </summary>
        CONT,

        /// <summary>
        /// Grabador de video móvil.
        /// </summary>
        MRV,

        /// <summary>
        /// Torniquete abordo.
        /// </summary>
        TA,

        /// <summary>
        /// Camara.
        /// </summary>
        CAM,

        /// <summary>
        /// Monitor.
        /// </summary>
        MON,

        /// <summary>
        /// PC Abordo.
        /// </summary>
        PCA,

        /// <summary>
        /// Planta electrógena de estación.
        /// </summary>
        PGE,

        /// <summary>
        /// Rack de estación.
        /// </summary>
        RACK,

        /// <summary>
        /// Luminarias
        /// </summary>
        LIGHT,

        /// <summary>
        /// Bocina SITI
        /// </summary>
        SPEAKER
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