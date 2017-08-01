using System;

namespace InnSyTech.Standard.Utils
{
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
        /// Crea una instancia nueva de una credencial de acceso.
        /// </summary>
        public Credential()
        {
            _username = String.Empty;
            _password = String.Empty;
            _type = String.Empty;
            _isRoot = false;
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